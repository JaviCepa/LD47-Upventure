using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace PlatformerPro
{
    /// <summary>
    /// A shop where a player can buy and sell items.
    /// </summary>
    public class Shop : Persistable
    {
        /// <summary>
        /// Gets the header string used to describe the component.
        /// </summary>
        /// <value>The header.</value>
        override public string Header
        {
            get
            {
                return "A shop where a player can buy and sell items.";
            }
        }

        /// <summary>
        /// The player preference identifier.
        /// </summary>
        public const string UniqueDataIdentifier = "Shop";

        /// <summary>
        /// The name of the shop. Must be unique for each shop as its used in persistence.
        /// </summary>
        [Tooltip("Name of the shop. Must be unique for each shop as its used in persistence.")]
        public string shopName = "Shop";

        [Tooltip("Item used as currency in the shop.")]
        [ItemType]
        public string currencyItem;

        [Header("Item Data")]
        [Tooltip("Items the shop sells.")]
        /// <summary>
        /// Items the shop sells.
        /// </summary>
        public List<ShopItem> items;

        [Header("Selling")]
        [Tooltip("How much do we markup prices as a percentage 0 = no mark, 100 = 100% mark up.")]
        /// <summary>
        /// How much do we markup prices as a percentage 0 = no mark, 100 = 100% mark up.
        /// </summary>
        [Range(-100, 200)]
        public int sellMarkUp;

        [Header("Buying")]
        [Tooltip("If true buy all items, if false buy only items that are listed as buyable in the item list.")]
        /// <summary>
        /// If true buy all items, if false buy only items that are listed as buyable in the item list.
        /// </summary>
        public bool buyAllItems;

        /// <summary>
        /// How much do we markdown prices as a percentage 0 = no mark down, 100 = 100% mark down (nothing paid).
        /// </summary>
        [Range(0, 100)]
        public int buyMarkDown;

        /// <summary>
        /// If true bought items which aren't in the item list still appear for sale using default sell price.
        /// </summary>
        [Tooltip("If true bought items which aren't in the item list still appear for sale using default price (plus markup).")]
        [DontShowWhen("buyAllItems", showWhenTrue = true)]
        public bool sellAllBoughtItems;


        [Header("Restocking")]
        public bool neverRestock;

        /// <summary>
        /// Restock interval in minutes.
        /// </summary>
        [DontShowWhen("neverRestock")]
        public int baseRestockInterval = 15;

        /// <summary>
        /// Maximum number of times to trigger restock regardless of time elapsed since last restock.
        /// </summary>
        [DontShowWhen("neverRestock")]
        public int maxRestock = 3;

        [Header("UI")]
        public GameObject visibleContent;

        [Header("Controls")]
        /// <summary>
        /// If true pressing escape always closes shop.
        /// </summary>
        public bool escapeClosesShop = true;


        /// <summary>
        /// Data about the actual items in the shop.
        /// </summary>
        protected List<ShopItemData> itemData;

        /// <summary>
        /// Cached list of things we sell.
        /// </summary>
        protected List<string> sellableItems;

        /// <summary>
        /// Item sold by shop.
        /// </summary>
        public event System.EventHandler<ItemEventArgs> ShopSoldItem;

        /// <summary>
        /// Raises the item sold event.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="character">Character.</param>
        virtual protected void OnShopSoldItem(string type, Character character)
        {
            if (SaveOnChange) Save(this);
            if (ShopSoldItem != null) ShopSoldItem(this, new ItemEventArgs(type, character));
        }

        /// <summary>
        /// Player tried to buy item but didn't have enough space or cash.
        /// </summary>
        public event System.EventHandler<PurchaseFailEventArgs> ShopPurchaseFailed;

        /// <summary>
        /// Raises the item sold event.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="character">Character.</param>
        virtual protected void OnShopPurchaseFailed(string type, Character character, PurchaseFailReason reason)
        {
            if (ShopPurchaseFailed != null) ShopPurchaseFailed(this, new PurchaseFailEventArgs(type, character, reason));
        }


        /// <summary>
        /// Item purchased by shop
        /// </summary>
        public event System.EventHandler<ItemEventArgs> ShopBoughtItem;

        /// <summary>
        /// Raises the item bought event.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="character">Character.</param>
        virtual protected void OnShopBoughtItem(string type, Character character)
        {
            if (SaveOnChange) Save(this);
            if (ShopBoughtItem != null) ShopBoughtItem(this, new ItemEventArgs(type, character));
        }


        /// <summary>
        /// Item purchased.
        /// </summary>
        public event System.EventHandler<EmptyEventArgs> ShopRestocked;

        /// <summary>
        /// Raises the shop restocked event.
        /// </summary>
        virtual protected void OnShopShopRestocked()
        {
            if (SaveOnChange) Save(this);
            if (ShopRestocked != null) ShopRestocked(this, EmptyEventArgs.Instance);
        }

        /// <summary>
        /// Get a list of all items this shop sells, including items that are usually sold but that aren't in stock, and 
        /// including bought items even if they aren't usually sold.
        /// </summary>
        /// <value>The sellable item.</value>
        public List<string> SellableItem
        {
            get
            {
                return sellableItems;
            }
        }

        private void Start()
        {
            Init();
        }

        void Update()
        {
            if (Application.isPlaying && visibleContent.activeInHierarchy) ProcessUserInput();
        }

        /// <summary>
        /// Checks for keys that close the store.
        /// </summary>
        virtual protected void ProcessUserInput()
        {
            if (escapeClosesShop && UnityEngine.Input.GetKeyDown(KeyCode.Escape)) HideShop();
        }

        /// <summary>
        /// Init the shop.
        /// </summary>
        virtual public void Init()
        {
            if (EnablePersistence)
            {
                PlatformerProGameManager.Instance.PhaseChanged += PhaseChange;
                LevelManager.Instance.Respawned += HandleRespawn;
            }
            else
            {
                Restock();
            }
            TimeManager.Instance.GameUnPaused += HandleUnpause;
        }

        /// <summary>
        /// Make sure we close the shop if the game becomes unpaused for any reason.
        /// </summary>
        virtual protected void HandleUnpause(object sender, System.EventArgs e)
        {
            if (visibleContent.activeInHierarchy) HideShop();
        }

        /// <summary>
        /// Initialise and show the shop.
        /// </summary>
        public void ShowShop()
        {
            if (!loaded) Load(this);
            Restock();
            ShowUI();
        }

        /// <summary>
        /// Gets the item for given slot.
        /// </summary>
        /// <returns>The item for slot.</returns>
        /// <param name="pos">position.</param>
        virtual public ShopItemData GetItemInPosition(int pos)
        {
            if (itemData != null && pos >= 0 && pos < itemData.Count) return itemData[pos];
            return null;
        }

        virtual public int GetSellPriceForItemAtPosition(int pos)
        {
            if (itemData != null && pos >= 0 && pos < itemData.Count && itemData[pos] != null && itemData[pos].amount > 0)
            {
                return CalculateSellPriceFor(itemData[pos]);
            }
            return 0;
        }

        virtual protected int CalculateSellPriceFor(ShopItemData data)
        {
            ShopItem item = items.Find(i => i.typeId == data.Data.typeId);
            if (item != null && item.priceOverride != -1) return item.priceOverride;
            return data.Data.price + (int)((float)data.Data.price * ((float)sellMarkUp / 100.0f));
        }

        /// <summary>
        /// Calculates the buy price for the given item.
        /// </summary>
        /// <returns>The buy price for item or zero if the item wont be bought.</returns>
        /// <param name="data">Item data.</param>
        virtual protected int CalculateBuyPriceFor(ShopItemData data)
        {
            ShopItem item = items.Find(i => i.typeId == data.Data.typeId);
            if (item == null) return 0;
            if (item.buyPriceOverride != -1) return item.buyPriceOverride;
            return data.Data.price - (int)((float)data.Data.price * ((float)buyMarkDown / 100.0f));
        }

        /// <summary>
        /// Calculates the buy price for the given item.
        /// </summary>
        /// <returns>The buy price for item or zero if the item wont be bought.</returns>
        /// <param name="data">Item data.</param>
        virtual protected int CalculateBuyPriceFor(ItemTypeData data)
        {
            // Is a stocked item
            ShopItemData shopItem = itemData.Find(i => i.Data.typeId == data.typeId);
            if (shopItem != null)
            {
                int price = CalculateBuyPriceFor(shopItem);
                if (price > 0) return price;
            }
            if (buyAllItems)
            {
                return (data.price - (int)((float)data.price * ((float)buyMarkDown / 100.0f)));
            }
            return 0;
        }

        public bool CanSellItem(string itemType)
        {
            return false;
        }

        public bool WillBuyItemOfType(string itemType)
        {
            ShopItem item = items.Find(i => i.typeId == itemType);
            // Explicit allow
            if (item != null && item.shopBuysItem) return true;
            // Buys everything
            if (buyAllItems) return true;
            if (item != null) return true;
            return false;
        }

        /// <summary>
        /// Gets the reason why a purcahse of the given slot would fail.
        /// </summary>
        /// <returns>The purchase fail reason in position.</returns>
        /// <param name="index">Index.</param>
        public PurchaseFailReason GetPurchaseFailReasonInPosition(int index, Character character)
        {
            ShopItemData data = GetItemInPosition(index);
            if (data == null) return PurchaseFailReason.NO_ITEM_SELECTED;
            if (data.amount == 0) return PurchaseFailReason.ITEM_NOT_IN_SHOP;
            int price = GetSellPriceForItemAtPosition(index);
            if (price == 0) Debug.LogWarning("Price calculation issue, sell price should never be zero!");
            int maxCanBuy = price == 0 ? 0 : character.ItemManager.ItemCount(currencyItem) / price;
            if (maxCanBuy == 0) return PurchaseFailReason.CANT_AFFORD;
            // We don't handle no room here
            return PurchaseFailReason.NONE;
        }

        /// <summary>
        /// Supplied character purchases the item at given shop index and character.
        /// </summary>
        /// <returns>Amount purcahsed (0 if no purchase made).</returns>
        /// <param name="index">Index.</param>
        /// <param name="character">Character.</param>
        /// <param name="desiredAmount">Number to purcahse or 0 for default.</param>
        public int PurchaseItemAt(int index, Character character, int desiredAmount)
        {
            ShopItemData data = GetItemInPosition(index);
            if (data == null) return 0;
            int amount = DoPurchaseItemAt(index, character, desiredAmount);
            if (amount == 0) {

                return 0;
            }
            OnShopSoldItem(data.ItemId, character);
            return amount;
        }

        /// <summary>
        /// Purchases 1 or more items returning the amount purchased.
        /// </summary>
        /// <returns>The amount purchased.</returns>
        /// <param name="index">Index of the item in shop.</param>
        /// <param name="character">Character doings the purchasing.</param>
        protected int DoPurchaseItemAt(int index, Character character, int desiredAmount)
        {
            if (character == null || character.ItemManager == null)
            {
                Debug.LogWarning("Tried to buy an item with a null character or a character without an inventory.");
                return 0;
            }
            ShopItemData data = GetItemInPosition(index);
            if (data == null || data.amount == 0)
            {
                OnShopPurchaseFailed(data.ItemId, character, PurchaseFailReason.ITEM_NOT_IN_SHOP);
                return 0;
            }
            ShopItem item = items.Find(i => i.typeId == data.Data.typeId);
            int price = GetSellPriceForItemAtPosition(index);
            int amount = 1;
            if (item != null)
            {
                amount = (item.sellAmount <= data.amount) ? item.sellAmount : data.amount;
            }
            if (desiredAmount > 0) amount = desiredAmount;
            // Check price
            int maxCanBuy = character.ItemManager.ItemCount(currencyItem) / price;
            // Can't afford
            if (maxCanBuy == 0)
            {
                OnShopPurchaseFailed(data.ItemId, character, PurchaseFailReason.CANT_AFFORD);
                return 0;
            }
            // Can't afford to buy the sell amount, so reduce
            if (maxCanBuy < amount) amount = maxCanBuy;
            // Try to add to inventory (which is implicitly a check for space)
            int actualAmount = character.ItemManager.CollectItem(data.Data.typeId, amount);
            // Subtract currency
            if (actualAmount > 0)
            {
                character.ItemManager.ConsumeItem(currencyItem, price * actualAmount);
            }
            else
            {
                OnShopPurchaseFailed(data.ItemId, character, PurchaseFailReason.NO_ROOM);
            }
            // Subtract amount
            data.amount -= actualAmount;
            // Remove from shop data if the shop doesn't sell this item type
            if (data.amount == 0 && item == null) itemData.RemoveAt(index);
            return actualAmount;
        }


        /// <summary>
        /// Supplied character purchases the item at given shop index and character.
        /// </summary>
        public bool SellItemToShop(ItemInstanceData data, Character character, int slot)
        {
            if (data.Data.maxDurability > 0 && data.durability < data.Data.maxDurability)
            {
                Debug.Log("Wont buy damaged item");
                return false;
            }
            if (WillBuyItemOfType(data.Data.typeId))
            {
                int price = CalculateBuyPriceFor(data.Data) * data.amount;
                if (price <= 0)
                {
                    Debug.Log("Doesn't buy items of that type");
                    return false;
                }

                // Update shop stocks
                ShopItemData shopItemData = itemData.Find(i => i.Data.typeId == data.Data.typeId);
                ShopItem shopItem = items.Find(i => i.typeId == data.Data.typeId);
                if (shopItemData != null)
                {
                    shopItemData.amount += data.amount;
                    // Apply max
                    if (shopItem != null && shopItemData.amount > shopItem.maxStock)
                    {
                        shopItemData.amount = shopItem.maxStock;
                    }
                }
                else if (shopItem != null)
                {
                    shopItemData = new ShopItemData(shopItem.typeId, data.amount);
                    // Apply max
                    if (shopItemData.amount > shopItem.maxStock)
                    {
                        shopItemData.amount = shopItem.maxStock;
                    }
                    itemData.Add(shopItemData);
                }
                else if (sellAllBoughtItems)
                {
                    shopItemData = new ShopItemData(data.Data.typeId, data.amount);
                    itemData.Add(shopItemData);
                }
                // Update character inventory
                character.ItemManager.ConsumeItemFromInventorySlot(slot);
                character.ItemManager.CollectItem(currencyItem, price);
                // Send event
                OnShopBoughtItem(data.Data.typeId, character);
            }
            else
            {
                Debug.Log("Doesn't buy items of that type");
            }
            return false;
        }

        virtual protected void UpdateSellableItems()
        {
            sellableItems = new List<string>();
            foreach (ShopItem i in items)
            {
                sellableItems.Add(i.typeId);
            }
            // Add bought items
            foreach (ShopItemData i in itemData)
            {
                if (!sellableItems.Contains(i.Data.typeId)) sellableItems.Add(i.Data.typeId);
            }
        }

        /// <summary>
        /// Checks the time since last restock and restocks once for each restock interval up to max restock times.
        /// </summary>
        virtual protected void Restock()
        {
            if (neverRestock) return;
            foreach (ShopItemData data in itemData)
            {
                int restockAmount = 0;
                ShopItem item = items.Find(i => i.typeId == data.Data.typeId);
                // Only restock items with matching shop item
                if (item != null && item.restockType != RestockType.NEVER_RESTOCK) {
                    // note this uses real time not game time, override if you wnat to use elapsed game time
                    System.TimeSpan d = System.DateTime.Now - data.lastRestockTime;
                    int minutes = (int)d.TotalMinutes;
                    restockAmount = minutes / baseRestockInterval;
                    if (restockAmount > maxRestock) restockAmount = maxRestock;
                    for (int i = 0; i < restockAmount; i++)
                    {
                        DoRestock(item, data);
                    }
                }
            }
            Save(this);
        }

        /// <summary>
        /// Does the actual restocking for the given item with no checks on time, etc (usually called from Restock).
        /// </summary>
        virtual protected void DoRestock(ShopItem item, ShopItemData data)
        {
            if (neverRestock) return;
            if (item == null || item.restockType == RestockType.NEVER_RESTOCK) return;
            switch (item.restockType)
            {
                case RestockType.USE_DEFAULT:
                    // By default add a sell amount each restock
                    data.amount += item.sellAmount;
                    break;
                case RestockType.USE_RANDOM:
                    data.amount += Random.Range(1, item.maxStock);
                    break;
                case RestockType.USE_RARITY:
                    if (Random.Range(0, item.rarity) == 0)
                    {
                        data.amount += item.sellAmount;
                    }
                    break;
            }
            // Don't restock above max
            if (data.amount > item.maxStock) data.amount = item.maxStock;
            data.lastRestockTime = System.DateTime.Now;
        }

        virtual protected void ShowUI()
        {
            TimeManager.Instance.Pause(false, true);
            visibleContent.SetActive(true);
        }

        virtual protected void HideShop()
        {
            if (EnablePersistence) Save(this);
            visibleContent.SetActive(false);
            TimeManager.Instance.UnPause(false);
        }


        #region Persitable methods

        override public string PlayerPrefsIdentifier
        {
            get
            {
                return string.Format("{0}_{1}", BasePlayerPrefId, Identifier);
            }
        }

        /// <summary>
        /// Gets the character reference.
        /// </summary>
        /// <value>The character.</value>
        override public Character Character
        {
            get
            {
                return null;
            }
            set
            {
                Debug.LogWarning("Shop doesn't allow character to be changed");
            }
        }

        /// <summary>
        /// Gets the data to save.
        /// </summary>
        override public object SaveData
        {
            get
            {
                return itemData;
            }
        }

        /// <summary>
        /// Get a unique identifier to use when saving the data (for example this could be used for part of the file name or player prefs name).
        /// </summary>
        /// <value>The identifier.</value>
        override public string Identifier
        {
            get
            {
                return UniqueDataIdentifier + shopName;
            }
        }

        /// <summary>
        /// Applies the save data to the object.
        /// </summary>
        override public void ApplySaveData(object t)
        {
            if (t is List<ShopItemData> )
            {
                itemData = (List<ShopItemData>)t;
                UpdateSellableItems();
                loaded = true;
                OnLoaded();
            }
            else
            {
                Debug.LogError("Tried to apply unepxected data: " + t.GetType());
            }
        }

        /// <summary>
        /// Get the type of object this Persistable saves.
        /// </summary>
        override public System.Type SavedObjectType()
        {
            return typeof(List<ShopItemData>);
        }

        /// <summary>
        /// Resets the save data back to default.
        /// </summary>
        override public void ResetSaveData()
        {
            itemData = new List<ShopItemData>();
            foreach (ShopItem i in items)
            {
                itemData.Add(new ShopItemData(i.typeId, i.defaultStock));
            }
#if UNITY_EDITOR
            Save(this);
#endif
            OnShopShopRestocked();
        }

        /// <summary>
        /// Support complex object serialisation by passing additional types to seralizer.
        /// </summary>
        override public System.Type[] GetExtraTypes()
        {
            return new System.Type[] { typeof(ShopItem), typeof(List<ShopItemData>), typeof(ShopItemData) };
        }

        #endregion
    }

    /// <summary>
    /// An item in a shop with details about pricing, etc.
    /// </summary>
    [System.Serializable]
    public class ShopItem
    {
        /// <summary>
        /// The type of the item.
        /// </summary>
        [ItemType]
        public string typeId;

        /// <summary>
        /// If not -1 this indicates the price the shop sells the item for. If its -1 the default will be used.
        /// </summary>
        public int priceOverride = -1;

        /// <summary>
        /// The number of items sold at one time.
        /// </summary>
        public int sellAmount = 1;

        /// <summary>
        /// Does the shop buy the item.
        /// </summary>
        public bool shopBuysItem;

        /// <summary>
        /// What price does the shop pay for the item. If -1 then the buy price is the price multiplied by the shops buy price.
        /// </summary>
        [DontShowWhen ("shopBuysItem", showWhenTrue = true)]
        public int buyPriceOverride = -1;

        /// <summary>
        /// The default stock for the given item used when the player first enters store.
        /// </summary>
        public int defaultStock;

        /// <summary>
        /// Controls how many items can be generated and also maximum buy limits. Use -1 for no limits.
        /// </summary>
        public int maxStock = -1;

        /// <summary>
        /// Additional information on top of the shops restock policy to cater for special cases like items that never restock.
        /// </summary>
        public RestockType restockType = RestockType.USE_DEFAULT;

        /// <summary>
        /// The larger the number the rarer it is to sotck the item. 1 = 1 in 1, 10 = 1 in 10, and so on.
        /// Only used if RestockType = USE_RARITY.
        /// </summary>
        public int rarity = 1;
    }

    [System.Serializable]
    public class ShopItemData : ItemInstanceData
    {
     
        /// <summary>
        /// When was the item last restocked.
        /// </summary>
        public System.DateTime lastRestockTime;

        public ShopItemData() : base()
        {

        }

        public ShopItemData(string itemType, int amount) : base(itemType, amount)
        {
            this.lastRestockTime = System.DateTime.Now;
        }

        public ShopItemData(string itemType, int amount, System.DateTime lastRestockTime) : base(itemType, amount)
        {
            this.lastRestockTime = lastRestockTime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformerPro.ShopItemData"/> class by cloning another one.
        /// </summary>
        public ShopItemData(ShopItemData source) : base(source)
        {
            lastRestockTime = source.lastRestockTime;
        }
    }

    /// <summary>
    /// Controls how an item can be restocked.
    /// </summary>
    public enum RestockType
    {
        NEVER_RESTOCK, 
        USE_DEFAULT,
        USE_RANDOM,
        USE_RARITY
    }

    /// <summary>
    /// Reasons we can fail a purchase
    /// </summary>
    public enum PurchaseFailReason
    {
        NONE,
        NO_ITEM_SELECTED,
        ITEM_NOT_IN_SHOP,
        CANT_AFFORD,
        NO_ROOM
    }
}