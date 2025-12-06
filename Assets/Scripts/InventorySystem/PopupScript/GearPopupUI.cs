// ──────────────────────────────────────────────────
// GearPopupUI.cs
// UI popup for displaying gear item details and actions
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Data.SO;
using Ascension.Systems;

public class GearPopupUI : MonoBehaviour
{
    [Header("Popup Container")]
    [SerializeField] private GameObject popupContainer;

    [Header("Header")]
    [SerializeField] private TMP_Text itemName;

    [Header("Item Display")]
    [SerializeField] private Image itemImage;

    [Header("Stat Panel")]
    [SerializeField] private Transform statPanelContent;
    [SerializeField] private GameObject itemBonusStatsPrefab; // Has: Text_Label, Text_value

    [Header("Effect Panel")]
    [SerializeField] private Transform effectPanelContent;
    [SerializeField] private GameObject itemEffectPrefab; // Has: Text

    [Header("Description Panel")]
    [SerializeField] private TMP_Text descriptionText;

    [Header("Action Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button equipButton;

    private ItemBaseSO currentItem;
    private ItemInstance currentItemInstance;
    private ItemLocation currentLocation;

    private void Start()
    {
        // Setup button listeners
        closeButton.onClick.AddListener(ClosePopup);
        equipButton.onClick.AddListener(OnEquipButtonClicked);

        popupContainer.SetActive(false);
    }

    public void ShowGear(ItemBaseSO item, ItemInstance itemInstance, ItemLocation fromLocation)
    {
        currentItem = item;
        currentItemInstance = itemInstance;
        currentLocation = fromLocation;

        popupContainer.SetActive(true);

        // Setup header
        itemName.text = item.ItemName;

        // Setup icon
        if (itemImage != null)
        {
            if (item.Icon != null)
            {
                itemImage.sprite = item.Icon;
                itemImage.enabled = true;
            }
            else
            {
                itemImage.sprite = null;
                itemImage.enabled = false;
            }
        }

        // Setup description
        if (descriptionText != null)
            descriptionText.text = item.Description;

        // Display stats based on item type
        if (item is WeaponSO weapon)
        {
            DisplayWeaponStats(weapon);
        }
        else
        {
            ClearStats();
        }

        // Setup equip button
        SetupEquipButton(itemInstance, fromLocation);
    }

    private void DisplayWeaponStats(WeaponSO weapon)
    {
        ClearStats();

        // PRIMARY STATS - Displayed first with emphasis
        if (weapon.BonusAD > 0) AddStatLine("Attack Damage", $"+{weapon.BonusAD}", "#ff6b6b");
        if (weapon.BonusAP > 0) AddStatLine("Ability Power", $"+{weapon.BonusAP}", "#4ecdc4");

        // DEFENSIVE STATS
        if (weapon.BonusHP > 0) AddStatLine("Health", $"+{weapon.BonusHP}");
        if (weapon.BonusDefense > 0) AddStatLine("Defense", $"+{weapon.BonusDefense}");

        // OFFENSIVE STATS
        if (weapon.BonusAttackSpeed > 0) AddStatLine("Attack Speed", $"+{weapon.BonusAttackSpeed}");
        if (weapon.BonusCritRate > 0) AddStatLine("Crit Rate", $"+{weapon.BonusCritRate}%");
        if (weapon.BonusCritDamage > 0) AddStatLine("Crit Damage", $"+{weapon.BonusCritDamage}%");

        // UTILITY STATS
        if (weapon.BonusEvasion > 0) AddStatLine("Evasion", $"+{weapon.BonusEvasion}%");
        if (weapon.BonusTenacity > 0) AddStatLine("Tenacity", $"+{weapon.BonusTenacity}%");
        if (weapon.BonusLethality > 0) AddStatLine("Lethality", $"+{weapon.BonusLethality}");
        if (weapon.BonusPenetration > 0) AddStatLine("Penetration", $"+{weapon.BonusPenetration}%");
        if (weapon.BonusLifesteal > 0) AddStatLine("Lifesteal", $"+{weapon.BonusLifesteal}%");

        // Display weapon skill if available
        if (weapon.DefaultWeaponSkill != null)
        {
            AddEffectLine($"[Skill] : {weapon.DefaultWeaponSkill.AbilityName}");
        }
    }

    private void ClearStats()
    {
        // Clear stat panel
        foreach (Transform child in statPanelContent)
        {
            Destroy(child.gameObject);
        }

        // Clear effect panel
        foreach (Transform child in effectPanelContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void AddStatLine(string label, string value, string hexColor = null)
    {
        if (itemBonusStatsPrefab == null)
        {
            Debug.LogWarning($"ItemBonusStatsPrefab not assigned! {label}: {value}");
            return;
        }

        GameObject statObj = Instantiate(itemBonusStatsPrefab, statPanelContent);
        
        TMP_Text labelText = statObj.transform.Find("Text_Label")?.GetComponent<TMP_Text>();
        TMP_Text valueText = statObj.transform.Find("Text_value")?.GetComponent<TMP_Text>();
        
        // Fallback: Get all TMP_Text components if specific finds fail
        if (labelText == null || valueText == null)
        {
            TMP_Text[] texts = statObj.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2)
            {
                labelText = texts[0];
                valueText = texts[1];
            }
        }
        
        if (labelText != null) labelText.text = label;
        if (valueText != null)
        {
            // Apply color if provided
            if (!string.IsNullOrEmpty(hexColor))
            {
                valueText.text = $"<color={hexColor}>{value}</color>";
            }
            else
            {
                valueText.text = value;
            }
        }
    }

    private void AddEffectLine(string effectText)
    {
        if (itemEffectPrefab == null)
        {
            Debug.LogWarning($"ItemEffectPrefab not assigned! {effectText}");
            return;
        }

        GameObject effectObj = Instantiate(itemEffectPrefab, effectPanelContent);
        
        TMP_Text text = effectObj.transform.Find("Text")?.GetComponent<TMP_Text>();
        
        // Fallback: Get first TMP_Text component
        if (text == null)
        {
            text = effectObj.GetComponentInChildren<TMP_Text>();
        }
        
        if (text != null) text.text = effectText;
    }

    private void SetupEquipButton(ItemInstance itemInstance, ItemLocation fromLocation)
    {
        var buttonText = equipButton.GetComponentInChildren<TMP_Text>();

        // In StorageRoomPanel: No equipping, only storage management
        switch (fromLocation)
        {
            case ItemLocation.Storage:
                // From storage: Move to bag
                if (buttonText != null) buttonText.text = "Add to Bag";
                break;
            case ItemLocation.Pocket:
                // Gear shouldn't be in pocket, but handle it anyway
                if (buttonText != null) buttonText.text = "Store";
                break;
            case ItemLocation.Bag:
                // From bag: Store it
                if (buttonText != null) buttonText.text = "Store";
                break;
        }

    }

    #region Action Handlers

    private void OnEquipButtonClicked()
    {
        var buttonText = equipButton.GetComponentInChildren<TMP_Text>()?.text;

        if (buttonText == "Add to Bag")
        {
            // From storage: Move to bag
            if (InventoryManager.Instance.Inventory.MoveToBag(currentItemInstance, 1))
            {
                Debug.Log($"Moved {currentItem.ItemName} to bag");
                ClosePopup();
            }
        }
        else if (buttonText == "Store")
        {
            // From bag or pocket: Move to storage
            if (InventoryManager.Instance.Inventory.MoveToStorage(currentItemInstance, 1))
            {
                Debug.Log($"Stored {currentItem.ItemName}");
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