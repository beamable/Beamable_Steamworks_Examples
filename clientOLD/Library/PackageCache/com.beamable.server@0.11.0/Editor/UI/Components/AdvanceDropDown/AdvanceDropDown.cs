using System.Collections.Generic;
using Beamable.Common.Content;
using Beamable.Editor.Content.Models;
using Beamable.Editor.UI.Buss;
using UnityEngine;
using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Microservice.UI.Components
{
    public class AdvanceDropDown : MicroserviceComponent
    {

        public new class UxmlFactory : UxmlFactory<BuildDropDown, UxmlTraits>
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
                var self = ve as AdvanceDropDown;
            }
        }

        private VisualElement _mainVisualElement;
        private Button withoutDebug, withDebug;

        public AdvanceDropDown() : base(nameof(AdvanceDropDown))
        {

        }

        public override void Refresh()
        {
            base.Refresh();

            _mainVisualElement = Root.Q<VisualElement>("mainVisualElement");

            var advance = Root.Q<Button>("advanceBtn");
            advance.clickable.clicked += () => {  OnClick_advance(); };
        }

        private void OnClick_advance()
        {
            throw new NotImplementedException();
        }
    }
}