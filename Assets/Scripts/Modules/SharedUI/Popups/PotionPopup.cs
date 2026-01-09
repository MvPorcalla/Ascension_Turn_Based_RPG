// ════════════════════════════════════════════
// Assets\Scripts\Modules\SharedUI\Popups\PotionPopup.cs
// ✅ REFACTORED: Uses PopupContext + PopupActionHandler
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;

namespace Ascension.SharedUI.Popups
{
    public class PotionPopup : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Configuration")]
        [SerializeField] private PopupConfig config;

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
        [SerializeField] private Button actionButton1;
        #endregion

        #region Private Fields
        private PotionSO currentPotion;
        private ItemInstance currentItem;
        private PopupContext currentContext; // ✅ NEW: Store context
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
                Debug.LogError("[PotionPopup] PopupConfig not assigned!");
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
            useButton.onClick.AddListener(OnUseClicked);
            actionButton1.onClick.AddListener(OnActionButton1Clicked);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// ✅ REFACTORED: Now accepts PopupContext
        /// </summary>
        public void ShowPotion(PotionSO potion, ItemInstance item, PopupContext context)
        {
            currentPotion = potion;
            currentItem = item;
            currentContext = context; // ✅ Store context
            selectedQuantity = 1;

            popupContainer.SetActive(true);

            itemName.text = potion.ItemName;

            if (potionIcon != null)
            {
                if (potion.Icon != null)
                {
                    potionIcon.sprite = potion.Icon;
                    potionIcon.enabled = true;
                }
                else
                {
                    potionIcon.sprite = null;
                    potionIcon.enabled = false;
                }
            }

            DisplayPotionType(potion);
            DisplayBuffEffects(potion);

            if (potionDescription != null)
                potionDescription.text = potion.Description;

            SetupQuantityControls(item.quantity);
            SetupActionButtons();
        }

        public void Hide()
        {
            popupContainer.SetActive(false);
            currentPotion = null;
            currentItem = null;
            currentContext = null; // ✅ Clear context
        }

        #endregion

        #region Private Methods - Display

        private void DisplayPotionType(PotionSO potion)
        {
            if (potionTypeLabel != null && config != null)
                potionTypeLabel.text = config.labelPotionType;

            if (potionTypeValue != null && config != null)
            {
                potionTypeValue.text = config.GetPotionTypeName(potion.potionType);
            }
        }

        private void DisplayBuffEffects(PotionSO potion)
        {
            foreach (Transform child in buffEffectContent)
            {
                Destroy(child.gameObject);
            }

            switch (potion.potionType)
            {
                case PotionType.HealthPotion:
                    DisplayHealthPotionEffect(potion);
                    break;

                case PotionType.ManaPotion:
                    DisplayManaPotionEffect(potion);
                    break;

                case PotionType.Rejuvenation:
                    DisplayRejuvenationEffect(potion);
                    break;

                case PotionType.BuffPotion:
                    DisplayBuffPotionEffects(potion);
                    break;

                case PotionType.Elixir:
                    AddBuffLine("HP & Mana", $"+{potion.HealthRestore} / +{potion.ManaRestore}", "Instant");
                    if (potion.GrantsBuff)
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

        private void DisplayHealthPotionEffect(PotionSO potion)
        {
            string valueText = GetRestoreValueText(potion.restoreType, potion.HealthRestore);
            string durationText = GetDurationText(potion.durationType, potion.restoreDuration);
            AddBuffLine("HP Restore", valueText, durationText);
        }

        private void DisplayManaPotionEffect(PotionSO potion)
        {
            string valueText = GetRestoreValueText(potion.restoreType, potion.ManaRestore);
            string durationText = GetDurationText(potion.durationType, potion.restoreDuration);
            AddBuffLine("Mana Restore", valueText, durationText);
        }

        private void DisplayRejuvenationEffect(PotionSO potion)
        {
            if (potion.IsTurnBased)
            {
                int turns = potion.TurnDuration;
                float perTurn = potion.HealthRestore / turns;
                string perTurnText = GetRestoreValueText(potion.restoreType, perTurn);
                string totalText = GetRestoreValueText(potion.restoreType, potion.HealthRestore);
                AddBuffLine("HP Restore", $"{perTurnText}/turn", $"{turns} turns (Total: {totalText})");
            }
            else
            {
                string valueText = GetRestoreValueText(potion.restoreType, potion.HealthRestore);
                string durationText = GetDurationText(potion.durationType, potion.restoreDuration);
                AddBuffLine("HP Restore", valueText, durationText);
            }
        }
        
        private string GetRestoreValueText(RestoreType restoreType, float value)
        {
            if (restoreType == RestoreType.Percentage)
            {
                return $"+{value}%";
            }
            else
            {
                return $"+{value:F0}";
            }
        }

        private string GetDurationText(DurationType durationType, float duration)
        {
            return durationType switch
            {
                DurationType.Instant => "Instant",
                DurationType.RealTime => $"{duration:F1}s (over time)",
                DurationType.TurnBased => $"{Mathf.RoundToInt(duration)} turns",
                _ => "Unknown"
            };
        }

        private void DisplayBuffPotionEffects(PotionSO potion)
        {
            if (potion.buffs == null || potion.buffs.Count == 0) return;

            foreach (var buff in potion.buffs)
            {
                string durationText = buff.durationType switch
                {
                    DurationType.TurnBased => $"{Mathf.RoundToInt(buff.duration)} turns",
                    DurationType.RealTime => $"{Mathf.RoundToInt(buff.duration)}s",
                    _ => "Instant"
                };

                string buffValueText = GetBuffValueText(buff.type, buff.value);
                string buffLabel = GetBuffLabel(buff.type);

                AddBuffLine(buffLabel, buffValueText, durationText);
            }
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
            if (buffType == BuffType.Speed || 
                buffType == BuffType.CritRate || 
                buffType == BuffType.AttackSpeed || 
                buffType == BuffType.Resistance)
            {
                return $"+{value}%";
            }
            
            if (buffType == BuffType.Invisibility || 
                buffType == BuffType.Invulnerability)
            {
                return "Active";
            }

            return $"+{value}";
        }

        private void AddBuffLine(string buffType, string value, string duration)
        {
            if (buffTypePrefab == null) return;

            GameObject buffObj = Instantiate(buffTypePrefab, buffEffectContent);

            TMP_Text labelText = buffObj.transform.Find("TextLabel")?.GetComponent<TMP_Text>();
            TMP_Text valueText = buffObj.transform.Find("textValue")?.GetComponent<TMP_Text>();
            TMP_Text durationText = buffObj.transform.Find("textDuration")?.GetComponent<TMP_Text>();

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

            if (labelText != null) labelText.text = buffType;
            if (valueText != null) valueText.text = value;
            if (durationText != null) durationText.text = duration;
        }

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

            // ✅ Use button is only available in bag (context.CanUse)
            if (useButton != null)
            {
                useButton.gameObject.SetActive(currentContext.CanUse);
                
                if (currentContext.CanUse)
                {
                    var buttonText = useButton.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = config.buttonUse;
                    }
                }
            }

            // ✅ Move button uses context
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

        /// <summary>
        /// ✅ REFACTORED: Uses PopupActionHandler instead of calling PotionManager directly
        /// </summary>
        private void OnUseClicked()
        {
            if (currentPotion == null || currentItem == null) return;

            // ✅ Delegate to PopupActionHandler
            PopupActionHandler.Instance.UsePotion(
                currentPotion, 
                currentItem, 
                selectedQuantity
            );
        }

        /// <summary>
        /// ✅ REFACTORED: Uses PopupActionHandler instead of calling InventoryManager directly
        /// </summary>
        private void OnActionButton1Clicked()
        {
            if (currentContext == null || currentItem == null) return;

            // Determine target location
            ItemLocation targetLocation = currentContext.SourceLocation == ItemLocation.Bag
                ? ItemLocation.Storage
                : ItemLocation.Bag;

            // ✅ Delegate to PopupActionHandler
            PopupActionHandler.Instance.MoveItem(
                currentItem, 
                selectedQuantity, 
                targetLocation
            );
        }

        #endregion
    }
}