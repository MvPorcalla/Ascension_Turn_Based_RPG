// ════════════════════════════════════════════
// CharacterCreationManager.cs
// Manages the character creation process and UI
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Ascension.Manager;
using Ascension.Character.Stat;
using Ascension.Data.SO.Character;

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
        private CharacterStats currentStats;
        private CharacterAttributes tempAttributes;
        private int pointsSpent = 0;

        // Preview cache (reused to avoid allocations)
        private CharacterDerivedStats previewStats = new CharacterDerivedStats();
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

            InitializeStats();
            SetupButtons();
            UpdateUI();
        }

        private void OnDestroy()
        {
            // Ensure button listeners are cleaned up to avoid leaks
            UnsubscribeButtons();
        }
        #endregion

        #region Public Methods
        // Exposed for external UI tests or editor-driven automation
        public void OpenCharacterCreation()
        {
            gameObject.SetActive(true);
        }

        public void CloseCharacterCreation()
        {
            gameObject.SetActive(false);
        }
        #endregion

        #region Private Methods
        private void InitializeStats()
        {
            currentStats = new CharacterStats();
            currentStats.Initialize(baseStats);

            // Clone attributes for temporary modification
            tempAttributes = currentStats.attributes.Clone();
            pointsSpent = 0;
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

        private void UpdateUI()
        {
            // Attributes
            if (strValueText) strValueText.text = tempAttributes.STR.ToString();
            if (intValueText) intValueText.text = tempAttributes.INT.ToString();
            if (agiValueText) agiValueText.text = tempAttributes.AGI.ToString();
            if (endValueText) endValueText.text = tempAttributes.END.ToString();
            if (wisValueText) wisValueText.text = tempAttributes.WIS.ToString();

            // Points remaining
            int pointsRemaining = totalPointsToAllocate - pointsSpent;
            if (pointsValueText) 
            {
                pointsValueText.text = pointsRemaining.ToString();
                pointsValueText.color = pointsRemaining > 0 ? Color.green : Color.red;
            }

            UpdateButtonStates();
            PreviewCombatStats();
        }

        private void PreviewCombatStats()
        {
            // Reuse previewStats (no new allocation)
            previewStats.Recalculate(baseStats, currentStats.Level, tempAttributes, currentStats.itemStats, null);

            if (adValueText) adValueText.text = previewStats.AD.ToString("F1");
            if (apValueText) apValueText.text = previewStats.AP.ToString("F1");
            if (attackSpeedValueText) attackSpeedValueText.text = previewStats.AttackSpeed.ToString("F1");
            if (critDamageValueText) critDamageValueText.text = previewStats.CritDamage.ToString("F1") + "%";
            if (critRateValueText) critRateValueText.text = previewStats.CritRate.ToString("F1") + "%";
            if (lethalityValueText) lethalityValueText.text = previewStats.Lethality.ToString("F0");
            if (penetrationValueText) penetrationValueText.text = previewStats.Penetration.ToString("F1") + "%";
            if (lifestealValueText) lifestealValueText.text = previewStats.Lifesteal.ToString("F1") + "%";
            if (hpValueText) hpValueText.text = previewStats.MaxHP.ToString("F0");
            if (defenseValueText) defenseValueText.text = previewStats.Defense.ToString("F1");
            if (evasionValueText) evasionValueText.text = previewStats.Evasion.ToString("F1") + "%";
            if (tenacityValueText) tenacityValueText.text = previewStats.Tenacity.ToString("F1") + "%";
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

        private void OnConfirmClicked()
        {
            if (string.IsNullOrWhiteSpace(nameInput?.text))
            {
                Debug.LogWarning("Please enter a character name!");
                return;
            }

            if (pointsSpent < totalPointsToAllocate)
            {
                Debug.LogWarning($"You still have {totalPointsToAllocate - pointsSpent} points to allocate!");
                return;
            }

            // Apply temp attributes to actual stats
            currentStats.attributes.CopyFrom(tempAttributes);
            currentStats.playerName = nameInput.text;

            // Recalculate with full heal
            currentStats.RecalculateStats(baseStats, fullHeal: true);

            // Load into CharacterManager instead of GameManager
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.LoadPlayer(currentStats);
            }
            else
            {
                Debug.LogError("[CharacterCreation] CharacterManager not found!");
                return;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("[CharacterCreation] GameManager not found!");
                return;
            }

            // Mark avatar creation as complete BEFORE saving
            GameManager.Instance.CompleteAvatarCreation();

            // Now save is allowed
            GameManager.Instance.SaveGame();

            // Proceed to main base
            GameManager.Instance.GoToMainBase();
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
        }
        #endregion

        #region Events
        private enum AttributeType { STR, INT, AGI, END, WIS }
        #endregion
    }
}
