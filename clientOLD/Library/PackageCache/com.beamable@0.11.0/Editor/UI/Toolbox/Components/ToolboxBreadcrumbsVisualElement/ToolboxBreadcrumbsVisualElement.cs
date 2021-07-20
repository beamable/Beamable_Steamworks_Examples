using Beamable.Editor.Content.Models;
using Beamable.Editor.Content;
using UnityEngine;
using Beamable.Editor.UI.Buss.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Editor.Content.Components;
using Beamable.Editor.Environment;
using Beamable.Editor.Realms;
using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.Toolbox.UI.Components;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Toolbox.Components
{
    public class ToolboxBreadcrumbsVisualElement : ToolboxComponent
    {
        public new class UxmlFactory : UxmlFactory<ToolboxBreadcrumbsVisualElement, UxmlTraits>
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
                var self = ve as ToolboxBreadcrumbsVisualElement;
            }
        }
        public event Action<Rect> OnAccountButtonClicked;
        public ToolboxModel Model { get; set; }
        private Button _accountButton;
        private Button _realmButton;
        private Label _realmLabel;

        public ToolboxBreadcrumbsVisualElement() : base(nameof(ToolboxBreadcrumbsVisualElement))
        {
        }

        public override void Refresh()
        {
            base.Refresh();
            _accountButton = Root.Q<Button>("accountButton");
            _accountButton.clickable.clicked += () => { OnAccountButtonClicked?.Invoke(_accountButton.worldBound); };

            _realmButton = Root.Q<Button>("realmButton");
            _realmButton.clickable.clicked += () => { RealmButton_OnClicked(_realmButton.worldBound); };

            _realmLabel = _realmButton.Q<Label>();
            if (Model.CurrentRealm == null)
            {
                _realmLabel.text = "Select realm";
            }
            else
            {
                _realmLabel.text = Model.CurrentRealm.DisplayName;
                if (Model.CurrentRealm.IsProduction)
                {
                    _realmButton.AddToClassList("production");
                }
                if (Model.CurrentRealm.IsStaging)
                {
                    _realmButton.AddToClassList("staging");
                }
            }
            Model.OnRealmChanged -= Model_OnRealmChanged;
            Model.OnRealmChanged += Model_OnRealmChanged;


            var portalButton = Root.Q<Button>("openPortalButton");
            portalButton.text = (BeamableConstants.OPEN + " " + BeamableConstants.PORTAL).ToUpper();
            portalButton.clickable.clicked += () => GetPortalUrl.Then(Application.OpenURL);
            var m = new ContextualMenuManipulator(rightClickEvt =>
            {
                rightClickEvt.menu.BeamableAppendAction("Copy Url",
                    mp => { GetPortalUrl.Then(url => { EditorGUIUtility.systemCopyBuffer = url; }); });
            }) {target = portalButton};

            var accountButton = Root.Q<Button>("accountButton");

        }

        private void Model_OnRealmChanged(RealmView realm)
        {
            _realmLabel.text = realm.DisplayName;
            if (realm.IsProduction)
            {
                _realmButton.AddToClassList("production");
            } else {
                _realmButton.RemoveFromClassList("production");
            }
            if (realm.IsStaging)
            {
                _realmButton.AddToClassList("staging");
            } else {
                _realmButton.RemoveFromClassList("staging");
            }
        }
        private Promise<string> GetPortalUrl => EditorAPI.Instance.Map(de =>
            $"{BeamableEnvironment.PortalUrl}/{de.CidOrAlias}/games/{de.ProductionRealm.Pid}/realms/{de.Pid}?refresh_token={de.Token.RefreshToken}");


        private void RealmButton_OnClicked(Rect visualElementBounds)
        {
            Rect popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);

            var content = new RealmDropdownVisualElement();
            content.Model = Model;
            var wnd = BeamablePopupWindow.ShowDropdown("Select Realm", popupWindowRect, new Vector2(200, 300), content);

            content.OnRealmSelected += (realm) =>
            {
                EditorAPI.Instance.Then(beamable => { beamable.SwitchRealm(realm).Then(_ => { wnd.Close(); }); });
            };
            content.Refresh();
        }



    }
}