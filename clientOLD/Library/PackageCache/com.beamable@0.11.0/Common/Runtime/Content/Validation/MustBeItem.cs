using Beamable.Common.Inventory;

namespace Beamable.Common.Content.Validation
{
   public class MustBeItem : MustReferenceContent
   {
      public MustBeItem(bool allowNull=false) : base(allowNull, allowedTypes:new[] {typeof(ItemContent)})
      {

      }
   }
}