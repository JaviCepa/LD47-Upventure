using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlatformerPro.Extras
{

    /// <summary>
    /// UI for a shop.
    /// </summary>
    public class UIShop : UIInventory
    {

        /// <summary>
        /// Cached shop ref.
        /// </summary>
        protected Shop shop;

        /// <summary>
        /// Gets the header string used to describe the component.
        /// </summary>
        /// <value>The header.</value>
        override public string Header
        {
            get
            {
                return "UI Representation of shop and the items it sells.";
            }
        }

        /// <summary>
        /// Gets the shop reference.
        /// </summary>
        /// <value>The shop.</value>
        public Shop Shop
        {
            get {
                return shop; 
            }
        }

        /// <summary>
        /// Handle the game phase by looking for READY phase and creating a UI when ready.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        override protected void HandlePhaseChange(object sender, GamePhaseEventArgs e)
        {
            if (e.Phase == GamePhase.READY && shop != null) CreateInventory();
        }

        /// <summary>
        /// Handles the character being loaded.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        override protected void HandleCharacterLoaded(object sender, CharacterEventArgs e)
        {
            if (playerId == PlatformerProGameManager.ANY_PLAYER || playerId == e.Character.PlayerId)
            {
                character = e.Character;
                itemManager = e.Character.ItemManager;
                shop = GetComponentInParent<Shop>();
                if (shop != null)
                {
                    shop.ShopBoughtItem += HandleItemChanges;
                    shop.ShopSoldItem += HandleItemChanges;
                    shop.ShopRestocked += HandleItemChanges;
                    shop.Loaded += HandleItemChanges;
                }
            }
        }

        override protected void CheckForActivation()
        {
            if (!slotContentHolder.activeInHierarchy)
            {
                isActive = false;
                return;
            }
            else
            {
                if (!isActive)
                {
                    ClearPicked();
                    SelectSlotAt(0);
                    isActive = true;
                }
            }
        }


        /// <summary>
        /// Updates the inventory slots to match Inventory content.
        /// </summary>
        override public void UpdateInventory()
        {
            if (slots == null) return;
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].UpdateWithItem(i, shop.GetItemInPosition(i), CurrentSelection == i, CurrentPick == i && CurrentPickInventory == this);
            }
            // Make sure we select an item if one is available
            if (CurrentSelection == -1 && Focused)
            {
                SelectFirstAvailable();
            }
        }

        /// <summary>
        /// Selects the first available slot.
        /// </summary>
        override protected void SelectFirstAvailable()
        {
            // Can't select an item if we don't have focus
            if (!Focused)
            {
                CurrentSelection = -1;
                return;
            }
            if (CurrentSelection != -1) slots[CurrentSelection].UpdateSelection(false);
            CurrentSelection = -1;
            int pos = 0;
            while (CurrentSelection == -1 && pos < slots.Count)
            {
                SelectSlotAt(pos);
                pos++;
            }
        }


        /// <summary>
        /// Creates the inventory UI.
        /// </summary>
        override protected void CreateInventory()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (!(slots[i] is UIShopSlot)) Debug.LogWarning("Shop  UI references slots that aren't shop slots!");
            }
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].UpdateWithItem(i, shop.GetItemInPosition(i), false, false);
                }
            }
            SelectFirstAvailable();
            GridLayoutGroup layout = null;
            if (slotContentHolder != null)
            {
                layout = slotContentHolder.GetComponentInChildren<GridLayoutGroup>();
                if (layout != null)
                {
                    columnCount = layout.constraintCount;
                }
            }
            if (columnCount == 0) columnCount = 1;
            if (columnCount > slots.Count) columnCount = slots.Count;
        }

        /// <summary>
        /// Gets details about the currently selected shop item.
        /// </summary>
        /// <value>The current selection details.</value>
        virtual public ShopItemData CurrentSelectionDetails
        {
            get
            {
                if (CurrentSelection == -1) return null;
                if (CurrentPick != -1) return null;
                return shop.GetItemInPosition(CurrentSelection);
            }
        }

        /// <summary>
        /// Gets the reason the currently selected item can't be purchased (or NONE if it can).
        /// </summary>
        /// <returns>The current purchase fail reason.</returns>
        /// <param name="character">Character.</param>
        virtual public PurchaseFailReason GetCurrentPurchaseFailReason(Character character)
        {
            if (CurrentSelection == -1) return PurchaseFailReason.NO_ITEM_SELECTED;
            return shop.GetPurchaseFailReasonInPosition(CurrentSelection, character);
        }

        /// <summary>
        /// Gets the sell price for the currently selected item.
        /// </summary>
        /// <returns>The current sell price.</returns>
        virtual public int GetCurrentSellPrice()
        {
            if (CurrentSelection == -1) return 0;
            return shop.GetSellPriceForItemAtPosition(CurrentSelection);
        }

        /// <summary>
        /// CHanges the selection to the given slot.
        /// </summary>
        /// <param name="index">Index.</param>
        override public void SelectSlotAt(int index)
        {
            ActiveInventory = this;
            if (CurrentSelection == index) return;
            if (index < 0) index = -1;
            if (index >= slots.Count) index = slots.Count - 1;
            if (CurrentSelection >= 0 && CurrentSelection < slots.Count) slots[CurrentSelection].UpdateSelection(false);
            CurrentSelection = index;
            if (index >= 0 && index < slots.Count) slots[index].UpdateSelection(true);
        }


        /// <summary>
        /// Determines whether this instance is a valid pick target for the CurrentPick
        /// </summary>
        /// <returns><c>true</c> if this instance is valid pick target; otherwise, <c>false</c>.</returns>
        override public bool IsPickTarget(ItemInstanceData data, int slotIndex, UIInventory targetInventory)
        {
            if (CurrentPickInventory != this) return CurrentPickInventory.IsPickTarget(data, slotIndex, this);
            return false;
        }

        /// <summary>
        /// Gets the data for slots at index. Null if no data there.
        /// </summary>
        /// <returns>The data for slot.</returns>
        /// <param name="index">Index of slot.</param>
        override public ItemInstanceData GetDataForSlot(int index)
        {
            if (index >= 0 && index <= slots.Count)
            {
            
                ItemInstanceData data = shop.GetItemInPosition(index);
                if (data == null) return null;
                return data;
            }
            return null;
        }

        /// <summary>
        /// Changes the pick item to the given slot. For shops this does nothing
        /// as you can't pick a shop item.
        /// </summary>
        /// <param name="index">Index.</param>
        override public void DoPicked(UIInventory target, int index)
        {
            if (target == null) return;
            // Always do the work on the targetted inventory
            if (target != this)
            {
                target.DoPicked(target, index);
                return;
            }
            // Can't start a pick on the shop
            if (CurrentPickInventory == null) return;
            // Otherwise handle the pick action
            DoPickAction(target, index);
            ClearPicked();
        }

        /// <summary>
        /// Once we have two valid picks selected, this tries to do an appropriate action. 
        /// </summary>
        /// <returns><c>true</c>, if pick action was doable, <c>false</c> otherwise</returns>
        /// <param name="target">Target UI.</param>
        /// <param name="index">Index of slot in target UI.</param>
        override protected bool DoPickAction(UIInventory target, int index)
        {
            if (CurrentPickInventory != this && CurrentPick != -1)
            {
                ItemInstanceData data = character.Inventory.GetItemAt(CurrentPick);
                if (data != null)
                {
                    shop.SellItemToShop(data, character, CurrentPick);
                }
            }
            ClearPicked();
            UpdateInventory();
            return false;
        }


        /// <summary>
        /// Activate the item in the given slot.
        /// </summary>
        /// <param name="index">Index.</param>
        override public void ActivateItemAt(int index)
        {
            int result = shop.PurchaseItemAt(index, character, 0);
            UpdateInventory();
        }

    }
}