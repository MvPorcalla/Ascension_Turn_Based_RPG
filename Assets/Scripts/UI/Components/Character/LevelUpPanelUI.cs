// ════════════════════════════════════════════
// Assets\Scripts\UI\Components\Character\LevelUpPanelUI.cs
// ✅ 100% PURE UI - Zero business logic, zero persistence decisions
// Only displays data and captures user input
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Core;
using Ascension.Character.Core;
using Ascension.Character.Manager;
using Ascension.Data.SO.Character;

namespace Ascension.Character.UI
{
    /// <summary>
    /// 100% Pure UI component for level-up panel
    /// - Displays current player stats
    /// - Captures attribute allocation input
    /// - Delegates ALL logic to CharacterManager
    /// - Makes ZERO decisions about when to save, grant rewards, etc.
    /// </summary>
    public class LevelUpPanelUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Panel")]
        [SerializeField] private GameObject levelUpPanel;

        [Header("Level Info")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text pointsRemainingText;
        [SerializeField] private TMP_Text transcendenceText;

        [Header("Attribute Buttons")]
        [SerializeField] private Button strPlusBtn;
        [SerializeField] private Button intPlusBtn;
        [SerializeField] private Button agiPlusBtn;
        [SerializeField] private Button endPlusBtn;
        [SerializeField] private Button wisPlusBtn;

        [Header("Attribute Display")]
        [SerializeField] private TMP_Text strText;
        [SerializeField] private TMP_Text intText;
        [SerializeField] private TMP_Text agiText;
        [SerializeField] private TMP_Text endText;
        [SerializeField] private TMP_Text wisText;

        [Header("Combat Stats Preview")]
        [SerializeField] private TMP_Text adText;
        [SerializeField] private TMP_Text apText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text attackSpeedText;
        [SerializeField] private TMP_Text defenseText;
        [SerializeField] private TMP_Text critRateText;
        [SerializeField] private TMP_Text evasionText;

        [Header("UI Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;
        #endregion

        #region Private Fields
        // ✅ UI state only - just for visual preview
        private CharacterAttributes tempAttributes;
        private int tempPointsSpent;
        private readonly CharacterDerivedStats previewStats = new CharacterDerivedStats();
        
        // ✅ Manager references (read-only access)
        private CharacterManager CharManager => GameBootstrap.Character;
        private CharacterStats CurrentPlayer => CharManager?.CurrentPlayer;
        private CharacterBaseStatsSO BaseStats => CharManager?.BaseStats;
        #endregion

        #region Unity Callbacks
        private void Start()
        {
            levelUpPanel?.SetActive(false);
            SetupButtons();
        }

        private void OnEnable()
        {
            // ✅ Subscribe to game events (reactive UI)
            GameEvents.OnLevelUp += OnPlayerLeveledUp;
        }

        private void OnDisable()
        {
            // ✅ Clean up subscriptions
            GameEvents.OnLevelUp -= OnPlayerLeveledUp;
        }
        #endregion

        #region Event Handlers (Reactive UI)
        /// <summary>
        /// ✅ React to game events - just update display
        /// </summary>
        private void OnPlayerLeveledUp(int newLevel)
        {
            // ✅ PURE UI: Just check if we should show the panel
            if (CurrentPlayer != null && CurrentPlayer.AttributePoints > 0)
            {
                OpenPanel();
            }
        }
        #endregion

        #region Public API (Display Control Only)
        /// <summary>
        /// ✅ PURE UI: Just show/hide the panel
        /// NO business logic, NO decisions about game state
        /// </summary>
        public void OpenPanel()
        {
            if (CurrentPlayer == null)
            {
                Debug.LogWarning("[LevelUpPanelUI] No active player!");
                return;
            }

            if (CurrentPlayer.AttributePoints <= 0)
            {
                Debug.Log("[LevelUpPanelUI] No points to allocate!");
                return;
            }

            // ✅ Initialize temp state for preview
            tempAttributes = CurrentPlayer.attributes.Clone();
            tempPointsSpent = 0;

            levelUpPanel?.SetActive(true);
            RefreshDisplay();
        }

        /// <summary>
        /// ✅ PURE UI: Just close the panel
        /// </summary>
        public void ClosePanel()
        {
            levelUpPanel?.SetActive(false);
        }
        #endregion

        #region Private Methods - Button Setup
        private void SetupButtons()
        {
            strPlusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.STR));
            intPlusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.INT));
            agiPlusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.AGI));
            endPlusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.END));
            wisPlusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.WIS));

            confirmButton?.onClick.AddListener(OnConfirmClicked);
            closeButton?.onClick.AddListener(OnCloseClicked);
        }
        #endregion

        #region Private Methods - Input Handlers
        /// <summary>
        /// ✅ PURE UI: Just update temp preview state
        /// NO actual stat changes
        /// </summary>
        private void OnAttributeButtonClicked(AttributeType attribute)
        {
            if (CurrentPlayer == null) return;
            if (tempPointsSpent >= CurrentPlayer.AttributePoints) return;

            // ✅ PURE UI: Just update local preview
            int currentValue = tempAttributes.GetAttribute(attribute);
            tempAttributes.SetAttribute(attribute, currentValue + 1);
            
            tempPointsSpent++;
            RefreshDisplay();
        }

        /// <summary>
        /// ✅ PURE UI: Delegate to manager, NO save decision
        /// CharacterManager decides what to do after applying points
        /// </summary>
        private void OnConfirmClicked()
        {
            if (CurrentPlayer == null || CharManager == null)
            {
                Debug.LogError("[LevelUpPanelUI] No active player or CharacterManager!");
                return;
            }

            // Validation
            if (tempPointsSpent < CurrentPlayer.AttributePoints)
            {
                int remaining = CurrentPlayer.AttributePoints - tempPointsSpent;
                Debug.LogWarning($"[LevelUpPanelUI] You still have {remaining} points to spend!");
                return;
            }

            // ✅ PURE UI: Just delegate to manager
            // Manager decides if/when to save
            bool success = CharManager.ApplyAttributePoints(tempAttributes, tempPointsSpent);

            if (success)
            {
                Debug.Log("[LevelUpPanelUI] Attribute points applied!");
                ClosePanel();
            }
            else
            {
                Debug.LogError("[LevelUpPanelUI] Failed to apply attribute points!");
            }
        }

        /// <summary>
        /// ✅ PURE UI: Just close if no points remain
        /// </summary>
        private void OnCloseClicked()
        {
            if (CurrentPlayer != null && CurrentPlayer.AttributePoints > 0)
            {
                Debug.LogWarning("[LevelUpPanelUI] You must allocate all points before closing!");
                return;
            }

            ClosePanel();
        }
        #endregion

        #region Private Methods - Display Updates
        /// <summary>
        /// ✅ PURE UI: Just refresh visual elements
        /// </summary>
        private void RefreshDisplay()
        {
            UpdateLevelInfo();
            UpdatePointsRemaining();
            UpdateAttributeDisplays();
            UpdateButtonStates();
            UpdatePreviewStats();
        }

        private void UpdateLevelInfo()
        {
            if (CurrentPlayer == null) return;

            if (levelText != null)
            {
                levelText.text = CurrentPlayer.IsTranscended
                    ? $"Level {CurrentPlayer.Level} (T{CurrentPlayer.TranscendenceLevel})"
                    : $"Level {CurrentPlayer.Level}";
            }

            if (transcendenceText != null)
            {
                transcendenceText.gameObject.SetActive(CurrentPlayer.IsTranscended);
                if (CurrentPlayer.IsTranscended)
                {
                    transcendenceText.text = $"<color=gold>Transcendence Level {CurrentPlayer.TranscendenceLevel}</color>";
                }
            }
        }

        private void UpdatePointsRemaining()
        {
            if (pointsRemainingText == null || CurrentPlayer == null) return;

            int remaining = CurrentPlayer.AttributePoints - tempPointsSpent;
            pointsRemainingText.text = $"Points: {remaining}";
            pointsRemainingText.color = remaining > 0 ? Color.green : Color.red;
        }

        private void UpdateAttributeDisplays()
        {
            if (CurrentPlayer == null) return;

            UpdateAttributeText(strText, tempAttributes.STR, CurrentPlayer.attributes.STR, "STR");
            UpdateAttributeText(intText, tempAttributes.INT, CurrentPlayer.attributes.INT, "INT");
            UpdateAttributeText(agiText, tempAttributes.AGI, CurrentPlayer.attributes.AGI, "AGI");
            UpdateAttributeText(endText, tempAttributes.END, CurrentPlayer.attributes.END, "END");
            UpdateAttributeText(wisText, tempAttributes.WIS, CurrentPlayer.attributes.WIS, "WIS");
        }

        private void UpdateAttributeText(TMP_Text text, int tempValue, int currentValue, string label)
        {
            if (text == null) return;
            
            string change = tempValue > currentValue 
                ? $" <color=green>(+{tempValue - currentValue})</color>" 
                : "";
            
            text.text = $"{label}: {tempValue}{change}";
        }

        private void UpdateButtonStates()
        {
            if (CurrentPlayer == null) return;

            bool canAdd = tempPointsSpent < CurrentPlayer.AttributePoints;
            
            if (strPlusBtn != null) strPlusBtn.interactable = canAdd;
            if (intPlusBtn != null) intPlusBtn.interactable = canAdd;
            if (agiPlusBtn != null) agiPlusBtn.interactable = canAdd;
            if (endPlusBtn != null) endPlusBtn.interactable = canAdd;
            if (wisPlusBtn != null) wisPlusBtn.interactable = canAdd;
        }

        private void UpdatePreviewStats()
        {
            if (BaseStats == null || CurrentPlayer == null) return;

            // ✅ Calculate preview stats (doesn't modify real data)
            previewStats.Recalculate(
                BaseStats, 
                CurrentPlayer.Level, 
                tempAttributes, // Preview with temp attributes
                CurrentPlayer.itemStats, 
                CurrentPlayer.equippedWeapon
            );

            // ✅ Display preview
            SetText(adText, $"AD: {previewStats.AD:F1}");
            SetText(apText, $"AP: {previewStats.AP:F1}");
            SetText(hpText, $"HP: {previewStats.MaxHP:F0}");
            SetText(attackSpeedText, $"Speed: {previewStats.AttackSpeed:F1}");
            SetText(defenseText, $"Defense: {previewStats.Defense:F1}");
            SetText(critRateText, $"Crit: {previewStats.CritRate:F1}%");
            SetText(evasionText, $"Evasion: {previewStats.Evasion:F1}%");
        }

        private void SetText(TMP_Text text, string value)
        {
            if (text != null) text.text = value;
        }
        #endregion

        #region Debug Tools
#if UNITY_EDITOR
        [ContextMenu("Debug: Open Panel")]
        private void DebugOpenPanel()
        {
            if (Application.isPlaying)
            {
                OpenPanel();
            }
            else
            {
                Debug.LogWarning("[LevelUpPanelUI] Only works in Play Mode");
            }
        }
#endif
        #endregion
    }
}