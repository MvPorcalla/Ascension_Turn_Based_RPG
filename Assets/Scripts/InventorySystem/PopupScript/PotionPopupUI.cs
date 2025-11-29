// -------------------------------
// PotionPopupUI.cs - Dedicated potion popup with buff display
// -------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PotionPopupUI : MonoBehaviour
{
    [Header("Popup Container")]
    [SerializeField] private GameObject popupContainer;

    [Header("Header")]
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private Button backButton;

    [Header("Potion Display")]
    [SerializeField] private Image potionIcon;
    [SerializeField] private TMP_Text potionTypeLabel;
    [SerializeField] private TMP_Text potionTypeValue;

    [Header("Buff Effects")]
    [SerializeField] private Transform buffEffectContent;
    [SerializeField] private GameObject buffTypePrefab;

    [Header("Description")]
    [SerializeField] private TMP_Text potionDescription;

    [Header("Quantity Controls")]
    [SerializeField] private TMP_Text quantityValue;
    [SerializeField] private Slider quantitySlider;
    [SerializeField] private Button plus5Button;
    [SerializeField] private Button plus1Button;
    [SerializeField] private Button minus1Button;
    [SerializeField] private Button minus5Button;

    [Header("Action Buttons")]
    [SerializeField] private Button useButton;
    [SerializeField] private Button addToPocketButton;
    [SerializeField] private Button addToBagButton;

    private PotionSO currentPotion;
    private ItemInstance currentItem;
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
        useButton.onClick.AddListener(OnUseClicked);
        addToPocketButton.onClick.AddListener(OnAddToPocketClicked);
        addToBagButton.onClick.AddListener(OnAddToBagClicked);

        popupContainer.SetActive(false);
    }

    public void ShowPotion(PotionSO potion, ItemInstance item, ItemLocation fromLocation)
    {
        currentPotion = potion;
        currentItem = item;
        currentLocation = fromLocation;
        selectedQuantity = 1;

        popupContainer.SetActive(true);

        // Setup header
        itemName.text = potion.potionName;

        // Setup icon
        if (potionIcon != null)
        {
            if (potion.icon != null)
            {
                potionIcon.sprite = potion.icon;
                potionIcon.enabled = true;
            }
            else
            {
                potionIcon.sprite = null;
                potionIcon.enabled = false;
            }
        }

        // Setup potion type
        DisplayPotionType(potion);

        // Setup buff effects
        DisplayBuffEffects(potion);

        // Setup description
        if (potionDescription != null)
            potionDescription.text = potion.description;

        // Setup quantity controls
        SetupQuantityControls(item.quantity);

        // Setup action buttons visibility
        SetupActionButtons(fromLocation, item);
    }

    private void DisplayPotionType(PotionSO potion)
    {
        if (potionTypeLabel != null)
            potionTypeLabel.text = "Potion Type";

        if (potionTypeValue != null)
        {
            potionTypeValue.text = potion.potionType switch
            {
                PotionType.HealthPotion => "Health Potion",
                PotionType.ManaPotion => "Mana Potion",
                PotionType.BuffPotion => "Buff Potion",
                PotionType.Elixir => "Elixir",
                PotionType.Antidote => "Antidote",
                PotionType.Utility => "Utility",
                _ => "Unknown"
            };
        }
    }

    private void DisplayBuffEffects(PotionSO potion)
    {
        // Clear existing buffs
        foreach (Transform child in buffEffectContent)
        {
            Destroy(child.gameObject);
        }

        // Display based on potion type
        switch (potion.potionType)
        {
            case PotionType.HealthPotion:
                if (potion.duration > 0)
                    AddBuffLine("HP Restore", $"+{potion.restoreAmount}", $"{potion.duration}s (over time)");
                else
                    AddBuffLine("HP Restore", $"+{potion.restoreAmount}", "Instant");
                break;

            case PotionType.ManaPotion:
                if (potion.duration > 0)
                    AddBuffLine("Mana Restore", $"+{potion.restoreAmount}", $"{potion.duration}s (over time)");
                else
                    AddBuffLine("Mana Restore", $"+{potion.restoreAmount}", "Instant");
                break;

            case PotionType.BuffPotion:
                DisplayBuffPotionEffects(potion);
                break;

            case PotionType.Elixir:
                AddBuffLine("HP & Mana", $"+{potion.restoreAmount}", "Instant");
                if (potion.grantsBuff)
                {
                    DisplayBuffPotionEffects(potion);
                }
                break;

            case PotionType.Antidote:
                AddBuffLine("Effect", "Cures Poison/Debuffs", "Instant");
                break;

            case PotionType.Utility:
                AddBuffLine("Utility Effect", "Special Effect", "Varies");
                break;
        }
    }

    private void DisplayBuffPotionEffects(PotionSO potion)
    {
        if (!potion.grantsBuff) return;

        // Convert buffDuration to turns (assuming 1 turn = X seconds, adjust as needed)
        // For now, showing duration in turns format
        string durationText = $"{Mathf.RoundToInt(potion.buffDuration)} turns";

        // Display buff based on type
        string buffValueText = GetBuffValueText(potion.buffType, potion.buffValue);
        string buffLabel = GetBuffLabel(potion.buffType);

        AddBuffLine(buffLabel, buffValueText, durationText);
    }

    private string GetBuffLabel(BuffType buffType)
    {
        return buffType switch
        {
            BuffType.AttackDamage => "Attack Damage",
            BuffType.AbilityPower => "Ability Power",
            BuffType.Defense => "Defense",
            BuffType.Speed => "Movement Speed",
            BuffType.CritRate => "Crit Rate",
            BuffType.AttackSpeed => "Attack Speed",
            BuffType.Regeneration => "Regeneration",
            BuffType.Resistance => "Resistance",
            BuffType.Invisibility => "Invisibility",
            BuffType.Invulnerability => "Invulnerability",
            _ => "Unknown Buff"
        };
    }

    private string GetBuffValueText(BuffType buffType, float value)
    {
        // Percentage-based buffs
        if (buffType == BuffType.Speed || 
            buffType == BuffType.CritRate || 
            buffType == BuffType.AttackSpeed || 
            buffType == BuffType.Resistance)
        {
            return $"+{value}%";
        }
        
        // Boolean buffs (no value needed)
        if (buffType == BuffType.Invisibility || 
            buffType == BuffType.Invulnerability)
        {
            return "Active";
        }

        // Flat value buffs
        return $"+{value}";
    }

    private void AddBuffLine(string buffType, string value, string duration)
    {
        if (buffTypePrefab == null)
        {
            Debug.LogWarning($"BuffTypePrefab not assigned! {buffType}: {value} ({duration})");
            return;
        }

        GameObject buffObj = Instantiate(buffTypePrefab, buffEffectContent);

        // Find text components - try multiple naming conventions
        TMP_Text labelText = buffObj.transform.Find("TextLabel")?.GetComponent<TMP_Text>();
        TMP_Text valueText = buffObj.transform.Find("textValue")?.GetComponent<TMP_Text>();
        TMP_Text durationText = buffObj.transform.Find("textDuration")?.GetComponent<TMP_Text>();

        // Alternative: Get all TMP_Text components if specific finds fail
        if (labelText == null || valueText == null || durationText == null)
        {
            TMP_Text[] texts = buffObj.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 3)
            {
                labelText = texts[0];
                valueText = texts[1];
                durationText = texts[2];
            }
        }

        // Set the text values
        if (labelText != null) labelText.text = buffType;
        if (valueText != null) valueText.text = value;
        if (durationText != null) durationText.text = duration;
    }

    private void SetupQuantityControls(int maxQuantity)
    {
        // Setup slider
        quantitySlider.minValue = 1;
        quantitySlider.maxValue = maxQuantity;
        quantitySlider.value = 1;

        UpdateQuantityDisplay();
    }

    private void SetupActionButtons(ItemLocation fromLocation, ItemInstance item)
    {
        // Use button - always show for consumables
        useButton.gameObject.SetActive(true);

        // Setup buttons based on current location
        switch (fromLocation)
        {
            case ItemLocation.Storage:
                // From storage: [Use Item] [Add to Pocket] [Add to Bag]
                addToPocketButton.gameObject.SetActive(true);
                addToBagButton.gameObject.SetActive(true);
                
                SetButtonText(addToPocketButton, "Add to Pocket");
                SetButtonText(addToBagButton, "Add to Bag");
                break;

            case ItemLocation.Pocket:
                // From pocket: [Use Item] [Add to Bag] [Store]
                addToPocketButton.gameObject.SetActive(false);
                addToBagButton.gameObject.SetActive(true);
                
                // Bag button becomes "Add to Bag" (middle position)
                SetButtonText(addToBagButton, "Add to Bag");
                
                // We need a third button for "Store" - use the pocket button but relabel
                addToPocketButton.gameObject.SetActive(true);
                SetButtonText(addToPocketButton, "Store");
                break;

            case ItemLocation.Bag:
                // From bag: [Use Item] [Add to Pocket] [Store]
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
            currentItem.quantity
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

    private void OnUseClicked()
    {
        // TODO: Implement potion usage system
        Debug.Log($"Using {selectedQuantity}x {currentPotion.potionName}");
        
        // Remove used quantity from inventory
        InventoryManager.Instance.Inventory.RemoveItem(currentItem, selectedQuantity);
        
        ClosePopup();
    }

    private void OnAddToPocketClicked()
    {
        var buttonText = addToPocketButton.GetComponentInChildren<TMP_Text>()?.text;
        
        if (buttonText == "Add to Pocket")
        {
            // From storage or bag: Move to pocket
            if (InventoryManager.Instance.Inventory.MoveToPocket(currentItem, selectedQuantity))
            {
                Debug.Log($"Moved {selectedQuantity}x {currentPotion.potionName} to pocket");
                ClosePopup();
            }
        }
        else if (buttonText == "Store")
        {
            // From pocket: Move to storage
            if (InventoryManager.Instance.Inventory.MoveToStorage(currentItem, selectedQuantity))
            {
                Debug.Log($"Stored {selectedQuantity}x {currentPotion.potionName}");
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
            if (InventoryManager.Instance.Inventory.MoveToBag(currentItem, selectedQuantity))
            {
                Debug.Log($"Moved {selectedQuantity}x {currentPotion.potionName} to bag");
                ClosePopup();
            }
        }
        else if (buttonText == "Store")
        {
            // From bag: Move to storage
            if (InventoryManager.Instance.Inventory.MoveToStorage(currentItem, selectedQuantity))
            {
                Debug.Log($"Stored {selectedQuantity}x {currentPotion.potionName}");
                ClosePopup();
            }
        }
    }

    private void ClosePopup()
    {
        popupContainer.SetActive(false);
        currentPotion = null;
        currentItem = null;
    }

    #endregion
}