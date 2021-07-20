using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Beamable.Common.Content.Serialization;
using Beamable.Common.Content.Validation;
using Beamable.Content;
using Beamable.Serialization.SmallerJSON;
using UnityEngine;
using UnityEngine.Scripting;

namespace Beamable.Common.Content
{
   [System.Serializable]
   public class ContentObject : ScriptableObject, IContentObject, IRawJsonProvider
   {
      public event ContentDelegate OnChanged;

      [Obsolete]
      public string ContentVersion => Version;

      public string ContentName { get; private set; }

      private string _contentTypeName;
      public string ContentType => _contentTypeName ?? (_contentTypeName = GetContentTypeName(GetType()));
      public string Id => $"{ContentType}.{ContentName}";
      public string Version { get; private set; }

      [SerializeField]
      [IgnoreContentField]
      [HideInInspector]
      private string[] _tags;

      public string[] Tags
      {
         get => _tags ?? (_tags = new []{"base"});
         set => _tags = value;
      }

      public void SetIdAndVersion(string id, string version)
      {
         // validate id.
         var typeName = ContentType;
         if (typeName == null)
         {
            // somehow, the runtime type isn't available. We should infer the typeName from the id.
            typeName = ContentRegistry.GetTypeNameFromId(id);
         }
         _contentTypeName = typeName;

         if (!id.StartsWith(typeName))
         {
            throw new Exception($"Content type of [{typeName}] cannot use id=[{id}]");
         }

         SetContentName(id.Substring(typeName.Length + 1)); // +1 for the dot.
         Version = version;
      }

      public ContentObject SetContentName(string newContentName)
      {
         ContentName = newContentName;
         if (Application.isPlaying)
         {
            name = newContentName; // only set the SO name if we are in-game. Internally, Beamable does not depend on the SO name, but a gameMaker may want to use it.
         }
         return this;
      }

      public void BroadcastUpdate()
      {
         OnChanged?.Invoke(this);
      }

      public static string GetContentTypeName(Type contentType)
      {
         return ContentRegistry.GetContentTypeName(contentType);
      }

      public static string GetContentType<TContent>()
         where TContent : ContentObject
      {
         return GetContentTypeName(typeof(TContent));
      }

      public static TContent Make<TContent>(string name)
         where TContent : ContentObject, new()
      {
         var instance = CreateInstance<TContent>();
         instance.SetContentName(name);
         return instance;
      }


      /// <summary>
      /// Validate this `ContentObject`.
      /// </summary>
      /// <exception cref="AggregateContentValidationException">Should throw if the content is semantically invalid.</exception>
      public virtual void Validate(IValidationContext ctx)
      {
         var errors = GetMemberValidationErrors(ctx);
         if (errors.Count > 0)
         {
            throw new AggregateContentValidationException(errors);
         }
      }

#if UNITY_EDITOR

      public event Action<List<ContentException>> OnValidationChanged;
      public event Action OnEditorValidation;
      public static IValidationContext ValidationContext { get; set; }
      [IgnoreContentField]
      private bool _hadValidationErrors;
      public Guid ValidationGuid { get; set; }
      public static bool ShowChecksum { get; set; }
      public bool SerializeToConsoleRequested { get; set; }
      private void OnValidate()
      {
         ValidationGuid = Guid.NewGuid();
         OnEditorValidation?.Invoke();
         // access the edit time validation context?
         var ctx = ValidationContext ?? new ValidationContext();
         if (HasValidationExceptions(ctx, out var exceptions))
         {
            _hadValidationErrors = true;
            OnValidationChanged?.Invoke(exceptions);

         } else if (_hadValidationErrors)
         {
            _hadValidationErrors = false;
            OnValidationChanged?.Invoke(null);
         }
      }

      public void ForceValidate()
      {
         OnValidate();

      }

      [ContextMenu("Force Validate")]
      public void LogValidationErrors()
      {
         ForceValidate();
         var ctx = ValidationContext ?? new ValidationContext();
         if (HasValidationExceptions(ctx, out var exceptions))
         {
            foreach (var ex in exceptions)
            {
               if (ex is ContentValidationException contentException)
               {
                  Debug.LogError($"Validation Failure. {Id}. {contentException.Message}");
               }
               else
               {
                  Debug.LogError($"Validation Failure. {Id}. {ex.FriendlyMessage}");

               }
            }
         }
      }

      [ContextMenu("Toggle Checksum")]
      public void ToggleShowChecksum()
      {
         ShowChecksum = !ShowChecksum;
      }

      [ContextMenu("Serialize To Console")]
      public void SerializeToConsole()
      {
         SerializeToConsoleRequested = true;
      }


#endif

      public bool HasValidationErrors(IValidationContext ctx, out List<string> errors)
      {
         errors = new List<string>();

         if (ContentName != null && ContentNameValidationException.HasNameValidationErrors(this, ContentName, out var nameErrors))
         {
            errors.AddRange(nameErrors.Select(e => e.Message));
         }

         errors.AddRange(GetMemberValidationErrors(ctx)
            .Select(e => e.Message));

         return errors.Count > 0;
      }

      public bool HasValidationExceptions(IValidationContext ctx, out List<ContentException> exceptions)
      {
         exceptions = new List<ContentException>();
         if (ContentName != null && ContentNameValidationException.HasNameValidationErrors(this, ContentName, out var nameErrors))
         {
            exceptions.AddRange(nameErrors);
         }
         exceptions.AddRange(GetMemberValidationErrors(ctx));
         return exceptions.Count > 0;
      }


      public List<ContentValidationException> GetMemberValidationErrors(IValidationContext ctx)
      {
         var errors = new List<ContentValidationException>();

         var seen = new HashSet<object>();
         var toExpand = new Queue<object>();

         toExpand.Enqueue(this);
         while (toExpand.Count > 0)
         {
            var obj = toExpand.Dequeue();
            if (seen.Contains(obj))
            {
               continue;
            }
            if (obj == null) continue;


            seen.Add(obj);
            var type = obj.GetType();

            if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            {
               var set = (IEnumerable) obj;
               foreach (var subObj in set)
               {
                  toExpand.Enqueue(subObj);
               }
            }

            foreach (var field in type.GetFields())
            {
               var fieldValue = field.GetValue(obj);

               if (typeof(Optional).IsAssignableFrom(field.FieldType))
               {
                  var optional = fieldValue as Optional;
                  if (optional == null || !optional.HasValue)
                  {
                     continue;
                  }
               }

               toExpand.Enqueue(fieldValue);

               foreach (var attribute in field.GetCustomAttributes<ValidationAttribute>())
               {
                  try
                  {
                     var wrapper = new ValidationFieldWrapper(field, obj);

                     if (typeof(IList).IsAssignableFrom(field.FieldType))
                     {
                        var value = field.GetValue(obj) as IList;
                        for (var i = 0; i < value.Count; i++)
                        {
                           attribute.Validate(ContentValidationArgs.Create(wrapper, this, ctx, i, true));
                        }
                     }
                     attribute.Validate(ContentValidationArgs.Create(wrapper, this, ctx));
                  }
                  catch (ContentValidationException e)
                  {
                     errors.Add(e);
                  }
               }

            }
         }

//         void Expand(object target, Type type)
//         {
//
//         }
//
//         foreach (var field in GetType().GetFields())
//         {
//            foreach (var attribute in field.GetCustomAttributes<ValidationAttribute>())
//            {
//               try
//               {
//                  attribute.Validate(field, this, ctx);
//               }
//               catch (ContentValidationException e)
//               {
//                  errors.Add(e);
//               }
//            }
//         }

         return errors;
      }

      public string ToJson()
      {
         return ClientContentSerializer.SerializeContent(this);
      }


   }

   public delegate void ContentDelegate(ContentObject content);

   public delegate void IContentDelegate(IContentObject content);

   public delegate void IContentRenamedDelegate(string oldId, IContentObject content, string nextAssetPath);
}