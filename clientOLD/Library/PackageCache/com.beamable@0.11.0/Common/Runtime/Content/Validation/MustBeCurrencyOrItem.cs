using Beamable.Common.Inventory;

namespace Beamable.Common.Content.Validation
{
   public class MustBeCurrencyOrItem : MustReferenceContent
   {
      public MustBeCurrencyOrItem(bool allowNull=false) : base(allowNull, allowedTypes:new[] {typeof(CurrencyContent), typeof(ItemContent)})
      {

      }
   }
}