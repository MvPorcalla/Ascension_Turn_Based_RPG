// ════════════════════════════════════════════
// Assets\Scripts\UI\ScreenPanels\ProfilePanelManager.cs
// ✅ 100% PURE UI - Profile panel for player stats and attribute allocation
// Full-screen additive scene, plan-then-confirm allocation pattern
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Ascension.Core;
using Ascension.Character.Core;
using Ascension.Character.Manager;
using Ascension.Data.SO.Character;
using Ascension.UI.Components.Character;

namespace Ascension.UI.Screens
{
    /// <summary>
    /// 100% Pure UI for player profile and attribute allocation
    /// - Displays player info and stats
    /// - Allows attribute point allocation with preview
    /// - Plan-then-confirm pattern (like LevelUpPanelUI)
    /// - Event-driven updates
    /// </summary>
    public class ProfilePanelUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Player Info")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text playerLevelText;
        [SerializeField] private TMP_Text guildRankText;
        
        [Header("Player Preview")]
        [SerializeField] private PlayerStatsPreviewUI playerPreview;
        
        [Header("Attribute Buttons")]
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
        
        [Header("Attribute Display")]
        [SerializeField] private TMP_Text strValueText;
        [SerializeField] private TMP_Text intValueText;
        [SerializeField] private TMP_Text agiValueText;
        [SerializeField] private TMP_Text endValueText;
        [SerializeField] private TMP_Text wisValueText;
        [SerializeField] private TMP_Text pointsValueText;
        
        [Header("Action Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button closeButton;
        
        [Header("Visual Settings")]
        [SerializeField] private Color pointsAvailableColor = Color.green;
        [SerializeField] private Color pointsNormalColor = Color.white;
        [SerializeField] private Color attributeIncreasedColor = Color.green;
        #endregion
        
        #region Private Fields
        // ✅ UI state only - temp preview data
        private CharacterAttributes tempAttributes;
        private int tempPointsSpent = 0;
        private Dictionary<AttributeType, int> originalAttributes = new Dictionary<AttributeType, int>();
        
        // ✅ Manager references
        private CharacterManager CharManager => GameBootstrap.Character;
        private CharacterStats CurrentPlayer => CharManager?.CurrentPlayer;
        private CharacterBaseStatsSO BaseStats => CharManager?.BaseStats;
        #endregion
        
        #region Unity Callbacks
        private void Start()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            // ✅ Subscribe to game events (reactive UI)
            GameEvents.OnStatsRecalculated += OnStatsChanged;
            GameEvents.OnPlayerNameChanged += OnPlayerNameChanged;
            
            // Refresh display when panel opens
            if (CurrentPlayer != null)
            {
                InitializeTempState();
                RefreshDisplay();
            }
        }

        private void OnDisable()
        {
            // ✅ Unsubscribe from events
            GameEvents.OnStatsRecalculated -= OnStatsChanged;
            GameEvents.OnPlayerNameChanged -= OnPlayerNameChanged;
        }
        #endregion
        
        #region Event Handlers (Reactive UI)
        /// <summary>
        /// ✅ React to stat changes (equipment, level-up, etc.)
        /// </summary>
        private void OnStatsChanged(CharacterStats stats)
        {
            if (stats == CurrentPlayer)
            {
                RefreshDisplay();
            }
        }

        /// <summary>
        /// ✅ React to name changes
        /// </summary>
        private void OnPlayerNameChanged(string newName)
        {
            if (playerNameText != null)
            {
                playerNameText.text = newName;
            }
        }
        #endregion
        
        #region Public API (Display Control)
        /// <summary>
        /// ✅ PURE UI: Open the profile panel
        /// Called by GlobalMenuController or other navigation systems
        /// </summary>
        public void OpenPanel()
        {
            if (CurrentPlayer == null)
            {
                Debug.LogWarning("[ProfilePanelUI] No active player!");
                return;
            }

            InitializeTempState();
            RefreshDisplay();
            gameObject.SetActive(true);
        }

        /// <summary>
        /// ✅ PURE UI: Close the panel
        /// </summary>
        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }
        #endregion
        
        #region Private Methods - Setup
        private void SetupButtons()
        {
            SetupAttributeButtons();
            SetupActionButtons();
        }
        
        private void SetupAttributeButtons()
        {
            strMinusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.STR, -1));
            strPlusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.STR, 1));
            
            intMinusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.INT, -1));
            intPlusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.INT, 1));
            
            agiMinusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.AGI, -1));
            agiPlusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.AGI, 1));
            
            endMinusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.END, -1));
            endPlusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.END, 1));
            
            wisMinusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.WIS, -1));
            wisPlusBtn?.onClick.AddListener(() => OnAttributeButtonClicked(AttributeType.WIS, 1));
        }
        
        private void SetupActionButtons()
        {
            confirmButton?.onClick.AddListener(OnConfirmClicked);
            resetButton?.onClick.AddListener(OnResetClicked);
            closeButton?.onClick.AddListener(OnCloseClicked);
        }
        #endregion
        
        #region Private Methods - Temp State Management
        /// <summary>
        /// ✅ PURE UI: Initialize temp state from current player
        /// </summary>
        private void InitializeTempState()
        {
            if (CurrentPlayer == null) return;

            // Clone current attributes for temp editing
            tempAttributes = CurrentPlayer.attributes.Clone();
            tempPointsSpent = 0;
            
            // Store original values for comparison
            originalAttributes[AttributeType.STR] = CurrentPlayer.attributes.STR;
            originalAttributes[AttributeType.INT] = CurrentPlayer.attributes.INT;
            originalAttributes[AttributeType.AGI] = CurrentPlayer.attributes.AGI;
            originalAttributes[AttributeType.END] = CurrentPlayer.attributes.END;
            originalAttributes[AttributeType.WIS] = CurrentPlayer.attributes.WIS;
        }
        #endregion
        
        #region Private Methods - Input Handlers
        /// <summary>
        /// ✅ PURE UI: Update temp attributes for preview
        /// NO actual stat changes
        /// </summary>
        private void OnAttributeButtonClicked(AttributeType attributeType, int change)
        {
            if (CurrentPlayer == null) return;

            int currentValue = tempAttributes.GetAttribute(attributeType);
            int originalValue = originalAttributes[attributeType];
            int newValue = currentValue + change;

            // Validation: Can't go below original value
            if (newValue < originalValue)
            {
                return;
            }

            // Validation: Need points to increase
            if (change > 0 && tempPointsSpent >= CurrentPlayer.AttributePoints)
            {
                return;
            }

            // Apply temp change
            tempAttributes.SetAttribute(attributeType, newValue);
            tempPointsSpent += change;

            RefreshDisplay();
        }

        /// <summary>
        /// ✅ PURE UI: Delegate to manager, NO save decision
        /// </summary>
        private void OnConfirmClicked()
        {
            if (CurrentPlayer == null || CharManager == null)
            {
                Debug.LogError("[ProfilePanelUI] No active player or CharacterManager!");
                return;
            }

            // Validation: Must have changes
            if (tempPointsSpent <= 0)
            {
                Debug.LogWarning("[ProfilePanelUI] No points allocated!");
                return;
            }

            // ✅ Delegate to manager (manager handles save)
            bool success = CharManager.ApplyAttributePoints(tempAttributes, tempPointsSpent);

            if (success)
            {
                Debug.Log($"[ProfilePanelUI] Allocated {tempPointsSpent} attribute points");
                
                // Reset temp state after successful allocation
                tempPointsSpent = 0;
                InitializeTempState();
                RefreshDisplay();
            }
            else
            {
                Debug.LogError("[ProfilePanelUI] Failed to apply attribute points!");
            }
        }

        /// <summary>
        /// ✅ PURE UI: Reset temp changes
        /// </summary>
        private void OnResetClicked()
        {
            if (tempPointsSpent == 0)
            {
                Debug.Log("[ProfilePanelUI] No changes to reset");
                return;
            }

            InitializeTempState();
            RefreshDisplay();
            
            Debug.Log("[ProfilePanelUI] Attribute changes reset");
        }

        /// <summary>
        /// ✅ PURE UI: Close panel (auto-reset if unsaved changes)
        /// </summary>
        private void OnCloseClicked()
        {
            if (tempPointsSpent > 0)
            {
                Debug.LogWarning("[ProfilePanelUI] Discarding unsaved attribute changes");
                InitializeTempState();
            }

            ClosePanel();
        }
        #endregion
        
        #region Private Methods - Display Updates
        /// <summary>
        /// ✅ PURE UI: Refresh all visual elements
        /// </summary>
        private void RefreshDisplay()
        {
            UpdatePlayerInfo();
            UpdateAttributeDisplay();
            UpdatePlayerPreview();
            UpdateButtonStates();
        }
        
        private void UpdatePlayerInfo()
        {
            if (CurrentPlayer == null) return;

            if (playerNameText != null)
            {
                playerNameText.text = CurrentPlayer.playerName;
            }
            
            if (playerLevelText != null)
            {
                playerLevelText.text = FormatPlayerLevel();
            }
            
            if (guildRankText != null)
            {
                guildRankText.text = CurrentPlayer.guildRank;
            }
        }
        
        private string FormatPlayerLevel()
        {
            if (CurrentPlayer.IsTranscended)
            {
                return $"Lv.{CurrentPlayer.Level} (T{CurrentPlayer.TranscendenceLevel})";
            }
            
            return $"Lv.{CurrentPlayer.Level}";
        }
        
        private void UpdateAttributeDisplay()
        {
            if (CurrentPlayer == null) return;

            if (strValueText != null)
            {
                strValueText.text = FormatAttributeText(AttributeType.STR);
            }
            if (intValueText != null)
            {
                intValueText.text = FormatAttributeText(AttributeType.INT);
            }
            if (agiValueText != null)
            {
                agiValueText.text = FormatAttributeText(AttributeType.AGI);
            }
            if (endValueText != null)
            {
                endValueText.text = FormatAttributeText(AttributeType.END);
            }
            if (wisValueText != null)
            {
                wisValueText.text = FormatAttributeText(AttributeType.WIS);
            }
            
            UpdatePointsDisplay();
        }
        
        private void UpdatePointsDisplay()
        {
            if (CurrentPlayer == null || pointsValueText == null) return;

            int pointsRemaining = CurrentPlayer.AttributePoints - tempPointsSpent;
            
            pointsValueText.text = pointsRemaining.ToString();
            pointsValueText.color = pointsRemaining > 0 
                ? pointsAvailableColor 
                : pointsNormalColor;
        }
        
        private string FormatAttributeText(AttributeType type)
        {
            int original = originalAttributes[type];
            int temp = tempAttributes.GetAttribute(type);
            
            if (temp > original)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(attributeIncreasedColor);
                return $"{temp} <color=#{colorHex}>(+{temp - original})</color>";
            }
            
            return temp.ToString();
        }
        
        private void UpdatePlayerPreview()
        {
            if (playerPreview == null)
            {
                Debug.LogError("[ProfilePanelUI] PlayerPreview not assigned!");
                return;
            }

            if (CurrentPlayer == null || BaseStats == null) return;

            // ✅ Preview stats with temp attributes
            playerPreview.PreviewStats(
                BaseStats, 
                CurrentPlayer.Level, 
                tempAttributes, // Use temp attributes for preview
                CurrentPlayer.itemStats,
                CurrentPlayer.equippedWeapon
            );
        }
        
        private void UpdateButtonStates()
        {
            if (CurrentPlayer == null) return;

            bool hasPointsToSpend = tempPointsSpent < CurrentPlayer.AttributePoints;
            bool hasChanges = tempPointsSpent > 0;
            
            // Plus buttons - enabled if points available
            SetPlusButtonStates(hasPointsToSpend);
            
            // Minus buttons - enabled if value > original
            SetMinusButtonStates();
            
            // Action buttons
            if (confirmButton != null)
            {
                confirmButton.interactable = hasChanges;
            }
            if (resetButton != null)
            {
                resetButton.interactable = hasChanges;
            }
        }
        
        private void SetPlusButtonStates(bool enabled)
        {
            if (strPlusBtn != null) strPlusBtn.interactable = enabled;
            if (intPlusBtn != null) intPlusBtn.interactable = enabled;
            if (agiPlusBtn != null) agiPlusBtn.interactable = enabled;
            if (endPlusBtn != null) endPlusBtn.interactable = enabled;
            if (wisPlusBtn != null) wisPlusBtn.interactable = enabled;
        }
        
        private void SetMinusButtonStates()
        {
            if (strMinusBtn != null)
            {
                strMinusBtn.interactable = tempAttributes.STR > originalAttributes[AttributeType.STR];
            }
            if (intMinusBtn != null)
            {
                intMinusBtn.interactable = tempAttributes.INT > originalAttributes[AttributeType.INT];
            }
            if (agiMinusBtn != null)
            {
                agiMinusBtn.interactable = tempAttributes.AGI > originalAttributes[AttributeType.AGI];
            }
            if (endMinusBtn != null)
            {
                endMinusBtn.interactable = tempAttributes.END > originalAttributes[AttributeType.END];
            }
            if (wisMinusBtn != null)
            {
                wisMinusBtn.interactable = tempAttributes.WIS > originalAttributes[AttributeType.WIS];
            }
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
                Debug.LogWarning("[ProfilePanelUI] Only works in Play Mode");
            }
        }

        [ContextMenu("Debug: Close Panel")]
        private void DebugClosePanel()
        {
            if (Application.isPlaying)
            {
                ClosePanel();
            }
            else
            {
                Debug.LogWarning("[ProfilePanelUI] Only works in Play Mode");
            }
        }

        [ContextMenu("Debug: Grant 10 Points")]
        private void DebugGrantPoints()
        {
            if (Application.isPlaying && CurrentPlayer != null)
            {
                CurrentPlayer.levelSystem.unallocatedPoints += 10;
                RefreshDisplay();
                Debug.Log("[ProfilePanelUI] Granted 10 attribute points");
            }
        }
#endif
        #endregion
    }
}