// -------------------------------
// ProfilePanelManager.cs
// Main profile panel for viewing and allocating attributes
// -------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfilePanelManager : MonoBehaviour
{
    [Header("Player Info Section")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerLevelText;
    [SerializeField] private TMP_Text guildRankText;
    
    [Header("Player Preview Prefab")]
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
    
    [Header("Attribute Value Text")]
    [SerializeField] private TMP_Text strValueText;
    [SerializeField] private TMP_Text intValueText;
    [SerializeField] private TMP_Text agiValueText;
    [SerializeField] private TMP_Text endValueText;
    [SerializeField] private TMP_Text wisValueText;
    [SerializeField] private TMP_Text pointsValueText;
    
    [Header("Action Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;
    
    [Header("Colors")]
    [SerializeField] private Color pointsAvailableColor = Color.green;
    [SerializeField] private Color pointsNormalColor = Color.white;
    [SerializeField] private Color attributeIncreasedColor = Color.green;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Temp allocation tracking
    private int tempSTR, tempINT, tempAGI, tempEND, tempWIS;
    private int tempPointsSpent = 0;
    
    // Quick access properties
    private CharacterBaseStatsSO BaseStats => GameManager.Instance.BaseStats;
    private PlayerStats Player => GameManager.Instance.CurrentPlayer;
    
    private void Start()
    {
        SetupButtons();
    }
    
    private void OnEnable()
    {
        if (debugMode)
            Debug.Log("[ProfilePanelManager] OnEnable called");
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("[ProfilePanelManager] GameManager.Instance is NULL!");
            return;
        }
        
        if (!GameManager.Instance.HasActivePlayer)
        {
            Debug.LogError("[ProfilePanelManager] No active player!");
            return;
        }
        
        InitializeTempValues();
        RefreshUI();
    }
    
    private void SetupButtons()
    {
        // Attribute buttons
        strMinusBtn?.onClick.AddListener(() => ModifyTempAttribute(ref tempSTR, -1));
        strPlusBtn?.onClick.AddListener(() => ModifyTempAttribute(ref tempSTR, 1));
        
        intMinusBtn?.onClick.AddListener(() => ModifyTempAttribute(ref tempINT, -1));
        intPlusBtn?.onClick.AddListener(() => ModifyTempAttribute(ref tempINT, 1));
        
        agiMinusBtn?.onClick.AddListener(() => ModifyTempAttribute(ref tempAGI, -1));
        agiPlusBtn?.onClick.AddListener(() => ModifyTempAttribute(ref tempAGI, 1));
        
        endMinusBtn?.onClick.AddListener(() => ModifyTempAttribute(ref tempEND, -1));
        endPlusBtn?.onClick.AddListener(() => ModifyTempAttribute(ref tempEND, 1));
        
        wisMinusBtn?.onClick.AddListener(() => ModifyTempAttribute(ref tempWIS, -1));
        wisPlusBtn?.onClick.AddListener(() => ModifyTempAttribute(ref tempWIS, 1));
        
        // Action buttons
        confirmButton?.onClick.AddListener(OnConfirmClicked);
        backButton?.onClick.AddListener(OnBackClicked);
    }
    
    private void InitializeTempValues()
    {
        tempSTR = Player.attributes.STR;
        tempINT = Player.attributes.INT;
        tempAGI = Player.attributes.AGI;
        tempEND = Player.attributes.END;
        tempWIS = Player.attributes.WIS;
        tempPointsSpent = 0;
    }
    
    private void ModifyTempAttribute(ref int tempAttribute, int change)
    {
        if (change > 0)
        {
            // Check if we have points to spend
            if (tempPointsSpent >= Player.UnallocatedPoints)
                return;
            
            tempAttribute++;
            tempPointsSpent++;
        }
        else if (change < 0)
        {
            // Get the original value for this attribute
            int originalValue = GetOriginalValue(ref tempAttribute);
            
            // Can't go below original value
            if (tempAttribute <= originalValue)
                return;
            
            tempAttribute--;
            tempPointsSpent--;
        }
        
        RefreshUI();
    }
    
    private int GetOriginalValue(ref int tempAttribute)
    {
        // Compare by reference to determine which attribute this is
        if (ReferenceEquals(tempAttribute, tempSTR)) return Player.attributes.STR;
        if (ReferenceEquals(tempAttribute, tempINT)) return Player.attributes.INT;
        if (ReferenceEquals(tempAttribute, tempAGI)) return Player.attributes.AGI;
        if (ReferenceEquals(tempAttribute, tempEND)) return Player.attributes.END;
        if (ReferenceEquals(tempAttribute, tempWIS)) return Player.attributes.WIS;
        return 0;
    }
    
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
            playerLevelText.text = $"Lv.{Player.Level}";
        
        if (guildRankText)
        {
            guildRankText.text = Player.IsTranscended 
                ? $"Transcended (T{Player.TranscendenceLevel})" 
                : "Adventurer";
        }
    }
    
    private void UpdateAttributeDisplay()
    {
        // Display attributes with color coding for increases
        if (strValueText) strValueText.text = FormatAttributeText(Player.attributes.STR, tempSTR);
        if (intValueText) intValueText.text = FormatAttributeText(Player.attributes.INT, tempINT);
        if (agiValueText) agiValueText.text = FormatAttributeText(Player.attributes.AGI, tempAGI);
        if (endValueText) endValueText.text = FormatAttributeText(Player.attributes.END, tempEND);
        if (wisValueText) wisValueText.text = FormatAttributeText(Player.attributes.WIS, tempWIS);
        
        // Update points remaining
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
            Debug.LogError("[ProfilePanelManager] PlayerPreview is NULL! Assign it in Inspector.");
            return;
        }
        
        if (debugMode)
            Debug.Log($"[ProfilePanelManager] UpdatePlayerPreview - STR:{tempSTR} INT:{tempINT}");
        
        // Create temp attributes for preview
        PlayerAttributes tempAttributes = new PlayerAttributes(tempSTR, tempINT, tempAGI, tempEND, tempWIS);
        
        // Preview stats with temp attributes
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
        bool hasPointsAllocated = tempPointsSpent > 0;
        
        // Plus buttons - only active if points available
        strPlusBtn.interactable = hasPointsToSpend;
        intPlusBtn.interactable = hasPointsToSpend;
        agiPlusBtn.interactable = hasPointsToSpend;
        endPlusBtn.interactable = hasPointsToSpend;
        wisPlusBtn.interactable = hasPointsToSpend;
        
        // Minus buttons - only active if attribute is above original
        strMinusBtn.interactable = tempSTR > Player.attributes.STR;
        intMinusBtn.interactable = tempINT > Player.attributes.INT;
        agiMinusBtn.interactable = tempAGI > Player.attributes.AGI;
        endMinusBtn.interactable = tempEND > Player.attributes.END;
        wisMinusBtn.interactable = tempWIS > Player.attributes.WIS;
        
        // Confirm button - only active if changes were made
        confirmButton.interactable = hasPointsAllocated;
    }
    
    private void OnConfirmClicked()
    {
        if (tempPointsSpent <= 0)
        {
            Debug.LogWarning("[ProfilePanelManager] No points to allocate!");
            return;
        }
        
        // Apply changes to actual stats
        Player.attributes.STR = tempSTR;
        Player.attributes.INT = tempINT;
        Player.attributes.AGI = tempAGI;
        Player.attributes.END = tempEND;
        Player.attributes.WIS = tempWIS;
        Player.levelSystem.unallocatedPoints -= tempPointsSpent;
        
        // Recalculate stats without full heal
        Player.RecalculateStats(BaseStats, fullHeal: false);
        
        // Save game
        GameManager.Instance.SaveGame();
        
        Debug.Log($"[ProfilePanelManager] Allocated {tempPointsSpent} attribute points successfully!");
        
        // Reset temp tracking
        tempPointsSpent = 0;
        
        // Refresh the HUD if it exists
        PlayerHUD hud = FindObjectOfType<PlayerHUD>();
        if (hud != null) 
            hud.RefreshHUD();
        
        RefreshUI();
    }
    
    private void OnBackClicked()
    {
        // Reset temp attributes if there are unsaved changes
        if (tempPointsSpent != 0)
        {
            InitializeTempValues();
            RefreshUI();
        }
        
        // Close the profile panel
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Public method to open the profile panel
    /// </summary>
    public void OpenProfile()
    {
        InitializeTempValues();
        RefreshUI();
        gameObject.SetActive(true);
    }
}