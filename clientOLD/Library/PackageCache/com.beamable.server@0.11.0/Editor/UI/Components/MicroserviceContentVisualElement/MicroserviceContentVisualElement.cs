using Beamable.Editor.Content.Models;
using Beamable.Editor.Content;
using UnityEngine;
using Beamable.Editor.UI.Buss.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Editor.Config;
using Beamable.Editor.Content.Components;
using Beamable.Editor.Environment;
using Beamable.Editor.Modules.Theme;
using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.Toolbox.UI.Components;
using Beamable.Editor.UI.Components;
using UnityEditor;
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
    public class MicroserviceContentVisualElement : MicroserviceComponent
    {
        private VisualElement _mainVisualElement;
        private ListView _listView;
        public event Action OnPopupBtnClicked;

        public new class UxmlFactory : UxmlFactory<MicroserviceContentVisualElement, UxmlTraits>
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
                var self = ve as MicroserviceContentVisualElement;

            }
        }


        public MicroserviceContentVisualElement() : base(nameof(MicroserviceContentVisualElement))
        {
        }
        
        
        public override void Refresh()
        {
            base.Refresh();
            
            _mainVisualElement = Root.Q<VisualElement>("mainVisualElement");
            
            

            var listRoot = Root.Q("listRoot");
            
            
            List<string> listSource = new List<string>();
            listSource.Add("test");
            
            foreach (string item in listSource)
            {
                var logVisualElement = new LogVisualElement();
                logVisualElement.OnPopupBtnClicked += () => {
                    OnPopupBtnClicked?.Invoke();
                };
                
                logVisualElement.Refresh();
                listRoot.Add(logVisualElement);
            }


            // _listView = CreateListView();
            // _mainVisualElement.Add(_listView);
            //
            // _listView.Refresh();
        }

        public void additem(string CreateNew)
        {
            
                var listRoot = Root.Q("listRoot");
                // List<string> listSource = new List<string>();
                // listSource.Add("test");
                // foreach (string item in listSource)
                
                    var logVisualElement = new LogVisualElement();
                    logVisualElement.OnPopupBtnClicked += () => {
                        OnPopupBtnClicked?.Invoke();
                    };
                
                    logVisualElement.Refresh();
                    listRoot.Add(logVisualElement);
                
        }

        // private ListView CreateListView()
        // {
        //     List<string> listSource = new List<string>();
        //     listSource.Add("test");
        //     listSource.Add("test");
        //     var view = new ListView()
        //     {
        //         makeItem = CreateListViewElement,
        //         bindItem = BindListViewElement,
        //         selectionType = ContentManagerConstants.ContentListSelectionType,
        //         itemHeight = 50,
        //         itemsSource = listSource
        //     };
        //     
        //     view.Refresh();
        //     return view;
        // }
        void BindListViewElement(VisualElement elem, int index)
        {
            LogVisualElement logVisualElement = (LogVisualElement)elem;
            
            // contentVisualElement.ContentItemDescriptor = Model.FilteredContents[index];//_contentItemDescriptorList[index];
            // contentVisualElement.OnRightMouseButtonClicked -= ContentVisualElement_OnRightMouseButtonClicked;
            // contentVisualElement.OnRightMouseButtonClicked += ContentVisualElement_OnRightMouseButtonClicked;
            logVisualElement.Refresh();
            // if (index % 2 == 0)
            // {
            //     contentVisualElement.Children().First().RemoveFromClassList("oddRow");
            // }
            // else
            // {
            //     contentVisualElement.Children().First().AddToClassList("oddRow");
            // }
            // ApplyColumnSizes(contentVisualElement);
            // contentVisualElement.MarkDirtyRepaint();
        }

        LogVisualElement CreateListViewElement()
        {
            LogVisualElement logVisualElement = new LogVisualElement();
            logVisualElement.OnPopupBtnClicked += () => {
                OnPopupBtnClicked?.Invoke();
            };
            return logVisualElement;
        }


    }
}