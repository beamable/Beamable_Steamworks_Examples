using UnityEngine.Serialization;

namespace Beamable.Common.Content
{
   [System.Serializable]
   [Agnostic]
   public class ClientPermissions
   {
      [FormerlySerializedAs("write_self")]
      [ContentField("write_self")]
      public bool writeSelf;
   }
}