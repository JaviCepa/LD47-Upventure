using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace PlatformerPro.Extras
{
    /// <summary>
    /// UI item for an individual slot in an inventory.
    /// </summary>
    public class UIShopSlot : UIInventorySlot
    {
         [Header("Shop")]
         [Tooltip("Format string used to format the item price.")]
         public string priceStringFormat = "{0:D5}";

        /// <summary>
        /// Optional Text field to show the item cost.
        /// </summary>
        [Tooltip("Optional Text field to show the item cost.")]
        public Text itemCostText;

        public Color inStockColor = Color.white;

        public Color notInStockColor = new Color(1, 1, 1, 0.5f);

        /// <summary>
        /// Init post awake. Called from start.
        /// </summary>
        override protected void PostInit()
        {
            // We must already be initialised, bail out
            if (uiInventory != null) return;
            if (selectionIndicator != null) selectionIndicatorImage = selectionIndicator.GetComponentInChildren<Image>();
            uiInventory = GetComponentInParent<UIShop>();
            if (uiInventory == null) Debug.LogWarning("No matching inventory type (UIShop) found for slot");
        }


        /// <summary>
        /// Updates slot with given item data.
        /// </summary>
        /// <param name="data">Iventory data.</param>
        /// <param name="isSelected">Are we selected.</param>
        override public void UpdateWithItem(int position, ItemInstanceData data, bool isSelected, bool isPicked)
        {
            // Don't show cost if there's no item or if there is an item but its stock level is 0
            if (data == null || data.amount == 0)
            {
                if (itemCostText != null) itemCostText.enabled = false;
            }
            else
            {
                ItemTypeData typeData = ItemTypeManager.Instance.GetTypeData(data.ItemId);
                if (data == null)
                {
                    Debug.LogWarning("Couldn't find data for item: " + data.ItemId);
                    return;
                }

                if (itemCostText != null)
                {
                    itemCostText.enabled = false;
                    int price = ((UIShop)uiInventory).Shop.GetSellPriceForItemAtPosition(position);
                    if (price > 0)
                    {
                        itemCostText.enabled = true;
                        itemCostText.text = string.Format(priceStringFormat, price);
                    }
                }
            }
            base.UpdateWithItem(position, data, isSelected, isPicked);

            // Change color for out of stock items
            if (data != null && data.amount == 0)
            {
                icon.color = notInStockColor;
            }
            else
            {
                icon.color = inStockColor;
            }

        }

        /// <summary>
        /// Should this slot show something?
        /// </summary>
        override protected bool ShouldShowItemInUiSlot(int position, ItemInstanceData data, bool isSelected, bool isPicked)
        {
            if (data == null) return false;
            return true;
        }
    }
}