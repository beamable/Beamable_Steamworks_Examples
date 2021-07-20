using System;
using System.Collections;
using System.Collections.Generic;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;
using UnityEngine;

namespace Beamable.Common.Shop
{
   [ContentType("listings")]
   [System.Serializable]
   public class ListingContent : ContentObject
   {
      public ListingPrice price;
      public ListingOffer offer;
      public OptionalPeriod activePeriod;

      [MustBePositive]
      public OptionalInt purchaseLimit;
      public OptionalStats playerStatRequirements;
      public OptionalOffers offerRequirements;
      public OptionalSerializableDictionaryStringToString clientData;

      [MustBePositive]
      public OptionalInt activeDurationSeconds;
      [MustBePositive]
      public OptionalInt activeDurationCoolDownSeconds;

      [MustBePositive]
      public OptionalInt activeDurationPurchaseLimit;
      public OptionalString buttonText; // TODO: This is a dictionary, not a string!

   }

   [System.Serializable]
   public class ListingOffer
   {
      [CannotBeEmpty]
      public OptionalNonBlankStringList titles;

      [CannotBeEmpty]
      public OptionalNonBlankStringList descriptions;

      public List<OfferObtainCurrency> obtainCurrency;
      public List<OfferObtainItem> obtainItems;
   }

   [System.Serializable]
   public class OptionalNonBlankStringList : Optional<NonBlankStringList> {}

   [System.Serializable]
   public class NonBlankStringList : DisplayableList
   {
      [CannotBeBlank]
      public List<string> listData = new List<string>();

      protected override IList InternalList => listData;
      public override string GetListPropertyPath() => nameof(listData);
   }

   [System.Serializable]
   [Agnostic]
   public class OfferObtainCurrency
   {
      [MustBeCurrency]
      public string symbol;
      public int amount;
   }

   [System.Serializable]
   public class OfferObtainItem
   {
      [MustBeItem]
      public string contentId;
      public List<OfferObtainItemProperty> properties;
   }

   [System.Serializable]
   public class OfferObtainItemProperty
   {
      [CannotBeBlank]
      public string name;
      public string value;
   }

   [System.Serializable]
   public class ListingPrice
   {
      [MustBeOneOf("sku", "currency")]
      public string type;
      [MustReferenceContent(false, typeof(CurrencyContent), typeof(SKUContent))]
      public string symbol;
      [MustBeNonNegative]
      public int amount;
   }

   [System.Serializable]
   public class ActivePeriod
   {
      [MustBeDateString]
      public string start;

      [MustBeDateString]
      public OptionalString end;
   }

   [System.Serializable]
   public class OfferRequirement
   {
      [MustReferenceContent(AllowedTypes = new []{typeof(ListingContent)})]
      public string offerSymbol;
      public OfferConstraint purchases;
   }

   [System.Serializable]
   public class StatRequirement
   {
      // TODO: StatRequirement, by way of OptionalStats, is used by AnnouncementContent too. Should this be in a shared location? ~ACM 2021-04-22

      [CannotBeBlank]
      public string stat;

      [MustBeComparatorString]
      public string constraint;
      public int value;
   }

   [System.Serializable]
   public class ContentDictionary
   {
      public List<KVPair> keyValues;
   }

   [System.Serializable]
   public class OfferConstraint
   {
      [MustBeComparatorString]
      public string constraint;
      public int value;
   }
   [System.Serializable]
   public class OptionalColor : Optional<Color>
   {
      public static OptionalColor From(Color color)
      {
         return new OptionalColor {HasValue = true, Value = color};
      }
   }

   [System.Serializable]
   public class OptionalPeriod : Optional<ActivePeriod> { }

   [System.Serializable]
   public class OptionalStats : Optional<List<StatRequirement>> { }

   [System.Serializable]
   public class OptionalOffers : Optional<List<OfferRequirement>> { }

   [System.Serializable]
   public class OptionalDict : Optional<ContentDictionary> { }
}
