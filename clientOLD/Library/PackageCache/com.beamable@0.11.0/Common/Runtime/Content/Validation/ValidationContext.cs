using System;
using System.Collections.Generic;
using System.Reflection;

namespace Beamable.Common.Content.Validation
{
   public class ValidationFieldWrapper
   {
      public FieldInfo Field { get; }
      public object Target { get; }

      public ValidationFieldWrapper(FieldInfo field, object target)
      {
         Field = field;
         Target = target;
      }
      public Type FieldType => Field?.FieldType;
      public object GetValue() => Field?.GetValue(Target);
      public T GetValue<T>() => (T) Field?.GetValue(Target);
   }

   public interface IValidationContext
   {
      bool ContentExists(string id);
      IEnumerable<string> ContentIds { get; }
      string GetTypeName(Type type);
      bool TryGetContent(string id, out IContentObject content);
   }

   public class ValidationContext : IValidationContext
   {
      public Dictionary<string, IContentObject> AllContent = new Dictionary<string, IContentObject>();
      public bool ContentExists(string id) => AllContent?.ContainsKey(id) ?? false;
      public IEnumerable<string> ContentIds => AllContent.Keys;

      public string GetTypeName(Type type)
      {
         return ContentRegistry.GetContentTypeName(type);
      }

      public bool TryGetContent(string id, out IContentObject content)
      {
         return AllContent.TryGetValue(id, out content);
      }
   }
}