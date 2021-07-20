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
    public class ActionBarVisualElement : MicroserviceComponent
    {
        public new class UxmlFactory : UxmlFactory<ActionBarVisualElement, UxmlTraits>
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
                var self = ve as ActionBarVisualElement;

            }
        }
        
        public ActionBarVisualElement() : base(nameof(ActionBarVisualElement))
        {
        }

        public event Action OnStartAllClicked;
        public event Action OnPublishClicked;
        public event Action OnRefreshButtonClicked;
        public event Action OnCreateNewClicked;
        private Button _refreshButton;
        private Button _createNew;
        private Button _startAll;
        private Button _infoButton;
        private Button _publish;
        
        public event Action OnInfoButtonClicked;

        public override void Refresh()
        {
            base.Refresh();
            
            
            _refreshButton = Root.Q<Button>("refreshButton");
            _refreshButton.clickable.clicked += () => { OnRefreshButtonClicked?.Invoke(); };
            
            _createNew = Root.Q<Button>("createNew");
            _createNew.clickable.clicked += () => { OnCreateNewClicked?.Invoke(); };
            
            _startAll = Root.Q<Button>("startAll");
            _startAll.clickable.clicked += () => { OnStartAllClicked?.Invoke(); };
            
            _publish = Root.Q<Button>("publish");
            _publish.clickable.clicked += () => { OnPublishClicked?.Invoke(); };
            
            _infoButton = Root.Q<Button>("infoButton");
            _infoButton.clickable.clicked += () => { OnInfoButtonClicked?.Invoke(); };
            
            
        }




    }

    
}