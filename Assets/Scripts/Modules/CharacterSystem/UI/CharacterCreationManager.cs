// ════════════════════════════════════════════
// Assets\Scripts\CharacterSystem\UI\CharacterCreationManager.cs
// Manages character creation UI and logic
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
        #endregion

        #region Private Fields
        private CharacterStats previewStats;
        private CharacterAttributes tempAttributes;
        private int pointsSpent = 0;
        private CharacterDerivedStats previewDerivedStats = new CharacterDerivedStats();
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
        }

        private void OnDestroy()
        {
            UnsubscribeButtons();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize preview stats for UI display (not the actual player yet)
        /// </summary>
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
                pointsValueText.color = pointsRemaining > 0 ? Color.green : Color.red;
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
            // Validation: Name
            if (string.IsNullOrWhiteSpace(nameInput?.text))
            {
                Debug.LogWarning("[CharacterCreation] Please enter a character name!");
                return;
            }

            // Validation: Points allocated
            if (pointsSpent < totalPointsToAllocate)
            {
                Debug.LogWarning($"[CharacterCreation] You still have {totalPointsToAllocate - pointsSpent} points to allocate!");
                return;
            }

            // Validation: GameManager exists
            if (GameManager.Instance == null)
            {
                Debug.LogError("[CharacterCreation] GameManager not found!");
                return;
            }

            // ✅ CREATE THE PLAYER NOW (not expecting Bootstrap to do it)
            CreateNewCharacter();
        }

        /// <summary>
        /// ✅ FIXED: Create the actual player character with customizations
        /// </summary>
        private void CreateNewCharacter()
        {
            string characterName = nameInput.text.Trim();
            
            Debug.Log($"[CharacterCreation] Creating new character: {characterName}");
            
            // Create player via GameManager (which calls CharacterManager.CreateNewPlayer)
            GameManager.Instance.StartNewGame();
            
            // Set custom name
            GameManager.Instance.SetPlayerName(characterName);
            
            // Apply custom attributes
            if (GameManager.Instance.CurrentPlayer != null)
            {
                GameManager.Instance.CurrentPlayer.attributes.CopyFrom(tempAttributes);
                
                // Recalculate stats with full heal
                if (CharacterManager.Instance != null)
                {
                    GameManager.Instance.CurrentPlayer.RecalculateStats(
                        CharacterManager.Instance.BaseStats, 
                        fullHeal: true
                    );
                }
                
                Debug.Log($"[CharacterCreation] ✓ Character created with attributes:");
                Debug.Log($"  STR:{tempAttributes.STR} INT:{tempAttributes.INT} AGI:{tempAttributes.AGI} END:{tempAttributes.END} WIS:{tempAttributes.WIS}");
            }
            else
            {
                Debug.LogError("[CharacterCreation] Failed to create player!");
                return;
            }
            
            // Save and proceed
            StartCoroutine(SaveAndProceedToMainBase());
        }

        private IEnumerator SaveAndProceedToMainBase()
        {
            // Mark avatar creation as complete
            GameManager.Instance.CompleteAvatarCreation();
            Debug.Log("[CharacterCreation] Avatar creation marked complete");

            // Wait one frame for state to propagate
            yield return null;

            // Attempt save
            bool saveSuccess = GameManager.Instance.SaveGame();
            
            if (saveSuccess)
            {
                Debug.Log("[CharacterCreation] ✓ Character saved successfully!");
                GameManager.Instance.GoToMainBase();
            }
            else
            {
                Debug.LogError("[CharacterCreation] ✗ Failed to save character!");
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
            Debug.Log("[CharacterCreation] Reset to default values");
        }
        #endregion

        #region Enums
        private enum AttributeType { STR, INT, AGI, END, WIS }
        #endregion
    }
}