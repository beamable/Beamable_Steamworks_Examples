using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Editor;
using Beamable.Editor.Login.UI;
using Beamable.Editor.Microservice.UI.Components;
using Beamable.Editor.UI.Buss.Components;
using UnityEditor;
using Beamable.Server.Editor.UI.Components;
using UnityEngine;
// using ActionBarVisualElement = Beamable.Editor.Microservice.UI.Components.ActionBarVisualElement;
// using MicroserviceBreadcrumbsVisualElement = Beamable.Editor.Microservice.UI.Components.MicroserviceBreadcrumbsVisualElement;
using Debug = UnityEngine.Debug;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif


namespace Beamable.Editor.Microservice.UI
{
    public class MicroserviceWindow : EditorWindow
    {
#if BEAMABLE_NEWMS
        [MenuItem(
            BeamableConstants.MENU_ITEM_PATH_WINDOW_BEAMABLE + "/" +
            BeamableConstants.OPEN + " " +
            BeamableConstants.MICROSERVICES_MANAGER,
            priority = BeamableConstants.MENU_ITEM_PATH_WINDOW_PRIORITY_2
        )]
#endif
        public static async void Init()
        {
            var inspector = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            await LoginWindow.CheckLogin(inspector);

            // Ensure at most one Beamable ContentManagerWindow exists
            // If exists, rebuild it from scratch (easy refresh mechanism)
            if (IsInstantiated)
            {
                if (Instance != null && Instance &&
                    EditorWindow.FindObjectOfType(typeof( MicroserviceWindow)) != null)
                {
                  Instance.Close();
                }

                DestroyImmediate(Instance);
            }

            // Create Beamable ContentManagerWindow and dock it next to Unity Hierarchy Window
            var contentManagerWindow = GetWindow< MicroserviceWindow>(BeamableConstants.MICROSERVICES_MANAGER, true, inspector);

            contentManagerWindow.Show(true);
        }
        
        private VisualElement _windowRoot;
        // private MicroserviceWindow _microserviceWindow;
        private ActionBarVisualElement _actionBarVisualElement;
        private MicroserviceBreadcrumbsVisualElement _microserviceBreadcrumbsVisualElement;
        private MicroserviceContentVisualElement _microserviceContentVisualElement;
        // private LogVisualElement _logVisualElement;


        public static  MicroserviceWindow Instance { get; private set; }

        public static bool IsInstantiated
        {
            get { return Instance != null; }
        }

        void SetForContent()
        {
            var root = this.GetRootVisualContainer();
            root.Clear();
            var uiAsset =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{Constants.SERVER_UI}/MicroserviceWindow.uxml");
            _windowRoot = uiAsset.CloneTree();
            _windowRoot.AddStyleSheet($"{Constants.SERVER_UI}/MicroserviceWindow.uss");
            _windowRoot.name = nameof(_windowRoot);

            root.Add(_windowRoot);
            
            _actionBarVisualElement = root.Q<ActionBarVisualElement>("actionBarVisualElement");
            _actionBarVisualElement.Refresh();

            _microserviceBreadcrumbsVisualElement = root.Q<MicroserviceBreadcrumbsVisualElement>("microserviceBreadcrumbsVisualElement");
            _microserviceBreadcrumbsVisualElement.Refresh();
            
            _microserviceContentVisualElement = root.Q<MicroserviceContentVisualElement>("microserviceContentVisualElement");
            _microserviceContentVisualElement.Refresh();
            
            _actionBarVisualElement.OnInfoButtonClicked += () =>
            {
                Debug.Log("Show info");
                Application.OpenURL(BeamableConstants.URL_BEAMABLE_DOCS_WEBSITE);
            };

            _actionBarVisualElement.OnCreateNewClicked += () =>
            {
            _microserviceContentVisualElement.additem("test");
            };

            _actionBarVisualElement.OnPublishClicked += () =>
            {
                var PublishPopup = new PublishPopup();
                var wnd = BeamablePopupWindow.ShowUtility(Constants.Publish, PublishPopup, this);
                wnd.minSize = Constants.WindowSizeMinimum;
                wnd.position = new Rect(wnd.position.x, wnd.position.y, wnd.minSize.x, wnd.minSize.y);
                
            };
            
            _microserviceContentVisualElement.OnPopupBtnClicked += () =>
            {
                var PopupBtn = new  LogVisualElement();
                var wnd = BeamablePopupWindow.ShowUtility(Constants.PopUpBtn,  PopupBtn, this);
                wnd.minSize = Constants.WindowSizeMinimum;
                wnd.position = new Rect(wnd.position.x, wnd.position.y, wnd.minSize.x, wnd.minSize.y);
            };

            
            
            
            _actionBarVisualElement.OnRefreshButtonClicked += () =>
            {
                RefreshWindow(true);
            };
            
        }

        public void RefreshWindow(bool isHardRefresh)
        {
            if (isHardRefresh)
            {
                MicroserviceWindow.Instance.Refresh();
            }
            else
            {
                RefreshServer();
            }
        }

        private void RefreshServer()
        {
            throw new NotImplementedException();
        }

        private void Refresh()
        {
            _microserviceContentVisualElement.Refresh();
            // throw new NotImplementedException();
        }

        private void OnEnable()
        {
            Instance = this;
            
            SetForContent();
        }
    }
    
}