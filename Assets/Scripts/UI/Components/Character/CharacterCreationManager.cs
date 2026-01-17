// ════════════════════════════════════════════════════════════════════════
// Assets\Scripts\CharacterSystem\UI\CharacterCreationManager.cs
// ✅ FIXED: Removed race condition and duplicate save call
// ✅ FIXED: Proper error handling and validation
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Character.Stat;
using Ascension.Data.SO.Character;
using Ascension.Character.Manager;
using Ascension.App;

namespace Ascension.Character.UI
{
    public class CharacterCreationManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Character Base Stats")]
        [SerializeField] private CharacterBaseStatsSO baseStats;

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

        [Header("UI - Attribute Value Text")]
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

        [Header("Settings")]
        [SerializeField] private int totalPointsToAllocate = 50;
        
        [Header("Validation Messages (Optional)")]
        [SerializeField] private TMP_Text errorMessageText;
        #endregion

        #region Private Fields
        private CharacterStats previewStats;
        private CharacterAttributes tempAttributes;
        private int pointsSpent = 0;
        private CharacterDerivedStats previewDerivedStats = new CharacterDerivedStats();
        private bool isProcessing = false; // ✅ NEW: Prevent double-clicks
        #endregion

        #region Unity Callbacks
        private void Start()
        {
            if (baseStats == null)
            {
                Debug.LogError("[CharacterCreation] CharacterBaseStatsSO is not assigned!", this);
                enabled = false;
                return;
            }

            InitializePreviewStats();
            SetupButtons();
            UpdateUI();
            ClearErrorMessage(); // ✅ NEW
        }

        private void OnDestroy()
        {
            UnsubscribeButtons();
        }
        #endregion

        #region Initialization
        private void InitializePreviewStats()
        {
            previewStats = new CharacterStats();
            previewStats.Initialize(baseStats);
            tempAttributes = previewStats.attributes.Clone();
            pointsSpent = 0;
            
            Debug.Log("[CharacterCreation] Preview stats initialized");
        }

        private void SetupButtons()
        {
            if (strMinusBtn != null) strMinusBtn.onClick.AddListener(() => ModifyAttribute(AttributeType.STR, -1));
            if (strPlusBtn != null) strPlusBtn.onClick.AddListener(() => ModifyAttribute(AttributeType.STR, 1));
            if (intMinusBtn != null) intMinusBtn.onClick.AddListener(() => ModifyAttribute(AttributeType.INT, -1));
            if (intPlusBtn != null) intPlusBtn.onClick.AddListener(() => ModifyAttribute(AttributeType.INT, 1));
            if (agiMinusBtn != null) agiMinusBtn.onClick.AddListener(() => ModifyAttribute(AttributeType.AGI, -1));
            if (agiPlusBtn != null) agiPlusBtn.onClick.AddListener(() => ModifyAttribute(AttributeType.AGI, 1));
            if (endMinusBtn != null) endMinusBtn.onClick.AddListener(() => ModifyAttribute(AttributeType.END, -1));
            if (endPlusBtn != null) endPlusBtn.onClick.AddListener(() => ModifyAttribute(AttributeType.END, 1));
            if (wisMinusBtn != null) wisMinusBtn.onClick.AddListener(() => ModifyAttribute(AttributeType.WIS, -1));
            if (wisPlusBtn != null) wisPlusBtn.onClick.AddListener(() => ModifyAttribute(AttributeType.WIS, 1));
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

        #region Attribute Modification
        private void ModifyAttribute(AttributeType attribute, int change)
        {
            int minValue = GetBaseAttributeMin(attribute);
            int currentValue = GetAttributeValue(attribute);
            int newValue = currentValue + change;

            if (newValue < minValue) return;
            if (change > 0 && pointsSpent >= totalPointsToAllocate) return;
            if (change < 0 && pointsSpent <= 0) return;

            SetAttributeValue(attribute, newValue);
            pointsSpent += change;
            UpdateUI();
            ClearErrorMessage(); // ✅ NEW: Clear errors when user makes changes
        }

        private int GetBaseAttributeMin(AttributeType attribute)
        {
            switch (attribute)
            {
                case AttributeType.STR: return baseStats.startingSTR;
                case AttributeType.INT: return baseStats.startingINT;
                case AttributeType.AGI: return baseStats.startingAGI;
                case AttributeType.END: return baseStats.startingEND;
                case AttributeType.WIS: return baseStats.startingWIS;
                default: return 0;
            }
        }

        private int GetAttributeValue(AttributeType attribute)
        {
            switch (attribute)
            {
                case AttributeType.STR: return tempAttributes.STR;
                case AttributeType.INT: return tempAttributes.INT;
                case AttributeType.AGI: return tempAttributes.AGI;
                case AttributeType.END: return tempAttributes.END;
                case AttributeType.WIS: return tempAttributes.WIS;
                default: return 0;
            }
        }

        private void SetAttributeValue(AttributeType attribute, int value)
        {
            switch (attribute)
            {
                case AttributeType.STR: tempAttributes.STR = value; break;
                case AttributeType.INT: tempAttributes.INT = value; break;
                case AttributeType.AGI: tempAttributes.AGI = value; break;
                case AttributeType.END: tempAttributes.END = value; break;
                case AttributeType.WIS: tempAttributes.WIS = value; break;
            }
        }
        #endregion

        #region UI Updates
        private void UpdateUI()
        {
            if (strValueText) strValueText.text = tempAttributes.STR.ToString();
            if (intValueText) intValueText.text = tempAttributes.INT.ToString();
            if (agiValueText) agiValueText.text = tempAttributes.AGI.ToString();
            if (endValueText) endValueText.text = tempAttributes.END.ToString();
            if (wisValueText) wisValueText.text = tempAttributes.WIS.ToString();

            int pointsRemaining = totalPointsToAllocate - pointsSpent;
            if (pointsValueText) 
            {
                pointsValueText.text = pointsRemaining.ToString();
                pointsValueText.color = pointsRemaining > 0 ? Color.yellow : Color.green;
            }

            UpdateButtonStates();
            PreviewCombatStats();
        }

        private void UpdateButtonStates()
        {
            bool hasPointsToSpend = pointsSpent < totalPointsToAllocate;
            bool canReduce = pointsSpent > 0;

            if (strPlusBtn != null) strPlusBtn.interactable = hasPointsToSpend;
            if (intPlusBtn != null) intPlusBtn.interactable = hasPointsToSpend;
            if (agiPlusBtn != null) agiPlusBtn.interactable = hasPointsToSpend;
            if (endPlusBtn != null) endPlusBtn.interactable = hasPointsToSpend;
            if (wisPlusBtn != null) wisPlusBtn.interactable = hasPointsToSpend;

            if (strMinusBtn != null) strMinusBtn.interactable = canReduce && tempAttributes.STR > baseStats.startingSTR;
            if (intMinusBtn != null) intMinusBtn.interactable = canReduce && tempAttributes.INT > baseStats.startingINT;
            if (agiMinusBtn != null) agiMinusBtn.interactable = canReduce && tempAttributes.AGI > baseStats.startingAGI;
            if (endMinusBtn != null) endMinusBtn.interactable = canReduce && tempAttributes.END > baseStats.startingEND;
            if (wisMinusBtn != null) wisMinusBtn.interactable = canReduce && tempAttributes.WIS > baseStats.startingWIS;
        }

        private void PreviewCombatStats()
        {
            previewDerivedStats.Recalculate(baseStats, previewStats.Level, tempAttributes, previewStats.itemStats, null);

            if (adValueText) adValueText.text = previewDerivedStats.AD.ToString("F1");
            if (apValueText) apValueText.text = previewDerivedStats.AP.ToString("F1");
            if (attackSpeedValueText) attackSpeedValueText.text = previewDerivedStats.AttackSpeed.ToString("F1");
            if (critDamageValueText) critDamageValueText.text = previewDerivedStats.CritDamage.ToString("F1") + "%";
            if (critRateValueText) critRateValueText.text = previewDerivedStats.CritRate.ToString("F1") + "%";
            if (lethalityValueText) lethalityValueText.text = previewDerivedStats.Lethality.ToString("F0");
            if (penetrationValueText) penetrationValueText.text = previewDerivedStats.Penetration.ToString("F1") + "%";
            if (lifestealValueText) lifestealValueText.text = previewDerivedStats.Lifesteal.ToString("F1") + "%";
            if (hpValueText) hpValueText.text = previewDerivedStats.MaxHP.ToString("F0");
            if (defenseValueText) defenseValueText.text = previewDerivedStats.Defense.ToString("F1");
            if (evasionValueText) evasionValueText.text = previewDerivedStats.Evasion.ToString("F1") + "%";
            if (tenacityValueText) tenacityValueText.text = previewDerivedStats.Tenacity.ToString("F1") + "%";
        }
        #endregion

        #region Button Handlers
        private void OnConfirmClicked()
        {
            // ✅ Prevent double-clicks
            if (isProcessing)
            {
                Debug.LogWarning("[CharacterCreation] Already processing character creation!");
                return;
            }

            // ✅ Validation with user feedback
            if (!ValidateCharacterCreation(out string errorMessage))
            {
                ShowErrorMessage(errorMessage);
                return;
            }

            isProcessing = true;
            SetConfirmButtonState(false);
            
            CreateNewCharacter();
        }

        /// <summary>
        /// ✅ NEW: Centralized validation with specific error messages
        /// </summary>
        private bool ValidateCharacterCreation(out string errorMessage)
        {
            errorMessage = string.Empty;

            // Check name
            if (string.IsNullOrWhiteSpace(nameInput?.text))
            {
                errorMessage = "Please enter a character name!";
                return false;
            }

            // Check name length
            if (nameInput.text.Trim().Length < 3)
            {
                errorMessage = "Name must be at least 3 characters!";
                return false;
            }

            if (nameInput.text.Trim().Length > 20)
            {
                errorMessage = "Name must be 20 characters or less!";
                return false;
            }

            // Check points allocated
            if (pointsSpent < totalPointsToAllocate)
            {
                int remaining = totalPointsToAllocate - pointsSpent;
                errorMessage = $"You have {remaining} attribute point{(remaining > 1 ? "s" : "")} remaining!";
                return false;
            }

            // Check managers exist
            if (GameManager.Instance == null)
            {
                errorMessage = "Game system not ready. Please restart.";
                Debug.LogError("[CharacterCreation] GameManager not found!");
                return false;
            }

            if (CharacterManager.Instance == null)
            {
                errorMessage = "Character system not ready. Please restart.";
                Debug.LogError("[CharacterCreation] CharacterManager not found!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// ✅ FIXED: Removed race condition, removed duplicate save, proper error handling
        /// 
        /// EXECUTION ORDER:
        /// 1. Create CharacterStats with custom attributes
        /// 2. Mark avatar creation complete (enables saving)
        /// 3. Load player into CharacterManager (triggers events)
        /// 4. Save immediately (single save call)
        /// 5. Transition to MainBase on success
        /// </summary>
        private void CreateNewCharacter()
        {
            string characterName = nameInput.text.Trim();
            
            Debug.Log($"[CharacterCreation] Creating character: {characterName}");
            Debug.Log($"  STR={tempAttributes.STR} INT={tempAttributes.INT} AGI={tempAttributes.AGI} END={tempAttributes.END} WIS={tempAttributes.WIS}");
            
            try
            {
                // ✅ Step 1: Create base character with custom attributes
                CharacterStats newPlayer = new CharacterStats();
                newPlayer.Initialize(baseStats);
                newPlayer.playerName = characterName;
                newPlayer.attributes.CopyFrom(tempAttributes);
                newPlayer.RecalculateStats(baseStats, fullHeal: true);
                
                // ✅ Step 2: Mark avatar creation complete BEFORE loading
                // This ensures CanSave() returns true when events fire
                GameManager.Instance.CompleteAvatarCreation();
                Debug.Log("[CharacterCreation] Avatar creation marked complete");
                
                // ✅ Step 3: Load player (triggers OnPlayerLoaded → GameEvents.TriggerGameLoaded)
                CharacterManager.Instance.LoadPlayer(newPlayer);
                Debug.Log($"[CharacterCreation] Player loaded: HP={newPlayer.CurrentHP}/{newPlayer.MaxHP}, AD={newPlayer.AD}");
                
                // ✅ Step 4: Save immediately (SINGLE save call, no coroutine)
                bool saveSuccess = GameManager.Instance.SaveGame();
                
                if (saveSuccess)
                {
                    Debug.Log("[CharacterCreation] ✓ Character saved successfully!");
                    
                    // ✅ Step 5: Transition to MainBase
                    if (SceneFlowManager.Instance != null)
                    {
                        SceneFlowManager.Instance.GoToMainBase();
                    }
                    else
                    {
                        Debug.LogError("[CharacterCreation] SceneFlowManager not available!");
                        ShowErrorMessage("Scene system not ready. Please restart.");
                        isProcessing = false;
                        SetConfirmButtonState(true);
                    }
                }
                else
                {
                    Debug.LogError("[CharacterCreation] ✗ Failed to save character!");
                    ShowErrorMessage("Failed to save character. Please try again.");
                    isProcessing = false;
                    SetConfirmButtonState(true);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CharacterCreation] Exception during character creation: {e.Message}\n{e.StackTrace}");
                ShowErrorMessage("An error occurred. Please try again.");
                isProcessing = false;
                SetConfirmButtonState(true);
            }
        }

        private void OnResetClicked()
        {
            tempAttributes.STR = baseStats.startingSTR;
            tempAttributes.INT = baseStats.startingINT;
            tempAttributes.AGI = baseStats.startingAGI;
            tempAttributes.END = baseStats.startingEND;
            tempAttributes.WIS = baseStats.startingWIS;

            pointsSpent = 0;
            if (nameInput != null) nameInput.text = string.Empty;

            UpdateUI();
            ClearErrorMessage();
            Debug.Log("[CharacterCreation] Reset to default values");
        }
        #endregion

        #region UI Feedback Helpers
        /// <summary>
        /// ✅ NEW: Show error message to player
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = message;
                errorMessageText.gameObject.SetActive(true);
            }
            
            Debug.LogWarning($"[CharacterCreation] {message}");
        }

        /// <summary>
        /// ✅ NEW: Clear error message
        /// </summary>
        private void ClearErrorMessage()
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = string.Empty;
                errorMessageText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// ✅ NEW: Enable/disable confirm button
        /// </summary>
        private void SetConfirmButtonState(bool enabled)
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = enabled;
            }
        }
        #endregion

        #region Enums
        private enum AttributeType { STR, INT, AGI, END, WIS }
        #endregion
    }
}