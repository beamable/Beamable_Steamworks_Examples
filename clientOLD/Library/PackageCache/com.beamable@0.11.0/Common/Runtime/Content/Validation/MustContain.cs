using System.Linq;

namespace Beamable.Common.Content.Validation
{
   public class MustContain : ValidationAttribute
   {
      public string[] Parts { get; }

      public MustContain(params string[] parts)
      {
         Parts = parts;
      }

      public override void Validate(ContentValidationArgs args)
      {
         var field = args.ValidationField;
         var obj = args.Content;
         var ctx = args.Context;

         if (field.FieldType != typeof(string))
         {
            throw new ContentValidationException(obj, field, "mustContain only works for string fields.");
         }

         var strValue = field.GetValue<string>();
         if (string.IsNullOrEmpty(strValue) && Parts.Length > 0)
         {
            throw new ContentValidationException(obj, field, "string is empty");
         }

         var missingParts = Parts.Where(part => !strValue.Contains(part)).ToList();
         if (missingParts.Count > 0)
         {
            throw new ContentValidationException(obj, field, $"must contain {string.Join(",", missingParts)}");
         }

      }
   }
}