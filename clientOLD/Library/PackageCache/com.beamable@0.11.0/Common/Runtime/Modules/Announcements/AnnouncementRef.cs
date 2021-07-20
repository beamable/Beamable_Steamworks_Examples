using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;
using UnityEngine;

namespace Beamable.Common.Announcements
{
   [Agnostic]
   [System.Serializable]
   public class AnnouncementRef : AnnouncementRef<AnnouncementContent>
   {

   }

   [System.Serializable]
   public class AnnouncementRef<TContent> : ContentRef<TContent> where TContent : AnnouncementContent, new()
   {

   }

   [System.Serializable]
   [Agnostic]
   public class AnnouncementAttachment
   {
      [Tooltip("This should be the contentId of the attachment. Either an item id, or a currency id.")]
      [MustBeCurrencyOrItem]
      public string symbol;

      [Tooltip("If the attachment is a currency, how much currency? If the attachment is an item, this should be 1.")]
      [MustBePositive]
      public int count = 1;

      [Tooltip("Must specify the type of the attachment symbol. If you referenced an item in the symbol, this should be \"items\", otherwise it should be \"currency\"")]
      [MustBeOneOf("currency", "items")]
      // TODO: [MustMatchReference(nameof(symbol))]
      public string type;
   }
}