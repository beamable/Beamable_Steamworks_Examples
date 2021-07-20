using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Beamable.Common.Inventory
{
   [ContentType("currency")]
   [System.Serializable]
   [Agnostic]
   public class CurrencyContent : ContentObject
   {
      [FormerlySerializedAs("Icon")]
      [ContentField("icon", FormerlySerializedAs = new []{"Icon"})]
      public AssetReferenceSprite icon;
      public ClientPermissions clientPermission;

      [MustBeNonNegative]
      public long startingAmount;
   }

   [System.Serializable]
   public class CurrencyChange
   {
      public string symbol;
      public long amount;
   }
}
