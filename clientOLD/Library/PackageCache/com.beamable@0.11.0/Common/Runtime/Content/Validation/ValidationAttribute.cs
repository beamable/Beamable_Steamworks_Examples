using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Common.Content.Validation
{
   [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
   public abstract class ValidationAttribute : PropertyAttribute
   {
      private static readonly List<Type> numericTypes = new List<Type>
      {
         typeof(byte),
         typeof(sbyte),
         typeof(short),
         typeof(ushort),
         typeof(int),
         typeof(uint),
         typeof(long),
         typeof(ulong),
         typeof(float),
         typeof(double),
         typeof(decimal)
      };

      protected static bool IsNumericType(Type type)
      {
         return numericTypes.Contains(type);
      }

      public abstract void Validate(ContentValidationArgs args);


   }

   public class ContentValidationArgs
   {
      public ValidationFieldWrapper ValidationField;
      public IContentObject Content;
      public IValidationContext Context;
      public int ArrayIndex;
      public bool IsArray;

      public static ContentValidationArgs Create(ValidationFieldWrapper field, IContentObject obj,
         IValidationContext ctx)
      {
         return new ContentValidationArgs
         {
            ValidationField = field,
            Content = obj,
            Context = ctx
         };
      }
      public static ContentValidationArgs Create(ValidationFieldWrapper field, IContentObject obj,
         IValidationContext ctx, int arrayIndex, bool isArray)
      {
         return new ContentValidationArgs
         {
            ValidationField = field,
            Content = obj,
            Context = ctx,
            ArrayIndex = arrayIndex,
            IsArray = isArray
         };
      }
   }
}