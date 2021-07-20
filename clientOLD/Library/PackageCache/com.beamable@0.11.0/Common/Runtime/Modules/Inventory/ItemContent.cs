using Beamable.Common.Content;
using Beamable.Content;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Beamable.Common.Inventory
{
   [ContentType("items")]
   [System.Serializable]
   [Agnostic]
   public class ItemContent : ContentObject
   {
      [FormerlySerializedAs("Icon")]
      [ContentField("icon", FormerlySerializedAs = new[] {"Icon"})]
      public AssetReferenceSprite icon;
      public ClientPermissions clientPermission;
   }
}