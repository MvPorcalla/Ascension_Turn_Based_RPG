// ════════════════════════════════════════════
// Assets\Scripts\Modules\SharedUI\Popups\GearPopup.cs
// Unified gear popup with context-based behavior
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;

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
        [SerializeField] private Button actionButton;
        #endregion

        #region Private Fields
        private ItemBaseSO _currentItem;
        private ItemInstance _currentItemInstance;
        private IGearPopupContext _currentContext;
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
            actionButton?.onClick.AddListener(OnActionButtonClicked);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Show popup with specific context
        /// </summary>
        public void Show(ItemBaseSO itemData, ItemInstance itemInstance, IGearPopupContext context)
        {
            if (itemData == null)
            {
                Debug.LogError("[GearPopup] Cannot show popup - itemData is null");
                return;
            }

            if (context == null)
            {
                Debug.LogError("[GearPopup] Cannot show popup - context is null");
                return;
            }

            _currentItem = itemData;
            _currentItemInstance = itemInstance;
            _currentContext = context;

            UpdateDisplay();
            popupContainer?.SetActive(true);

            Debug.Log($"[GearPopup] Showing: {itemData.ItemName} with context: {context.GetType().Name}");
        }

        /// <summary>
        /// Hide popup and clear state
        /// </summary>
        public void Hide()
        {
            popupContainer?.SetActive(false);
            _currentItem = null;
            _currentItemInstance = null;
            _currentContext = null;
        }
        #endregion

        #region Private Methods - Display
        private void UpdateDisplay()
        {
            if (_currentItem == null || _currentContext == null) return;

            // Header
            if (itemName != null)
                itemName.text = _currentItem.ItemName;

            // Icon
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

            // Description
            if (descriptionText != null)
                descriptionText.text = _currentItem.Description;

            // Stats
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

            // Action button
            SetupActionButton();
        }

        private void DisplayWeaponStats(WeaponSO weapon)
        {
            ClearStats();

            // PRIMARY STATS
            if (weapon.BonusAD > 0) AddStatLine("Attack Damage", $"+{weapon.BonusAD}", "#ff6b6b");
            if (weapon.BonusAP > 0) AddStatLine("Ability Power", $"+{weapon.BonusAP}", "#4ecdc4");

            // DEFENSIVE STATS
            if (weapon.BonusHP > 0) AddStatLine("Health", $"+{weapon.BonusHP}", "#00ff15ff");
            if (weapon.BonusDefense > 0) AddStatLine("Defense", $"+{weapon.BonusDefense}", "#ff7b00ff");

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

            // Weapon skill
            if (weapon.DefaultWeaponSkill != null)
            {
                AddEffectLine($"[Skill] : {weapon.DefaultWeaponSkill.AbilityName}");
            }
        }

        private void DisplayGearStats(GearSO gear)
        {
            ClearStats();

            // Defensive stats first
            if (gear.BonusHP > 0) AddStatLine("HP", $"+{gear.BonusHP}", "#00ff15ff");
            if (gear.BonusDefense > 0) AddStatLine("Defense", $"+{gear.BonusDefense}", "#ff7b00ff");

            // Offensive stats
            if (gear.BonusAD > 0) AddStatLine("Attack Damage", $"+{gear.BonusAD}", "#ff6b6b");
            if (gear.BonusAP > 0) AddStatLine("Ability Power", $"+{gear.BonusAP}", "#4ecdc4");
            if (gear.BonusAttackSpeed > 0) AddStatLine("Attack Speed", $"+{gear.BonusAttackSpeed}");
            if (gear.BonusCritRate > 0) AddStatLine("Crit Rate", $"+{gear.BonusCritRate}%");
            if (gear.BonusCritDamage > 0) AddStatLine("Crit Damage", $"+{gear.BonusCritDamage}%");

            // Utility stats
            if (gear.BonusEvasion > 0) AddStatLine("Evasion", $"+{gear.BonusEvasion}%");
            if (gear.BonusTenacity > 0) AddStatLine("Tenacity", $"+{gear.BonusTenacity}%");
            if (gear.BonusLethality > 0) AddStatLine("Lethality", $"+{gear.BonusLethality}");
            if (gear.BonusPenetration > 0) AddStatLine("Penetration", $"+{gear.BonusPenetration}%");
            if (gear.BonusLifesteal > 0) AddStatLine("Lifesteal", $"+{gear.BonusLifesteal}%");
        }

        private void ClearStats()
        {
            foreach (Transform child in statPanelContent)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in effectPanelContent)
            {
                Destroy(child.gameObject);
            }
        }

        private void AddStatLine(string label, string value, string hexColor = null)
        {
            if (itemBonusStatsPrefab == null)
            {
                Debug.LogWarning($"[GearPopup] ItemBonusStatsPrefab not assigned! {label}: {value}");
                return;
            }

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
                Debug.LogWarning($"[GearPopup] ItemEffectPrefab not assigned! {effectText}");
                return;
            }

            GameObject effectObj = Instantiate(itemEffectPrefab, effectPanelContent);
            
            TMP_Text text = effectObj.transform.Find("Text")?.GetComponent<TMP_Text>();
            
            if (text == null)
            {
                text = effectObj.GetComponentInChildren<TMP_Text>();
            }
            
            if (text != null) text.text = effectText;
        }

        private void SetupActionButton()
        {
            var buttonText = actionButton?.GetComponentInChildren<TMP_Text>();
            if (buttonText == null || _currentContext == null) return;

            buttonText.text = _currentContext.GetButtonText(_currentItem, _currentItemInstance);
        }
        #endregion

        #region Event Handlers
        private void OnActionButtonClicked()
        {
            if (_currentContext == null)
            {
                Debug.LogError("[GearPopup] No context set - cannot perform action");
                return;
            }

            if (!_currentContext.CanPerformAction(_currentItem, _currentItemInstance))
            {
                Debug.LogError("[GearPopup] Action cannot be performed in current state");
                return;
            }

            bool success = _currentContext.OnButtonClicked(_currentItem, _currentItemInstance);

            if (success)
            {
                Hide();
            }
        }
        #endregion
    }
}