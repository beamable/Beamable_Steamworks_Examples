using System;
using System.Collections.Generic;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;

namespace Beamable.Common.Shop
{
   [ContentType("stores")]
   [System.Serializable]
   public class StoreContent : ContentObject
   {
      [CannotBeBlank]
      public string title;

      [MustReferenceContent]
      public List<ListingLink> listings;
      public bool showInactiveListings;
   }
}
