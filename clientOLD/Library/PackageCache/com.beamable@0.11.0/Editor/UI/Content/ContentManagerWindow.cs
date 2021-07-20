using System;
using System.Collections.Generic;
using Beamable.Common.Api.Auth;
using Beamable.Editor.Content.Components;
using Beamable.Editor.Content.Models;
using Beamable.Editor;
using Beamable.Editor.Login.UI;
using UnityEditor;
using Beamable.Editor.NoUser;
using Beamable.Editor.Realms;
using Beamable.Editor.UI.Buss.Components;
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

namespace Beamable.Editor.Content
{
   public class ContentManagerWindow : EditorWindow
   {
      [MenuItem(
      BeamableConstants.MENU_ITEM_PATH_WINDOW_BEAMABLE + "/" +
      BeamableConstants.OPEN + " " +
      BeamableConstants.CONTENT_MANAGER,
      priority = BeamableConstants.MENU_ITEM_PATH_WINDOW_PRIORITY_2
      )]
      public static async void Init()
      {
         await LoginWindow.CheckLogin(typeof(ContentManagerWindow), typeof(SceneView));
         // Create Beamable ContentManagerWindow and dock it next to Unity Hierarchy Window
         var contentManagerWindow = GetWindow<ContentManagerWindow>(BeamableConstants.CONTENT_MANAGER, true, typeof(ContentManagerWindow),typeof(SceneView));
         contentManagerWindow.Show(true);
      }


      public static ContentManagerWindow Instance { get; private set; }
      public static bool IsInstantiated { get { return Instance != null; } }


      private ContentManager _contentManager;
      private VisualElement _windowRoot;
      private VisualElement _explorerContainer, _statusBarContainer;

      private ActionBarVisualElement _actionBarVisualElement;
      private ExplorerVisualElement _explorerElement;
      private StatusBarVisualElement _statusBarElement;

      private void OnEnable()
      {
         // Refresh if/when the user logs-in or logs-out while this window is open
         EditorAPI.Instance.Then(de =>
         {
            de.OnUserChange += HandleUserChange;
            de.OnRealmChange += HandleRealmChange;
         });
         minSize = new Vector2(560, 300);

         // Force refresh to build the initial window
         Refresh();
      }

      private void OnDisable()
      {
         EditorAPI.Instance.Then(de =>
         {
            de.OnUserChange -= HandleUserChange;
            de.OnRealmChange -= HandleRealmChange;
         });
      }

      private void HandleRealmChange(RealmView realm)
      {
         Refresh();
      }

      private void HandleUserChange(User user)
      {
         Refresh();
      }

      public void Refresh()
      {
         EditorAPI.Instance.Then(beamable =>
         {
            var isLoggedIn = beamable.HasToken;
            if (!isLoggedIn)
            {
               Debug.LogWarning("You are accessing the Beamable Content Manager, but you are not logged in. You may see out of sync data.");
            }
         });
         SetForContent();
      }

      void SetForLogin()
      {
         var root = this.GetRootVisualContainer();
         root.Clear();
         var noUserVisualElement = new NoUserVisualElement();
         root.Add(noUserVisualElement);
      }


      public void SoftReset()
      {
         _contentManager.Model.TriggerSoftReset();
      }

      void SetForContent()
      {
         ContentManager oldManager = null;
         if (Instance != null)
         {
            oldManager = Instance._contentManager;
            oldManager?.Destroy();
         }

         _contentManager?.Destroy();
         _contentManager = new ContentManager();
         _contentManager.Initialize();
         Instance = this;

         var root = this.GetRootVisualContainer();

         root.Clear();
         var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{ContentManagerConstants.BASE_PATH}/ContentManagerWindow.uxml");
         _windowRoot = uiAsset.CloneTree();
         _windowRoot.AddStyleSheet($"{ContentManagerConstants.BASE_PATH}/ContentManagerWindow.uss");
         _windowRoot.name = nameof(_windowRoot);

         root.Add(_windowRoot);

         _actionBarVisualElement = root.Q<ActionBarVisualElement>("actionBarVisualElement");
         _actionBarVisualElement.Model = _contentManager.Model;
         _actionBarVisualElement.Refresh();

         // Handlers for Buttons (Left To Right in UX)
         _actionBarVisualElement.OnAddItemButtonClicked += (typeDescriptor) =>
         {
            _contentManager.AddItem(typeDescriptor);
         };
         //
         _actionBarVisualElement.OnValidateButtonClicked += () =>
         {
            var validatePopup = new ValidateContentVisualElement();
            validatePopup.DataModel = _contentManager.Model;
            var wnd = BeamablePopupWindow.ShowUtility(ContentManagerConstants.ValidateContent, validatePopup, this);
            wnd.minSize = ContentManagerConstants.WindowSizeMinimum;

            validatePopup.OnCancelled += () => wnd.Close();
            validatePopup.OnClosed += () => wnd.Close();

            _contentManager.ValidateContent(validatePopup.SetProgress, validatePopup.HandleValidationErrors)
               .Then(_ => validatePopup.HandleFinished());
         };

         _actionBarVisualElement.OnPublishButtonClicked += () =>
         {
            // validate and create publish set.
            var validatePopup = new ValidateContentVisualElement();
            validatePopup.DataModel = _contentManager.Model;

            var wnd = BeamablePopupWindow.ShowUtility(ContentManagerConstants.ValidateContent, validatePopup, this);
            wnd.minSize = ContentManagerConstants.WindowSizeMinimum;
            wnd.position = new Rect(wnd.position.x, wnd.position.y, wnd.minSize.x, wnd.minSize.y);

            validatePopup.OnCancelled += () => wnd.Close();
            validatePopup.OnClosed += () => wnd.Close();

            _contentManager.ValidateContent(validatePopup.SetProgress, validatePopup.HandleValidationErrors)
               .Then(errors =>
               {
                  validatePopup.HandleFinished();

                  if (errors.Count != 0) return;

                  var publishPopup = new PublishContentVisualElement();
                  publishPopup.DataModel = _contentManager.Model;
                  publishPopup.PublishSet = _contentManager.CreatePublishSet();
                  wnd.SwapContent(publishPopup);
                  wnd.titleContent = new GUIContent("Publish Content");

                  publishPopup.OnCancelled += () => wnd.Close();
                  publishPopup.OnCompleted += () =>
                  {
                     wnd.Close();
                  };
                  publishPopup.OnPublishRequested += (set, prog, finished) =>
                  {
                     _contentManager.PublishContent(set, prog, finished).Then(_ => SoftReset());
                  };

               });

         };

         _actionBarVisualElement.OnDownloadButtonClicked += () =>
         {
            var downloadPopup = new DownloadContentVisualElement();

            downloadPopup.Model = _contentManager.PrepareDownloadSummary();
            var wnd = BeamablePopupWindow.ShowUtility(ContentManagerConstants.DownloadContent, downloadPopup, this);
            wnd.minSize = ContentManagerConstants.WindowSizeMinimum;
            wnd.position = new Rect(wnd.position.x, wnd.position.y, wnd.minSize.x, wnd.minSize.y);

            downloadPopup.OnRefreshContentManager += () => _contentManager.RefreshWindow(true);
            downloadPopup.OnClosed += () => wnd.Close();
            downloadPopup.OnCancelled += () => wnd.Close();
            downloadPopup.OnDownloadStarted += (summary, prog, finished) =>
            {
               _contentManager.DownloadContent(summary, prog, finished).Then(_ => SoftReset());
            };
         };

         _actionBarVisualElement.OnRefreshButtonClicked += () =>
         {
            _contentManager.RefreshWindow(true);
         };

         _actionBarVisualElement.OnDocsButtonClicked += () =>
         {
            _contentManager.ShowDocs();
         };

         _explorerContainer = root.Q<VisualElement>("explorer-container");
         _statusBarContainer = root.Q<VisualElement>("status-bar-container");

         _explorerElement = new ExplorerVisualElement();
         _explorerContainer.Add(_explorerElement);
         _explorerElement.OnAddItemButtonClicked += ExplorerElement_OnAddItemButtonClicked;
         _explorerElement.OnAddItemRequested += ExplorerElement_OnAddItem;
         _explorerElement.OnItemDownloadRequested += ExplorerElement_OnDownloadItem;

         _explorerElement.Model = _contentManager.Model;
         _explorerElement.Refresh();

         _statusBarElement = new StatusBarVisualElement();
         _statusBarElement.Model = _contentManager.Model;
         _statusBarContainer.Add(_statusBarElement);
         _statusBarElement.Refresh();



      }

      private void ExplorerElement_OnAddItemButtonClicked()
      {
         var newContent = _contentManager.AddItem();
         EditorApplication.delayCall += () =>
         {
            if (_contentManager.Model.GetDescriptorForId(newContent.Id, out var item))
            {
               item.ForceRename();
            }
         };
      }

      private void ExplorerElement_OnAddItem(ContentTypeDescriptor type)
      {
         var newContent = _contentManager.AddItem(type);
         EditorApplication.delayCall += () =>
         {
            if (_contentManager.Model.GetDescriptorForId(newContent.Id, out var item))
            {
               item.ForceRename();
            }
         };
      }

      private void ExplorerElement_OnDownloadItem(List<ContentItemDescriptor> items)
      {
         var downloadPopup = new DownloadContentVisualElement();

         downloadPopup.Model = _contentManager.PrepareDownloadSummary(items.ToArray());
         var wnd = BeamablePopupWindow.ShowUtility(ContentManagerConstants.DownloadContent, downloadPopup, this);
         wnd.minSize = ContentManagerConstants.WindowSizeMinimum;

         downloadPopup.OnClosed += () => wnd.Close();
         downloadPopup.OnCancelled += () => wnd.Close();
         downloadPopup.OnDownloadStarted += (summary, prog, finished) =>
         {
            _contentManager.DownloadContent(summary, prog, finished).Then(_ => Refresh());
         };
      }
   }
}