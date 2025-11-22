// -------------------------------
// LevelUpManager.cs
// -------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the level-up UI panel for allocating attribute points.
/// Can be used as a popup panel or separate scene.
/// </summary>
public class LevelUpManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject levelUpPanel; // The UI panel to show/hide
    
    [Header("UI - Level Info")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text pointsRemainingText;
    
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
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text mrText;
    [SerializeField] private TMP_Text critRateText;
    
    [Header("UI - Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button closeButton;

    private CharacterBaseStatsSO baseStats => GameManager.Instance.BaseStats;

    private PlayerStats playerStats;
    private int tempSTR, tempINT, tempAGI, tempEND, tempWIS;
    private int tempPointsSpent = 0;
    
    private void Start()
    {
        // Hide panel initially
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
        
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        strPlusBtn.onClick.AddListener(() => AddPoint(ref tempSTR));
        intPlusBtn.onClick.AddListener(() => AddPoint(ref tempINT));
        agiPlusBtn.onClick.AddListener(() => AddPoint(ref tempAGI));
        endPlusBtn.onClick.AddListener(() => AddPoint(ref tempEND));
        wisPlusBtn.onClick.AddListener(() => AddPoint(ref tempWIS));
        
        confirmButton.onClick.AddListener(OnConfirmClicked);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }
    
    /// <summary>
    /// Opens the level-up panel. Call this when player levels up.
    /// </summary>
    public void OpenLevelUpPanel(PlayerStats stats)
    {
        if (stats.unallocatedPoints <= 0)
        {
            Debug.Log("No points to allocate!");
            return;
        }
        
        playerStats = stats;
        
        // Store current values in temp variables
        tempSTR = playerStats.STR;
        tempINT = playerStats.INT;
        tempAGI = playerStats.AGI;
        tempEND = playerStats.END;
        tempWIS = playerStats.WIS;
        tempPointsSpent = 0;
        
        if (levelUpPanel != null)
            levelUpPanel.SetActive(true);
        
        UpdateUI();
    }
    
    private void AddPoint(ref int attribute)
    {
        // Check if we have points to spend
        if (tempPointsSpent >= playerStats.unallocatedPoints)
            return;
        
        attribute++;
        tempPointsSpent++;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        // Update level display
        if (levelText)
            levelText.text = $"Level {playerStats.level}";
        
        // Update points remaining
        int pointsRemaining = playerStats.unallocatedPoints - tempPointsSpent;

        if (pointsRemainingText) 
        { pointsRemainingText.text = $"Points: {pointsRemaining}"; 
            if (pointsRemaining > 0) 
                pointsRemainingText.color = Color.green; 
            else 
                pointsRemainingText.color = Color.red; 
            }
        
        // Update attribute displays with changes highlighted
        if (strText)
        {
            string change = tempSTR > playerStats.STR ? $" <color=green>(+{tempSTR - playerStats.STR})</color>" : "";
            strText.text = $"STR: {tempSTR}{change}";
        }
        if (intText)
        {
            string change = tempINT > playerStats.INT ? $" <color=green>(+{tempINT - playerStats.INT})</color>" : "";
            intText.text = $"INT: {tempINT}{change}";
        }
        if (agiText)
        {
            string change = tempAGI > playerStats.AGI ? $" <color=green>(+{tempAGI - playerStats.AGI})</color>" : "";
            agiText.text = $"AGI: {tempAGI}{change}";
        }
        if (endText)
        {
            string change = tempEND > playerStats.END ? $" <color=green>(+{tempEND - playerStats.END})</color>" : "";
            endText.text = $"END: {tempEND}{change}";
        }
        if (wisText)
        {
            string change = tempWIS > playerStats.WIS ? $" <color=green>(+{tempWIS - playerStats.WIS})</color>" : "";
            wisText.text = $"WIS: {tempWIS}{change}";
        }
        
        // Update button interactability
        bool hasPoints = tempPointsSpent < playerStats.unallocatedPoints;
        strPlusBtn.interactable = hasPoints;
        intPlusBtn.interactable = hasPoints;
        agiPlusBtn.interactable = hasPoints;
        endPlusBtn.interactable = hasPoints;
        wisPlusBtn.interactable = hasPoints;
        
        // Preview combat stats with temp values
        PreviewCombatStats();
    }
    
    private void PreviewCombatStats()
    {
        // Create temp stats for preview
        PlayerStats previewStats = new PlayerStats
        {
            level = playerStats.level,
            STR = tempSTR,
            INT = tempINT,
            AGI = tempAGI,
            END = tempEND,
            WIS = tempWIS,
            // Copy item bonuses
            ItemAD = playerStats.ItemAD,
            ItemAP = playerStats.ItemAP,
            ItemHP = playerStats.ItemHP,
            ItemArmor = playerStats.ItemArmor,
            ItemMR = playerStats.ItemMR,
            ItemCritRate = playerStats.ItemCritRate,
        };
        
        previewStats.CalculateCombatStats(baseStats);
        
        // Display preview
        if (adText) adText.text = $"AD: {previewStats.AD:F1}";
        if (apText) apText.text = $"AP: {previewStats.AP:F1}";
        if (hpText) hpText.text = $"HP: {previewStats.HP:F0}";
        if (armorText) armorText.text = $"Armor: {previewStats.Armor:F1}";
        if (mrText) mrText.text = $"MR: {previewStats.MR:F1}";
        if (critRateText) critRateText.text = $"Crit: {previewStats.CritRate:F1}%";
    }
    
    private void OnConfirmClicked()
    {
        // Check if all points are spent
        if (tempPointsSpent < playerStats.unallocatedPoints)
        {
            Debug.LogWarning($"You still have {playerStats.unallocatedPoints - tempPointsSpent} points to spend!");
            // TODO: Add UI warning message for unspent points
            // TODO: Add UI warning message for empty name field
            return;
        }
        
        // Apply changes to actual stats
        playerStats.STR = tempSTR;
        playerStats.INT = tempINT;
        playerStats.AGI = tempAGI;
        playerStats.END = tempEND;
        playerStats.WIS = tempWIS;
        playerStats.unallocatedPoints = 0;
        
        // Recalculate combat stats
        playerStats.CalculateCombatStats(baseStats);
        
        // Save
        SavePlayerStats();
        
        // Close panel
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
        
        Debug.Log("Level up points allocated!");
    }
    
    private void OnCloseClicked()
    {
        // Don't allow closing if there are unspent points
        if (playerStats.unallocatedPoints > 0)
        {
            Debug.LogWarning("You must allocate all points before closing!");
            return;
        }
        
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }

    private void SavePlayerStats()
    {
        // NEW: Use GameManager
        GameManager.Instance.SaveGame();
    }
    
    /// <summary>
    /// Example: Call this after winning a battle
    /// </summary>
    public void GainExperience(int exp)
    {
        // Use GameManager as single source of truth
        playerStats = GameManager.Instance.CurrentPlayer;
        
        if (playerStats == null)
        {
            Debug.LogError("No player stats found!");
            return;
        }
        
        bool leveledUp = playerStats.AddExperience(exp, baseStats);
        
        if (leveledUp)
        {
            Debug.Log($"LEVEL UP! Now level {playerStats.level}");
            GameManager.Instance.SaveGame(); // Auto-save
            OpenLevelUpPanel(playerStats);
        }
        else
        {
            GameManager.Instance.SaveGame(); // Save exp progress too
        }
    }
}