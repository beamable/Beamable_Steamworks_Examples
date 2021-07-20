using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.Toolbox.UI.Components;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
namespace Beamable.Editor.Toolbox.Components
{
   public class ToolboxAnnouncementVisualElement : ToolboxComponent
   {

      public ToolboxAnnouncement Announcement { get; set; }
      public ToolboxModel Model { get; set; }
      public ToolboxAnnouncementVisualElement() : base(nameof(ToolboxAnnouncementVisualElement))
      {
      }

      public override void Refresh()
      {
         base.Refresh();

         var titleLabel = Root.Q<VisualElement>("title");
         var descLabel = Root.Q<VisualElement>("desc");
         var actionButton = Root.Q<Button>();

         titleLabel.Add(Announcement.TitleElement);
         descLabel.Add(Announcement.DescriptionElement);

         actionButton.text = Announcement.ActionText;
         actionButton.clickable.clicked += OnActionClicked;

         if (Announcement.CustomIcon != null)
         {
            var icon = Root.Q<VisualElement>("icon");
            icon.style.backgroundImage = Announcement.CustomIcon;
         }

         switch (Announcement.Status)
         {
            case ToolboxAnnouncementStatus.INFO:
               AddToClassList("info");
               break;
            case ToolboxAnnouncementStatus.WARNING:
               AddToClassList("warning");
               break;
            case ToolboxAnnouncementStatus.DANGER:
               AddToClassList("danger");
               break;
         }
      }

      private void OnActionClicked()
      {
         // TODO: maybe do something flashy?
         Announcement?.Action?.Invoke();
      }
   }
}