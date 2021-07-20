using System.Collections.Generic;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;

namespace Beamable.Common.Groups
{
   [ContentType("donations")]
   [System.Serializable]
   public class GroupDonationsContent : ContentObject
   {
      [MustBeNonNegative]
      public long requestCooldownSecs;

      // TODO: This really "should" be a list of currency ref but refs don't serialize to just strings currently.
      [MustBeCurrency]
      public List<string> allowedCurrencies;
   }

}