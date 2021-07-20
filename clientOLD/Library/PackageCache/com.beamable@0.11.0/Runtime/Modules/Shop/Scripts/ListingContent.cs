using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Common.Inventory;
using Beamable.Common.Shop;
using Beamable.UI.Scripts;
using UnityEngine;

namespace Beamable.Shop
{
   public static class OfferObtainCurrencyExtensions
   {
      public static Promise<Dictionary<string, Sprite>> ResolveAllIcons(this List<OfferObtainCurrency> self)
      {
         List<Promise<CurrencyContent>> toContentPromises = self
            .Select(x => x.symbol)
            .Distinct()
            .Select(x => new CurrencyRef {Id = x}.Resolve())
            .ToList();

         return Promise.Sequence(toContentPromises)
            .Map(contentSet => contentSet.ToDictionary(
               content => content.Id,
               content => content.icon.LoadSprite().ToPromise())
            ).FlatMap(dict =>
               Promise
                  .Sequence(dict.Values.ToList())
                  .Map(_ => dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetResult()))
            );
      }
   }


}