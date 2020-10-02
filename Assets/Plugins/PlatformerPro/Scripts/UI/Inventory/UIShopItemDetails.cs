using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace PlatformerPro.Extras {

    public class UIShopItemDetails : PlatformerProMonoBehaviour
    {
        [Header("UI")]
        public GameObject visibleContent;
        public UIShop shopUi;
        public Text nameText;
        public Text priceText;
        public Text descriptionText;
        public Text reasonText;
        public Image itemImage;

        [Header("Messages")]
        public string cantAffordMessage = "Not enough coins!";
        public string outOfStockMessage = "None in stock!";
        public string priceFormat = "${0}";

        /// <summary>
        /// Gets the header string used to describe the component.
        /// </summary>
        /// <value>The header.</value>
        override public string Header
        {
            get
            {
                return "UI componet that shows details about the current selection in a shop UI";
            }
        }


        void Start()
        {
            shopUi.SelectionChanged += ShopUi_SelectionChanged;
        }

        void OnDestroy()
        {
            shopUi.SelectionChanged -= ShopUi_SelectionChanged;
        }


        void Update()
        {
            
            if (shopUi != null && shopUi.Focused)
            {
               // UpdateShopUI();
            }
            else 
            { 
                visibleContent.SetActive(false);
            }
        }

        void ShopUi_SelectionChanged(object sender, System.EventArgs e)
        {
            UpdateShopUI();
        }


        virtual protected void UpdateShopUI()
        {
            ShopItemData data = shopUi.CurrentSelectionDetails;
            if (data != null)
            {
                visibleContent.SetActive(true);
                // TODO: This supports single player only as it just gets first active character
                PurchaseFailReason reason = shopUi.GetCurrentPurchaseFailReason(PlatformerProGameManager.Instance.GetCharacterForPlayerId(-1));
                nameText.text = data.Data.humanReadableName;
                descriptionText.text = data.Data.description;
                if (shopUi.GetCurrentSellPrice() > 0) 
                { 
                    priceText.text = string.Format(priceFormat, shopUi.GetCurrentSellPrice());
                }
                else
                {
                    priceText.text = "";
                }
                itemImage.sprite = data.Data.Icon;
                switch (reason)
                {
                    case PurchaseFailReason.CANT_AFFORD: reasonText.text = cantAffordMessage; break;
                    case PurchaseFailReason.ITEM_NOT_IN_SHOP: reasonText.text = outOfStockMessage; break;
                    default: reasonText.text = ""; break;
                }
            }
            else
            {
                visibleContent.SetActive(false);
            }

        }
    }
}