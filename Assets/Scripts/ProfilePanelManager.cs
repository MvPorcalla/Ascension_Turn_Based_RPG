// ════════════════════════════════════════════
// ProfilePanelManager.cs
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Ascension.App;
using Ascension.Character.Stat;
using Ascension.Character.Manager;
using Ascension.Data.SO.Character;

namespace Ascension.UI
{
    public class ProfilePanelManager : MonoBehaviour
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
        
        [Header("Attribute Values")]
        [SerializeField] private TMP_Text strValueText;
        [SerializeField] private TMP_Text intValueText;
        [SerializeField] private TMP_Text agiValueText;
        [SerializeField] private TMP_Text endValueText;
        [SerializeField] private TMP_Text wisValueText;
        [SerializeField] private TMP_Text pointsValueText;
        
        [Header("Action Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button backButton;
        
        [Header("Visual Settings")]
        [SerializeField] private Color pointsAvailableColor = Color.green;
        [SerializeField] private Color pointsNormalColor = Color.white;
        [SerializeField] private Color attributeIncreasedColor = Color.green;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        #endregion
        
        #region Private Fields
        private int tempSTR, tempINT, tempAGI, tempEND, tempWIS;
        private int tempPointsSpent = 0;
        
        // ✅ FIXED: Use AttributeType enum instead of string
        private Dictionary<AttributeType, int> _originalAttributes = new Dictionary<AttributeType, int>();
        private bool isInitialized = false;
        #endregion
        
        #region Properties
        private CharacterBaseStatsSO BaseStats => GameManager.Instance.BaseStats;
        private CharacterStats Player => GameManager.Instance.CurrentPlayer;
        #endregion
        
        #region Unity Callbacks
        private void OnEnable()
        {
            if (debugMode)
                Debug.Log("[ProfilePanelManager] OnEnable called");
            
            // ✅ MOVE BUTTON SETUP HERE (in case buttons were destroyed/recreated)
            if (!isInitialized)
            {
                SetupButtons();
                isInitialized = true;
            }
            
            if (!ValidateGameManager())
                return;
            
            InitializeTempValues();
            RefreshUI();
        }

        private void OnDisable()
        {
            // ProfilePanelManager doesn't subscribe to GameEvents
            // But good to have for consistency
            if (debugMode)
                Debug.Log("[ProfilePanelManager] OnDisable called");
        }
        #endregion
        
        #region Public Methods
        public void OpenProfile()
        {
            InitializeTempValues();
            RefreshUI();
            gameObject.SetActive(true);
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
            // ✅ FIXED: Use enum instead of string
            strMinusBtn?.onClick.AddListener(() => ModifyAttribute(AttributeType.STR, -1));
            strPlusBtn?.onClick.AddListener(() => ModifyAttribute(AttributeType.STR, 1));
            
            intMinusBtn?.onClick.AddListener(() => ModifyAttribute(AttributeType.INT, -1));
            intPlusBtn?.onClick.AddListener(() => ModifyAttribute(AttributeType.INT, 1));
            
            agiMinusBtn?.onClick.AddListener(() => ModifyAttribute(AttributeType.AGI, -1));
            agiPlusBtn?.onClick.AddListener(() => ModifyAttribute(AttributeType.AGI, 1));
            
            endMinusBtn?.onClick.AddListener(() => ModifyAttribute(AttributeType.END, -1));
            endPlusBtn?.onClick.AddListener(() => ModifyAttribute(AttributeType.END, 1));
            
            wisMinusBtn?.onClick.AddListener(() => ModifyAttribute(AttributeType.WIS, -1));
            wisPlusBtn?.onClick.AddListener(() => ModifyAttribute(AttributeType.WIS, 1));
        }
        
        private void SetupActionButtons()
        {
            confirmButton?.onClick.AddListener(OnConfirmClicked);
            backButton?.onClick.AddListener(OnBackClicked);
        }
        
        private bool ValidateGameManager()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[ProfilePanelManager] GameManager.Instance is NULL!");
                return false;
            }
            
            if (!GameManager.Instance.HasActivePlayer)
            {
                Debug.LogError("[ProfilePanelManager] No active player!");
                return false;
            }
            
            return true;
        }
        #endregion
        
        #region Private Methods - Temp Value Management
        private void InitializeTempValues()
        {
            tempSTR = Player.attributes.STR;
            tempINT = Player.attributes.INT;
            tempAGI = Player.attributes.AGI;
            tempEND = Player.attributes.END;
            tempWIS = Player.attributes.WIS;
            tempPointsSpent = 0;
            
            // ✅ FIXED: Use enum keys
            _originalAttributes[AttributeType.STR] = Player.attributes.STR;
            _originalAttributes[AttributeType.INT] = Player.attributes.INT;
            _originalAttributes[AttributeType.AGI] = Player.attributes.AGI;
            _originalAttributes[AttributeType.END] = Player.attributes.END;
            _originalAttributes[AttributeType.WIS] = Player.attributes.WIS;
        }
        
        // ✅ FIXED: Use enum parameter instead of string
        private void ModifyAttribute(AttributeType attributeType, int change)
        {
            ref int tempAttribute = ref GetTempAttribute(attributeType);
            int originalValue = _originalAttributes[attributeType];
            
            if (change > 0)
            {
                if (!CanAllocatePoints()) return;
                
                tempAttribute++;
                tempPointsSpent++;
            }
            else if (change < 0)
            {
                if (tempAttribute <= originalValue) return;
                
                tempAttribute--;
                tempPointsSpent--;
            }
            
            RefreshUI();
        }
        
        // ✅ FIXED: Use enum parameter instead of string
        private ref int GetTempAttribute(AttributeType attributeType)
        {
            switch (attributeType)
            {
                case AttributeType.STR: return ref tempSTR;
                case AttributeType.INT: return ref tempINT;
                case AttributeType.AGI: return ref tempAGI;
                case AttributeType.END: return ref tempEND;
                case AttributeType.WIS: return ref tempWIS;
                default: throw new System.ArgumentException($"Invalid attribute: {attributeType}");
            }
        }
        
        private bool CanAllocatePoints()
        {
            return tempPointsSpent < Player.UnallocatedPoints;
        }
        #endregion
        
        #region Private Methods - UI Updates
        private void RefreshUI()
        {
            UpdatePlayerInfo();
            UpdateAttributeDisplay();
            UpdatePlayerPreview();
            UpdateButtonStates();
        }
        
        private void UpdatePlayerInfo()
        {
            if (playerNameText) 
                playerNameText.text = Player.playerName;
            
            if (playerLevelText)
                playerLevelText.text = FormatPlayerLevel();
            
            if (guildRankText)
                guildRankText.text = Player.guildRank;
        }
        
        private string FormatPlayerLevel()
        {
            if (Player.IsTranscended)
                return $"Lv.{Player.Level} (T{Player.TranscendenceLevel})";
            
            return $"Lv.{Player.Level}";
        }
        
        private void UpdateAttributeDisplay()
        {
            // ✅ FIXED: Use enum keys
            if (strValueText) strValueText.text = FormatAttributeText(_originalAttributes[AttributeType.STR], tempSTR);
            if (intValueText) intValueText.text = FormatAttributeText(_originalAttributes[AttributeType.INT], tempINT);
            if (agiValueText) agiValueText.text = FormatAttributeText(_originalAttributes[AttributeType.AGI], tempAGI);
            if (endValueText) endValueText.text = FormatAttributeText(_originalAttributes[AttributeType.END], tempEND);
            if (wisValueText) wisValueText.text = FormatAttributeText(_originalAttributes[AttributeType.WIS], tempWIS);
            
            UpdatePointsDisplay();
        }
        
        private void UpdatePointsDisplay()
        {
            int pointsRemaining = Player.UnallocatedPoints - tempPointsSpent;
            
            if (pointsValueText)
            {
                pointsValueText.text = pointsRemaining.ToString();
                pointsValueText.color = pointsRemaining > 0 ? pointsAvailableColor : pointsNormalColor;
            }
        }
        
        private string FormatAttributeText(int original, int temp)
        {
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
                Debug.LogError("[ProfilePanelManager] PlayerPreview is NULL! Assign in Inspector.");
                return;
            }
            
            if (debugMode)
                Debug.Log($"[ProfilePanelManager] UpdatePlayerPreview - STR:{tempSTR} INT:{tempINT}");
            
            CharacterAttributes tempAttributes = new CharacterAttributes(tempSTR, tempINT, tempAGI, tempEND, tempWIS);
            
            playerPreview.PreviewStats(
                BaseStats, 
                Player.Level, 
                tempAttributes, 
                Player.itemStats,
                Player.equippedWeapon
            );
        }
        
        private void UpdateButtonStates()
        {
            bool hasPointsToSpend = tempPointsSpent < Player.UnallocatedPoints;
            bool hasChanges = tempPointsSpent > 0;
            
            UpdatePlusButtons(hasPointsToSpend);
            UpdateMinusButtons();
            
            confirmButton.interactable = hasChanges;
        }
        
        private void UpdatePlusButtons(bool hasPoints)
        {
            strPlusBtn.interactable = hasPoints;
            intPlusBtn.interactable = hasPoints;
            agiPlusBtn.interactable = hasPoints;
            endPlusBtn.interactable = hasPoints;
            wisPlusBtn.interactable = hasPoints;
        }
        
        private void UpdateMinusButtons()
        {
            // ✅ FIXED: Use enum keys
            strMinusBtn.interactable = tempSTR > _originalAttributes[AttributeType.STR];
            intMinusBtn.interactable = tempINT > _originalAttributes[AttributeType.INT];
            agiMinusBtn.interactable = tempAGI > _originalAttributes[AttributeType.AGI];
            endMinusBtn.interactable = tempEND > _originalAttributes[AttributeType.END];
            wisMinusBtn.interactable = tempWIS > _originalAttributes[AttributeType.WIS];
        }
        #endregion
        
        #region Private Methods - Button Callbacks
        private void OnConfirmClicked()
        {
            if (tempPointsSpent <= 0)
            {
                Debug.LogWarning("[ProfilePanelManager] No points to allocate!");
                return;
            }
            
            ApplyAttributeChanges();
            GameManager.Instance.SaveGame();
            
            Debug.Log($"[ProfilePanelManager] Allocated {tempPointsSpent} attribute points");
            
            tempPointsSpent = 0;
            RefreshUI();
        }

        private void ApplyAttributeChanges()
        {
            var tempAttributes = new CharacterAttributes(tempSTR, tempINT, tempAGI, tempEND, tempWIS);
            
            bool success = CharacterManager.Instance.ApplyAttributePoints(tempAttributes, tempPointsSpent);
            
            if (!success)
            {
                Debug.LogError("[ProfilePanelManager] Failed to apply attribute points!");
            }
        }
        
        private void OnBackClicked()
        {
            if (tempPointsSpent != 0)
            {
                InitializeTempValues();
                RefreshUI();
            }
            
            gameObject.SetActive(false);
        }
        #endregion
    }
}