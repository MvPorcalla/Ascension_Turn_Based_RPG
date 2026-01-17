// ════════════════════════════════════════════
// Assets\Scripts\Modules\SharedUI\Popups\ItemPopup.cs
// Popup for displaying item details and actions
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;

namespace Ascension.UI.Popups
{
    public class ItemPopup : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Configuration")]
        [SerializeField] private PopupConfig config;

        [Header("Popup Container")]
        [SerializeField] private GameObject popupContainer;

        [Header("Header")]
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private Button backButton;

        [Header("Item Display")]
        [SerializeField] private Image itemIcon;

        [Header("Item Description")]
        [SerializeField] private TMP_Text itemDescription;

        [Header("Quantity Display")]
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private TMP_Text quantityValue;

        [Header("Quantity Controls")]
        [SerializeField] private Button plus5Button;
        [SerializeField] private Button plus1Button;
        [SerializeField] private Button minus1Button;
        [SerializeField] private Button minus5Button;
        [SerializeField] private Slider quantitySlider;

        [Header("Action Buttons")]
        [SerializeField] private Button actionButton1;
        #endregion

        #region Private Fields
        private ItemBaseSO currentItem;
        private ItemInstance currentItemInstance;
        private PopupContext currentContext;
        private int selectedQuantity = 1;
        #endregion

        #region Unity Callbacks
        private void Start()
        {
            ValidateConfig();
            SetupButtons();
            popupContainer.SetActive(false);
        }

        private void ValidateConfig()
        {
            if (config == null)
            {
                Debug.LogError("[ItemPopup] PopupConfig not assigned!");
            }
        }

        private void SetupButtons()
        {
            backButton.onClick.AddListener(Hide);
            
            if (config != null)
            {
                plus5Button.onClick.AddListener(() => AdjustQuantity(config.quickAdjustLarge));
                plus1Button.onClick.AddListener(() => AdjustQuantity(config.quickAdjustSmall));
                minus1Button.onClick.AddListener(() => AdjustQuantity(-config.quickAdjustSmall));
                minus5Button.onClick.AddListener(() => AdjustQuantity(-config.quickAdjustLarge));
            }

            quantitySlider.onValueChanged.AddListener(OnSliderChanged);
            actionButton1.onClick.AddListener(OnActionButton1Clicked);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Now accepts PopupContext
        /// </summary>
        public void ShowItem(ItemBaseSO item, ItemInstance itemInstance, PopupContext context)
        {
            currentItem = item;
            currentItemInstance = itemInstance;
            currentContext = context; // ✅ Store context
            selectedQuantity = 1;

            popupContainer.SetActive(true);

            itemName.text = item.ItemName;

            if (itemIcon != null)
            {
                if (item.Icon != null)
                {
                    itemIcon.sprite = item.Icon;
                    itemIcon.enabled = true;
                }
                else
                {
                    itemIcon.sprite = null;
                    itemIcon.enabled = false;
                }
            }

            if (itemDescription != null)
                itemDescription.text = item.Description;

            if (quantityText != null && config != null)
                quantityText.text = config.labelQuantity;

            SetupQuantityControls(itemInstance.quantity);
            SetupActionButtons();
        }

        public void Hide()
        {
            popupContainer.SetActive(false);
            currentItem = null;
            currentItemInstance = null;
            currentContext = null; // ✅ Clear context
        }

        #endregion

        #region Private Methods

        private void SetupQuantityControls(int maxQuantity)
        {
            quantitySlider.minValue = 1;
            quantitySlider.maxValue = maxQuantity;
            quantitySlider.value = 1;

            UpdateQuantityDisplay();
        }

        /// <summary>
        /// Uses context to determine button visibility
        /// </summary>
        private void SetupActionButtons()
        {
            if (config == null || currentContext == null) return;

            if (actionButton1 != null)
            {
                actionButton1.gameObject.SetActive(currentContext.CanMove);
                
                if (currentContext.CanMove)
                {
                    SetButtonText(actionButton1, config.GetMoveButtonText(currentContext.SourceLocation));
                }
            }
        }

        private void SetButtonText(Button button, string text)
        {
            if (button != null)
            {
                var textComponent = button.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = text;
                }
            }
        }

        #endregion

        #region Quantity Controls

        private void AdjustQuantity(int amount)
        {
            selectedQuantity = Mathf.Clamp(
                selectedQuantity + amount, 
                1, 
                currentItemInstance.quantity
            );
            
            quantitySlider.value = selectedQuantity;
            UpdateQuantityDisplay();
        }

        private void OnSliderChanged(float value)
        {
            selectedQuantity = Mathf.RoundToInt(value);
            UpdateQuantityDisplay();
        }

        private void UpdateQuantityDisplay()
        {
            if (quantityValue != null)
                quantityValue.text = selectedQuantity.ToString();
        }

        #endregion

        #region Action Handlers

        /// <summary>
        /// Uses PopupActionHandler instead of calling InventoryManager directly
        /// </summary>
        private void OnActionButton1Clicked()
        {
            if (currentContext == null || currentItemInstance == null) return;

            // Determine target location
            ItemLocation targetLocation = currentContext.SourceLocation == ItemLocation.Bag
                ? ItemLocation.Storage
                : ItemLocation.Bag;

            // Delegate to PopupActionHandler
            PopupActionHandler.Instance.MoveItem(
                currentItemInstance, 
                selectedQuantity, 
                targetLocation
            );
        }

        #endregion
    }
}