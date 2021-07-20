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

namespace Beamable.Editor.Microservice.UI.Components
{
    public class LogVisualElement : MicroserviceComponent
    {
        private Button _buildDropDown;
        private Button _advanceDropDown;

        private VisualElement _logListRoot;
        private ListView _listView;
        private string _statusClassName;


        public new class UxmlFactory : UxmlFactory<LogVisualElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription
                {name = "custom-text", defaultValue = "nada"};

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var self = ve as LogVisualElement;

            }
        }
        
        public event Action OnPopupBtnClicked;
        private TextField _nameTextField;
        private string _nameBackup;
        private List<LogMessageModel> testLogList;
        private Label _statusLabel;
        private VisualElement _statusIcon;
        private VisualElement _remoteStatusIcon;
        private Label _remoteStatusLabel;
        private Button _popupBtn;

        private object _logVisualElement;

        // private LoadingBarVisualElement _loadingBar;

        public override void Refresh()
        {
            base.Refresh();
            
            // _loadingBar = Root.Q<LoadingBarVisualElement>();
            // var loadingBlocker = Root.Q<LoadingIndicatorVisualElement>();
            RegisterCallback<MouseDownEvent>(OnMouseDownEvent,
                TrickleDown.TrickleDown);
            
            _nameTextField = Root.Q<TextField>("microserviceTitle");
            _nameTextField.SetEnabled(false);
            
            _nameTextField.RegisterCallback<FocusEvent>(NameLabel_OnFocus,
                TrickleDown.TrickleDown);
            _nameTextField.RegisterCallback<BlurEvent>(NameLabel_OnBlur,
                TrickleDown.TrickleDown);
            _nameTextField.RegisterCallback<KeyDownEvent>(NameLabel_OnKeydown,
                TrickleDown.TrickleDown);
            _nameTextField.RegisterCallback<KeyUpEvent>(NameLabel_OnKeyup,
                TrickleDown.TrickleDown);
            
            _buildDropDown = Root.Q<Button>("buildDropDown");
            _buildDropDown.clickable.clicked += () => { BuildDropDown_OnClicked(_buildDropDown.worldBound); };
            
            _advanceDropDown = Root.Q<Button>("advanceBtn");
            _advanceDropDown.clickable.clicked += () => { AdvanceDropDown_OnClicked(_advanceDropDown.worldBound); };
            
            _popupBtn = Root.Q<Button>("popupBtn");
            _popupBtn.clickable.clicked += () => {
                OnPopupBtnClicked?.Invoke();
            };

            _statusLabel = Root.Q<Label>("statusTitle");
            _remoteStatusLabel = Root.Q<Label>("remoteStatusTitle");

            _statusIcon = Root.Q<VisualElement>("statusIcon");
            UpdateStatusIcon();

            _remoteStatusIcon = Root.Q<VisualElement>("remoteStatusIcon");
            UpdateRemoteStatusIcon();
            
            //List
            testLogList = new List<LogMessageModel>();
            testLogList.Add(new LogMessageModel{Message = "test", Time ="[11 11 11]"});
            testLogList.Add(new LogMessageModel{Message = "test2", Time ="[11 11 12]"});
            _logListRoot = Root.Q("logListRoot");
            _listView = CreateListView();
            _logListRoot.Add(_listView);


            // Model.OnSelectedContentChanged += Model_OnSelectedContentChanged;
            // Model.OnFilteredContentsChanged += Model_OnFilteredContentChanged;
            //
            // var manipulator = new ContextualMenuManipulator(ContentVisualElement_OnContextMenuOpen);
            // _listView.AddManipulator(manipulator);

            _listView.Refresh();
            
        }

        private void Update()
        {
            // TODO: Uncomment this to update mode ltitle name
            // _nameTextField.value = ContentItemDescriptor.Name;
        }
        
         public void RenameGestureBegin()
         {
             _nameBackup = _nameTextField.value;
             _nameTextField.SetEnabled(true);
             _nameTextField.BeamableFocus();
          }


          private void NameLabel_OnFocus(FocusEvent evt)
          {
             _nameTextField.SelectAll();
          }

          private void NameLabel_OnKeydown(KeyDownEvent evt)
          {
             evt.StopPropagation();
             switch (evt.keyCode)
             {
                case KeyCode.Escape:
                   CancelName();
                   break;
                case KeyCode.Return:
                   CommitName();
                   break;
             }
          }

          private void NameLabel_OnKeyup(KeyUpEvent evt)
          {
             CheckName();
          }

          private void NameLabel_OnBlur(BlurEvent evt)
          {
             CommitName();
          }

          private void CommitName()
          {
             //if (string.Equals(_nameBackup, _nameTextField.value)) return;

             _nameTextField.SelectRange(0, 0);
             _nameTextField.SetEnabled(false);
             //Invokes internal event
             try
             {
                 //TODO: Assign text field value to model title name
                // ContentItemDescriptor.Name = _nameTextField.value;
             }
             catch(Exception ex)
             {
                Debug.LogWarning($"Cannot assign name. message=[{ex.Message}]");
                CancelName();
             }
             finally
             {
                _nameTextField.Blur();
             }
          }

          private void CancelName()
          {
             _nameTextField.SetValueWithoutNotify(_nameBackup);
             _nameTextField.Blur();
          }

          public void CheckName()
          {
             var name = _nameTextField.value;
             // TODO: Handle naming exceptions
             // var content = ContentItemDescriptor?.GetContent();
             // if (content != null && ContentNameValidationException.HasNameValidationErrors(content, name, out var errors))
             // {
             //    foreach (var error in errors)
             //    {
             //       var replaceWith = error.InvalidChar == ' ' ? "_" : "";
             //       name = name.Replace(error.InvalidChar.ToString(), replaceWith);
             //    }
             // }

             _nameTextField.value = name;
          }

          private void OnMouseDownEvent(MouseDownEvent evt)
          {
              //Double click
              if (evt.clickCount == 2)
              {
                  RenameGestureBegin();
              }
          }



          // private void microserviceTitle_OnItemRenameGestureBegin(LogItemDescriptor logItemDescriptor)
        // {
        //     ContentVisualElement contentVisualElement = GetVisualItemByData(logItemDescriptor);
        //     contentVisualElement.RenameGestureBegin();
        // }

        private void UpdateRemoteStatusIcon()
        {
            if (!string.IsNullOrEmpty(_statusClassName))
            {
                _remoteStatusIcon.RemoveFromClassList(_statusClassName);
            }

            string statusRemote = "remoteDeploying";
            switch (statusRemote)
            {
                case "remoteDeploying":
                    _statusClassName = "remoteDeploying";
                    _remoteStatusLabel.text = "Remote Deploying";
                    break;
                case "remoteRunning":
                    _statusClassName = "remoteRunning";
                    _remoteStatusLabel.text = "Remote Running";
                    break;
                default:
                    _statusClassName = "remoteStopped";
                    _remoteStatusLabel.text = "Remote Stopped";
                    break;
            }
            _remoteStatusIcon.AddToClassList(_statusClassName);
        
        }

        private void UpdateStatusIcon()
        {

            if (!string.IsNullOrEmpty(_statusClassName))
            {
                _statusIcon.RemoveFromClassList(_statusClassName);
            }

            string status = "localRunning";
            switch (status)
            {
                case "localRunning":
                    _statusLabel.text = "Local Running";
                    _statusClassName = "localRunning";
                    break;
                case "localBuilding":
                    _statusClassName = "localBuilding";
                    _statusLabel.text = "Local Building";
                    break;
                case "localStopped":
                    _statusClassName = "localStopped";
                    _statusLabel.text = "Local Stopped";
                    break;
                default:
                    _statusClassName = "different";
                    _statusLabel.text = "Different";
                    break;
            }
            _statusIcon.AddToClassList(_statusClassName);
        }

        private void AdvanceDropDown_OnClicked(Rect visualElementBounds)
        {
            Rect popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);
            var content = new AdvanceDropDown();
            // content.Model = Model;
            var longest = "";
            var width = Mathf.Max(150, longest.Length * 2 + 30);
            var wnd = BeamablePopupWindow.ShowDropdown("advance", popupWindowRect, new Vector2(width, 60), content);

            content.Refresh();
        }

        private void BuildDropDown_OnClicked(Rect visualElementBounds)
        {
            Rect popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);
            var content = new BuildDropDown();
            // content.Model = Model;
            var longest = "";
            var width = Mathf.Max(150, longest.Length * 2 + 30);
            var wnd = BeamablePopupWindow.ShowDropdown("build", popupWindowRect, new Vector2(width, 60), content);

            content.Refresh();
        }
        
        private ListView CreateListView()
        {
            var view = new ListView()
            {
                makeItem = CreateListViewElement,
                bindItem = BindListViewElement,
                selectionType = SelectionType.Single,
                itemHeight = 24,
                itemsSource = testLogList
            };

            view.BeamableOnItemChosen(ListView_OnItemChosen);
            view.BeamableOnSelectionsChanged(ListView_OnSelectionChanged);
            view.Refresh();
            return view;
        }
        
        ConsoleLogVisualElement CreateListViewElement()
        {
            ConsoleLogVisualElement contentVisualElement = new ConsoleLogVisualElement();

            return contentVisualElement;
        }
        
        void BindListViewElement(VisualElement elem, int index)
        {
            ConsoleLogVisualElement consoleLogVisualElement = (ConsoleLogVisualElement)elem;
            consoleLogVisualElement.Refresh();
            consoleLogVisualElement.SetNewModel(testLogList[index]);
            // testLogList[index]; 
            // contentVisualElement.ContentItemDescriptor = Model.FilteredContents[index];//_contentItemDescriptorList[index];
            // contentVisualElement.OnRightMouseButtonClicked -= ContentVisualElement_OnRightMouseButtonClicked;
            // contentVisualElement.OnRightMouseButtonClicked += ContentVisualElement_OnRightMouseButtonClicked;
            
            // if (index % 2 == 0)
            // {
            //     contentVisualElement.Children().First().RemoveFromClassList("oddRow");
            // }
            // else
            // {
            //     contentVisualElement.Children().First().AddToClassList("oddRow");
            // }
            // ApplyColumnSizes(contentVisualElement);
            consoleLogVisualElement.MarkDirtyRepaint();
        }
        
        
        
        private void ListView_OnItemChosen(object obj)
        {
            LogMessageModel model = (LogMessageModel)obj;

            // SelectItemInInspectorWindow(contentItemDescriptor);
            // PingItemInProjectWindow(contentItemDescriptor);
        }
        
        private void ListView_OnSelectionChanged(IEnumerable<object> objs)
        {
            var model = objs.Cast<LogMessageModel>().ToArray()[0];

            // SelectItemInInspectorWindow(model);
            // OnSelectionChanged?.Invoke(contentItemDescriptors.ToList());
        }

        public LogVisualElement() : base(nameof(LogVisualElement))
        {
        }
    }

    public class PopupBtn
    {
    }
}