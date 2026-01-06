// ──────────────────────────────────────────────────
// InventoryItemPopup.cs
// UI popup for displaying item details and actions
// ✅ FIXED: Pocket logic completely removed
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Manager;

namespace Ascension.Inventory.Popup
{
    public class InventoryItemPopup : MonoBehaviour
    {
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
        [SerializeField] private Button actionButton2;

        private ItemBaseSO currentItem;
        private ItemInstance currentItemInstance;
        private ItemLocation currentLocation;
        private int selectedQuantity = 1;

        private void Start()
        {
            backButton.onClick.AddListener(ClosePopup);
            
            plus5Button.onClick.AddListener(() => AdjustQuantity(5));
            plus1Button.onClick.AddListener(() => AdjustQuantity(1));
            minus1Button.onClick.AddListener(() => AdjustQuantity(-1));
            minus5Button.onClick.AddListener(() => AdjustQuantity(-5));
            quantitySlider.onValueChanged.AddListener(OnSliderChanged);

            actionButton1.onClick.AddListener(OnActionButton1Clicked);
            actionButton2.onClick.AddListener(OnActionButton2Clicked);

            popupContainer.SetActive(false);
        }

        public void ShowItem(ItemBaseSO item, ItemInstance itemInstance, ItemLocation fromLocation)
        {
            currentItem = item;
            currentItemInstance = itemInstance;
            currentLocation = fromLocation;
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

            if (quantityText != null)
                quantityText.text = "Quantity:";

            SetupQuantityControls(itemInstance.quantity);
            SetupActionButtons(fromLocation);
        }

        private void SetupQuantityControls(int maxQuantity)
        {
            quantitySlider.minValue = 1;
            quantitySlider.maxValue = maxQuantity;
            quantitySlider.value = 1;

            UpdateQuantityDisplay();
        }

        private void SetupActionButtons(ItemLocation fromLocation)
        {
            // ✅ SIMPLIFIED: Only 2 buttons - Bag and Storage
            switch (fromLocation)
            {
                case ItemLocation.Storage:
                    // From storage: [Add to Bag] [Close]
                    actionButton1.gameObject.SetActive(true);
                    actionButton2.gameObject.SetActive(false);
                    SetButtonText(actionButton1, "Add to Bag");
                    break;

                case ItemLocation.Bag:
                    // From bag: [Store] [Close]
                    actionButton1.gameObject.SetActive(true);
                    actionButton2.gameObject.SetActive(false);
                    SetButtonText(actionButton1, "Store");
                    break;

                default:
                    actionButton1.gameObject.SetActive(false);
                    actionButton2.gameObject.SetActive(false);
                    break;
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

        private void OnActionButton1Clicked()
        {
            var buttonText = actionButton1.GetComponentInChildren<TMP_Text>()?.text;
            var database = InventoryManager.Instance.Database;
            InventoryResult result;
            
            if (buttonText == "Add to Bag")
            {
                result = InventoryManager.Instance.Inventory.MoveToBag(currentItemInstance, selectedQuantity, database);
            }
            else if (buttonText == "Store")
            {
                result = InventoryManager.Instance.Inventory.MoveToStorage(currentItemInstance, selectedQuantity, database);
            }
            else
            {
                Debug.LogWarning($"[InventoryItemPopup] Unknown button action: {buttonText}");
                return;
            }

            if (result.Success)
            {
                Debug.Log($"[InventoryItemPopup] {result.Message}");
                ClosePopup();
            }
            else
            {
                Debug.LogWarning($"[InventoryItemPopup] {result.Message}");
            }
        }

        private void OnActionButton2Clicked()
        {
            // Reserved for future use
            ClosePopup();
        }

        private void ClosePopup()
        {
            popupContainer.SetActive(false);
            currentItem = null;
            currentItemInstance = null;
        }

        #endregion
    }
}