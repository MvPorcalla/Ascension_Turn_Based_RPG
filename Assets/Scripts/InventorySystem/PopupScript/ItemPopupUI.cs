// -------------------------------
// ItemPopupUI.cs - Dedicated popup for stackable items (Materials, Misc, Ingredients)
// -------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemPopupUI : MonoBehaviour
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
    [SerializeField] private Button addToPocketButton;
    [SerializeField] private Button addToBagButton;

    private ItemBaseSO currentItem;
    private ItemInstance currentItemInstance;
    private ItemLocation currentLocation;
    private int selectedQuantity = 1;

    private void Start()
    {
        // Setup button listeners
        backButton.onClick.AddListener(ClosePopup);
        
        // Quantity controls
        plus5Button.onClick.AddListener(() => AdjustQuantity(5));
        plus1Button.onClick.AddListener(() => AdjustQuantity(1));
        minus1Button.onClick.AddListener(() => AdjustQuantity(-1));
        minus5Button.onClick.AddListener(() => AdjustQuantity(-5));
        quantitySlider.onValueChanged.AddListener(OnSliderChanged);

        // Action buttons
        addToPocketButton.onClick.AddListener(OnAddToPocketClicked);
        addToBagButton.onClick.AddListener(OnAddToBagClicked);

        popupContainer.SetActive(false);
    }

    public void ShowItem(ItemBaseSO item, ItemInstance itemInstance, ItemLocation fromLocation)
    {
        currentItem = item;
        currentItemInstance = itemInstance;
        currentLocation = fromLocation;
        selectedQuantity = 1;

        popupContainer.SetActive(true);

        // Setup header
        itemName.text = item.itemName;

        // Setup icon
        if (itemIcon != null)
        {
            if (item.icon != null)
            {
                itemIcon.sprite = item.icon;
                itemIcon.enabled = true;
            }
            else
            {
                itemIcon.sprite = null;
                itemIcon.enabled = false;
            }
        }

        // Setup description
        if (itemDescription != null)
            itemDescription.text = item.description;

        // Setup quantity display
        if (quantityText != null)
            quantityText.text = "Quantity:";

        // Setup quantity controls
        SetupQuantityControls(itemInstance.quantity);

        // Setup action buttons visibility
        SetupActionButtons(fromLocation);
    }

    private void SetupQuantityControls(int maxQuantity)
    {
        // Setup slider
        quantitySlider.minValue = 1;
        quantitySlider.maxValue = maxQuantity;
        quantitySlider.value = 1;

        UpdateQuantityDisplay();
    }

    private void SetupActionButtons(ItemLocation fromLocation)
    {
        // Setup buttons based on current location
        switch (fromLocation)
        {
            case ItemLocation.Storage:
                // From storage: [Add to Pocket] [Add to Bag]
                addToPocketButton.gameObject.SetActive(true);
                addToBagButton.gameObject.SetActive(true);
                
                SetButtonText(addToPocketButton, "Add to Pocket");
                SetButtonText(addToBagButton, "Add to Bag");
                break;

            case ItemLocation.Pocket:
                // From pocket: [Add to Bag] [Store]
                addToPocketButton.gameObject.SetActive(true); // ✅ Show both buttons
                addToBagButton.gameObject.SetActive(true);
                
                SetButtonText(addToPocketButton, "Add to Bag");
                SetButtonText(addToBagButton, "Store");
                break;

            case ItemLocation.Bag:
                // From bag: [Add to Pocket] [Store]
                addToPocketButton.gameObject.SetActive(true);
                addToBagButton.gameObject.SetActive(true);
                
                SetButtonText(addToPocketButton, "Add to Pocket");
                SetButtonText(addToBagButton, "Store");
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

    private void OnAddToPocketClicked()
    {
        var buttonText = addToPocketButton.GetComponentInChildren<TMP_Text>()?.text;
        
        if (buttonText == "Add to Pocket")
        {
            // From storage or bag: Move to pocket
            if (InventoryManager.Instance.Inventory.MoveToPocket(currentItemInstance, selectedQuantity))
            {
                Debug.Log($"Moved {selectedQuantity}x {currentItem.itemName} to pocket");
                ClosePopup();
            }
        }
        else if (buttonText == "Add to Bag") // ✅ Add this case
        {
            // From pocket: Move to bag
            if (InventoryManager.Instance.Inventory.MoveToBag(currentItemInstance, selectedQuantity))
            {
                Debug.Log($"Moved {selectedQuantity}x {currentItem.itemName} to bag");
                ClosePopup();
            }
        }
    }

    private void OnAddToBagClicked()
    {
        var buttonText = addToBagButton.GetComponentInChildren<TMP_Text>()?.text;
        
        if (buttonText == "Add to Bag")
        {
            // From storage or pocket: Move to bag
            if (InventoryManager.Instance.Inventory.MoveToBag(currentItemInstance, selectedQuantity))
            {
                Debug.Log($"Moved {selectedQuantity}x {currentItem.itemName} to bag");
                ClosePopup();
            }
        }
        else if (buttonText == "Store")
        {
            // From bag or pocket: Move to storage
            if (InventoryManager.Instance.Inventory.MoveToStorage(currentItemInstance, selectedQuantity))
            {
                Debug.Log($"Stored {selectedQuantity}x {currentItem.itemName}");
                ClosePopup();
            }
        }
    }

    private void ClosePopup()
    {
        popupContainer.SetActive(false);
        currentItem = null;
        currentItemInstance = null;
    }

    #endregion
}