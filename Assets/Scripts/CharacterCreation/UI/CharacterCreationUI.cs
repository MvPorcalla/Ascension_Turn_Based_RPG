// ════════════════════════════════════════════════════════════════════════
// Assets\Scripts\CharacterCreation\UI\CharacterCreationUI.cs
// ✅ REFACTORED: Fixed button state calculation - no longer calls non-existent methods
// Pure presentation layer - NO BUSINESS LOGIC!
// Handles: UI updates, button clicks, visual feedback
// Calls: CharacterCreationManager for all logic
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.CharacterCreation.Manager;
using Ascension.CharacterCreation.Data;
using Ascension.Character.Core;

namespace Ascension.CharacterCreation.UI
{
    /// <summary>
    /// Presentation layer for character creation screen
    /// All business logic delegated to CharacterCreationManager
    /// </summary>
    public class CharacterCreationUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Manager Reference")]
        [SerializeField] private CharacterCreationManager creationManager;

        [Header("UI - Name Input")]
        [SerializeField] private TMP_InputField nameInput;

        [Header("UI - Attribute Buttons")]
        [SerializeField] private Button strMinusBtn;
        [SerializeField] private Button strPlusBtn;
        [SerializeField] private Button intMinusBtn;
        [SerializeField] private Button intPlusBtn;
        [SerializeField] private Button agiMinusBtn;
        [SerializeField] private Button agiPlusBtn;
        [SerializeField] private Button endMinusBtn;
        [SerializeField] private Button endPlusBtn;
        [SerializeField] private Button wisMinusBtn;
        [SerializeField] private Button wisPlusBtn;

        [Header("UI - Attribute Display")]
        [SerializeField] private TMP_Text strValueText;
        [SerializeField] private TMP_Text intValueText;
        [SerializeField] private TMP_Text agiValueText;
        [SerializeField] private TMP_Text endValueText;
        [SerializeField] private TMP_Text wisValueText;
        [SerializeField] private TMP_Text pointsValueText;

        [Header("UI - Combat Stats Display")]
        [SerializeField] private TMP_Text adValueText;
        [SerializeField] private TMP_Text apValueText;
        [SerializeField] private TMP_Text critDamageValueText;
        [SerializeField] private TMP_Text critRateValueText;
        [SerializeField] private TMP_Text lethalityValueText;
        [SerializeField] private TMP_Text penetrationValueText;
        [SerializeField] private TMP_Text lifestealValueText;
        [SerializeField] private TMP_Text attackSpeedValueText;
        [SerializeField] private TMP_Text hpValueText;
        [SerializeField] private TMP_Text defenseValueText;
        [SerializeField] private TMP_Text evasionValueText;
        [SerializeField] private TMP_Text tenacityValueText;

        [Header("UI - Action Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button resetButton;

        [Header("UI - Feedback")]
        [SerializeField] private TMP_Text errorMessageText;
        [SerializeField] private GameObject loadingIndicator;

        [Header("Visual Settings")]
        [SerializeField] private Color pointsRemainingColor = Color.yellow;
        [SerializeField] private Color allPointsSpentColor = Color.green;
        #endregion

        #region Unity Callbacks
        private void Start()
        {
            if (creationManager == null)
            {
                Debug.LogError("[CharacterCreationUI] CharacterCreationManager not assigned!", this);
                enabled = false;
                return;
            }

            SetupButtons();
            RefreshUI();
            ClearErrorMessage();
            HideLoadingIndicator();
        }

        private void OnDestroy()
        {
            UnsubscribeButtons();
        }
        #endregion

        #region Button Setup
        private void SetupButtons()
        {
            // Attribute buttons
            if (strMinusBtn != null) strMinusBtn.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.STR, -1));
            if (strPlusBtn != null) strPlusBtn.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.STR, 1));
            if (intMinusBtn != null) intMinusBtn.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.INT, -1));
            if (intPlusBtn != null) intPlusBtn.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.INT, 1));
            if (agiMinusBtn != null) agiMinusBtn.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.AGI, -1));
            if (agiPlusBtn != null) agiPlusBtn.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.AGI, 1));
            if (endMinusBtn != null) endMinusBtn.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.END, -1));
            if (endPlusBtn != null) endPlusBtn.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.END, 1));
            if (wisMinusBtn != null) wisMinusBtn.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.WIS, -1));
            if (wisPlusBtn != null) wisPlusBtn.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.WIS, 1));

            // Action buttons
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);
            if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
        }

        private void UnsubscribeButtons()
        {
            if (strMinusBtn != null) strMinusBtn.onClick.RemoveAllListeners();
            if (strPlusBtn != null) strPlusBtn.onClick.RemoveAllListeners();
            if (intMinusBtn != null) intMinusBtn.onClick.RemoveAllListeners();
            if (intPlusBtn != null) intPlusBtn.onClick.RemoveAllListeners();
            if (agiMinusBtn != null) agiMinusBtn.onClick.RemoveAllListeners();
            if (agiPlusBtn != null) agiPlusBtn.onClick.RemoveAllListeners();
            if (endMinusBtn != null) endMinusBtn.onClick.RemoveAllListeners();
            if (endPlusBtn != null) endPlusBtn.onClick.RemoveAllListeners();
            if (wisMinusBtn != null) wisMinusBtn.onClick.RemoveAllListeners();
            if (wisPlusBtn != null) wisPlusBtn.onClick.RemoveAllListeners();
            if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
            if (resetButton != null) resetButton.onClick.RemoveAllListeners();
        }
        #endregion

        #region Button Handlers
        private void OnAttributeButtonClicked(AttributeType attributeType, int change)
        {
            if (creationManager.TryModifyAttribute(attributeType, change))
            {
                RefreshUI();
                ClearErrorMessage();
            }
        }

        private void OnConfirmClicked()
        {
            if (creationManager.IsProcessing)
            {
                return; // Already processing
            }

            string characterName = nameInput?.text ?? string.Empty;

            // Show loading state
            ShowLoadingIndicator();
            SetConfirmButtonEnabled(false);

            // Create character (manager handles all logic)
            var result = creationManager.CreateCharacter(characterName);

            if (result.Success)
            {
                // Success - manager will transition to main game
                creationManager.CompleteCreation();
            }
            else
            {
                // Failed - show error and re-enable button
                ShowErrorMessage(result.ErrorMessage);
                HideLoadingIndicator();
                SetConfirmButtonEnabled(true);
            }
        }

        private void OnResetClicked()
        {
            creationManager.ResetAttributes();
            
            if (nameInput != null)
                nameInput.text = string.Empty;

            RefreshUI();
            ClearErrorMessage();
        }
        #endregion

        #region UI Update Methods
        /// <summary>
        /// Refresh entire UI based on current manager state
        /// </summary>
        public void RefreshUI()
        {
            CharacterCreationData data = creationManager.CreationData;

            UpdateAttributeValues(data);
            UpdatePointsDisplay(data);
            UpdateButtonStates(data);
            UpdateCombatStatsPreview(data);
        }

        private void UpdateAttributeValues(CharacterCreationData data)
        {
            if (strValueText) strValueText.text = data.currentAttributes.STR.ToString();
            if (intValueText) intValueText.text = data.currentAttributes.INT.ToString();
            if (agiValueText) agiValueText.text = data.currentAttributes.AGI.ToString();
            if (endValueText) endValueText.text = data.currentAttributes.END.ToString();
            if (wisValueText) wisValueText.text = data.currentAttributes.WIS.ToString();
        }

        private void UpdatePointsDisplay(CharacterCreationData data)
        {
            if (pointsValueText)
            {
                int remaining = data.PointsRemaining;
                pointsValueText.text = remaining.ToString();
                pointsValueText.color = remaining > 0 ? pointsRemainingColor : allPointsSpentColor;
            }
        }

        /// <summary>
        /// ✅ FIXED: Calculate button states from CreationData instead of calling non-existent methods
        /// </summary>
        private void UpdateButtonStates(CharacterCreationData data)
        {
            // Plus buttons - can increase if we have points remaining
            bool canIncrease = data.HasPointsToSpend;

            if (strPlusBtn != null) strPlusBtn.interactable = canIncrease;
            if (intPlusBtn != null) intPlusBtn.interactable = canIncrease;
            if (agiPlusBtn != null) agiPlusBtn.interactable = canIncrease;
            if (endPlusBtn != null) endPlusBtn.interactable = canIncrease;
            if (wisPlusBtn != null) wisPlusBtn.interactable = canIncrease;

            // Minus buttons - can decrease if current value > base value
            if (creationManager.BaseStats != null)
            {
                if (strMinusBtn != null) 
                    strMinusBtn.interactable = data.currentAttributes.STR > creationManager.BaseStats.startingSTR;
                
                if (intMinusBtn != null) 
                    intMinusBtn.interactable = data.currentAttributes.INT > creationManager.BaseStats.startingINT;
                
                if (agiMinusBtn != null) 
                    agiMinusBtn.interactable = data.currentAttributes.AGI > creationManager.BaseStats.startingAGI;
                
                if (endMinusBtn != null) 
                    endMinusBtn.interactable = data.currentAttributes.END > creationManager.BaseStats.startingEND;
                
                if (wisMinusBtn != null) 
                    wisMinusBtn.interactable = data.currentAttributes.WIS > creationManager.BaseStats.startingWIS;
            }
            else
            {
                // Fallback: disable all minus buttons if BaseStats not available
                if (strMinusBtn != null) strMinusBtn.interactable = false;
                if (intMinusBtn != null) intMinusBtn.interactable = false;
                if (agiMinusBtn != null) agiMinusBtn.interactable = false;
                if (endMinusBtn != null) endMinusBtn.interactable = false;
                if (wisMinusBtn != null) wisMinusBtn.interactable = false;
            }
        }

        private void UpdateCombatStatsPreview(CharacterCreationData data)
        {
            var stats = data.previewStats;

            if (adValueText) adValueText.text = stats.AD.ToString("F1");
            if (apValueText) apValueText.text = stats.AP.ToString("F1");
            if (attackSpeedValueText) attackSpeedValueText.text = stats.AttackSpeed.ToString("F1");
            if (critDamageValueText) critDamageValueText.text = stats.CritDamage.ToString("F1") + "%";
            if (critRateValueText) critRateValueText.text = stats.CritRate.ToString("F1") + "%";
            if (lethalityValueText) lethalityValueText.text = stats.Lethality.ToString("F0");
            if (penetrationValueText) penetrationValueText.text = stats.Penetration.ToString("F1") + "%";
            if (lifestealValueText) lifestealValueText.text = stats.Lifesteal.ToString("F1") + "%";
            if (hpValueText) hpValueText.text = stats.MaxHP.ToString("F0");
            if (defenseValueText) defenseValueText.text = stats.Defense.ToString("F1");
            if (evasionValueText) evasionValueText.text = stats.Evasion.ToString("F1") + "%";
            if (tenacityValueText) tenacityValueText.text = stats.Tenacity.ToString("F1") + "%";
        }
        #endregion

        #region UI Feedback
        private void ShowErrorMessage(string message)
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = message;
                errorMessageText.gameObject.SetActive(true);
            }
        }

        private void ClearErrorMessage()
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = string.Empty;
                errorMessageText.gameObject.SetActive(false);
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(true);
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
        }

        private void SetConfirmButtonEnabled(bool enabled)
        {
            if (confirmButton != null)
                confirmButton.interactable = enabled;
        }
        #endregion
    }
}