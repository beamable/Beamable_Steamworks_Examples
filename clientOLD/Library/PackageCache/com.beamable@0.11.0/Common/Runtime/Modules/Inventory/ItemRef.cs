using Beamable.Common.Content;

namespace Beamable.Common.Inventory
{
   [System.Serializable]
   [Agnostic]
   public class ItemRef : ItemRef<ItemContent>
   {
      public ItemRef()
      {

      }

      public ItemRef(string id)
      {
         Id = id;
      }
   }

   [System.Serializable]
   [Agnostic]
   public class ItemRef<TContent> : ContentRef<TContent> where TContent : ContentObject, new()
   {

   }
}