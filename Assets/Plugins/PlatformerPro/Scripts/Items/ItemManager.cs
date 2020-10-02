/**
 * This code is part of Platformer PRO and is copyright John Avery 2014.
 */

using UnityEngine;
using System.Collections.Generic;

namespace PlatformerPro
{
	/// <summary>
	/// Stores details about the items a character has collected. Should be on the same 
	/// Game Object as the character.
	/// </summary>
	public class ItemManager : ItemStatProvider, ICoreComponent
	{
		override public string Header => "Stores details about the items a character has collected. Required for using any kind of items, equipments or power-ups. ";
		
		/// <summary>
		/// Which action button to use for pickups, or -1 for autopickup
		/// </summary>
		[Header("Pick Ups")]
		[Tooltip ("Which action button to use for pickups, or -1 for autopickup")]
		public int pickUpActionButton = -1;

		/// <summary>
		/// If we don't automatically pick up Items then this object will be shown when an object can be picked up.
		/// </summary>
		[Tooltip("If we don't automatically pick up Items then this object will be shown when an object can be picked up")]
		public ItemPickupBox itemPickupBox;
		
		[Header ("Dropping")]
		/// <summary>
		/// Should we drop one item at a time from a stack, or all items in a stack.
		/// </summary>
		[Tooltip("Should we drop one item at a time from a stack, or all items in a stack.")]
		public bool dropAllItemsInStack = true;

		/// <summary>
		/// Where does the drop spawn from relative to character.
		/// </summary>
		[Tooltip ("Where does the drop spawn from relative to character.")]
		public Vector3 dropOffset;

		/// <summary>
		/// Should we impart some velocity to an item when dropped?
		/// </summary>
		[Tooltip ("Should we impart some velocity to an item when dropped?")]
		public Vector2 dropImpulse = new Vector2(0, 1f);

		/// <summary>
		/// If true then we allow zero valued multipliers. Otherwise we ignore them and raise a warning.
		/// </summary>
		[Header("Modifiers")] public bool ignoreZeroValuedMultipliers = true;
		
		/// <summary>
		/// The character this item manager applies to.
		/// </summary>
		protected Character character;

		/// <summary>
		/// Data about available item types.
		/// </summary>
		protected List<ItemTypeData> itemTypeData;

		/// <summary>
		/// The item data.
		/// </summary>
		protected ItemData itemData;

		/// <summary>
		/// Should we recalculate the effects of items this frame?
		/// </summary>
		protected bool recalculateEffectsOfItems;
	
		/// <summary>
		/// The player preference identifier.
		/// </summary>
		public const string UniqueDataIdentifier = "ItemManagerData";

		#region events

		/// <summary>
		/// Item collected.
		/// </summary>
		public event System.EventHandler <ItemEventArgs> ItemCollected;

		/// <summary>
		/// Raises the item collected event.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="amount">Number collected.</param>
		/// <param name="character">Character.</param>
		virtual protected void OnItemCollected(string type, int amount, Character character)
		{
			if (SaveOnChange) Save (this);
			if (ItemCollected != null)
			{
				ItemCollected(this, new ItemEventArgs(type, amount, character));
			}
		}

		/// <summary>
		/// Item consumed.
		/// </summary>
		public event System.EventHandler <ItemEventArgs> ItemConsumed;

        /// <summary>
        /// Raises the item consumed event.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="amount">Number consumed.</param>
        /// <param name="character">Character.</param>
        virtual protected void OnItemConsumed(string type, int amount, Character character)
        {
            if (ItemConsumed != null)
            {
                ItemConsumed(this, new ItemEventArgs(type, amount, character));
            }
        }

        /// <summary>
        /// Occurs when item damaged.
        /// </summary>
        public event System.EventHandler<ItemEventArgs> ItemDamaged;

        /// <summary>
        /// Raises the item damaged event
        /// </summary>
        /// <param name="data">Item Data.</param>
        virtual protected void OnItemDamaged(ItemInstanceData data)
        {
            if (ItemDamaged != null)
            {
                ItemDamaged(this, new ItemEventArgs(data.ItemId, character));
            }
        }

        /// <summary>
        /// Occurs when item damaged.
        /// </summary>
        public event System.EventHandler<ItemEventArgs> ItemDestroyed;

        /// <summary>
        /// Raises the item consumed event.
        /// </summary>
        /// <param name="data">Item data.</param>
        virtual protected void OnItemDestroyed(ItemInstanceData data)
        {
            if (ItemDestroyed != null)
            {
                ItemDestroyed(this, new ItemEventArgs(data.ItemId, character));
            }
        }

        /// <summary>
        /// Sent when item is consumed and none remain.
        /// </summary>
        public event System.EventHandler <ItemEventArgs> ItemDepleted;
		
		/// <summary>
		/// Raises the item depleted event.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="character">Character.</param>
		virtual protected void OnItemDepleted(string type, Character character)
		{
			if (ItemDepleted != null)
			{
				ItemDepleted(this, new ItemEventArgs(type, 0, character));
			}
		}

		/// <summary>
		/// Sent when item is dropped.
		/// </summary>
		public event System.EventHandler <ItemEventArgs> ItemDropped;

		/// <summary>
		/// Raises the item dropped event.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="character">Character.</param>
		virtual protected void OnItemDropped(string type, Character character)
		{
			if (ItemDropped != null)
			{
				ItemDropped(this, new ItemEventArgs(type, 0, character));
			}
		}

		/// <summary>
		/// Item collected.
		/// </summary>
		public event System.EventHandler <ItemEventArgs> ItemMaxUpdated;

		/// <summary>
		/// Raises the item max updated event.
		/// </summary>
		/// <param name="itemClass">Item class.</param>
		/// <param name="type">Type.</param>
		/// <param name="amount">Amount.</param>
		/// <param name="character">Character.</param>
		virtual protected void OnItemMaxUpdated(string type, int amount, Character character)
		{
			if (ItemMaxUpdated != null)
			{
				ItemMaxUpdated(this, new ItemEventArgs(type, amount, character));
			}
		}

		/// <summary>
		/// Called when inventory items are changed without neccessarily updating item counts (for example rearranging).
		/// </summary>
		public event System.EventHandler <CharacterEventArgs> InventoryChanged;

		/// <summary>
		/// Raises the inventory rearranged event.
		/// </summary>
		virtual public void OnInventoryChanged()
		{
            if (SaveOnChange) Save(this);
            if (InventoryChanged != null)
			{
				InventoryChanged(this, new CharacterEventArgs(character));
			}
		}

		#endregion

		/// <summary>
		/// Init with the specified character.
		/// </summary>
		/// <param name="character">Character.</param>
		virtual public void Init(Character character)
		{
			this.character = character;
			ConfigureEventListeners ();
			if (!loaded)
			{
				itemData = new ItemData();
				foreach (ItemTypeData typeData in ItemTypeManager.Instance.ItemData)
				{
					if (typeData.itemClass == ItemClass.NON_INVENTORY)
					{
						itemData.AddItem (typeData.typeId, typeData.startingCount);
					} 
					else if (typeData.itemClass == ItemClass.NORMAL)
					{
						if (typeData.startingCount > 0)
						{
							character.Inventory.AddItem (typeData.typeId, typeData.startingCount);
						}
					}
				}

				if (pickUpActionButton != -1) 
				{
					if (itemPickupBox == null)
					{
						itemPickupBox = FindObjectOfType<ItemPickupBox>();
					}
					itemPickupBox.Init(this);
				}
			}
		}
		
		/// <summary>
		/// Collect the given item.
		/// </summary>
		/// <returns>The actual number collected.</returns>
		/// <param name="item">Item.</param>
		virtual public int CollectItem(Item item)
		{
			return CollectItem (item.instanceData);
		}

		/// <summary>
		/// Drops the item in slot.
		/// </summary>
		/// <returns>True if dropped or false otherwise.</returns>
		/// <param name="index">slot index.</param>
		virtual public bool DropItemFromInventorySlot(int index)
		{
			if (character.Inventory == null)
			{
				Debug.LogWarning ("Tried to drop an item from an inventory but the Character doesn't have one");
				return false;
			}
			ItemInstanceData item = character.Inventory.GetItemAt (index);
			if (item == null || item.amount == 0) return false;
			ItemTypeData itemInstanceData = ItemTypeManager.Instance.GetTypeData (item.ItemId);
			if (itemInstanceData == null || !itemInstanceData.allowDrop) return false;
			int amount = dropAllItemsInStack ? item.amount : 1;
			if (itemInstanceData.DropPrefab != null)
			{
				SpawnDroppedItem (item, amount);
			}
			character.Inventory.RemoveItemAt (index, amount);
			OnItemDropped (item.ItemId, character);
			OnInventoryChanged();
			if (ItemCount (item.ItemId) == 0) OnItemDepleted (item.ItemId, character);
			RecalculateEffectsOfItems();
			return true;
		}

		/// <summary>
		/// Spawns a dropped item in the scene at Character position.
		/// </summary>
		/// <param name="itemTypeData">Item type data.</param>
		/// <param name="amountToDrop">How many items appear in the dropped stack.</param>
		virtual protected void SpawnDroppedItem(ItemInstanceData itemInstanceData, int amountToDrop)
		{
			if (itemInstanceData.Data.DropPrefab == null) return;
			itemInstanceData.amount = amountToDrop;
			GameObject itemGo = GameObject.Instantiate (itemInstanceData.Data.DropPrefab);
			itemGo.transform.position = character.transform.position + new Vector3(dropOffset.x * (float)character.LastFacedDirection, dropOffset.y, dropOffset.z);
			Item item = itemGo.GetComponentInChildren<Item> ();
			if (item != null)
			{
                item.instanceData = new ItemInstanceData(itemInstanceData);
                item.Amount = amountToDrop;
			}
			Rigidbody2D body = itemGo.GetComponent<Rigidbody2D> ();
			if (body != null && dropImpulse != Vector2.zero)
			{
				body.AddForce (new Vector2(dropImpulse.x * (float)character.LastFacedDirection, dropImpulse.y), ForceMode2D.Impulse);
			}
			if (item != null)
			{
				item.DoDrop (itemInstanceData.Data.dropPrefabName);
			}
		}

		/// <summary>
		/// Applies the item effects for the item with given id.
		/// </summary>
		/// <param name="typeId">Type identifier.</param>
		virtual public void ApplyItemEffects(string typeId) 
		{
			ItemTypeData data = ItemTypeManager.Instance.GetTypeData(typeId);
			if (data == null)
			{
				Debug.LogWarning ("Item type " + typeId + " was not found");
			}
			else
			{
				ApplyItemEffects (data);
			}
		}

		/// <summary>
		/// Apply effects from item data.
		/// </summary>
		/// <param name="itemData">Item data.</param>
		virtual public void ApplyItemEffects(ItemTypeData itemTypeData) 
		{
			// Attack

			// Defence
			if (itemTypeData.invulnerability) character.CharacterHealth.SetInvulnerable();

            // Agility

            // Health
            if (itemTypeData.healthAdjustment != 0&& 
                itemTypeData.itemBehaviour != ItemBehaviour.EQUIPPABLE &&
                itemTypeData.itemBehaviour != ItemBehaviour.UPGRADE ) character.CharacterHealth.Heal(itemTypeData.healthAdjustment);
            // Equipment and upgrades can increase max health but they do it through the multiplier calculations
            if (itemTypeData.maxHealthAdjustment != 0 && 
                itemTypeData.itemBehaviour != ItemBehaviour.EQUIPPABLE &&
                itemTypeData.itemBehaviour != ItemBehaviour.UPGRADE ) character.CharacterHealth.MaxHealth += itemTypeData.maxHealthAdjustment;
            if (itemTypeData.breathAdjustment != 0 && 
                itemTypeData.itemBehaviour != ItemBehaviour.EQUIPPABLE &&
                itemTypeData.itemBehaviour != ItemBehaviour.UPGRADE ) character.Breath.GainBreath((float)itemTypeData.breathAdjustment);
        }

		/// <summary>
		/// Removes the item effects for the item with given id.
		/// </summary>
		/// <param name="typeId">Type identifier.</param>
		virtual public void RemoveItemEffects(string typeId) 
		{
			ItemTypeData data = ItemTypeManager.Instance.GetTypeData(typeId);
			if (data == null)
			{
				Debug.LogWarning ("Item type " + typeId + " was not found");
			}
			else
			{
				RemoveItemEffects (data);
			}
		}

		/// <summary>
		/// Shows the item pickup box (for when auto pickup is off).
		/// </summary>
		/// <param name="item"></param>
		virtual public void ShowItemPickUpBox(Item item)
		{
			if (itemPickupBox == null) Debug.LogWarning("Auto pickup is off, but no itemPickUpBox is defined");
			itemPickupBox.Show(item);
		}
		
		/// <summary>
		/// Hides the item pickup box (for when auto pickup is off).
		/// </summary>
		virtual public void HideItemPickUpBox(Item item)
		{
			if (itemPickupBox == null) Debug.LogWarning("Auto pickup is off, but no itemPickUpBox is defined");
			itemPickupBox.Hide(item);
		}
		
		/// <summary>
		/// Removes the item effects.
		/// </summary>
		/// <param name="itemData">Item data.</param>
		virtual public void RemoveItemEffects(ItemTypeData itemData) 
		{ 
			// Defence
			if (itemData.invulnerability)
			{
				// TODO Check if anything else makes them invulnerable before removing invulnerability
				character.CharacterHealth.SetVulnerable ();
			}
		}

		/// <summary>
		/// Sets the item count without raising a collection event.
		/// </summary>
		/// <returns>The item count.</returns>
		/// <param name="itemType">Item type.</param>
		/// <param name="amount">Item count.</param>
		virtual public int SetItemCount(string itemType, int amount)
		{
			ItemTypeData typeData = ItemTypeManager.Instance.GetTypeData (itemType);
			if (typeData.itemClass != ItemClass.NON_INVENTORY)
			{
				Debug.LogError ("Not yet implmeneted for inventory items");
				return 0;
			}
			if (itemData.ContainsKey(itemType))
			{
				if (amount > ItemMax(itemType)) amount = ItemMax (itemType);
				itemData[itemType] = amount;
				return amount;
			}
			else
			{
				if (amount > ItemMax(itemType)) amount = ItemMax (itemType);
				itemData[itemType] = amount;
				itemData.AddItem (itemType, amount);
				return amount;
			}
		}

		/// <summary>
		/// Gets the number of items of the given type.
		/// </summary>
		/// <returns>The count.</returns>
		/// <param name="itemType">Item type.</param>
		virtual public int ItemCount(string itemType)
		{
            int result = 0;
			if (itemData != null && itemData.ContainsKey(itemType)) result += itemData[itemType];
			if (character != null && character.Inventory != null) result += character.Inventory.ItemCount (itemType);
            if (character != null && character.EquipmentManager != null) result += character.EquipmentManager.ItemCount(itemType);
            return result;
		}

		/// <summary>
		/// Gets the maximum number of items for the given type.
		/// </summary>
		/// <returns>The count.</returns>
		/// <param name="itemType">Item type.</param>
		virtual public int ItemMax(string itemType)
		{
			ItemTypeData type = ItemTypeManager.Instance.GetTypeData (itemType);
			if (type != null) return type.itemMax;
			return 0;
		}

        /// <summary>
        /// Consumes the given amount of items of type itemType without applying any effects!
        /// Does not work for equipped items.
        /// </summary>
        /// <param name="itemType">Item type.</param>
        /// <param name="amount">Amount to consume.</param>
        /// <returns>The actual amount consumed.</returns>
        virtual public int ConsumeItem(string itemType, int amount)
        {
            ItemTypeData itemTypeData = ItemTypeManager.Instance.GetTypeData(itemType);
            int actualAmount = amount;
            if (itemTypeData == null)
            {
                Debug.LogWarning("Item type " + itemType + " was not found");
                return 0;
            }
            if (itemTypeData.itemClass == ItemClass.INSTANT)
            {
                Debug.LogWarning("You can't consume an INSTANT item, it will be used when it is collected");
                return 0;
            }
            if (itemTypeData.itemClass == ItemClass.NON_INVENTORY)
            {
                if (itemData.ContainsKey(itemType))
                {
                    actualAmount = itemData.Consume(itemType, amount);
                    OnItemConsumed(itemTypeData.typeId, amount, character);
                    if (ItemCount(itemTypeData.typeId) == 0) OnItemDepleted(itemTypeData.typeId, character);
                    RecalculateEffectsOfItems();
                    return actualAmount;
                }
                return 0;
            }
            if (character.Inventory != null && character.Inventory.ItemCount(itemType) > 0)
            {
                actualAmount = character.Inventory.Consume(itemType, amount);
                OnItemConsumed(itemTypeData.typeId, amount, character);
                if (ItemCount(itemTypeData.typeId) == 0) OnItemDepleted(itemTypeData.typeId, character);
                RecalculateEffectsOfItems();
                return actualAmount;
            }
            return 0;
        }

        /// <summary>
        /// Consumes the items in the given inventory slot without applying any effects!
        /// </summary>
        /// <param name="index">Inventory slot.</param>
        /// <returns>The actual amount consumed.</returns>
        virtual public int ConsumeItemFromInventorySlot(int index)
        {
            int actualAmount = 1;
            if (character.Inventory == null)
            {
                Debug.LogWarning("Tried to consume an item from an inventory but the Character doesn't have one");
                return 0;
            }
            ItemInstanceData item = character.Inventory.GetItemAt(index);
            if (item == null || item.amount == 0) return 0;
            actualAmount = item.amount;
            character.Inventory.RemoveItemAt(index, item.amount);
            OnItemConsumed(item.Data.typeId, item.amount, character);
            if (ItemCount(item.Data.typeId) == 0) OnItemDepleted(item.Data.typeId, character);
            return actualAmount;
        }

        /// <summary>
        /// Uses the given amount of items of type itemType applying any effects.
        /// </summary>
        /// <param name="itemType">Item type.</param>
        /// <param name="amount">Amount to consume.</param>
        /// <returns>The actual amount consumed.</returns>
        virtual public int UseItem(string itemType, int amount)
		{
			ItemTypeData itemTypeData = ItemTypeManager.Instance.GetTypeData (itemType);
			int actualAmount = amount;
			if (itemTypeData == null)
			{
				Debug.LogWarning("Item type " + itemType + " was not found");
				return 0;
			}
			if (itemTypeData.itemClass == ItemClass.INSTANT)
			{
				Debug.LogWarning ("You can't use an INSTANT item, it will be used when it is collected");
				return 0;
			}

            // Special case using an equippable item that is equipped
            if (character.EquipmentManager != null && character.EquipmentManager.IsEquipped(itemType) && itemTypeData.consumableOnceEquipped)
            {
                DoUseItem(itemTypeData, actualAmount);
                actualAmount = character.EquipmentManager.ConsumeFromSlot(itemTypeData.slot, amount);
                if (ItemCount(itemTypeData.typeId) == 0)
                {
                    OnItemDepleted(itemTypeData.typeId, character);
                    RecalculateEffectsOfItems();
                }
                return actualAmount;
            }
            // Non Inventory
            else if (itemTypeData.itemClass == ItemClass.NON_INVENTORY)
			{
				if (itemData.ContainsKey (itemType))
				{
					if (itemTypeData.itemBehaviour == ItemBehaviour.CONSUMABLE ||
					    itemTypeData.itemBehaviour == ItemBehaviour.POWER_UP)
					{
                        actualAmount = itemData.Consume (itemType, amount);
					}
                    if (itemTypeData.itemBehaviour == ItemBehaviour.EQUIPPABLE)
                    {
                        Debug.Log("Equipping non inventory items is not supported");
                    }
                    DoUseItem (itemTypeData, actualAmount);
					if (ItemCount(itemTypeData.typeId) == 0) OnItemDepleted(itemTypeData.typeId, character);
					RecalculateEffectsOfItems();
					return actualAmount;
				}
				return 0;
			}
			// Inventory
			else if (character.Inventory != null && character.Inventory.ItemCount(itemType) > 0)
			{
				if (itemTypeData.itemBehaviour == ItemBehaviour.CONSUMABLE ||
					itemTypeData.itemBehaviour == ItemBehaviour.POWER_UP)
				{
					actualAmount = character.Inventory.Consume (itemType, amount);
				}
				if (itemTypeData.itemBehaviour == ItemBehaviour.EQUIPPABLE)
				{
                    if (character.EquipmentManager == null)
                    {
                        Debug.LogWarning("Tried to use an equippable without an EquipmentManager added to your Character");
                        return 0;
                    }
                    int inventorySlot = character.Inventory.GetFirstSlotForItem (itemType);
					if (inventorySlot >= 0) character.EquipmentManager.EquipFromInventory(itemTypeData.slot, inventorySlot);
				}
				DoUseItem (itemTypeData, actualAmount);
				if (ItemCount(itemTypeData.typeId) == 0) OnItemDepleted(itemTypeData.typeId, character);
				RecalculateEffectsOfItems();
				return actualAmount;
			}
			return 0;
		}

		/// <summary>
		/// Uses a single item from a specific inventory slot.
		/// </summary>
		/// <returns><c>true</c>, if item from inventory slot was used, <c>false</c> otherwise.</returns>
		/// <param name="index">Index.</param>
		virtual public bool UseItemFromInventorySlot(int index)
		{
            int actualAmount = 1;
			if (character.Inventory == null)
			{
				Debug.LogWarning ("Tried to use an item from an inventory but the Character doesn't have one");
				return false;
			}
			ItemInstanceData item = character.Inventory.GetItemAt (index);
			if (item == null || item.amount == 0) return false;
            if (!item.Data.clickToConsume && item.Data.itemBehaviour == ItemBehaviour.CONSUMABLE) return false;
            if (item.Data.itemBehaviour == ItemBehaviour.CONSUMABLE ||
                item.Data.itemBehaviour == ItemBehaviour.POWER_UP)
			{
				character.Inventory.RemoveItemAt(index, 1);
			}
			if (item.Data.itemBehaviour == ItemBehaviour.EQUIPPABLE)
			{
				character.EquipmentManager.EquipFromInventory(item.Data.slot, index);
                actualAmount = item.amount;
			}
			return DoUseItem (item.Data, actualAmount);
		}

        virtual public bool DamageItemInEquipmentSlot(string slot, int amount)
        {
            ItemInstanceData itemInstanceData = character.EquipmentManager.GetItemForSlot(slot);
            if (itemInstanceData == null) return false;
            int damageDone = itemInstanceData.Damage(amount);
            if (damageDone == -1)
            {
                character.EquipmentManager.DestroyItem(slot);
                OnItemDestroyed(itemInstanceData);
                RecalculateEffectsOfItems();
            }
            else if (damageDone > 0)
            {
                OnItemDamaged(itemInstanceData);
            }
            return false;
        }

        /// <summary>
        /// Uses the given item, but does not handle removing it from stack/inventory.
        /// </summary>
        /// <returns><c>true</c>, if item can be used, <c>false</c> otherwise false.</returns>
        /// <param name="itemTypeData">Item type data.</param>
        virtual protected bool DoUseItem(ItemTypeData itemTypeData, int amount)
		{
			switch (itemTypeData.itemBehaviour)
			{
			case ItemBehaviour.EQUIPPABLE:
				RecalculateEffectsOfItems();
				// EQUIPPABLE must be handled by the calling function as how it works depends
				// on if you are using from an inventory or not (i.e. have to ensure enough space to unequip stuff)
				break;
			case ItemBehaviour.CONSUMABLE:
				OnItemConsumed(itemTypeData.typeId, amount, character);
                ApplyItemEffects(itemTypeData);
				RecalculateEffectsOfItems();
                return true;
			case ItemBehaviour.POWER_UP:
				if (amount != 1) Debug.LogWarning ("Tried to use more than one power up at the same time");
				if (character.PowerUpManager == null)
				{
					Debug.LogError ("If you wish to use power ups you must add a PowerUpManager to your Character");
				} else
				{
					OnItemConsumed(itemTypeData.typeId, amount, character);
					character.PowerUpManager.Collect (itemTypeData.typeId);
					RecalculateEffectsOfItems();
					return true;
				}
				break;
			}
				
			return false;
		}

		/// <summary>
		/// Gets a list of items that the character has.
		/// </summary>
		/// <returns>The items the character has.</returns>///
		virtual public List<ItemAndCount> GetItems()
		{
			List<ItemAndCount> result = new List<ItemAndCount> ();
			for(int i = 0; i < itemData.stackableItemCountsIds.Count; i++)
			{
				result.Add(new ItemAndCount( itemData.stackableItemCountsIds[i],  itemData.stackableItemCountsCounts[i]));
			}
			return result;
		}

		/// <summary>
		/// Returns true if the character has at least one of the given item.
		/// </summary>
		/// <param name="itemType">Item type.</param>
		/// <returns>True if the character has the item, false otherwise.</returns>
		virtual public bool HasItem(string itemType)
		{
			return ItemCount(itemType) > 0;
		}

		/// <summary>
		/// Handle collecting an item, based on type dta instead ofinstance data. NOTE: You should use instance data wherever possible or
		/// else variables like durability will not be tracked.
		/// </summary>
		/// <returns>The actual number collected (taking in to account max items).</returns>
		/// <param name="itemId">Id oif the itme to add</param>
		virtual public int CollectItem(string itemId, int amount)
		{
			ItemTypeData typeData = ItemTypeManager.Instance.GetTypeData(itemId);
			ItemInstanceData data = new ItemInstanceData();
			data.ItemId = itemId;
			data.durability = typeData.maxDurability;
			data.amount = amount;
			data.xp = 0;
			return CollectItem(data);
		
		}
		
		/// <summary>
		/// Handle collecting an item.
		/// </summary>
		/// <returns>The actual number collected (taking in to account max items).</returns>
		/// <param name="data">Item instance data, including amount, etc.</param>
		virtual public int CollectItem(ItemInstanceData data) 
		{
			ItemTypeData typeData = ItemTypeManager.Instance.GetTypeData(data.ItemId);

			// Handle INSTANT items
			if (typeData.itemClass == ItemClass.INSTANT)
			{
				if (typeData.itemBehaviour == ItemBehaviour.POWER_UP)
				{
					if (character.PowerUpManager == null)
					{
						Debug.LogError ("If you wish to use power ups you must add a PowerUpManager to your Character");
					} else
					{
						character.PowerUpManager.Collect (typeData.typeId);
					}
				}
				else if (typeData.itemBehaviour == ItemBehaviour.CONSUMABLE)
				{
					ApplyItemEffects (typeData);
				}
				return 1;
			}
			// Handle NON_INVENTORY (custom stack) items
			if (typeData.itemClass == ItemClass.NON_INVENTORY)
			{
				int max = ItemMax (data.ItemId);
				if (itemData.ContainsKey (data.ItemId))
				{
					if (itemData [data.ItemId] + data.amount > ItemMax (data.ItemId))
					{
						int remainder = data.amount - (max - itemData [data.ItemId]);
						itemData [data.ItemId] = max;
						OnItemCollected (data.ItemId, remainder, character);
						return remainder;
					}
					itemData [data.ItemId] += data.amount;
					OnItemCollected (data.ItemId, data.amount, character);
					RecalculateEffectsOfItems();
					return data.amount;
				} 
				else
				{
					itemData.AddItem (data.ItemId, data.amount);
					OnItemCollected (data.ItemId, data.amount, character);
					RecalculateEffectsOfItems();
					return data.amount;
				}
			}
			// Handle Inventory Items
			if (character.Inventory)
			{
				int result = character.Inventory.AddItem(data);
				if (result > 0)
				{
					OnItemCollected (data.ItemId, data.amount, character);
					RecalculateEffectsOfItems();
				}
				return result;
			}
			else
			{
				Debug.LogWarning ("Tried to add item to an inventory but Character does not have an Inventory");
			}
			return 0;
		}
		
		/// <summary>
		/// Updates item multiplier stats.
		/// </summary>
		override protected void RecalculateEffectsOfItems()
		{
			totalJumpHeightMultiplier = 1.0f;
			totalMoveSpeedMultiplier = 1.0f;
			totalRunSpeedMultiplier = 1.0f;
			totalAccelerationMultiplier = 1.0f;
			totalDamageMultiplier = 1.0f;
			totalWeaponSpeedMultiplier = 1.0f;
			totalMaxHealthAdjustment = 0;
			
			// Walk through each non-inventory item
			for (int j = 0; j < itemData.stackableItemCountsIds.Count; j++)
			{
				ItemTypeData i = ItemTypeManager.Instance.GetTypeData(itemData.stackableItemCountsIds[j]);
				if (i == null || i.itemBehaviour != ItemBehaviour.UPGRADE || itemData.stackableItemCountsCounts[j] == 0) continue;
				if (!ignoreZeroValuedMultipliers && i.jumpHeightMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued jump multiplier");
				}
				else
				{
					totalJumpHeightMultiplier *= (i.upgradesStack ? Mathf.Pow(i.jumpHeightMultiplier , itemData.stackableItemCountsCounts[j]) : i.jumpHeightMultiplier );
				}
				if (!ignoreZeroValuedMultipliers && i.moveSpeedMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued move speed multiplier, try a value of 1.0f");
				}
				else
				{
					totalMoveSpeedMultiplier *= (i.upgradesStack ? Mathf.Pow(i.moveSpeedMultiplier , itemData.stackableItemCountsCounts[j]) : i.moveSpeedMultiplier );
				}
				if (!ignoreZeroValuedMultipliers && i.runSpeedMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued run speed multiplier, try a value of 1.0f");
				}
				else
				{
					totalRunSpeedMultiplier *= (i.upgradesStack ? Mathf.Pow(i.runSpeedMultiplier , itemData.stackableItemCountsCounts[j]) : i.runSpeedMultiplier );
				}
				if (!ignoreZeroValuedMultipliers && i.accelerationMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued acceleration speed multiplier, you should use a value of 1.0f");
				}
				else
				{
					totalAccelerationMultiplier *= (i.upgradesStack ? Mathf.Pow(i.accelerationMultiplier , itemData.stackableItemCountsCounts[j]) : i.accelerationMultiplier );
				}
				if (!ignoreZeroValuedMultipliers && i.damageMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued damage multiplier, you should use a value of 1.0f");
				}
				else
				{
					totalDamageMultiplier *= (i.upgradesStack ? Mathf.Pow(i.damageMultiplier , itemData.stackableItemCountsCounts[j]) : i.damageMultiplier );
				}
				if (i.weaponSpeedMultiplier < 0.0f)
				{
					Debug.LogWarning("Weapon speed modifier cannot be 0 or less, try a value of 1.0f");
				}
				else
				{
					totalWeaponSpeedMultiplier *= (i.upgradesStack ? Mathf.Pow(i.weaponSpeedMultiplier , itemData.stackableItemCountsCounts[j]) : i.weaponSpeedMultiplier );
				}
				// Max health adds not multiplies
				totalMaxHealthAdjustment += i.maxHealthAdjustment * (i.upgradesStack ? itemData.stackableItemCountsCounts[j] : 1);
			}
			// Walk through inventory items
			if (character.Inventory != null)
			{
				ItemInstanceData[] data = character.Inventory.AllItems;
				for (int j = 0; j < data.Length; j++)
				{
					ItemInstanceData i = character.Inventory.AllItems[j];
					if (i == null || i.Data == null || i.Data.itemBehaviour != ItemBehaviour.UPGRADE || i.amount == 0) continue;
					if (!ignoreZeroValuedMultipliers && i.Data.jumpHeightMultiplier == 0.0f)
					{
						Debug.LogWarning("Skipping zero valued jump multiplier");
					}
					else
					{
						totalJumpHeightMultiplier *= (i.Data.upgradesStack ? Mathf.Pow(i.Data.jumpHeightMultiplier , i.amount) : i.Data.jumpHeightMultiplier );
					}
					if (!ignoreZeroValuedMultipliers && i.Data.moveSpeedMultiplier == 0.0f)
					{
						Debug.LogWarning("Skipping zero valued move speed multiplier, try a value of 1.0f");
					}
					else
					{
						totalMoveSpeedMultiplier *= (i.Data.upgradesStack ? Mathf.Pow(i.Data.moveSpeedMultiplier , i.amount) : i.Data.moveSpeedMultiplier );
					}
					if (!ignoreZeroValuedMultipliers && i.Data.runSpeedMultiplier == 0.0f)
					{
						Debug.LogWarning("Skipping zero valued run speed multiplier, try a value of 1.0f");
					}
					else
					{
						totalRunSpeedMultiplier *= (i.Data.upgradesStack ? Mathf.Pow(i.Data.runSpeedMultiplier , i.amount) : i.Data.runSpeedMultiplier );
					}
					if (!ignoreZeroValuedMultipliers && i.Data.accelerationMultiplier == 0.0f)
					{
						Debug.LogWarning("Skipping zero valued acceleration speed multiplier, you should use a value of 1.0f");
					}
					else
					{
						totalAccelerationMultiplier *= (i.Data.upgradesStack ? Mathf.Pow(i.Data.accelerationMultiplier , i.amount) : i.Data.accelerationMultiplier );
					}
					if (!ignoreZeroValuedMultipliers && i.Data.damageMultiplier == 0.0f)
					{
						Debug.LogWarning("Skipping zero valued damage multiplier, you should use a value of 1.0f");
					}
					else
					{
						totalDamageMultiplier *= (i.Data.upgradesStack ? Mathf.Pow(i.Data.damageMultiplier , i.amount) : i.Data.damageMultiplier );
					}
					if (i.Data.weaponSpeedMultiplier < 0.0f)
					{
						Debug.LogWarning("Weapon speed modifier cannot be 0 or less, try a value of 1.0f");
					}
					else
					{
						totalWeaponSpeedMultiplier *= (i.Data.upgradesStack ? Mathf.Pow(i.Data.weaponSpeedMultiplier , i.amount) : i.Data.weaponSpeedMultiplier );
					}
					// Max health adds not multiplies
					totalMaxHealthAdjustment += i.Data.maxHealthAdjustment * (i.Data.upgradesStack ? i.amount  : 1);
				}
			}
			recalculateEffectsOfItems = false;
		}
		
		#region Persitable methods

		/// <summary>
		/// Gets the character reference.
		/// </summary>
		/// <value>The character.</value>
		override public Character Character
		{
			get
			{
				#if UNITY_EDITOR
				if (character == null) return GetComponentInParent<Character>();
				#endif
				return character;
			}
            set
            {
                Debug.LogWarning("ItemManager doesn't allow character to be changed");
            }
        }

		/// <summary>
		/// Gets the data to save.
		/// </summary>
		override public object SaveData
		{
			get
			{
                return new object[]{itemData, 
					(Character == null || Character.Inventory == null) ? null : Character.Inventory.SaveData };
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
				return UniqueDataIdentifier;
			}
		}
		
		/// <summary>
		/// Applies the save data to the object.
		/// </summary>
		override public void ApplySaveData(object t)
		{
			if (t is object[])
			{
				if (((object[])t)[0] is ItemData && (((object[])t)[1] == null || ((object[])t)[1] is InventoryData))
				{
					itemData = (ItemData)((object[])t)[0];
					if (character.Inventory != null)
					{
						character.Inventory.ApplySaveData (((object[])t)[1]);
					}
					loaded = true;
					RecalculateEffectsOfItems();
					OnLoaded();
				}
				else 
				{
					Debug.LogError("Tried to apply unexpected data: " + ((object[])t)[0].GetType() + " " + ((object[])t)[1].GetType());
				}
			}
			else 
			{
				Debug.LogError("Tried to apply unexpected data: " + t.GetType());
			}
		}
		
		/// <summary>
		/// Get the type of object this Persistable saves.
		/// </summary>
		override public System.Type SavedObjectType()
		{
			return typeof(object[]);
		}

		/// <summary>
		/// Resets the save data back to default.
		/// </summary>
		override public void ResetSaveData()
		{
			itemData = new ItemData ();
			foreach (ItemTypeData itemTypeData in ItemTypeManager.Instance.ItemData)
			{
				if (itemTypeData.itemClass == ItemClass.NON_INVENTORY)
				{
					itemData.AddItem (itemTypeData.typeId, itemTypeData.startingCount);
				} 
				else if (itemTypeData.itemClass == ItemClass.NORMAL)
				{
					if (itemTypeData.startingCount > 0)
					{
						character.Inventory.AddItem (itemTypeData.typeId, itemTypeData.startingCount);
					}
				}
			}

			RecalculateEffectsOfItems();
#if UNITY_EDITOR
			Save(this);
#endif
		}

		/// <summary>
		/// Support complex object serialisation by passing additional types to seralizer.
		/// </summary>
		override public System.Type[] GetExtraTypes() 
		{
			return new System.Type[]{ typeof(ItemData), typeof(InventoryData), typeof(ItemInstanceData), typeof(EquipmentData) };
		}

		#endregion
	}
}