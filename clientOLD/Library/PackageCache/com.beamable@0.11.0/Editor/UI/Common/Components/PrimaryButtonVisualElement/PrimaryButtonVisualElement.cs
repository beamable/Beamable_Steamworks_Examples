
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Beamable.Common;
using Beamable.Editor.UI.Buss;
using Beamable.Editor.UI.Common;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
namespace Beamable.Editor.UI.Components
{
   public class PrimaryButtonVisualElement : BeamableVisualElement
   {
      private static char[] _digitChars = new[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
      private LoadingSpinnerVisualElement _spinner;

      private Dictionary<string, bool> _fieldValid = new Dictionary<string, bool>();
      private List<FormConstraint> _constraints = new List<FormConstraint>();

      public string Text { get; private set; }

      public Button Button { get; private set; }


      public PrimaryButtonVisualElement() : base($"{BeamableComponentsConstants.UI_PACKAGE_PATH}/Common/Components/{nameof(PrimaryButtonVisualElement)}/{nameof(PrimaryButtonVisualElement)}")
      {
      }

      public new class UxmlFactory : UxmlFactory<PrimaryButtonVisualElement, UxmlTraits> { }
      public new class UxmlTraits : VisualElement.UxmlTraits
      {
         UxmlStringAttributeDescription text = new UxmlStringAttributeDescription { name = "text", defaultValue = "Continue" };

         public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
         {
            get { yield break; }
         }
         public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
         {
            base.Init(ve, bag, cc);
            var self = ve as PrimaryButtonVisualElement;

            self.Text = text.GetValueFromBag(bag, cc);
            self.Text = string.IsNullOrEmpty(self.Text)
               ? text.defaultValue
               : self.Text;

            self.Refresh();
         }
      }

      public void SetText(string text)
      {
         Text = text;
         Button.text = text;
      }

      public void AddGateKeeper(params FormConstraint[] constraints)
      {
         foreach (var constraint in constraints){
            _constraints.Add(constraint);
            constraint.OnValidate += () => CheckConstraints(constraint);

            CheckConstraints(constraint);
         }
      }

      void CheckConstraints(FormConstraint src)
      {
         var valid = _constraints.All( v => v.IsValid );
         _fieldValid[name] = valid;

         for (var i = 0; i < _constraints.Count; i++)
         {
            if (_constraints[i] == src) continue;
            _constraints[i].Notify();
         }

         var missingFields = _constraints.Where(kvp => !kvp.IsValid).Select(kvp => kvp.Name).ToList();
         if (missingFields.Count == 0)
         {
            tooltip = "";
            Enable();
         }
         else
         {
            Disable();
            tooltip = $"Required: {string.Join(",", missingFields)}";
         }
      }

      public void Disable()
      {
         Button.SetEnabled(false);
         AddToClassList("disabled");
      }

      public void Enable()
      {
         RemoveFromClassList("disabled");
         Button.SetEnabled(true);
      }

      public void Load<T>(Promise<T> promise)
      {
         AddToClassList("loading");
         var startText = Button.text;
         Button.text = "";
         void Finish()
         {
            Button.text = startText;
            RemoveFromClassList("loading");
         }

         promise
            .Then(_ => Finish())
            .Error(_ => Finish());
      }

      public override void Refresh()
      {
         base.Refresh();
         Button = Root.Q<Button>();
         Button.text = Text;

         _spinner = Root.Q<LoadingSpinnerVisualElement>();
         EditorApplication.delayCall += () =>
         {
            var coef = .5f;
            #if UNITY_2018
            coef = 1;
            #endif

            _spinner.style.SetLeft(Button.worldBound.width * .5f - _spinner.Size * coef);
            _spinner.style.SetTop(Button.worldBound.height * .5f - _spinner.Size * coef);
         };
      }

      public static string AliasErrorHandler(string alias)
      {
         if (string.IsNullOrEmpty(alias)) return "Alias is required";
         if (!IsSlug(alias)) return "Alias must start with a lowercase letter, and must contain all lower case letters, numbers, or dashes";
         return null;
      }

      public static string AliasOrCidErrorHandler(string aliasOrCid)
      {
         if (string.IsNullOrEmpty(aliasOrCid)) return "Alias or CID required";

         // if there are leading numbers, this is a CID. Otherwise, it is an alias.
         var isCid = _digitChars.Contains(aliasOrCid[0]);

         if (!isCid) return AliasErrorHandler(aliasOrCid);

         // there can only be digits...
         var replaced = Regex.Replace(aliasOrCid, @"^\d+", "");
         if (replaced.Length > 0)
         {
            return "CID can only contain numbers";
         }

         return null;
      }

      public static string EmailErrorHandler(string email)
      {
         return PrimaryButtonVisualElement.IsValidEmail(email)
            ? null
            : "Email is not valid";
      }

      public static string PasswordErrorHandler(string password)
      {
         return PrimaryButtonVisualElement.IsPassword(password)
            ? null
            : "A valid password must be at least 4 characters long";
      }

      public static string LegalErrorHandler(bool read)
      {
         return read ? null : "Must agree to legal terms";
      }

      public static bool IsPassword(string password)
      {
         return password.Length > 1; // TODO: Implement actual password check
      }
      public static Func<string, bool> MatchesTextField(TextField tf)
      {
         return (str => string.Equals(tf.value, str)) ;
      }
      public static bool IsSlug(string slug)
      {
         if (slug == null) return false;
         return slug.Length > 1 && GenerateSlug(slug).Equals(slug.Trim());
      }

      public static string GenerateSlug(string phrase)
      {
         string str = phrase.ToLower().Trim();
         // invalid chars
         str = Regex.Replace(str,  @"^\d+", ""); // remove leading numbers..
         str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
         // convert multiple spaces into one space
         str = Regex.Replace(str, @"\s+", " ").Trim();
         // cut and trim
         str = Regex.Replace(str, @"\s", "-").Trim(); // hyphens
         return str;
      }


      public static bool IsValidEmail(string email)
      {
         try
         {
            email = email.Trim();
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
         }
         catch {
            return false;
         }
      }
   }
}