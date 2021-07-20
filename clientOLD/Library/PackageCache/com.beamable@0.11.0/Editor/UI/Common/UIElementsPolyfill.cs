using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Beamable.Editor.UI.Components;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;

#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

#if UNITY_2018
namespace UnityEngine.Experimental.UIElements
{
    using StyleSheets;

    public static class UIElementsPolyfill2018
    {

      public static void AddSplitPane(this VisualElement self, VisualElement left, VisualElement right)
      {
        var splitterElem = new SplitterVisualElement(){name="splitter"};

        var leftWrapper = new VisualElement();
        leftWrapper.AddToClassList("splitWrapper");
        leftWrapper.AddToClassList("leftSplit");
        var rightWrapper = new VisualElement();
        rightWrapper.AddToClassList("splitWrapper");
        rightWrapper.AddToClassList("rightSplit");
        leftWrapper.Add(left);
        rightWrapper.Add(right);

        splitterElem.Add(leftWrapper);
        splitterElem.Add(rightWrapper);

        self.Add(splitterElem);
      }

        public static VisualElement AddTextWrapStyle(this VisualElement self)
        {
          self.style.wordWrap = true;
          return self;
        }

        public static VisualElement CloneTree(this VisualTreeAsset self)
        {
            return self.CloneTree(null);
        }

        public static void AddStyleSheet(this VisualElement self, string path)
        {
          var paths = UssLoader.GetAvailableSheetPaths(path);
          foreach (var ussPath in paths)
          {
            self.AddStyleSheetPath(ussPath);
          }
        }

        public static void RemoveStyleSheet(this VisualElement self, string path)
        {
          self.RemoveStyleSheetPath(path);
        }

        public static void SetRight(this IStyle self, float value)
        {
            self.positionRight = value;
        }
        public static void SetLeft(this IStyle self, float value)
        {
          self.positionLeft = value;
        }
       public static void SetMarginLeft(this IStyle self, float value)
        {
          self.marginLeft = value;
        }
        public static float GetLeft(this VisualElement self)
        {
          return self.style.paddingLeft;
        }
        public static void SetTop(this IStyle self, float value)
        {
          self.positionTop = value;
        }
        public static void SetBottom(this IStyle self, float value)
        {
          self.positionBottom = value;
        }

        public static float GetMaxHeight(this IStyle self)
        {
            return self.maxHeight;
        }

        public static void SetImage(this Image self, Texture texture)
        {
            self.image = StyleValue<Texture>.Create(texture);
        }

        public static void BeamableFocus(this TextField self)
        {
            self.Focus();
        }

        public static void BeamableAppendAction(this DropdownMenu self, string title, Action<Vector2> callback)
        {
          self.AppendAction(title, evt => callback(evt.eventInfo.mousePosition), DropdownMenu.MenuAction.AlwaysEnabled);
        }

        public static bool RegisterValueChangedCallback<T>(
          this INotifyValueChanged<T> control,
          EventCallback<ChangeEvent<T>> callback)
        {
          CallbackEventHandler callbackEventHandler = control as CallbackEventHandler;
          if (callbackEventHandler == null)
            return false;
          callbackEventHandler.RegisterCallback<ChangeEvent<T>>(callback, TrickleDown.NoTrickleDown);
          return true;
        }

        public static bool UnregisterValueChangedCallback<T>(
          this INotifyValueChanged<T> control,
          EventCallback<ChangeEvent<T>> callback)
        {
          CallbackEventHandler callbackEventHandler = control as CallbackEventHandler;
          if (callbackEventHandler == null)
            return false;
          callbackEventHandler.UnregisterCallback<ChangeEvent<T>>(callback, TrickleDown.NoTrickleDown);
          return true;
        }
    }
}
#endif

#if UNITY_2019_1_OR_NEWER
namespace UnityEditor
{
  public static class UnityEditorPolyfill
  {
    public static VisualElement GetRootVisualContainer(this EditorWindow self)
    {
      return self.rootVisualElement;
    }
  }
}

namespace UnityEngine.UIElements
{

  public static class UIElementsPolyfill2019
  {

    public static void AddSplitPane(this VisualElement self, VisualElement left, VisualElement right) {

      var splitterElem = new SplitterVisualElement(){name="splitter"};

      var leftWrapper = new VisualElement();
      leftWrapper.AddToClassList("splitWrapper");
      leftWrapper.AddToClassList("leftSplit");
      var rightWrapper = new VisualElement();
      rightWrapper.AddToClassList("splitWrapper");
      rightWrapper.AddToClassList("rightSplit");
      leftWrapper.Add(left);
      rightWrapper.Add(right);

      splitterElem.Add(leftWrapper);
      splitterElem.Add(rightWrapper);

      self.Add(splitterElem);

//      self.Add(left);
//      self.Add(right);


    }



    public static VisualElement AddTextWrapStyle(this VisualElement self)
    {
      self.style.whiteSpace = WhiteSpace.Normal;
      return self;
    }
    public static void AddStyleSheet(this VisualElement self, string path)
    {
      var paths = UssLoader.GetAvailableSheetPaths(path);
      foreach (var ussPath in paths)
      {
        self.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath));
        //self.AddStyleSheetPath(ussPath);
      }
    }

    public static void RemoveStyleSheet(this VisualElement self, string path)
    {
      self.styleSheets.Remove(AssetDatabase.LoadAssetAtPath<StyleSheet>(path));
    }
    public static void SetRight(this IStyle self, float value)
    {
      self.right = new StyleLength(value);
    }
    public static void SetTop(this IStyle self, float value)
    {
      self.top = new StyleLength(value);
    }

    public static void SetLeft(this IStyle self, float value)
    {
      self.left = new StyleLength(value);
    }

    public static float GetLeft(this VisualElement self)
    {
      return self.resolvedStyle.paddingLeft;
    }
    public static void SetLeft(this IStyle self, UIElements.Length length)
    {
      self.left = new StyleLength(length.value);
    }
    public static void SetMarginLeft(this IStyle self, UIElements.Length length)
    {
      self.marginLeft = new StyleLength(length.value);
    }
    public static void SetBottom(this IStyle self, float value)
    {
      self.bottom = new StyleLength(value);
    }

    public static float GetMaxHeight(this IStyle self)
    {
      return self.maxHeight.value.value;
    }

    public static void SetImage(this Image self, Texture texture)
    {
      self.image = texture;
    }

    public static void BeamableFocus(this TextField self)
    {
      self.Q("unity-text-input").Focus();
    }

    public static void BeamableAppendAction(this DropdownMenu self, string title, Action<Vector2> callback)
    {
      self.AppendAction(title, evt => callback(evt.eventInfo.mousePosition));
    }
  }
}
#endif


#if UNITY_2020_1_OR_NEWER
public static class UIElementsPolyfill2020
{
  public static void BeamableOnSelectionsChanged(this ListView listView, Action<IEnumerable<object>> cb)
  {
    listView.onSelectionChange += cb;
  }
  public static void BeamableOnItemChosen(this ListView listView, Action<object> cb)
  {
    listView.onItemsChosen += set => cb(set.FirstOrDefault());
  }
}
#else
public static class UIElementsPolyfillPre2020
{
  public static void BeamableOnSelectionsChanged(this ListView listView, Action<List<object>> cb)
  {
    listView.onSelectionChanged += cb;
  }

  public static void BeamableOnItemChosen(this ListView listView, Action<object> cb)
  {
    listView.onItemChosen += cb;
  }

}
#endif



public static class UssLoader
{
  public static List<string> GetAvailableSheetPaths(string ussPath)
  {
    var ussPaths = new List<string> {ussPath};

    var darkPath = ussPath.Replace(".uss", ".dark.uss");
    var lightPath = ussPath.Replace(".uss", ".light.uss");
    var u2018Path = ussPath.Replace(".uss", ".2018.uss");
    var u2019Path = ussPath.Replace(".uss", ".2019.uss");
    var u2020Path = ussPath.Replace(".uss", ".2020.uss");
    var darkAvailable = File.Exists(darkPath);
    var lightAvailable = File.Exists(lightPath);
    var u2018Available = File.Exists(u2018Path);
    var u2019Available = File.Exists(u2019Path);
    var u2020Available = File.Exists(u2020Path);

    if (EditorGUIUtility.isProSkin && darkAvailable)
    {
      ussPaths.Add(darkPath);
    } else if (!EditorGUIUtility.isProSkin && lightAvailable)
    {
      ussPaths.Add(lightPath);
    }

    if (u2018Available)
    {
      #if UNITY_2018
      ussPaths.Add(u2018Path);
      #endif
    }

    if (u2019Available)
    {
      #if UNITY_2019
        ussPaths.Add(u2019Path);
      #endif
    }

    if (u2020Available)
    {
      #if UNITY_2020_1_OR_NEWER // 2020 is the max supported version, so we forward lean and assume all uss works in 2021.
        ussPaths.Add(u2020Path);
      #endif
    }

    return ussPaths;
  }
}