﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlatformerPro
{
	/// <summary>
	/// Managed equipping items.
	/// </summary>
	public class EquipmentManager : ItemStatProvider, ICoreComponent
	{

		/// <summary>
		/// If true automatically equip a weapon if no weapon is currently equipped
		/// </summary>
		[Tooltip("If true automatically equip an item if no item is currently equipped in the items slot")]
		public bool autoEquip;

		/// <summary>
		/// If true automatically equip a weapon if no weapon is currently equipped
		/// </summary>
		[Tooltip("If true automatically equip an item even if the slot is not empty")] [DontShowWhen("autoEquip", true)]
		public bool alwaysAutoEquip;

		/// <summary>
		/// If true automatically drop the item that was in the auto equip slot.
		/// </summary>
		[Tooltip("If true automatically drop the item that was in the auto equip slot.")]
		[DontShowWhen("alwaysAutoEquip", true)]
		public bool dropOnAutoEquip;

		/// <summary>
		/// A list of slots that auto equip will apply to.
		/// </summary>
		[Tooltip("A list of slots that auto equip will apply to.")] [DontShowWhenAttribute("autoEquip", true)]
		public List<string> autoEquipSlots;

		/// <summary>
		/// If true then we allow zero valued multipliers. Otherwise we ignore them and raise a warning.
		/// </summary>
		[Header("Modifiers")]
		public bool ignoreZeroValuedMultipliers = true;

		/// <summary>
		/// Slots in here will have their multipliers ignored
		/// </summary>
		public List<string> ignoredSlots;
		
		/// <summary>
		/// Lookup for getting equipment state.
		/// </summary>
		protected Dictionary<string, EquipmentData> data;

		/// <summary>
		/// Which character does this inventory belong to.
		/// </summary>
		protected Character character;

		/// <summary>
		/// The player preference identifier.
		/// </summary>
		public const string UniqueDataIdentifier = "EquipmentManager";

	
	
		#region events

		/// <summary>
		/// Item collected.
		/// </summary>
		public event System.EventHandler<ItemEventArgs> ItemEquipped;

		/// <summary>
		/// Raises the item equipped event.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="character">Character.</param>
		virtual protected void OnItemEquipped(string type, Character character)
		{
			if (SaveOnChange) Save(this);
			if (ItemEquipped != null)
			{
				ItemEquipped(this, new ItemEventArgs(type, 1, character));
			}
		}

		/// <summary>
		/// Item collected.
		/// </summary>
		public event System.EventHandler<ItemEventArgs> ItemUnequipped;

		/// <summary>
		/// Raises the item unequipped event.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="character">Character.</param>
		virtual protected void OnItemUnequipped(string type, Character character)
		{
			if (SaveOnChange) Save(this);
			if (ItemUnequipped != null)
			{
				ItemUnequipped(this, new ItemEventArgs(type, 1, character));
			}
		}


		#endregion

		/// <summary>
		/// Init this instance. Called from awake.
		/// </summary>
		virtual public void Init(Character character)
		{
			this.character = character;
			ConfigureEventListeners();
			if (!loaded)
			{
				data = new Dictionary<string, EquipmentData>();
				RecalculateEffectsOfItems();
			}
		}

		override protected void ConfigureEventListeners()
		{
			base.ConfigureEventListeners();
			if (autoEquip)
			{
				Character.ItemManager.ItemCollected += HandleItemCollected;
			}
		}

		/// <summary>
		/// Handles an item being collected. Used for autoequip.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		virtual protected void HandleItemCollected(object sender, ItemEventArgs e)
		{
			if (autoEquip)
			{
				ItemTypeData data = ItemTypeManager.Instance.GetTypeData(e.Type);
				if (data != null && autoEquipSlots.Contains(data.slot))
				{
					var currentItem = GetItemForSlot(data.slot);
					if (currentItem == null)
					{
						EquipFromInventory(data.slot, Character.Inventory.GetFirstSlotForItem(data.typeId));
					}
					else if (alwaysAutoEquip)
					{
						int inventorySlot = Character.Inventory.GetFirstSlotForItem(data.typeId);
						EquipFromInventory(data.slot, inventorySlot);
						if (dropOnAutoEquip) Character.ItemManager.DropItemFromInventorySlot(inventorySlot);
					}
				}
			}
		}

		/// <summary>
		/// Gets the item data for the given slot.
		/// </summary>
		/// <returns>The item for given slot.</returns>
		/// <param name="slot">Slot.</param>
		virtual public EquipmentData GetItemForSlot(string slot)
		{
			if (data.ContainsKey(slot)) return data[slot];
			return null;
		}

		/// <summary>
		/// Equips an item from the inventory slot provided.
		/// </summary>
		/// <returns><c>true</c>, if item from inventory was equiped, <c>false</c> otherwise.</returns>
		/// <param name="slot">name of equippable slot.</param>
		/// <param name="inventorySlot">Index of inventory slot.</param>
		public bool EquipFromInventory(string slot, int inventorySlot)
		{
			if (character.Inventory == null)
			{
				Debug.LogWarning("Can't equip from inventory as the character doesn't have an inventory.");
				return false;
			}

			ItemInstanceData inventoryData = character.Inventory.GetItemAt(inventorySlot);
			if (inventoryData == null || inventoryData.amount == 0)
			{
				Debug.LogWarning("Can't equip from inventory slot as nothing is in the given slot.");
				return false;
			}

			if (!ItemTypeManager.Instance.slots.Contains(slot))
			{
				Debug.LogWarning("Can't equip item to slot " + slot + ", slot doesn't exist.");
				return false;
			}

			ItemTypeData itemTypeData = ItemTypeManager.Instance.GetTypeData(inventoryData.ItemId);
			if (itemTypeData.itemBehaviour != ItemBehaviour.EQUIPPABLE || itemTypeData.slot != slot)
			{
				return false;
			}

			// Do equip
			int amount = 1;
			if (itemTypeData.equipAll) amount = inventoryData.amount;
			EquipmentData ed = new EquipmentData(inventoryData);
			ed.slot = slot;
			ed.amount = amount;
			EquipmentData existingEd = null;
			if (data.ContainsKey(slot))
			{
				existingEd = data[slot];
			}

			// Put existing item in inventory when old item will still exist (i.e. when we are putting on 1 from a stack of many)
			if (existingEd != null && inventoryData.amount != amount)
			{
				int result = character.Inventory.AddItem(existingEd);
				if (result == 0)
				{
					// TODO EVENT - No space to equip
					return false;
				}

				character.Inventory.RemoveItemAt(inventorySlot, amount);
			}
			else if (existingEd != null)
			{
				// Existing item that will be replaced - remove first then add
				character.Inventory.RemoveItemAt(inventorySlot, amount);
				character.Inventory.AddItemAt(existingEd, inventorySlot);
			}
			else
			{
				character.Inventory.RemoveItemAt(inventorySlot, amount);
			}

			data[slot] = ed;
			if (existingEd != null) OnItemUnequipped(existingEd.ItemId, character);
			character.ItemManager.OnInventoryChanged();
			OnItemEquipped(itemTypeData.typeId, character);
			RecalculateEffectsOfItems();
			return true;
		}

		/// <summary>
		/// Unequips to a given inventory slot.
		/// </summary>
		/// <returns><c>true</c>, if able to unequipped, <c>false</c> otherwise.</returns>
		/// <param name="slot">Slot.</param>
		public bool UnequipToInventory(string slot)
		{
			if (data.ContainsKey(slot))
			{
				ItemInstanceData itemData = data[slot];
				int amount = character.Inventory.AddItem(itemData);
				if (amount == 0)
				{
					// TODO No room
					return false;
				}

				data[slot] = null;
				character.ItemManager.OnInventoryChanged();
				OnItemUnequipped(itemData.ItemId, character);
				RecalculateEffectsOfItems();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Unequips to a specific inventory slot.
		/// </summary>
		/// <returns><c>true</c>, if able to unequip to slot, <c>false</c> otherwise.</returns>
		/// <param name="slot">Slot.</param>
		/// <param name="inventorySlot">Inventory slot.</param>
		public bool UnequipToInventoryAtSlot(string slot, int inventorySlot)
		{
			if (data.ContainsKey(slot))
			{
				ItemInstanceData itemData = data[slot];
				int amount = character.Inventory.AddItemAt(itemData, inventorySlot);
				if (amount == 0)
				{
					// TODO No room
					return false;
				}

				data[slot] = null;
				OnItemUnequipped(itemData.ItemId, character);
				character.ItemManager.OnInventoryChanged();
				RecalculateEffectsOfItems();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Destroy an item (Unequip the given item and don't add it to inventory).
		/// </summary>
		/// <returns><c>true</c>, if item was destroyed, <c>false</c> if it couldn't be found.</returns>
		/// <param name="slot">Slot.</param>
		public bool DestroyItem(string slot)
		{
			if (data.ContainsKey(slot))
			{
				ItemInstanceData itemData = data[slot];
				data[slot] = null;
				OnItemUnequipped(itemData.ItemId, character);
				RecalculateEffectsOfItems();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns true if an item with the given id is equipped in any slot.
		/// </summary>
		/// <returns><c>true</c>, If item is in slot.</returns>
		/// <param name="itemId">Item identifier.</param>
		public bool IsEquipped(string itemId)
		{
			foreach (EquipmentData ed in data.Values)
			{
				if (ed != null && ed.ItemId == itemId) return true;
			}

			return false;
		}

		/// <summary>
		/// Returns true if an item with the given id is in the given slot.
		/// </summary>
		/// <returns><c>true</c>, If item is in slot.</returns>
		/// <param name="itemId">Item identifier.</param>
		/// <param name="slot">Slot.</param>
		public bool IsEquippedinSlot(string itemId, string slot)
		{
			if (data.ContainsKey(slot) && data[slot] != null)
			{
				ItemInstanceData itemData = data[slot];
				if (data[slot].ItemId == itemId) return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the count of a given item type across all stacks.
		/// </summary>
		/// <returns>The count.</returns>
		/// <param name="itemId">Item identifier.</param>
		public int ItemCount(string itemId)
		{
			int result = 0;
			foreach (EquipmentData d in data.Values)
			{
				if (d != null && d.ItemId == itemId) result += d.amount;
			}

			return result;
		}

		/// <summary>
		/// Consume the amount of item from the specificed slot.
		/// </summary>
		/// <returns>The actual number consumed.</returns>
		/// <param name="slot">Slot to consume from.</param>
		/// <param name="amount">Amount.</param>
		public int ConsumeFromSlot(string slot, int amount)
		{
			if (data.ContainsKey(slot))
			{
				EquipmentData itemData = data[slot];
				if (itemData.amount <= amount)
				{
					int actualAmount = itemData.amount;
					DestroyItem(slot);
					return actualAmount;
				}
				else
				{
					itemData.amount -= amount;
					return amount;
				}
			}
			return 0;
		}

		/// <summary>
		/// Updates item multipler stats.
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

			foreach (ItemInstanceData i in data.Values)
			{
				if (i == null || i.Data == null || ignoredSlots.Contains(i.Data.slot)) continue;
				if (!ignoreZeroValuedMultipliers && i.Data.jumpHeightMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued jump multiplier");
				}
				else
				{
					totalJumpHeightMultiplier *= i.Data.jumpHeightMultiplier;
				}
				if (!ignoreZeroValuedMultipliers && i.Data.moveSpeedMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued move speed multiplier");
				}
				else
				{
					totalMoveSpeedMultiplier *= i.Data.moveSpeedMultiplier;
				}
				if (!ignoreZeroValuedMultipliers && i.Data.runSpeedMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued run speed multiplier");
				}
				else
				{
					totalRunSpeedMultiplier *= i.Data.runSpeedMultiplier;
				}
				if (!ignoreZeroValuedMultipliers && i.Data.accelerationMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued acceleration speed multiplier, you should use a value of 1.0f");
				}
				else
				{
					totalAccelerationMultiplier *= i.Data.accelerationMultiplier;
				}
				if (!ignoreZeroValuedMultipliers && i.Data.damageMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued damage multiplier, you should use a value of 1.0f");
				}
				else
				{
					totalDamageMultiplier *= i.Data.damageMultiplier;
				}
				if (i.Data.weaponSpeedMultiplier < 0.0f)
				{
					Debug.LogWarning("Weapon speed modifier cannot be 0 or less, try a value of 1.0f");
				}
				else
				{
					totalWeaponSpeedMultiplier *= i.Data.weaponSpeedMultiplier;
				}
				// Max health adds not multiplies
				totalMaxHealthAdjustment += i.Data.maxHealthAdjustment;
			}
		}
		
		
		#region Persistable methods

        /// <summary>
        /// Gets the character.
        /// </summary>
        override public Character Character 
		{
			get 
            { 
                return character; 
            }
            set
            {
                Debug.LogWarning("EquipmentManager doesn't allow character to be changed");
            }
        }

		/// <summary>
		/// Gets the data to save.
		/// </summary>
		override public object SaveData 
		{
			get
			{
				return data.Values.Where(e => e != null && e.amount > 0).ToList ();
//				if (result.Count == 0) rreturn new List<EquipmentData>();
//				return result;
			}
		}

		/// <summary>
		/// Get a unique identifier to use when saving the data (for example this could be used for part of the file name or player prefs name).
		/// </summary>
		override public  string Identifier{ 
			get { return UniqueDataIdentifier; }
		}

		/// <summary>
		/// Applies the save data to the object.
		/// </summary>
		override public void ApplySaveData(object t)
		{
			if (t is List<EquipmentData>)
			{
				data = new Dictionary<string, EquipmentData> ();
				foreach (EquipmentData ed in (List<EquipmentData>)t)
				{
					data.Add (ed.slot, ed);
					if (ed != null && ed.amount >= 0) OnItemEquipped (ed.ItemId, character);
				}
				loaded = true;
				RecalculateEffectsOfItems();
				return;
			} 
			else if (t != null)
			{
				Debug.LogWarning ("Invalid type for EquipmentData");
			}
			data = new Dictionary<string, EquipmentData> ();
			loaded = true;
			RecalculateEffectsOfItems();
			return;
		}

		/// <summary>
		/// Get the type of object this Persistable saves.
		/// </summary>
		override public System.Type SavedObjectType()
		{
			return typeof(List<EquipmentData>);
		}

		/// <summary>
		/// Resets the save data back to default.
		/// </summary>
		override public void ResetSaveData()
		{
			data = new Dictionary<string, EquipmentData> ();
			RecalculateEffectsOfItems();
		}


		/// <summary>
		/// Support complex object serialisation by passing additional types to seralizer.
		/// </summary>
		override public System.Type[] GetExtraTypes() 
		{
			return new System.Type[]{ typeof(EquipmentData), typeof(ItemInstanceData), typeof(List<ItemInstanceData>), typeof(List<EquipmentData>)};
		}


		#endregion
	}

	/// <summary>
	/// Stores data about what the player has equipped. Equipped items are quite like inventory items so we 
	/// extend that class.
	/// </summary>
	[System.Serializable]
	public class EquipmentData : ItemInstanceData
	{
		/// <summary>
		/// The slot the item is in.
		/// </summary>
		public string slot;

		/// <summary>
		/// Initializes a new instance of the <see cref="PlatformerPro.EquipmentData"/> class.
		/// </summary>
		public EquipmentData () : base() 
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PlatformerPro.EquipmentData"/> class.
		/// </summary>
		/// <param name="data">Data.</param>
		public EquipmentData (ItemInstanceData data) : base() 
		{
            itemId = data.ItemId;
            amount = data.amount;
            durability = data.durability;
            xp = data.xp;
            // TODO Custom properties
		}

	}
}