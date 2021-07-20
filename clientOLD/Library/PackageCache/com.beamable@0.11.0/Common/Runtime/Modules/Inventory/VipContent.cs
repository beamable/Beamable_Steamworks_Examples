using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using System.Collections.Generic;

namespace Beamable.Common.Inventory
{
    [ContentType("vip")]
    [System.Serializable]
    [Agnostic]
    public class VipContent : ContentObject
    {
        [MustReferenceContent]
        public CurrencyRef currency;

        [CannotBeEmpty]
        public List<VipTier> tiers;
    }

    [System.Serializable]
    public class VipTier
    {
        [CannotBeBlank]
        public string name;

        [MustBeNonNegative]
        public long qualifyThreshold;

        [MustBeNonNegative]
        public long disqualifyThreshold;

        [CannotBeBlank]
        public List<VipBonus> multipliers;
    }

    [System.Serializable]
    public class VipBonus
    {
        [MustBeCurrency]
        public string currency;

        [MustBePositive]
        public double multiplier;

        [MustBePositive]
        public int roundToNearest;
    }
}