using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// A button on an action bar that consumes an item.
	/// </summary>
	public class UIActionBarButton_EquipItem : UIActionBarButton 
	{
		[Header ("Item Data")]
		/// <summary>
		/// The item.
		/// </summary>
		[SerializeField]
        [ItemType]
		protected string item;

        /// <summary>
        /// The slot to equip to.
        /// </summary>
        protected string slot;

        /// <summary>
        /// Init this instance.
        /// </summary>
        override protected void Init()
		{
			itemId = item;
            ItemTypeData data = ItemTypeManager.Instance.GetTypeData(item);
            if (data != null) slot = data.slot;
            if (slot == null) Debug.Log("Item in the EquipItem Actionbutton doesn't have a slot defined");
			base.Init ();
		}

        /// <summary>
        /// Get item manager reference and register listerns.
        /// </summary>
        override protected void GetItemManager()
        {
            if (Character == null) return;
            base.GetItemManager();
            if (Character.EquipmentManager != null) Character.EquipmentManager.ItemEquipped += HandleItemEquipped;
            if (Character.EquipmentManager != null) Character.EquipmentManager.ItemUnequipped += HandleItemUnequipped;
            if (Character.EquipmentManager != null && Character.EquipmentManager.IsEquipped(itemId))
            {
                Enable();
                Activate();
            }
        }

        /// <summary>
        /// Updates the item count. Here we also want to check equipment manager.
        /// </summary>
        override protected void UpdateItemCount()
        {
            int count = itemManager.ItemCount(itemId);
            if (itemCountText != null) itemCountText.text = count.ToString();
            if (count < 1 && Character.EquipmentManager != null && Character.EquipmentManager.IsEquipped(itemId))
            {
                count = 1;
            }
            if (count < 1) Disable();
            else if (!isActive) Enable();
        }

        /// <summary>
        /// Do the destroy actions (remove event listeners).
        /// </summary>
        override protected void DoDestroy()
        {
            base.DoDestroy();
            if (Character.EquipmentManager != null) Character.EquipmentManager.ItemEquipped -= HandleItemEquipped;
            if (Character.EquipmentManager != null) Character.EquipmentManager.ItemUnequipped -= HandleItemUnequipped;
        }

        /// <summary>
        /// Handles the item equipped event.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        protected void HandleItemEquipped(object sender, ItemEventArgs e)
        {
            if (e.Type == itemId)
            {
                Activate();
            }
            UpdateItemCount();
        }

        /// <summary>
        /// Handles the item unequipped event.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        protected void HandleItemUnequipped(object sender, ItemEventArgs e)
        {
            if (e.Type == itemId)
            {
                Deactivate();
            }
            UpdateItemCount();
        }

        /// <summary>
        /// Handles the click event.
        /// </summary>
        override protected void DoPointerClick()
		{
			if (!canClickWhenPaused && TimeManager.Instance.Paused) return;
            if (allowDeactivate)
            {
                EquipmentData ed = character.EquipmentManager.GetItemForSlot(slot);
                if (ed != null && ed.ItemId == item)
                {
                    bool result = character.EquipmentManager.UnequipToInventory(slot);
                    if (result) { 
                        Enable();
                    }
                    return;
                }
            }
            int inventorySlot = character.Inventory.GetFirstSlotForItem(item);
            if (inventorySlot != -1) {
                character.EquipmentManager.EquipFromInventory(slot, inventorySlot);
                Activate();
            }
		}
	}
}
