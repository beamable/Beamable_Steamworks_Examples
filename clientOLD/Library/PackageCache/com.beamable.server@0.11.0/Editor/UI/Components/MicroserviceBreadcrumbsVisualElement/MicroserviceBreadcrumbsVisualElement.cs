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
    public class MicroserviceBreadcrumbsVisualElement : MicroserviceComponent
    {
        public new class UxmlFactory : UxmlFactory<MicroserviceBreadcrumbsVisualElement, UxmlTraits>
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
                var self = ve as MicroserviceBreadcrumbsVisualElement;

            }
        }
        private Button _realmButton;

        public MicroserviceBreadcrumbsVisualElement() : base(nameof(MicroserviceBreadcrumbsVisualElement))
        {
        }
        
    }
    
}