// ════════════════════════════════════════════
// ProfilePanelManager.cs - FIXED: GetOriginalValue
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Ascension.App;
using Ascension.Character.Stat;
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
        [SerializeField] private PlayerPreviewUI playerPreview;
        
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
        
        // ✅ FIX: Store original values in a dictionary
        private Dictionary<string, int> _originalAttributes = new Dictionary<string, int>();
        #endregion
        
        #region Properties
        private CharacterBaseStatsSO BaseStats => GameManager.Instance.BaseStats;
        private CharacterStats Player => GameManager.Instance.CurrentPlayer;
        #endregion
        
        #region Unity Callbacks
        private void Start()
        {
            SetupButtons();
        }
        
        private void OnEnable()
        {
            if (debugMode)
                Debug.Log("[ProfilePanelManager] OnEnable called");
            
            if (!ValidateGameManager())
                return;
            
            InitializeTempValues();
            RefreshUI();
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
            // ✅ FIX: Pass attribute name instead of ref int
            strMinusBtn?.onClick.AddListener(() => ModifyAttribute("STR", -1));
            strPlusBtn?.onClick.AddListener(() => ModifyAttribute("STR", 1));
            
            intMinusBtn?.onClick.AddListener(() => ModifyAttribute("INT", -1));
            intPlusBtn?.onClick.AddListener(() => ModifyAttribute("INT", 1));
            
            agiMinusBtn?.onClick.AddListener(() => ModifyAttribute("AGI", -1));
            agiPlusBtn?.onClick.AddListener(() => ModifyAttribute("AGI", 1));
            
            endMinusBtn?.onClick.AddListener(() => ModifyAttribute("END", -1));
            endPlusBtn?.onClick.AddListener(() => ModifyAttribute("END", 1));
            
            wisMinusBtn?.onClick.AddListener(() => ModifyAttribute("WIS", -1));
            wisPlusBtn?.onClick.AddListener(() => ModifyAttribute("WIS", 1));
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
            
            // ✅ FIX: Store original values for comparison
            _originalAttributes["STR"] = Player.attributes.STR;
            _originalAttributes["INT"] = Player.attributes.INT;
            _originalAttributes["AGI"] = Player.attributes.AGI;
            _originalAttributes["END"] = Player.attributes.END;
            _originalAttributes["WIS"] = Player.attributes.WIS;
        }
        
        // ✅ FIX: Cleaner attribute modification with string key
        private void ModifyAttribute(string attributeName, int change)
        {
            ref int tempAttribute = ref GetTempAttribute(attributeName);
            int originalValue = _originalAttributes[attributeName];
            
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
        
        // Helper to get reference to temp attribute by name
        private ref int GetTempAttribute(string attributeName)
        {
            switch (attributeName)
            {
                case "STR": return ref tempSTR;
                case "INT": return ref tempINT;
                case "AGI": return ref tempAGI;
                case "END": return ref tempEND;
                case "WIS": return ref tempWIS;
                default: throw new System.ArgumentException($"Invalid attribute: {attributeName}");
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
            if (strValueText) strValueText.text = FormatAttributeText(_originalAttributes["STR"], tempSTR);
            if (intValueText) intValueText.text = FormatAttributeText(_originalAttributes["INT"], tempINT);
            if (agiValueText) agiValueText.text = FormatAttributeText(_originalAttributes["AGI"], tempAGI);
            if (endValueText) endValueText.text = FormatAttributeText(_originalAttributes["END"], tempEND);
            if (wisValueText) wisValueText.text = FormatAttributeText(_originalAttributes["WIS"], tempWIS);
            
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
            strMinusBtn.interactable = tempSTR > _originalAttributes["STR"];
            intMinusBtn.interactable = tempINT > _originalAttributes["INT"];
            agiMinusBtn.interactable = tempAGI > _originalAttributes["AGI"];
            endMinusBtn.interactable = tempEND > _originalAttributes["END"];
            wisMinusBtn.interactable = tempWIS > _originalAttributes["WIS"];
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
            SaveChanges();
            RefreshExternalUI();
            
            Debug.Log($"[ProfilePanelManager] Allocated {tempPointsSpent} attribute points");
            
            tempPointsSpent = 0;
            RefreshUI();
        }
        
        private void ApplyAttributeChanges()
        {
            Player.attributes.STR = tempSTR;
            Player.attributes.INT = tempINT;
            Player.attributes.AGI = tempAGI;
            Player.attributes.END = tempEND;
            Player.attributes.WIS = tempWIS;
            Player.levelSystem.unallocatedPoints -= tempPointsSpent;
            
            Player.RecalculateStats(BaseStats, fullHeal: false);
        }
        
        private void SaveChanges()
        {
            GameManager.Instance.SaveGame();
        }
        
        private void RefreshExternalUI()
        {
            PlayerHUD hud = FindObjectOfType<PlayerHUD>();
            if (hud != null) 
                hud.RefreshHUD();
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