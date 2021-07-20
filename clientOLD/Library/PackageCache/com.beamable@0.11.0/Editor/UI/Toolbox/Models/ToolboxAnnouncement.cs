using System;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
namespace Beamable.Editor.Toolbox.Models
{
   public class ToolboxAnnouncement
   {
      public void SetTitle(string title)
      {
         TitleElement = new Label(title);
      }
      public void SetDescription(string desc)
      {
         DescriptionElement = new Label(desc);
         DescriptionElement.AddTextWrapStyle();
      }

      public VisualElement TitleElement;
      public string ActionText;
      public Texture2D CustomIcon;
      public VisualElement DescriptionElement;
      public Action Action;
      public ToolboxAnnouncementStatus Status;
   }

   public enum ToolboxAnnouncementStatus
   {
      INFO,
      WARNING,
      DANGER
   }

}