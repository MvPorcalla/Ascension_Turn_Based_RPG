// ════════════════════════════════════════════
// Assets\Scripts\Modules\SharedUI\Popups\GearPopup.cs
// Gear item popup UI logic
// ════════════════════════════════════════════

using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.Equipment.Manager;
using Ascension.Inventory.Manager;

namespace Ascension.SharedUI.Popups
{
    public class GearPopup : MonoBehaviour
    {
        #region Singleton
        public static GearPopup Instance { get; private set; }
        #endregion

        #region Serialized Fields
        [Header("Configuration")]
        [SerializeField] private GearPopupConfig config;

        [Header("Popup Container")]
        [SerializeField] private GameObject popupContainer;

        [Header("Header")]
        [SerializeField] private TMP_Text itemName;

        [Header("Item Display")]
        [SerializeField] private Image itemImage;

        [Header("Stat Panel")]
        [SerializeField] private Transform statPanelContent;
        [SerializeField] private GameObject itemBonusStatsPrefab;

        [Header("Effect Panel")]
        [SerializeField] private Transform effectPanelContent;
        [SerializeField] private GameObject itemEffectPrefab;

        [Header("Description Panel")]
        [SerializeField] private TMP_Text descriptionText;

        [Header("Action Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button equipToggleButton;
        [SerializeField] private TMP_Text equipToggleButtonText;
        [SerializeField] private Button moveButton;
        [SerializeField] private TMP_Text moveButtonText;

        [Header("Error Display")]
        [SerializeField] private TMP_Text errorText;
        #endregion

        #region Private Fields - Error Messages (Hardcoded)
        
        // User-facing error messages (hardcoded for simplicity)
        private const string ERROR_INVENTORY_FULL = "Cannot unequip: Inventory full!";
        private const string ERROR_ALREADY_EQUIPPED = "Item is already equipped!";
        private const string ERROR_CANNOT_EQUIP = "Cannot equip this item right now.";
        private const string ERROR_NO_ITEM = "No item selected";
        private const string ERROR_SYSTEM_UNAVAILABLE = "Equipment system unavailable";
        private const string ERROR_CANNOT_MOVE_EQUIPPED = "Cannot move equipped item";

        private ItemBaseSO _currentItem;
        private ItemInstance _currentItemInstance;
        private Coroutine _errorCoroutine;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            InitializeSingleton();
            ValidateConfig();
            SetupButtons();
            Hide();
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void ValidateConfig()
        {
            if (config == null)
            {
                Debug.LogError("[GearPopup] GearPopupConfig not assigned! Please assign in Inspector.");
            }
        }

        private void SetupButtons()
        {
            closeButton?.onClick.AddListener(Hide);
            equipToggleButton?.onClick.AddListener(OnEquipToggleClicked);
            moveButton?.onClick.AddListener(OnMoveButtonClicked);
        }
        #endregion

        #region Public Methods

        public void Show(ItemBaseSO itemData, ItemInstance itemInstance)
        {
            if (itemData == null)
            {
                Debug.LogError("[GearPopup] Cannot show popup - itemData is null");
                return;
            }

            _currentItem = itemData;
            _currentItemInstance = itemInstance;

            UpdateDisplay();
            popupContainer?.SetActive(true);
        }

        public void ShowEquipped(ItemBaseSO itemData, string itemId)
        {
            if (itemData == null)
            {
                Debug.LogError("[GearPopup] Cannot show popup - itemData is null");
                return;
            }

            _currentItem = itemData;

            var inventory = InventoryManager.Instance?.Inventory;
            if (inventory != null)
            {
                _currentItemInstance = inventory.allItems
                    .FirstOrDefault(i => i.itemID == itemId && i.location == ItemLocation.Equipped);
            }

            UpdateDisplay();
            popupContainer?.SetActive(true);
        }

        public void Hide()
        {
            popupContainer?.SetActive(false);
            
            if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }
            
            if (_errorCoroutine != null)
            {
                StopCoroutine(_errorCoroutine);
                _errorCoroutine = null;
            }
            
            _currentItem = null;
            _currentItemInstance = null;
        }

        #endregion

        #region Private Methods - Display

        private void UpdateDisplay()
        {
            if (_currentItem == null) return;

            UpdateHeader();
            UpdateIcon();
            UpdateDescription();
            UpdateStats();
            UpdateButtons();
        }

        private void UpdateHeader()
        {
            if (itemName != null)
                itemName.text = _currentItem.ItemName;
        }

        private void UpdateIcon()
        {
            if (itemImage != null)
            {
                if (_currentItem.Icon != null)
                {
                    itemImage.sprite = _currentItem.Icon;
                    itemImage.enabled = true;
                }
                else
                {
                    itemImage.sprite = null;
                    itemImage.enabled = false;
                }
            }
        }

        private void UpdateDescription()
        {
            if (descriptionText != null)
                descriptionText.text = _currentItem.Description;
        }

        private void UpdateStats()
        {
            if (_currentItem is WeaponSO weapon)
            {
                DisplayWeaponStats(weapon);
            }
            else if (_currentItem is GearSO gear)
            {
                DisplayGearStats(gear);
            }
            else
            {
                ClearStats();
            }
        }

        // ✅ Uses config for button labels only
        private void UpdateButtons()
        {
            if (config == null) return;

            bool isEquipped = IsCurrentItemEquipped();
            ItemLocation? currentLocation = _currentItemInstance?.location;

            // Update equip/unequip button (uses config)
            if (equipToggleButton != null)
            {
                equipToggleButton.gameObject.SetActive(true);

                if (equipToggleButtonText != null)
                {
                    equipToggleButtonText.text = isEquipped 
                        ? config.buttonUnequip 
                        : config.buttonEquip;
                }
            }

            // Update move button (uses config)
            if (moveButton != null)
            {
                moveButton.gameObject.SetActive(!isEquipped);

                if (moveButtonText != null)
                {
                    if (currentLocation == ItemLocation.Bag)
                        moveButtonText.text = config.buttonStore;
                    else if (currentLocation == ItemLocation.Storage)
                        moveButtonText.text = config.buttonTake;
                    else
                        moveButtonText.text = config.buttonMove;
                }
            }
        }

        private bool IsCurrentItemEquipped()
        {
            if (_currentItem == null) return false;
            
            var equipMgr = EquipmentManager.Instance;
            return equipMgr != null && equipMgr.IsItemEquipped(_currentItem.ItemID);
        }

        private void DisplayWeaponStats(WeaponSO weapon)
        {
            ClearStats();

            if (weapon.BonusAD > 0) AddStatLine("Attack Damage", $"+{weapon.BonusAD}", "#ff6b6b");
            if (weapon.BonusAP > 0) AddStatLine("Ability Power", $"+{weapon.BonusAP}", "#4ecdc4");
            if (weapon.BonusHP > 0) AddStatLine("Health", $"+{weapon.BonusHP}", "#00ff15ff");
            if (weapon.BonusDefense > 0) AddStatLine("Defense", $"+{weapon.BonusDefense}", "#ff7b00ff");
            if (weapon.BonusAttackSpeed > 0) AddStatLine("Attack Speed", $"+{weapon.BonusAttackSpeed}");
            if (weapon.BonusCritRate > 0) AddStatLine("Crit Rate", $"+{weapon.BonusCritRate}%");
            if (weapon.BonusCritDamage > 0) AddStatLine("Crit Damage", $"+{weapon.BonusCritDamage}%");
            if (weapon.BonusEvasion > 0) AddStatLine("Evasion", $"+{weapon.BonusEvasion}%");
            if (weapon.BonusTenacity > 0) AddStatLine("Tenacity", $"+{weapon.BonusTenacity}%");
            if (weapon.BonusLethality > 0) AddStatLine("Lethality", $"+{weapon.BonusLethality}");
            if (weapon.BonusPenetration > 0) AddStatLine("Penetration", $"+{weapon.BonusPenetration}%");
            if (weapon.BonusLifesteal > 0) AddStatLine("Lifesteal", $"+{weapon.BonusLifesteal}%");

            if (weapon.DefaultWeaponSkill != null)
                AddEffectLine($"[Skill]: {weapon.DefaultWeaponSkill.AbilityName}");
        }

        private void DisplayGearStats(GearSO gear)
        {
            ClearStats();

            if (gear.BonusHP > 0) AddStatLine("HP", $"+{gear.BonusHP}", "#00ff15ff");
            if (gear.BonusDefense > 0) AddStatLine("Defense", $"+{gear.BonusDefense}", "#ff7b00ff");
            if (gear.BonusAD > 0) AddStatLine("Attack Damage", $"+{gear.BonusAD}", "#ff6b6b");
            if (gear.BonusAP > 0) AddStatLine("Ability Power", $"+{gear.BonusAP}", "#4ecdc4");
            if (gear.BonusAttackSpeed > 0) AddStatLine("Attack Speed", $"+{gear.BonusAttackSpeed}");
            if (gear.BonusCritRate > 0) AddStatLine("Crit Rate", $"+{gear.BonusCritRate}%");
            if (gear.BonusCritDamage > 0) AddStatLine("Crit Damage", $"+{gear.BonusCritDamage}%");
            if (gear.BonusEvasion > 0) AddStatLine("Evasion", $"+{gear.BonusEvasion}%");
            if (gear.BonusTenacity > 0) AddStatLine("Tenacity", $"+{gear.BonusTenacity}%");
            if (gear.BonusLethality > 0) AddStatLine("Lethality", $"+{gear.BonusLethality}");
            if (gear.BonusPenetration > 0) AddStatLine("Penetration", $"+{gear.BonusPenetration}%");
            if (gear.BonusLifesteal > 0) AddStatLine("Lifesteal", $"+{gear.BonusLifesteal}%");
        }

        private void ClearStats()
        {
            foreach (Transform child in statPanelContent)
                Destroy(child.gameObject);

            foreach (Transform child in effectPanelContent)
                Destroy(child.gameObject);
        }

        private void AddStatLine(string label, string value, string hexColor = null)
        {
            if (itemBonusStatsPrefab == null) return;

            GameObject statObj = Instantiate(itemBonusStatsPrefab, statPanelContent);
            
            TMP_Text labelText = statObj.transform.Find("Text_Label")?.GetComponent<TMP_Text>();
            TMP_Text valueText = statObj.transform.Find("Text_value")?.GetComponent<TMP_Text>();
            
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
                valueText.text = !string.IsNullOrEmpty(hexColor) 
                    ? $"<color={hexColor}>{value}</color>" 
                    : value;
            }
        }

        private void AddEffectLine(string effectText)
        {
            if (itemEffectPrefab == null) return;

            GameObject effectObj = Instantiate(itemEffectPrefab, effectPanelContent);
            TMP_Text text = effectObj.transform.Find("Text")?.GetComponent<TMP_Text>() 
                ?? effectObj.GetComponentInChildren<TMP_Text>();
            
            if (text != null) text.text = effectText;
        }

        #endregion

        #region Event Handlers

        // ✅ Uses hardcoded error messages
        private void OnEquipToggleClicked()
        {
            if (config == null) return;

            if (_currentItem == null)
            {
                ShowError(ERROR_NO_ITEM);
                return;
            }

            var equipMgr = EquipmentManager.Instance;
            if (equipMgr == null)
            {
                ShowError(ERROR_SYSTEM_UNAVAILABLE);
                return;
            }

            bool isEquipped = IsCurrentItemEquipped();
            bool success;

            if (isEquipped)
            {
                // UNEQUIP
                success = equipMgr.UnequipItem(_currentItem.ItemID);

                if (success)
                {
                    Debug.Log($"[GearPopup] Unequipped {_currentItem.ItemName}");
                    RefreshCurrentItemInstance();
                    Hide();
                }
                else
                {
                    ShowError(ERROR_INVENTORY_FULL);
                    Debug.LogWarning($"[GearPopup] Failed to unequip {_currentItem.ItemName}");
                }
            }
            else
            {
                // EQUIP
                success = equipMgr.EquipItem(_currentItem.ItemID);

                if (success)
                {
                    Debug.Log($"[GearPopup] Equipped {_currentItem.ItemName}");
                    Hide();
                }
                else
                {
                    var location = _currentItemInstance?.location;
                    
                    if (location == ItemLocation.Equipped)
                    {
                        ShowError(ERROR_ALREADY_EQUIPPED);
                    }
                    else
                    {
                        ShowError(ERROR_CANNOT_EQUIP);
                    }
                    
                    Debug.LogWarning($"[GearPopup] Failed to equip {_currentItem.ItemName}");
                }
            }
        }

        private void RefreshCurrentItemInstance()
        {
            if (_currentItem == null) return;

            var inventory = InventoryManager.Instance?.Inventory;
            if (inventory == null) return;

            _currentItemInstance = inventory.allItems
                .FirstOrDefault(i => i.itemID == _currentItem.ItemID && 
                    (i.location == ItemLocation.Bag || 
                    i.location == ItemLocation.Storage ||
                    i.location == ItemLocation.Equipped));

            UpdateButtons();
        }

        private void OnMoveButtonClicked()
        {
            if (config == null) return;

            if (_currentItemInstance == null || _currentItem == null)
            {
                Debug.LogError("[GearPopup] Cannot move - no item instance");
                return;
            }

            var inventory = InventoryManager.Instance?.Inventory;
            var database = InventoryManager.Instance?.Database;

            if (inventory == null || database == null)
            {
                ShowError(ERROR_SYSTEM_UNAVAILABLE);
                return;
            }

            Inventory.Data.InventoryResult result;

            if (_currentItemInstance.location == ItemLocation.Bag)
            {
                result = inventory.MoveToStorage(_currentItemInstance, _currentItemInstance.quantity, database);
            }
            else if (_currentItemInstance.location == ItemLocation.Storage)
            {
                result = inventory.MoveToBag(_currentItemInstance, _currentItemInstance.quantity, database);
            }
            else
            {
                ShowError(ERROR_CANNOT_MOVE_EQUIPPED);
                return;
            }

            if (result.Success)
            {
                Debug.Log($"[GearPopup] {result.Message}");
                Hide();
            }
            else
            {
                // Use error from inventory system
                ShowError(result.Message);
            }
        }

        // ✅ Uses config for duration/color only
        private void ShowError(string message)
        {
            if (config == null || errorText == null) return;

            Debug.LogWarning($"[GearPopup] Error: {message}");

            if (_errorCoroutine != null)
            {
                StopCoroutine(_errorCoroutine);
            }

            errorText.text = message;
            errorText.color = config.errorTextColor;
            errorText.gameObject.SetActive(true);

            _errorCoroutine = StartCoroutine(HideErrorAfterDelay());
        }

        private System.Collections.IEnumerator HideErrorAfterDelay()
        {
            if (config == null) yield break;

            yield return new WaitForSeconds(config.errorDisplayDuration);
            
            if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }
            
            _errorCoroutine = null;
        }

        #endregion
    }
}