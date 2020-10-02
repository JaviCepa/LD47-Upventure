using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlatformerPro
{
    /// <summary>
    /// This UI element is shown when a character stands over an item and the pick up button is not equal to -1 (i.e. auto pick-up is off).
    /// </summary>
    public class ItemPickupBox : PlatformerProMonoBehaviour
    {
        /// <summary>
        /// Root element holding the visible content
        /// </summary>
        [Tooltip("Root element holding the visible content")]
        public GameObject visibleContent;
        
        /// <summary>
        /// Image to show the item type. Can be null.
        /// </summary>
        [Tooltip("Image to show the item type. Can be null.")]
        public Image itemImage;
        
        /// <summary>
        /// Text to show item name. Can be null.
        /// </summary>
        [Tooltip("Text to show item name. Can be null.")]
        public Text itemName;
        
        /// <summary>
        /// Text to show item count. Can be null.
        /// </summary>
        [Tooltip("Text to show item count. Can be null.")]
        public Text itemCount;

        /// <summary>
        /// If true the box with be shown at transform of relevant item. You can use the visible content position relative to parent to offset.
        /// </summary>
        [Tooltip ("If true the box with be shown at transform of relevant item. You can use the visible content position relative to parent to offset.")]
        public bool showInWorldSpace;
        
        /// <summary>
        /// Camera to use for world space positioning. Will be searched for if null.
        /// </summary>
        [Tooltip("Camera to use for world space positioning")]
        [DontShowWhen("showInWorldSpace", true)]
        public Camera uiCamera;

        protected ItemManager itemManager;

        protected List<Item> activeItems;
        
        protected Canvas canvas;

        
        /// <summary>
        /// The item being shown.
        /// </summary>
        protected Item currentItem;

        public void Init(ItemManager itemManager)
        {
            this.itemManager = itemManager;
            activeItems = new List<Item>();
            if (showInWorldSpace)
            {
                canvas = GetComponentInParent<Canvas>();
                if (uiCamera == null) uiCamera = FindObjectOfType<PlatformerProStandardCamera>()?.GetComponent<Camera>();
                if (uiCamera == null) uiCamera = FindObjectOfType<Camera>();
            }
        }
        
        void Update()
        {
            if (itemManager != null && currentItem != null && !TimeManager.Instance.Paused)
            {
                if (!currentItem.isActiveAndEnabled)
                {
                    Hide(currentItem);
                }
                else if (itemManager.Character.Input.GetActionButtonState(itemManager.pickUpActionButton) == ButtonState.DOWN)
                {
                    currentItem.DoCollect(itemManager.Character);
                    Hide(currentItem);
                }
            }
        }
        
        /// <summary>
        /// Unity LateUpdate() hook.
        /// </summary>
        void LateUpdate()
        {
            if (currentItem != null && showInWorldSpace)
            {
                Vector3 viewPort = uiCamera.WorldToViewportPoint(currentItem.transform.position);
                ((RectTransform)transform).anchoredPosition = new Vector2(viewPort.x * canvas.pixelRect.width, viewPort.y * canvas.pixelRect.height);
            }
        }

        
        public override string Header
        {
            get
            {
                return "This UI element is shown when a character stands over an item and the pick up button is not equal to -1 (i.e. auto pick-up is off).";
            }
        }

        virtual public void Show(Item item)
        {
            if (currentItem == item) return;
            if (!item.isActiveAndEnabled)
            {
                if (activeItems.Contains(item))
                {
                    activeItems.Remove(item);
                    if (activeItems.Count > 0)
                    { 
                        Show(activeItems[0]);
                    }
                }
                return;
            }
            currentItem = item;
            if (!activeItems.Contains(item)) activeItems.Add(item);
            visibleContent.SetActive(true);
            if (itemImage != null) itemImage.sprite = item.instanceData.Data.Icon;
            if (itemCount != null) itemCount.text = "${item.Amount}";
            if (itemName != null) itemName.text = item.instanceData.Data.humanReadableName;
        }
        
        virtual public void Hide(Item item)
        {
            // We can only hide if the item matches the active item.
            if (activeItems.Contains(currentItem)) activeItems.Remove(currentItem);
            if (currentItem != item) return;
            if (activeItems.Count > 0)
            { 
                Show(activeItems[0]);
            }
            else 
            {
                currentItem = null;
                visibleContent.SetActive(false);
            }
        }
    }
}