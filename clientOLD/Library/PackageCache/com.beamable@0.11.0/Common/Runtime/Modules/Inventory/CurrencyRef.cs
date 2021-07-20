using Beamable.Common.Content;
using Beamable.Common.Content.Validation;

namespace Beamable.Common.Inventory
{
   /// <summary>
   /// This class defines the %reference to a %content %type.
   /// 
   /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
   /// 
   /// #### Related Links
   /// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-feature">Content</a> feature documentation
   /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
   /// - See Beamable.Api.Inventory.InventoryService script reference
   ///
   /// ![img beamable-logo]
   ///
   /// </summary>
   [System.Serializable]
   [Agnostic]
   public class CurrencyRef : CurrencyRef<CurrencyContent>
   {
      public CurrencyRef(){}

      public CurrencyRef(string id)
      {
         Id = id;
      }
   }

   [System.Serializable]
   public class CurrencyRef<TContent> : ContentRef<TContent> where TContent : CurrencyContent, new()
   {

   }

   [System.Serializable]
   [Agnostic]
   public class CurrencyAmount
   {
      public int amount;
      [MustReferenceContent]
      public CurrencyRef symbol;
   }
}