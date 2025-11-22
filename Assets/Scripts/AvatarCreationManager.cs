// -------------------------------
// AvatarCreationManager.cs
// -------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class AvatarCreationManager : MonoBehaviour
{
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
    [SerializeField] private TMP_Text physicalPenValueText;
    [SerializeField] private TMP_Text magicPenValueText;
    [SerializeField] private TMP_Text hpValueText;
    [SerializeField] private TMP_Text armorValueText;
    [SerializeField] private TMP_Text mrValueText;
    [SerializeField] private TMP_Text evasionValueText;
    [SerializeField] private TMP_Text tenacityValueText;
    
    [Header("UI - Action Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button resetButton;
    
    [Header("Settings")]
    [SerializeField] private int totalPointsToAllocate = 50;
    
    private PlayerStats currentStats;
    private int pointsSpent = 0;
    
    private void Start()
    {
        if (baseStats == null)
        {
            Debug.LogError("CharacterBaseStatsSO is not assigned!");
            return;
        }
        
        InitializeStats();
        SetupButtons();
        UpdateUI();
    }
    
    private void InitializeStats()
    {
        currentStats = new PlayerStats();
        currentStats.Initialize(baseStats);
    }
    
    private void SetupButtons()
    {
        // STR buttons
        strMinusBtn.onClick.AddListener(() => ModifyAttribute(ref currentStats.STR, -1, baseStats.startingSTR));
        strPlusBtn.onClick.AddListener(() => ModifyAttribute(ref currentStats.STR, 1, baseStats.startingSTR));
        
        // INT buttons
        intMinusBtn.onClick.AddListener(() => ModifyAttribute(ref currentStats.INT, -1, baseStats.startingINT));
        intPlusBtn.onClick.AddListener(() => ModifyAttribute(ref currentStats.INT, 1, baseStats.startingINT));
        
        // AGI buttons
        agiMinusBtn.onClick.AddListener(() => ModifyAttribute(ref currentStats.AGI, -1, baseStats.startingAGI));
        agiPlusBtn.onClick.AddListener(() => ModifyAttribute(ref currentStats.AGI, 1, baseStats.startingAGI));
        
        // END buttons
        endMinusBtn.onClick.AddListener(() => ModifyAttribute(ref currentStats.END, -1, baseStats.startingEND));
        endPlusBtn.onClick.AddListener(() => ModifyAttribute(ref currentStats.END, 1, baseStats.startingEND));
        
        // WIS buttons
        wisMinusBtn.onClick.AddListener(() => ModifyAttribute(ref currentStats.WIS, -1, baseStats.startingWIS));
        wisPlusBtn.onClick.AddListener(() => ModifyAttribute(ref currentStats.WIS, 1, baseStats.startingWIS));
        
        // Action buttons
        confirmButton.onClick.AddListener(OnConfirmClicked);
        
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetClicked);
    }
    
    private void ModifyAttribute(ref int attribute, int change, int minValue)
    {
        int newValue = attribute + change;
        
        // Check minimum value
        if (newValue < minValue)
            return;
        
        // Check if we have points to spend (for positive changes)
        if (change > 0 && pointsSpent >= totalPointsToAllocate)
            return;
        
        // Check if we can reduce (for negative changes)
        if (change < 0 && pointsSpent <= 0)
            return;
        
        // Apply change
        attribute = newValue;
        pointsSpent += change;
        
        // Recalculate stats
        currentStats.CalculateCombatStats(baseStats);
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        // Update attribute value displays
        strValueText.text = currentStats.STR.ToString();
        intValueText.text = currentStats.INT.ToString();
        agiValueText.text = currentStats.AGI.ToString();
        endValueText.text = currentStats.END.ToString();
        wisValueText.text = currentStats.WIS.ToString();
        
        // Update points remaining
        int pointsRemaining = totalPointsToAllocate - pointsSpent;
        pointsValueText.text = pointsRemaining.ToString();
        
        // Change color based on points remaining
        if (pointsRemaining > 0)
            pointsValueText.color = Color.green;
        else if (pointsRemaining == 0)
            pointsValueText.color = Color.red;
        else
            pointsValueText.color = Color.red;
        
        // Update button interactability
        UpdateButtonStates();
        
        // Update combat stats display
        if (adValueText) adValueText.text = currentStats.AD.ToString("F1");
        if (apValueText) apValueText.text = currentStats.AP.ToString("F1");
        if (critDamageValueText) critDamageValueText.text = currentStats.CritDamage.ToString("F1") + "%";
        if (critRateValueText) critRateValueText.text = currentStats.CritRate.ToString("F1") + "%";
        if (lethalityValueText) lethalityValueText.text = currentStats.Lethality.ToString("F0");
        if (physicalPenValueText) physicalPenValueText.text = currentStats.PhysicalPenetration.ToString("F1") + "%";
        if (magicPenValueText) magicPenValueText.text = currentStats.MagicPenetration.ToString("F1") + "%";
        if (hpValueText) hpValueText.text = currentStats.HP.ToString("F0");
        if (armorValueText) armorValueText.text = currentStats.Armor.ToString("F1");
        if (mrValueText) mrValueText.text = currentStats.MR.ToString("F1");
        if (evasionValueText) evasionValueText.text = currentStats.Evasion.ToString("F1") + "%";
        if (tenacityValueText) tenacityValueText.text = currentStats.Tenacity.ToString("F1") + "%";
    }
    
    private void UpdateButtonStates()
    {
        bool hasPointsToSpend = pointsSpent < totalPointsToAllocate;
        bool canReduce = pointsSpent > 0;
        
        // Enable/disable plus buttons
        strPlusBtn.interactable = hasPointsToSpend;
        intPlusBtn.interactable = hasPointsToSpend;
        agiPlusBtn.interactable = hasPointsToSpend;
        endPlusBtn.interactable = hasPointsToSpend;
        wisPlusBtn.interactable = hasPointsToSpend;
        
        // Enable/disable minus buttons (also check if at minimum)
        strMinusBtn.interactable = canReduce && currentStats.STR > baseStats.startingSTR;
        intMinusBtn.interactable = canReduce && currentStats.INT > baseStats.startingINT;
        agiMinusBtn.interactable = canReduce && currentStats.AGI > baseStats.startingAGI;
        endMinusBtn.interactable = canReduce && currentStats.END > baseStats.startingEND;
        wisMinusBtn.interactable = canReduce && currentStats.WIS > baseStats.startingWIS;
    }
    
    private void OnConfirmClicked()
    {
        if (string.IsNullOrWhiteSpace(nameInput.text))
        {
            Debug.LogWarning("Please enter a character name!");
            // TODO: Show UI warning message
            return;
        }
        
        if (pointsSpent < totalPointsToAllocate)
        {
            Debug.LogWarning($"You still have {totalPointsToAllocate - pointsSpent} points to allocate!");
            // TODO: Show UI warning message
            return;
        }
        
        currentStats.playerName = nameInput.text;

        // Finalize stats and set full HP before saving
        currentStats.RecalculateStats(baseStats, fullHeal: true);
        
        // Use GameManager - save and go to MainBase
        GameManager.Instance.SetPlayerStats(currentStats);
        GameManager.Instance.SaveGame();
        GameManager.Instance.GoToMainBase(); // Goes to 03_MainBase
    }
    
    private void OnResetClicked()
    {
        // Reset to starting values
        currentStats.STR = baseStats.startingSTR;
        currentStats.INT = baseStats.startingINT;
        currentStats.AGI = baseStats.startingAGI;
        currentStats.END = baseStats.startingEND;
        currentStats.WIS = baseStats.startingWIS;
        
        pointsSpent = 0;
        
        nameInput.text = "";
        
        currentStats.CalculateCombatStats(baseStats);
        UpdateUI();
    }
}