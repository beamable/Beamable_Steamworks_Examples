using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Editor.Content.Components;
using Beamable.Editor.Content.Models;
using Beamable.Editor;
using Beamable.Editor.Login.UI;
using UnityEditor;
using Beamable.Editor.NoUser;
using Beamable.Editor.Toolbox.Components;
using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.Toolbox.UI.Components;
using Beamable.Editor.UI.Buss;
using Beamable.Editor.UI.Buss.Components;
using Beamable.Editor.UI.Components;
using Beamable.Platform.SDK;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Toolbox.UI
{
    public class ToolboxWindow : EditorWindow
    {
        [MenuItem(
            BeamableConstants.MENU_ITEM_PATH_WINDOW_BEAMABLE + "/" +
            BeamableConstants.OPEN + " " +
            BeamableConstants.TOOLBOX,
            priority = BeamableConstants.MENU_ITEM_PATH_WINDOW_PRIORITY_1
        )]
        public static async void Init()
        {
            await LoginWindow.CheckLogin(typeof(SceneView));

            // Ensure at most one Beamable ContentManagerWindow exists
            // If exists, rebuild it from scratch (easy refresh mechanism)
            if (ToolboxWindow.IsInstantiated)
            {
                if (ToolboxWindow.Instance != null && Instance && EditorWindow.FindObjectOfType(typeof(ToolboxWindow)) != null)
                {
                    ToolboxWindow.Instance.Close();
                }
                DestroyImmediate(ToolboxWindow.Instance);
            }

            // Create Beamable ContentManagerWindow and dock it next to Unity Hierarchy Window
            var contentManagerWindow = GetWindow<ToolboxWindow>(BeamableConstants.TOOLBOX, true, typeof(SceneView));

            contentManagerWindow.Show(true);
        }
        public static ToolboxWindow Instance { get; private set; }
        public static bool IsInstantiated { get { return Instance != null; } }

        private ToolboxComponent _toolboxComponent;
        private VisualElement _windowRoot;

        private ToolboxActionBarVisualElement _actionBarVisualElement;
        private ToolboxBreadcrumbsVisualElement _breadcrumbsVisualElement;
        private ToolboxContentListVisualElement _contentListVisualElement;
        // private ToolboxSelectionListVisualElement _selectionListVisualElement;
        private SearchBarVisualElement _searchBarVisualElement;

        private ToolboxModel _model;
        private ToolboxAnnouncementListVisualElement _announcementListVisualElement;


        private void OnEnable()
        {
            Instance = this;
            minSize = new Vector2(560, 300);


            // Refresh if/when the user logs-in or logs-out while this window is open
            EditorAPI.Instance.Then(de => { de.OnUserChange += _ => Refresh(); });

            // Force refresh to build the initial window
            Refresh();
        }

        private void OnGUI()
        {

        }


        private async void Refresh()
        {
            if (Instance != null)
            {
                Instance._model?.Destroy();
            }
            Instance._model = new ToolboxModel();
            Instance._model.UseDefaultWidgetSource();
            Instance._model.Initialize();
            var de = await EditorAPI.Instance;
            var isLoggedIn = de.User != null;
            if (isLoggedIn)
            {
                SetForContent();
            }
            else
            {
                SetForLogin();
            }
        }


      void SetForLogin()
      {
         var root = this.GetRootVisualContainer();
         root.Clear();
         var noUserVisualElement = new NoUserVisualElement();
         root.Add(noUserVisualElement);
      }

      void SetForContent()
      {
         var root = this.GetRootVisualContainer();
         root.Clear();
         var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{ToolboxConstants.BASE_PATH}/ToolboxWindow.uxml");
         _windowRoot = uiAsset.CloneTree();
         _windowRoot.AddStyleSheet($"{ToolboxConstants.BASE_PATH}/ToolboxWindow.uss");
         _windowRoot.name = nameof(_windowRoot);

         root.Add(_windowRoot);

         _actionBarVisualElement = root.Q<ToolboxActionBarVisualElement>("actionBarVisualElement");
         _actionBarVisualElement.Model = _model;
         _actionBarVisualElement.Refresh();

         _breadcrumbsVisualElement = root.Q<ToolboxBreadcrumbsVisualElement>("breadcrumbsVisualElement");
         _breadcrumbsVisualElement.Model = _model;
         _breadcrumbsVisualElement.Refresh();

         _contentListVisualElement = root.Q<ToolboxContentListVisualElement>("contentListVisualElement");
         _contentListVisualElement.Model = _model;
         _contentListVisualElement.Refresh();

         _announcementListVisualElement = root.Q<ToolboxAnnouncementListVisualElement>();
         _announcementListVisualElement.Model = _model;
         _announcementListVisualElement.Refresh();
         _announcementListVisualElement.OnHeightChanged += AnnouncementList_OnHeightChanged;

         _actionBarVisualElement.OnInfoButtonClicked += () =>
         {
             Debug.Log("Show info");
             Application.OpenURL(BeamableConstants.URL_TOOL_WINDOW_TOOLBOX);
         };

         _breadcrumbsVisualElement.OnAccountButtonClicked += (position) =>
         {
             var wnd = LoginWindow.Init();
             Rect popupWindowRect = BeamablePopupWindow.GetLowerRightOfBounds(position);
             wnd.position = new Rect(popupWindowRect.x - wnd.minSize.x, popupWindowRect.y + 10, wnd.minSize.x, wnd.minSize.y);
         };

         _model.OnAnnouncementsChanged += OnAnnouncementsChanged;



         CheckForDeps();
      }

      private void AnnouncementList_OnHeightChanged(float height)
      {
          // TODO: animate the height...
          _contentListVisualElement?.style.SetTop(65 + height);
      }

      private void OnAnnouncementsChanged(IEnumerable<ToolboxAnnouncement> announcements)
      {
          var list = announcements.ToList();

      }

      private void CheckForDeps()
      {
          EditorAPI.Instance.Then(b =>
          {
              if (b.HasDependencies()) return;

              var depAnnouncement = new ToolboxAnnouncement();

              depAnnouncement.CustomIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.beamable/Editor/UI/Common/Icons/welcome.png");
              depAnnouncement.TitleElement = new Label("Beamable + TextMeshPro + Addressables = ♥");

              var descElement = new VisualElement();
              depAnnouncement.DescriptionElement = descElement;
              descElement.Add(new Label("Welcome to Beamable! This package includes official Unity assets").AddTextWrapStyle());
              var tmpProButton = new Button(() => Application.OpenURL("https://docs.unity3d.com/Manual/com.unity.textmeshpro.html"))
              {
                  text = "TextMeshPro"
              };
              tmpProButton.AddToClassList("noBackground");
              tmpProButton.AddToClassList("announcementButton");

              descElement.Add(tmpProButton);

              descElement.Add(new Label("and").AddTextWrapStyle());
              var addressablesButton = new Button(() => Application.OpenURL("https://docs.unity3d.com/Manual/com.unity.addressables.html"))
              {
                  text = "Addressables"
              };
              addressablesButton.AddToClassList("noBackground");
              addressablesButton.AddToClassList("announcementButton");

              descElement.Add(addressablesButton);
              descElement.Add(new Label("in order to provide UI prefabs you can easily drag & drop into your game. To complete the installation, we must add them to your project now.").AddTextWrapStyle());

              depAnnouncement.ActionText = "Import Assets";
              depAnnouncement.Status = ToolboxAnnouncementStatus.INFO;
              depAnnouncement.Action = () =>
              {
                  b.CreateDependencies().Then(_ =>
                  {
                      _model.RemoveAnnouncement(depAnnouncement);
                  });
              };
              _model.AddAnnouncement(depAnnouncement);

          });
      }
    }
}