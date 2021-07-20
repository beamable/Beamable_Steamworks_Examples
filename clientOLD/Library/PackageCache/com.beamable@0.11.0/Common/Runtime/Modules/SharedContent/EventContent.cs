using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;
using Beamable.Common.Shop;
using Beamable.Content;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beamable.Common.Content
{
    [ContentType("events")]
    [System.Serializable]
    public class EventContent : ContentObject , ISerializationCallbackReceiver
    {
        [CannotBeBlank]
        public new string name;

        [FormerlySerializedAs("start_date")]
        [ContentField("start_date")]
        [MustBeDateString]
        public string startDate;

        [FormerlySerializedAs("partition_size")]
        [ContentField("partition_size")]
        [MustBePositive]
        public int partitionSize;

        [FormerlySerializedAs("phases")]
        [SerializeField]
        [HideInInspector]
        [IgnoreContentField]
        private List<EventPhase> legacyPhases;

        [CannotBeEmpty]
        public PhaseList phases;

        [FormerlySerializedAs("score_rewards")]
        [ContentField("score_rewards")]
        public List<EventPlayerReward> scoreRewards;

        [FormerlySerializedAs("rank_rewards")]
        [ContentField("rank_rewards")]
        public List<EventPlayerReward> rankRewards;
        public List<StoreRef> stores;

        [ContentField("group_rewards")]
        public EventGroupRewards groupRewards;

        public ClientPermissions permissions;


        public void OnBeforeSerialize()
        {
            // never save the legacy phases...
            legacyPhases = null;
        }

        public void OnAfterDeserialize()
        {
            // if anything is in the legacy phases, move them into the new list.
            if (legacyPhases != null && legacyPhases.Count > 0)
            {
                phases = new PhaseList
                {
                    listData = legacyPhases.ToList()
                };
            }

            legacyPhases = null;
        }
    }

    [System.Serializable]
    public class EventRef : ContentRef<EventContent> // TODO: Factor
    {

    }

    [System.Serializable]
    public class PhaseList : DisplayableList<EventPhase>
    {
        public List<EventPhase> listData = new List<EventPhase>();

        protected override IList InternalList => listData;
        public override string GetListPropertyPath() => nameof(listData);
    }

    [System.Serializable]
    public class EventPhase
    {
        [CannotBeBlank]
        public string name;

        [FormerlySerializedAs("duration_minutes")]
        [MustBePositive]
        [ContentField("duration_minutes")]
        public int durationMinutes;

        public List<EventRule> rules;
    }

    [System.Serializable]
    public class EventRule
    {
        [CannotBeBlank]
        public string rule;

        [CannotBeBlank]
        public string value;
    }

    [System.Serializable]
    public class EventPlayerReward
    {
        [MustBeNonNegative]
        public double min;

        [MustBeNonNegative]
        public OptionalDouble max;

        public OptionalEventCurrencyList currencies;

        public OptionalEventItemList items;

    }

    [Serializable]
    public class EventGroupRewards
    {
        public List<EventPlayerReward> scoreRewards;
    }


    [System.Serializable]
    public class EventObtain
    {
        public string symbol; // TODO: Is this inventory? Is this entitlement?
        public int count;
    }

    [Serializable]
    public class OptionalEventCurrencyList : Optional<List<EventCurrencyObtain>>
    {

    }

    [Serializable]
    public class OptionalEventItemList : Optional<List<EventItemObtain>>{}

    [Serializable]
    public class EventCurrencyObtain
    {
        [MustBeCurrency]
        public string id;

        [MustBePositive]
        public long amount;
    }

    [Serializable]
    public class EventItemObtain
    {
        [MustBeItem]
        public string id;

        public OptionalSerializableDictionaryStringToString properties;
    }
}
