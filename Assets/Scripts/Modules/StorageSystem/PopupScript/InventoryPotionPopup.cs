// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\PopupScript\InventoryPotionPopup.cs
// UI Popup for displaying potion details and actions
// ✅ FIXED: Pocket logic completely removed
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Ascension.GameSystem;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Character;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Manager;
using Ascension.Character.Stat;
using Ascension.Character.Manager;

namespace Ascension.Inventory.Popup
{
    public class InventoryPotionPopup : MonoBehaviour
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
        [SerializeField] private Button actionButton1;
        [SerializeField] private Button actionButton2;

        private PotionSO currentPotion;
        private ItemInstance currentItem;
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

            useButton.onClick.AddListener(OnUseClicked);
            actionButton1.onClick.AddListener(OnActionButton1Clicked);
            actionButton2.onClick.AddListener(OnActionButton2Clicked);

            popupContainer.SetActive(false);
        }

        public void ShowPotion(PotionSO potion, ItemInstance item, ItemLocation fromLocation)
        {
            currentPotion = potion;
            currentItem = item;
            currentLocation = fromLocation;
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
            SetupActionButtons(fromLocation);
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
                    PotionType.Rejuvenation => "Rejuvenation",
                    PotionType.Utility => "Utility",
                    _ => "Unknown"
                };
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
            if (buffTypePrefab == null)
            {
                Debug.LogWarning($"BuffTypePrefab not assigned! {buffType}: {value} ({duration})");
                return;
            }

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

        private void SetupActionButtons(ItemLocation fromLocation)
        {
            useButton.gameObject.SetActive(true);

            // ✅ SIMPLIFIED: Only 2 movement buttons - Bag and Storage
            switch (fromLocation)
            {
                case ItemLocation.Storage:
                    // From storage: [Use] [Add to Bag] [Close]
                    actionButton1.gameObject.SetActive(true);
                    actionButton2.gameObject.SetActive(false);
                    SetButtonText(actionButton1, "Add to Bag");
                    break;

                case ItemLocation.Bag:
                    // From bag: [Use] [Store] [Close]
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
            if (PotionManager.Instance == null)
            {
                Debug.LogError("[PotionPopupUI] PotionManager not found!");
                return;
            }

            if (CharacterManager.Instance == null)
            {
                Debug.LogError("[PotionPopupUI] CharacterManager not found!");
                return;
            }

            if (!CharacterManager.Instance.HasActivePlayer)
            {
                Debug.LogError("[PotionPopupUI] No active player!");
                return;
            }

            CharacterStats CharacterStats = CharacterManager.Instance.CurrentPlayer;
            CharacterBaseStatsSO baseStats = CharacterManager.Instance.BaseStats;

            if (CharacterStats == null || baseStats == null)
            {
                Debug.LogError("[PotionPopupUI] Player stats not initialized!");
                return;
            }

            int successfulUses = 0;
            for (int i = 0; i < selectedQuantity; i++)
            {
                bool success = PotionManager.Instance.UsePotion(currentPotion, CharacterStats, baseStats);
                
                if (success)
                {
                    successfulUses++;
                }
                else
                {
                    if (successfulUses > 0)
                    {
                        Debug.LogWarning($"[PotionPopupUI] Could only use {successfulUses}/{selectedQuantity} potions");
                    }
                    break;
                }
            }

            if (successfulUses > 0)
            {
                InventoryManager.Instance.Inventory.RemoveItem(currentItem, successfulUses);
                Debug.Log($"[PotionPopupUI] Used {successfulUses}x {currentPotion.ItemName}");
                
                if (successfulUses == selectedQuantity || currentItem.quantity <= 0)
                {
                    ClosePopup();
                }
                else
                {
                    selectedQuantity = Mathf.Min(selectedQuantity, currentItem.quantity);
                    SetupQuantityControls(currentItem.quantity);
                }
            }
            else
            {
                Debug.LogWarning($"[PotionPopupUI] Failed to use {currentPotion.ItemName}");
            }
        }

        private void OnActionButton1Clicked()
        {
            var buttonText = actionButton1.GetComponentInChildren<TMP_Text>()?.text;
            var database = InventoryManager.Instance.Database;
            InventoryResult result;
            
            if (buttonText == "Add to Bag")
            {
                result = InventoryManager.Instance.Inventory.MoveToBag(currentItem, selectedQuantity, database);
            }
            else if (buttonText == "Store")
            {
                result = InventoryManager.Instance.Inventory.MoveToStorage(currentItem, selectedQuantity, database);
            }
            else
            {
                Debug.LogWarning($"[InventoryPotionPopup] Unknown button action: {buttonText}");
                return;
            }

            if (result.Success)
            {
                Debug.Log($"[InventoryPotionPopup] {result.Message}");
                ClosePopup();
            }
            else
            {
                Debug.LogWarning($"[InventoryPotionPopup] {result.Message}");
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
            currentPotion = null;
            currentItem = null;
        }

        #endregion
    }
}