using System.Collections;
using System.Linq;

namespace Beamable.Common.Content.Validation
{
   public class CannotBeEmpty : ValidationAttribute
   {
      public override void Validate(ContentValidationArgs args)
      {

         var type = args.ValidationField.FieldType;
         var value = args.ValidationField.GetValue();
         if (typeof(Optional).IsAssignableFrom(type))
         {
            var optional = value as Optional;
            if (!optional.HasValue) return;

            value = optional.GetValue();
            type = optional.GetOptionalType();
         }

         var isDisplayList = typeof(DisplayableList).IsAssignableFrom(type);
         if (isDisplayList)
         {
            var displayList = value as DisplayableList;
            if (displayList == null || displayList.Count == 0)
            {
               throw new ContentValidationException(args.Content, args.ValidationField, "Cannot be empty");
            }
         }
      }
   }
}