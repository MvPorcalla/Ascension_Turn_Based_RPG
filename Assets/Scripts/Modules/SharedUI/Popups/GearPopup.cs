// ════════════════════════════════════════════
// Assets\Scripts\Modules\SharedUI\Popups\GearPopup.cs
// ✅ REFACTORED: Uses PopupContext + PopupActionHandler
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
        #region Serialized Fields
        [Header("Configuration")]
        [SerializeField] private PopupConfig config;

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

        // [Header("Error Display")]
        // [SerializeField] private TMP_Text errorText;
        #endregion

        #region Private Fields
        private const string ERROR_NO_ITEM = "No item selected";
        private const string ERROR_CANNOT_MOVE_EQUIPPED = "Cannot move equipped item";

        private ItemBaseSO _currentItem;
        private ItemInstance _currentItemInstance;
        private PopupContext _currentContext; // ✅ NEW: Store context

        // private Coroutine _errorCoroutine;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            ValidateConfig();
            SetupButtons();
            Hide();
        }

        private void ValidateConfig()
        {
            if (config == null)
            {
                Debug.LogError("[GearPopup] PopupConfig not assigned!");
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

        /// <summary>
        /// ✅ NEW: Show with context
        /// </summary>
        public void Show(ItemBaseSO itemData, ItemInstance itemInstance, PopupContext context)
        {
            if (itemData == null)
            {
                Debug.LogError("[GearPopup] Cannot show popup - itemData is null");
                return;
            }

            _currentItem = itemData;
            _currentItemInstance = itemInstance;
            _currentContext = context; // ✅ Store context

            UpdateDisplay();
            popupContainer?.SetActive(true);
        }

        public void Hide()
        {
            popupContainer?.SetActive(false);
            
            // if (errorText != null)
            // {
            //     errorText.gameObject.SetActive(false);
            // }
            
            // if (_errorCoroutine != null)
            // {
            //     StopCoroutine(_errorCoroutine);
            //     _errorCoroutine = null;
            // }
            
            _currentItem = null;
            _currentItemInstance = null;
            _currentContext = null; // ✅ Clear context
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

        /// <summary>
        /// Uses context to determine button visibility and text
        /// </summary>
        private void UpdateButtons()
        {
            if (config == null || _currentContext == null) return;

            // Equip/Unequip button
            if (equipToggleButton != null)
            {
                // ✅ FIX: Show button for equipped items OR when CanEquip is true
                bool isEquipped = _currentContext.SourceLocation == ItemLocation.Equipped;
                equipToggleButton.gameObject.SetActive(isEquipped || _currentContext.CanEquip);

                if (equipToggleButtonText != null)
                {
                    equipToggleButtonText.text = isEquipped 
                        ? config.buttonUnequip 
                        : config.buttonEquip;
                }
            }

            // Move button
            if (moveButton != null)
            {
                moveButton.gameObject.SetActive(_currentContext.CanMove);

                if (moveButtonText != null)
                {
                    moveButtonText.text = config.GetMoveButtonText(_currentContext.SourceLocation);
                }
            }
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
        /// ✅ REFACTORED: Uses PopupActionHandler instead of calling EquipmentManager directly
        /// </summary>
        private void OnEquipToggleClicked()
        {
            if (_currentItem == null || _currentItemInstance == null)
            {
                ShowError(ERROR_NO_ITEM);
                return;
            }

            bool isEquipped = _currentContext.SourceLocation == ItemLocation.Equipped;

            if (isEquipped)
            {
                // ✅ Delegate to PopupActionHandler
                PopupActionHandler.Instance.UnequipItem(_currentItem, _currentItemInstance);
            }
            else
            {
                // ✅ Delegate to PopupActionHandler
                PopupActionHandler.Instance.EquipItem(_currentItem, _currentItemInstance);
            }
        }

        /// <summary>
        /// ✅ REFACTORED: Uses PopupActionHandler instead of calling InventoryManager directly
        /// </summary>
        private void OnMoveButtonClicked()
        {
            if (_currentItemInstance == null || _currentItem == null)
            {
                Debug.LogError("[GearPopup] Cannot move - no item instance");
                return;
            }

            if (!_currentContext.CanMove)
            {
                ShowError(ERROR_CANNOT_MOVE_EQUIPPED);
                return;
            }

            // Determine target location
            ItemLocation targetLocation = _currentContext.SourceLocation == ItemLocation.Bag
                ? ItemLocation.Storage
                : ItemLocation.Bag;

            // ✅ Delegate to PopupActionHandler
            PopupActionHandler.Instance.MoveItem(
                _currentItemInstance, 
                _currentItemInstance.quantity, 
                targetLocation
            );
        }

        private void ShowError(string message)
        {
            // if (config == null || errorText == null) return;

            Debug.LogWarning($"[GearPopup] Error: {message}");

            // if (_errorCoroutine != null)
            // {
            //     StopCoroutine(_errorCoroutine);
            // }

            // errorText.text = message;
            // errorText.color = config.errorTextColor;
            // errorText.gameObject.SetActive(true);

            // _errorCoroutine = StartCoroutine(HideErrorAfterDelay());
        }

        // private System.Collections.IEnumerator HideErrorAfterDelay()
        // {
        //     if (config == null) yield break;

        //     yield return new WaitForSeconds(config.errorDisplayDuration);
            
        //     if (errorText != null)
        //     {
        //         errorText.gameObject.SetActive(false);
        //     }
            
        //     _errorCoroutine = null;
        // }

        #endregion
    }
}