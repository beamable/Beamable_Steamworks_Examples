
using UnityEditor;
using UnityEngine;

namespace Beamable.Server.Editor.UI.Components
{
   public static class Constants
   {
      public const string SERVER_UI = "Packages/com.beamable.server/Editor/UI";
      public const string COMP_PATH = "Packages/com.beamable.server/Editor/UI/Components";
      
      // Configuration
      public static string UssExt => EditorGUIUtility.isProSkin ? "uss" : "light.uss";
      public static Vector2 WindowSizeMinimum = new Vector2(500, 400);
      
      public static string Publish = "Publish";
      public static string PopUpBtn = "Console Log";
      
   }
}