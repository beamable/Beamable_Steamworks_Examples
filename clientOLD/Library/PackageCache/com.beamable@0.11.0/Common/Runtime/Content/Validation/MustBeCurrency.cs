using Beamable.Common.Inventory;

namespace Beamable.Common.Content.Validation
{
   public class MustBeCurrency : MustReferenceContent
   {
      public MustBeCurrency(bool allowNull=false) : base(allowNull, allowedTypes:new[] {typeof(CurrencyContent)})
      {

      }
   }
}