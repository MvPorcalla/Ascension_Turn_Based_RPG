// -------------------------------
// LevelUpManager.cs
// -------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject levelUpPanel;
    
    [Header("UI - Level Info")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text pointsRemainingText;
    [SerializeField] private TMP_Text transcendenceText;
    
    [Header("UI - Attribute Buttons")]
    [SerializeField] private Button strPlusBtn;
    [SerializeField] private Button intPlusBtn;
    [SerializeField] private Button agiPlusBtn;
    [SerializeField] private Button endPlusBtn;
    [SerializeField] private Button wisPlusBtn;
    
    [Header("UI - Attribute Display")]
    [SerializeField] private TMP_Text strText;
    [SerializeField] private TMP_Text intText;
    [SerializeField] private TMP_Text agiText;
    [SerializeField] private TMP_Text endText;
    [SerializeField] private TMP_Text wisText;
    
    [Header("UI - Combat Stats Preview")]
    [SerializeField] private TMP_Text adText;
    [SerializeField] private TMP_Text apText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text attackSpeedText;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text critRateText;
    [SerializeField] private TMP_Text evasionText;
    
    [Header("UI - Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button closeButton;

    // Get base stats from CharacterManager
    private CharacterBaseStatsSO baseStats => CharacterManager.Instance?.BaseStats;
    private PlayerStats playerStats;
    
    // Temp allocation state
    private PlayerAttributes tempAttributes;
    private int tempPointsSpent = 0;
    
    // Preview cache (reused to avoid allocations)
    private PlayerDerivedStats previewStats = new PlayerDerivedStats();
    
    private void Start()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
        
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        strPlusBtn.onClick.AddListener(() => AddPoint("STR"));
        intPlusBtn.onClick.AddListener(() => AddPoint("INT"));
        agiPlusBtn.onClick.AddListener(() => AddPoint("AGI"));
        endPlusBtn.onClick.AddListener(() => AddPoint("END"));
        wisPlusBtn.onClick.AddListener(() => AddPoint("WIS"));
        
        confirmButton.onClick.AddListener(OnConfirmClicked);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }
    
    public void OpenLevelUpPanel(PlayerStats stats)
    {
        if (stats.UnallocatedPoints <= 0)
        {
            Debug.Log("No points to allocate!");
            return;
        }
        
        playerStats = stats;
        
        // Clone current attributes for temporary modification
        tempAttributes = playerStats.attributes.Clone();
        tempPointsSpent = 0;
        
        if (levelUpPanel != null)
            levelUpPanel.SetActive(true);
        
        UpdateUI();
    }
    
    private void AddPoint(string attributeName)
    {
        if (tempPointsSpent >= playerStats.UnallocatedPoints)
            return;
        
        switch (attributeName)
        {
            case "STR": tempAttributes.STR++; break;
            case "INT": tempAttributes.INT++; break;
            case "AGI": tempAttributes.AGI++; break;
            case "END": tempAttributes.END++; break;
            case "WIS": tempAttributes.WIS++; break;
        }
        
        tempPointsSpent++;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        // Level display with transcendence
        if (levelText)
        {
            if (playerStats.IsTranscended)
            {
                levelText.text = $"Level {playerStats.Level} (T{playerStats.TranscendenceLevel})";
            }
            else
            {
                levelText.text = $"Level {playerStats.Level}";
            }
        }
        
        // Transcendence display (optional separate field)
        if (transcendenceText != null)
        {
            if (playerStats.IsTranscended)
            {
                transcendenceText.gameObject.SetActive(true);
                transcendenceText.text = $"<color=gold>Transcendence Level {playerStats.TranscendenceLevel}</color>";
            }
            else
            {
                transcendenceText.gameObject.SetActive(false);
            }
        }
        
        // Points remaining
        int pointsRemaining = playerStats.UnallocatedPoints - tempPointsSpent;
        if (pointsRemainingText) 
        { 
            pointsRemainingText.text = $"Points: {pointsRemaining}"; 
            pointsRemainingText.color = pointsRemaining > 0 ? Color.green : Color.red;
        }
        
        // Attributes with changes
        UpdateAttributeText(strText, tempAttributes.STR, playerStats.attributes.STR, "STR");
        UpdateAttributeText(intText, tempAttributes.INT, playerStats.attributes.INT, "INT");
        UpdateAttributeText(agiText, tempAttributes.AGI, playerStats.attributes.AGI, "AGI");
        UpdateAttributeText(endText, tempAttributes.END, playerStats.attributes.END, "END");
        UpdateAttributeText(wisText, tempAttributes.WIS, playerStats.attributes.WIS, "WIS");
        
        // Button states
        bool hasPoints = tempPointsSpent < playerStats.UnallocatedPoints;
        strPlusBtn.interactable = hasPoints;
        intPlusBtn.interactable = hasPoints;
        agiPlusBtn.interactable = hasPoints;
        endPlusBtn.interactable = hasPoints;
        wisPlusBtn.interactable = hasPoints;
        
        PreviewCombatStats();
    }
    
    private void UpdateAttributeText(TMP_Text text, int tempValue, int currentValue, string label)
    {
        if (text == null) return;
        
        string change = tempValue > currentValue ? $" <color=green>(+{tempValue - currentValue})</color>" : "";
        text.text = $"{label}: {tempValue}{change}";
    }
    
    private void PreviewCombatStats()
    {
        // Reuse previewStats (no new allocation!)
        // Pass player's equipped weapon for accurate preview
        previewStats.Recalculate(baseStats, playerStats.Level, tempAttributes, playerStats.itemStats, playerStats.equippedWeapon);
        
        if (adText) adText.text = $"AD: {previewStats.AD:F1}";
        if (apText) apText.text = $"AP: {previewStats.AP:F1}";
        if (hpText) hpText.text = $"HP: {previewStats.MaxHP:F0}";
        if (attackSpeedText) attackSpeedText.text = $"Speed: {previewStats.AttackSpeed:F1}";
        if (defenseText) defenseText.text = $"Defense: {previewStats.Defense:F1}";
        if (critRateText) critRateText.text = $"Crit: {previewStats.CritRate:F1}%";
        if (evasionText) evasionText.text = $"Evasion: {previewStats.Evasion:F1}%";
    }
    
    private void OnConfirmClicked()
    {
        if (tempPointsSpent < playerStats.UnallocatedPoints)
        {
            Debug.LogWarning($"You still have {playerStats.UnallocatedPoints - tempPointsSpent} points to spend!");
            return;
        }
        
        // Apply temp attributes to actual stats
        playerStats.attributes.CopyFrom(tempAttributes);
        playerStats.levelSystem.unallocatedPoints = 0;
        
        // Recalculate through CharacterManager
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.RecalculateStats();
        }
        else
        {
            // Fallback
            playerStats.RecalculateStats(baseStats, fullHeal: false);
        }
        
        GameManager.Instance.SaveGame();
        
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
        
        Debug.Log("Level up points allocated!");
    }
    
    private void OnCloseClicked()
    {
        if (playerStats.UnallocatedPoints > 0)
        {
            Debug.LogWarning("You must allocate all points before closing!");
            return;
        }
        
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }
    
    public void GainExperience(int exp)
    {
        // Use CharacterManager
        if (!CharacterManager.Instance.HasActivePlayer)
        {
            Debug.LogError("No active player!");
            return;
        }
        
        playerStats = CharacterManager.Instance.CurrentPlayer;
        int oldLevel = playerStats.Level;
        
        // Add experience through CharacterManager
        CharacterManager.Instance.AddExperience(exp);
        
        bool leveledUp = playerStats.Level > oldLevel;
        
        if (leveledUp)
        {
            Debug.Log($"LEVEL UP! Now level {playerStats.Level}");
            GameManager.Instance.SaveGame();
            OpenLevelUpPanel(playerStats);
        }
        else
        {
            GameManager.Instance.SaveGame();
        }
    }
}