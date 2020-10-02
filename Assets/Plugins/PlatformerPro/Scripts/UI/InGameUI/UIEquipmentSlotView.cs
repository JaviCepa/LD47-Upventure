using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace PlatformerPro
{

    /// <summary>
    /// Shows the item equipped to a given slot for a given player.
    /// </summary>
    public class UIEquipmentSlotView : PlatformerProMonoBehaviour
    {

        /// <summary>
        /// Slot to show.
        /// </summary>
        [Tooltip ("Slot to show the item for")]
        public string slot;

        /// <summary>
        /// The player identifier.
        /// </summary>
        [Tooltip("Player ID or -1 for any player")]
        public int playerId = -1;

        [Header("UI")]
        /// <summary>
        /// The image component used to show equipped item.
        /// </summary>
        public Image itemImage;

        /// <summary>
        /// If non-null this GO will be disabled when an item is equipped and enabled when no item is equipped.
        /// </summary>
        [Tooltip("If non-null this GO will be disabled when an item is equipped and enabled when no item is equipped.")]
        public GameObject nothingEquippedGo;

        /// <summary>
        /// Cached game manager ref.
        /// </summary>
        PlatformerProGameManager gameManager;

        /// <summary>
        /// Cached equipment manager ref.
        /// </summary>
        EquipmentManager equipmentManager;

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <value>The header.</value>
        override public string Header
        {
            get
            {
                return "Shows the item equipped to a given slot for a given player.";
            }
        }

        /// <summary>
        /// Unity start hook.
        /// </summary>
        void Start()
        {
            Init();
        }

        /// <summary>
        /// Init this instance.
        /// </summary>
        virtual protected void Init()
        {
            GetCharacter();
        }

        /// <summary>
        /// Gets a character ref from a loader.
        /// </summary>
        virtual protected void GetCharacter()
        {
            gameManager = FindObjectOfType<PlatformerProGameManager>();
            if (gameManager != null)
            {
                gameManager.CharacterLoaded += HandleCharacterLoaded;
            }
        }

        /// <summary>
        /// Handles character loaded by assigning equipment manager.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        protected virtual void HandleCharacterLoaded(object sender, CharacterEventArgs e)
        {
            if (playerId == -1 || e.PlayerId == playerId)
            {
                equipmentManager = e.Character.EquipmentManager;
                if (equipmentManager == null) {
                    Debug.LogWarning("Loaded character has no equipment mananger, deactivating");
                    enabled = false;
                }
                else
                {
                    equipmentManager.ItemEquipped += HanldeItemEquipped;
                    equipmentManager.ItemUnequipped += HanldeItemUnequipped;
                    EquipmentData data = equipmentManager.GetItemForSlot(slot);
                    if (data != null)
                    {
                        itemImage.enabled = true;
                        itemImage.sprite = data.Data.Icon;
                        if (nothingEquippedGo != null) nothingEquippedGo.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Unity destroy hook.
        /// </summary>
        void OnDestroy()
        {
            DoDestroy();
        }

        /// <summary>
        /// Object destroyed, clear event listeners.
        /// </summary>
        virtual protected void DoDestroy() { 
            if (equipmentManager != null)
            {
                equipmentManager.ItemEquipped -= HanldeItemEquipped;
                equipmentManager.ItemUnequipped -= HanldeItemUnequipped;
            }
        }

        /// <summary>
        /// Hanldes an item being equipped.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        protected virtual void HanldeItemEquipped(object sender, ItemEventArgs e)
        {
            ItemTypeData data = ItemTypeManager.Instance.GetTypeData(e.Type);
            if (data.slot == slot)
            {
                itemImage.enabled = true;
                itemImage.sprite = data.Icon;
                if (nothingEquippedGo != null) nothingEquippedGo.SetActive(false);
            }
        }

        /// <summary>
        /// Hanldes an item being unequipped.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        protected virtual void HanldeItemUnequipped(object sender, ItemEventArgs e)
        {
            ItemTypeData data = ItemTypeManager.Instance.GetTypeData(e.Type);
            if (data.slot == slot)
            {
                itemImage.sprite = null;
                itemImage.enabled = false;
                if (nothingEquippedGo != null) nothingEquippedGo.SetActive(true);
            }
        }

    }
}