using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common.Inventory;

namespace Beamable.Common.Api.Inventory
{
    /// <summary>
    /// This interface defines the %API for %InventoryService.
    ///
    /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
    ///
    /// #### Related Links
    /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
    /// - See InventoryService script reference
    ///
    /// </summary>
    public interface IInventoryApi : ISupportsGet<InventoryView>
    {
        /// <summary>
        /// Provides the VIP Bonus multipliers that are applicable for this player according to their tier.
        /// </summary>
        /// <returns></returns>
        Promise<GetMultipliersResponse> GetMultipliers();

        /// <summary>
        /// Players may sometimes receive additional currency as a result of qualifying for a VIP Tier
        /// This API previews what that amount of currency would be ahead of an update.
        /// </summary>
        /// <param name="currencyIdsToAmount"></param>
        /// <returns></returns>
        Promise<PreviewCurrencyGainResponse> PreviewCurrencyGain(Dictionary<string, long> currencyIdsToAmount);

        /// <summary>
        /// Sets the currency.
        /// </summary>
        /// <param name="currencyId"></param>
        /// <param name="amount"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Promise<Unit> SetCurrency(string currencyId, long amount, string transaction = null);

        /// <summary>
        /// Sets the currency.
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Promise<Unit> SetCurrency(CurrencyRef currency, long amount, string transaction = null);

        /// <summary>
        /// Adds the currency
        /// </summary>
        /// <param name="currencyId"></param>
        /// <param name="amount"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Promise<Unit> AddCurrency(string currencyId, long amount, string transaction = null);

        /// <summary>
        /// Adds the currency
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Promise<Unit> AddCurrency(CurrencyRef currency, long amount, string transaction = null);

        Promise<Unit> SetCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null);
        Promise<Unit> SetCurrencies(Dictionary<CurrencyRef, long> currencyToAmount, string transaction = null);
        Promise<Unit> AddCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null);
        Promise<Unit> AddCurrencies(Dictionary<CurrencyRef, long> currencyToAmount, string transaction = null);

        Promise<Dictionary<string, long>> GetCurrencies(string[] currencyIds);
        Promise<Dictionary<CurrencyRef, long>> GetCurrencies(CurrencyRef[] currencyRefs);

        /// <summary>
        /// Gets the currency.
        /// </summary>
        /// <param name="currencyId"></param>
        /// <returns></returns>
        Promise<long> GetCurrency(string currencyId);

        /// <summary>
        /// Gets the currency.
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        Promise<long> GetCurrency(CurrencyRef currency);

        Promise<Unit> AddItem(ItemRef itemRef, Dictionary<string, string> properties=null, string transaction = null);
        Promise<Unit> AddItem(string contentId, Dictionary<string, string> properties=null, string transaction = null);
        Promise<Unit> DeleteItem(string contentId, long itemId, string transaction = null);

        Promise<Unit> UpdateItem(ItemRef itemRef, long itemId, Dictionary<string, string> properties,
            string transaction = null);
        Promise<Unit> UpdateItem(string contentId, long itemId, Dictionary<string, string> properties,
            string transaction = null);

        Promise<Unit> Update(Action<InventoryUpdateBuilder> action, string transaction = null);
        Promise<Unit> Update(InventoryUpdateBuilder builder, string transaction = null);

        Promise<List<InventoryObject<TContent>>> GetItems<TContent>()
            where TContent : ItemContent,new();

        Promise<List<InventoryObject<TContent>>> GetItems<TContent>(params ItemRef<TContent>[] itemReferences)
            where TContent : ItemContent, new();

    }

    /// <summary>
    /// This class defines the %content related to the %InventoryService.
    ///
    /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
    ///
    /// #### Related Links
    /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
    /// - See Beamable.Api.Inventory.InventoryService script reference
    ///
    /// ![img beamable-logo]
    ///
    /// </summary>
    /// <typeparam name="TContent"></typeparam>
    [System.Serializable]
    public class InventoryObject<TContent> where TContent : ItemContent
    {
        /// <summary>
        /// The base piece of content that the inventory item derives from.
        /// </summary>
        public TContent ItemContent;

        /// <summary>
        /// The dynamic properties of the inventory item instance.
        /// </summary>
        public Dictionary<string, string> Properties;

        /// <summary>
        /// The id of the item within the inventory group.
        /// </summary>
        public long Id;

        /// <summary>
        /// The timestamp of when the item was added to the player inventory.
        /// </summary>
        public long CreatedAt;

        /// <summary>
        /// The timestamp of when the last modification to item occured.
        /// </summary>
        public long UpdatedAt;
    }

    /// <summary>
    /// This class defines the %Inventory feature's get request.
    ///
    /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
    ///
    /// #### Related Links
    /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
    /// - See Beamable.Api.Inventory.InventoryService script reference
    ///
    /// ![img beamable-logo]
    ///
    /// </summary>
    [System.Serializable]
    public class GetInventoryResponse
    {
        public List<Currency> currencies;
    }

    /// <summary>
    /// This class defines the %Inventory feature's update request.
    ///
    /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
    ///
    /// #### Related Links
    /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
    /// - See Beamable.Api.Inventory.InventoryService script reference
    ///
    /// ![img beamable-logo]
    ///
    /// </summary>
    [System.Serializable]
    public class InventoryUpdateRequest
    {
        public string transaction; // will be set by api
        public Dictionary<string, long> currencies;
    }

   /// <summary>
   /// This class defines the response of fresh data loaded, related to the %InventoryService.
   ///
   /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
   ///
   /// #### Related Links
   /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
   /// - See Beamable.Api.Inventory.InventoryService script reference
   ///
   /// ![img beamable-logo]
   ///
   /// </summary>
    [Serializable]
    public class InventoryResponse
    {
        public string scope;
        public List<Currency> currencies = new List<Currency>();
        public List<ItemGroup> items = new List<ItemGroup>();

        private HashSet<string> _scopes;
        public HashSet<string> Scopes {
            get {
                if (_scopes == null) {
                    if (!string.IsNullOrEmpty(scope))
                        _scopes = new HashSet<string>(scope.Split(','));
                    else
                        _scopes = new HashSet<string>();
                }

                return _scopes;
            }
        }

        public HashSet<string> GetNotifyScopes()
        {
            var notifyScopes = new HashSet<string>();
            notifyScopes.UnionWith(currencies.Select(currency => currency.id));
            notifyScopes.UnionWith(items.Select(item => item.id));
            notifyScopes.UnionWith(Scopes);
            notifyScopes.Add(""); // always notify the root scope
            // TODO: if a scope is in notifySCopes, 'a.b.c', we should also make sure 'a.b', and 'a' are also in the set, so that item parent/child relationships are respected.

            return ResolveAllScopes(notifyScopes);
        }

        private HashSet<string> ResolveAllScopes(IEnumerable<string> notifyScopes)
        {
            var resolved = new HashSet<string>();

            foreach (string notifyScope in notifyScopes)
            {
                var newScopes = ResolveScope(notifyScope);
                resolved.UnionWith(newScopes);
            }

            return resolved;
        }

        private HashSet<string> ResolveScope(string notifyScope)
        {
            var result = new HashSet<string>();
            string[] slicedScopes = notifyScope.Split('.');

            foreach (string slicedScope in slicedScopes)
            {
                if (result.Count == 0)
                {
                    result.Add(slicedScope);
                }
                else
                {
                    string newScope = string.Join(".", result.Last(), slicedScope);
                    result.Add(newScope);
                }
            }

            return result;
        }

        private HashSet<string> ResolveMergeScopes(InventoryView view)
        {
            var resolved = new HashSet<string>();
            var scopes = Scopes;

            var scopesLookup = new HashSet<string>();
            scopesLookup.UnionWith(scopes);

            // add the current scopes
            resolved.UnionWith(scopes);

            // the view may have data that is a child of a scope modified in the response.
            resolved.UnionWith(view.currencies.Keys.Where(currencyType => SetContainsPrefixOf(scopesLookup, currencyType)));
            resolved.UnionWith(view.items.Keys.Where(itemType => SetContainsPrefixOf(scopesLookup, itemType)));
            resolved.UnionWith(currencies.Select(currency => currency.id));
            resolved.UnionWith(items.Select(item => item.id));

            return resolved;
        }

        private bool SetContainsPrefixOf(HashSet<string> set, string element)
        {
            return set.Any(element.StartsWith);
        }

        public void MergeView(InventoryView view)
        {
            var relevantScopes = ResolveMergeScopes(view);
            foreach(var contentId in view.currencies.Keys.ToList().Where(relevantScopes.Contains))
            {
                view.currencies.Remove(contentId);
            }

            foreach(var contentId in view.items.Keys.ToList().Where(relevantScopes.Contains))
            {
                view.items.Remove(contentId);
            }

            foreach (var currency in currencies)
            {
                view.currencies[currency.id] = currency.amount;
            }

            foreach (var itemGroup in items)
            {
                var itemViews = itemGroup.items.Select(item =>
                {
                    ItemView itemView = new ItemView();
                    itemView.id = long.Parse(item.id);
                    var properties = new Dictionary<string, string>();
                    if (item.properties != null)
                    {
                        foreach (var prop in item.properties)
                        {
                            if (properties.ContainsKey(prop.name))
                            {
                                BeamableLogger.LogWarning($"Inventory item has duplicate key. Overwriting existing key. item=[{itemGroup.id}] id=[{item.id}] key=[{prop.name}]");
                            }
                            properties[prop.name] = prop.value;
                        }
                    }
                    itemView.properties = properties;
                    itemView.createdAt = item.createdAt;
                    itemView.updatedAt = item.updatedAt;

                    return itemView;
                });

                List<ItemView> itemList = new List<ItemView>(itemViews);
                view.items[itemGroup.id] = itemList;
            }
        }
    }

   /// <summary>
   /// This class defines the multipliers response for the %InventoryService.
   ///
   /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
   ///
   /// #### Related Links
   /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
   /// - See Beamable.Api.Inventory.InventoryService script reference
   ///
   /// ![img beamable-logo]
   ///
   /// </summary>
    [Serializable]
    public class GetMultipliersResponse
    {
        public List<VipBonus> multipliers;
    }

    /// <summary>
    /// This class defines look-ahead %Currency data related to the %InventoryService.
    ///
    /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
    ///
    /// #### Related Links
    /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
    /// - See Beamable.Api.Inventory.InventoryService script reference
    ///
    /// ![img beamable-logo]
    ///
    /// </summary>
    [Serializable]
    public class PreviewCurrencyGainResponse
    {
        public List<CurrencyPreview> currencies;
    }

    /// <summary>
    /// This class defines look-ahead %Currency data related to the %InventoryService.
    ///
    /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
    ///
    /// #### Related Links
    /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
    /// - See Beamable.Api.Inventory.InventoryService script reference
    ///
    /// ![img beamable-logo]
    ///
    /// </summary>
    [Serializable]
    public class CurrencyPreview
    {
        public string id;
        public long amount;
        public long delta;
        public long originalAmount;
    }

    /// <summary>
    /// This class defines the %Beamable %currency %content related to the %InventoryService.
    ///
    /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
    ///
    /// #### Related Links
    /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
    /// - See Beamable.Api.Inventory.InventoryService script reference
    ///
    /// ![img beamable-logo]
    ///
    /// </summary>
    [Serializable]

    public class Currency
    {
        public string id;
        public long amount;
    }

    /// <summary>
    /// This class defines the %Beamable item %content related to the %InventoryService.
    ///
    /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
    ///
    /// #### Related Links
    /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
    /// - See Beamable.Api.Inventory.InventoryService script reference
    ///
    /// ![img beamable-logo]
    ///
    /// </summary>
    [Serializable]
    public class Item
    {
        public string id;
        public List<ItemProperty> properties;
        public long createdAt;
        public long updatedAt;
    }

    /// <summary>
    /// This class defines a collection of inventory items.
    ///
    /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
    ///
    /// #### Related Links
    /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
    /// - See Beamable.Api.Inventory.InventoryService script reference
    ///
    /// ![img beamable-logo]
    ///
    /// </summary>
    [Serializable]
    public class ItemGroup
    {
        public string id;
        public List<Item> items;
    }

    /// <summary>
    /// This class defines the %Inventory feature's update request.
    ///
    /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
    ///
    /// #### Related Links
    /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
    /// - See Beamable.Api.Inventory.InventoryService script reference
    ///
    /// ![img beamable-logo]
    ///
    /// </summary>
    [Serializable]
    public class ItemProperty
    {
        public string name;
        public string value;
    }

    /// <summary>
    /// This class defines the render-friendly data of the %InventoryService.
    ///
    /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
    ///
    /// #### Related Links
    /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
    /// - See Beamable.Api.Inventory.InventoryService script reference
    ///
    /// ![img beamable-logo]
    ///
    /// </summary>
    public class InventoryView
    {
        public Dictionary<string, long> currencies = new Dictionary<string, long>();
        public Dictionary<string, List<ItemView>> items = new Dictionary<string, List<ItemView>>();

        public void Clear()
        {
            currencies.Clear();
            items.Clear();
        }
    }

    /// <summary>
    /// This class defines the render-friendly data of the %InventoryService.
    ///
    /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
    ///
    /// #### Related Links
    /// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
    /// - See Beamable.Api.Inventory.InventoryService script reference
    ///
    /// ![img beamable-logo]
    ///
    /// </summary>
    public class ItemView
    {
        public long id;
        public Dictionary<string, string> properties;
        public long createdAt;
        public long updatedAt;
    }
}