// ════════════════════════════════════════════
// Assets\Scripts\Modules\SharedUI\Popups\GearPopup.cs
// ✅ THREE-BUTTON VERSION: [Close] [Equip/Unequip] [Move Storage/Bag]
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
        [SerializeField] private Button equipToggleButton; // ✅ Single toggle button
        [SerializeField] private TMP_Text equipToggleButtonText; // ✅ Text changes: "Equip" / "Unequip"
        [SerializeField] private Button moveButton;
        [SerializeField] private TMP_Text moveButtonText;
        #endregion

        #region Private Fields
        private ItemBaseSO _currentItem;
        private ItemInstance _currentItemInstance;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            InitializeSingleton();
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

        private void SetupButtons()
        {
            closeButton?.onClick.AddListener(Hide);
            equipToggleButton?.onClick.AddListener(OnEquipToggleClicked);
            moveButton?.onClick.AddListener(OnMoveButtonClicked);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Show popup for item (from bag or storage)
        /// </summary>
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

        /// <summary>
        /// Show popup for equipped item (click from equipment slot)
        /// </summary>
        public void ShowEquipped(ItemBaseSO itemData, string itemId)
        {
            if (itemData == null)
            {
                Debug.LogError("[GearPopup] Cannot show popup - itemData is null");
                return;
            }

            _currentItem = itemData;

            // Find equipped item instance
            var inventory = InventoryManager.Instance?.Inventory;
            if (inventory != null)
            {
                _currentItemInstance = inventory.allItems
                    .FirstOrDefault(i => i.itemID == itemId && i.location == ItemLocation.Equipped);
            }

            UpdateDisplay();
            popupContainer?.SetActive(true);
        }

        /// <summary>
        /// Hide popup and clear state
        /// </summary>
        public void Hide()
        {
            popupContainer?.SetActive(false);
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

        private void UpdateButtons()
        {
            bool isEquipped = IsCurrentItemEquipped();
            ItemLocation? currentLocation = _currentItemInstance?.location;

            // ✅ SINGLE TOGGLE BUTTON: Shows "Equip" or "Unequip" based on state
            if (equipToggleButton != null)
            {
                equipToggleButton.gameObject.SetActive(true); // Always visible

                // Update button text
                if (equipToggleButtonText != null)
                {
                    equipToggleButtonText.text = isEquipped ? "Unequip" : "Equip";
                }
            }

            // MOVE button (show only if NOT equipped)
            if (moveButton != null)
            {
                moveButton.gameObject.SetActive(!isEquipped);

                // Update button text based on current location
                if (moveButtonText != null)
                {
                    if (currentLocation == ItemLocation.Bag)
                        moveButtonText.text = "Store";
                    else if (currentLocation == ItemLocation.Storage)
                        moveButtonText.text = "Take";
                    else
                        moveButtonText.text = "Move";
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

        /// <summary>
        /// ✅ TOGGLE BUTTON: Equip if not equipped, Unequip if equipped
        /// </summary>
        private void OnEquipToggleClicked()
        {
            if (_currentItem == null)
            {
                Debug.LogError("[GearPopup] Cannot toggle equip - no item selected");
                return;
            }

            var equipMgr = EquipmentManager.Instance;
            if (equipMgr == null)
            {
                Debug.LogError("[GearPopup] EquipmentManager not available");
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
                    RefreshCurrentItemInstance();
                }

                Debug.Log(success 
                    ? $"[GearPopup] Unequipped {_currentItem.ItemName}" 
                    : $"[GearPopup] Failed to unequip {_currentItem.ItemName}");
            }
            else
            {
                // EQUIP
                success = equipMgr.EquipItem(_currentItem.ItemID);
                Debug.Log(success 
                    ? $"[GearPopup] Equipped {_currentItem.ItemName}" 
                    : $"[GearPopup] Failed to equip {_currentItem.ItemName}");
            }

            if (success)
            {
                Hide();
            }
        }

        /// <summary>
        /// ✅ NEW: Refresh the current item instance after location changes
        /// </summary>
        private void RefreshCurrentItemInstance()
        {
            if (_currentItem == null) return;

            var inventory = InventoryManager.Instance?.Inventory;
            if (inventory == null) return;

            // Find the item in its new location
            _currentItemInstance = inventory.allItems
                .FirstOrDefault(i => i.itemID == _currentItem.ItemID && 
                    (i.location == ItemLocation.Bag || 
                    i.location == ItemLocation.Storage ||
                    i.location == ItemLocation.Equipped));

            // Update button display
            UpdateButtons();
        }

        /// <summary>
        /// ✅ MOVE button: Move between Bag ↔ Storage
        /// </summary>
        private void OnMoveButtonClicked()
        {
            if (_currentItemInstance == null || _currentItem == null)
            {
                Debug.LogError("[GearPopup] Cannot move - no item instance");
                return;
            }

            var inventory = InventoryManager.Instance?.Inventory;
            var database = InventoryManager.Instance?.Database;

            if (inventory == null || database == null)
            {
                Debug.LogError("[GearPopup] Inventory system not available");
                return;
            }

            Inventory.Data.InventoryResult result;

            if (_currentItemInstance.location == ItemLocation.Bag)
            {
                // Move to Storage
                result = inventory.MoveToStorage(_currentItemInstance, _currentItemInstance.quantity, database);
            }
            else if (_currentItemInstance.location == ItemLocation.Storage)
            {
                // Move to Bag
                result = inventory.MoveToBag(_currentItemInstance, _currentItemInstance.quantity, database);
            }
            else
            {
                Debug.LogWarning("[GearPopup] Cannot move equipped item");
                return;
            }

            if (result.Success)
            {
                Debug.Log($"[GearPopup] {result.Message}");
                Hide();
            }
            else
            {
                Debug.LogError($"[GearPopup] Failed to move: {result.Message}");
            }
        }

        #endregion
    }
}