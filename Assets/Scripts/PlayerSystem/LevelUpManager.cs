// -------------------------------
// LevelUpManager.cs (Updated for Attack Speed)
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
    [SerializeField] private TMP_Text transcendenceText; // NEW
    
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
    [SerializeField] private TMP_Text attackSpeedText; // NEW
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text critRateText;
    [SerializeField] private TMP_Text evasionText;
    
    [Header("UI - Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button closeButton;

    private CharacterBaseStatsSO baseStats => GameManager.Instance.BaseStats;
    private PlayerStats playerStats;
    private int tempSTR, tempINT, tempAGI, tempEND, tempWIS;
    private int tempPointsSpent = 0;
    
    private void Start()
    {
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
    
    public void OpenLevelUpPanel(PlayerStats stats)
    {
        if (stats.unallocatedPoints <= 0)
        {
            Debug.Log("No points to allocate!");
            return;
        }
        
        playerStats = stats;
        
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
        if (tempPointsSpent >= playerStats.unallocatedPoints)
            return;
        
        attribute++;
        tempPointsSpent++;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        // Level display with transcendence
        if (levelText)
        {
            if (playerStats.isTranscended)
                levelText.text = $"Level {playerStats.level} ★";
            else
                levelText.text = $"Level {playerStats.level}";
        }
        
        // Transcendence display
        if (transcendenceText != null)
        {
            if (playerStats.isTranscended)
            {
                transcendenceText.gameObject.SetActive(true);
                transcendenceText.text = $"<color=gold>Transcendence Lv.{playerStats.transcendenceLevel}</color>";
            }
            else
            {
                transcendenceText.gameObject.SetActive(false);
            }
        }
        
        // Points remaining
        int pointsRemaining = playerStats.unallocatedPoints - tempPointsSpent;
        if (pointsRemainingText) 
        { 
            pointsRemainingText.text = $"Points: {pointsRemaining}"; 
            pointsRemainingText.color = pointsRemaining > 0 ? Color.green : Color.red;
        }
        
        // Attributes with changes
        UpdateAttributeText(strText, tempSTR, playerStats.STR, "STR");
        UpdateAttributeText(intText, tempINT, playerStats.INT, "INT");
        UpdateAttributeText(agiText, tempAGI, playerStats.AGI, "AGI");
        UpdateAttributeText(endText, tempEND, playerStats.END, "END");
        UpdateAttributeText(wisText, tempWIS, playerStats.WIS, "WIS");
        
        // Button states
        bool hasPoints = tempPointsSpent < playerStats.unallocatedPoints;
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
        PlayerStats previewStats = new PlayerStats
        {
            level = playerStats.level,
            STR = tempSTR,
            INT = tempINT,
            AGI = tempAGI,
            END = tempEND,
            WIS = tempWIS,
            ItemAD = playerStats.ItemAD,
            ItemAP = playerStats.ItemAP,
            ItemHP = playerStats.ItemHP,
            ItemDefense = playerStats.ItemDefense,
            ItemAttackSpeed = playerStats.ItemAttackSpeed, // NEW
            ItemCritRate = playerStats.ItemCritRate,
            ItemCritDamage = playerStats.ItemCritDamage,
            ItemEvasion = playerStats.ItemEvasion,
            ItemTenacity = playerStats.ItemTenacity,
            ItemLethality = playerStats.ItemLethality,
            ItemPenetration = playerStats.ItemPenetration,
            ItemLifesteal = playerStats.ItemLifesteal
        };

        previewStats.CalculateCombatStats(baseStats);
        
        if (adText) adText.text = $"AD: {previewStats.AD:F1}";
        if (apText) apText.text = $"AP: {previewStats.AP:F1}";
        if (hpText) hpText.text = $"HP: {previewStats.HP:F0}";
        if (attackSpeedText) attackSpeedText.text = $"Speed: {previewStats.AttackSpeed:F1}"; // NEW
        if (defenseText) defenseText.text = $"Defense: {previewStats.Defense:F1}";
        if (critRateText) critRateText.text = $"Crit: {previewStats.CritRate:F1}%";
        if (evasionText) evasionText.text = $"Evasion: {previewStats.Evasion:F1}%";
    }
    
    private void OnConfirmClicked()
    {
        if (tempPointsSpent < playerStats.unallocatedPoints)
        {
            Debug.LogWarning($"You still have {playerStats.unallocatedPoints - tempPointsSpent} points to spend!");
            return;
        }
        
        playerStats.STR = tempSTR;
        playerStats.INT = tempINT;
        playerStats.AGI = tempAGI;
        playerStats.END = tempEND;
        playerStats.WIS = tempWIS;
        playerStats.unallocatedPoints = 0;
        
        playerStats.CalculateCombatStats(baseStats);
        GameManager.Instance.SaveGame();
        
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
        
        Debug.Log("Level up points allocated!");
    }
    
    private void OnCloseClicked()
    {
        if (playerStats.unallocatedPoints > 0)
        {
            Debug.LogWarning("You must allocate all points before closing!");
            return;
        }
        
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }
    
    public void GainExperience(int exp)
    {
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
            GameManager.Instance.SaveGame();
            OpenLevelUpPanel(playerStats);
        }
        else
        {
            GameManager.Instance.SaveGame();
        }
    }
}