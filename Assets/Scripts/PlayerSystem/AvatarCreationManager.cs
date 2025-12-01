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
    
    private PlayerStats currentStats;
    private PlayerAttributes tempAttributes;
    private int pointsSpent = 0;
    
    // Preview cache (reused to avoid allocations)
    private PlayerDerivedStats previewStats = new PlayerDerivedStats();
    
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
        
        // Clone attributes for temporary modification
        tempAttributes = currentStats.attributes.Clone();
        pointsSpent = 0;
    }
    
    private void SetupButtons()
    {
        strMinusBtn.onClick.AddListener(() => ModifyAttribute("STR", -1, baseStats.startingSTR));
        strPlusBtn.onClick.AddListener(() => ModifyAttribute("STR", 1, baseStats.startingSTR));
        
        intMinusBtn.onClick.AddListener(() => ModifyAttribute("INT", -1, baseStats.startingINT));
        intPlusBtn.onClick.AddListener(() => ModifyAttribute("INT", 1, baseStats.startingINT));
        
        agiMinusBtn.onClick.AddListener(() => ModifyAttribute("AGI", -1, baseStats.startingAGI));
        agiPlusBtn.onClick.AddListener(() => ModifyAttribute("AGI", 1, baseStats.startingAGI));
        
        endMinusBtn.onClick.AddListener(() => ModifyAttribute("END", -1, baseStats.startingEND));
        endPlusBtn.onClick.AddListener(() => ModifyAttribute("END", 1, baseStats.startingEND));
        
        wisMinusBtn.onClick.AddListener(() => ModifyAttribute("WIS", -1, baseStats.startingWIS));
        wisPlusBtn.onClick.AddListener(() => ModifyAttribute("WIS", 1, baseStats.startingWIS));
        
        confirmButton.onClick.AddListener(OnConfirmClicked);
        
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetClicked);
    }
    
    private void ModifyAttribute(string attributeName, int change, int minValue)
    {
        int currentValue = GetAttributeValue(attributeName);
        int newValue = currentValue + change;
        
        if (newValue < minValue)
            return;
        
        if (change > 0 && pointsSpent >= totalPointsToAllocate)
            return;
        
        if (change < 0 && pointsSpent <= 0)
            return;
        
        SetAttributeValue(attributeName, newValue);
        pointsSpent += change;
        
        UpdateUI();
    }
    
    private int GetAttributeValue(string attributeName)
    {
        switch (attributeName.ToUpper())
        {
            case "STR": return tempAttributes.STR;
            case "INT": return tempAttributes.INT;
            case "AGI": return tempAttributes.AGI;
            case "END": return tempAttributes.END;
            case "WIS": return tempAttributes.WIS;
            default: return 0;
        }
    }
    
    private void SetAttributeValue(string attributeName, int value)
    {
        switch (attributeName.ToUpper())
        {
            case "STR": tempAttributes.STR = value; break;
            case "INT": tempAttributes.INT = value; break;
            case "AGI": tempAttributes.AGI = value; break;
            case "END": tempAttributes.END = value; break;
            case "WIS": tempAttributes.WIS = value; break;
        }
    }
    
    private void UpdateUI()
    {
        // Attributes
        strValueText.text = tempAttributes.STR.ToString();
        intValueText.text = tempAttributes.INT.ToString();
        agiValueText.text = tempAttributes.AGI.ToString();
        endValueText.text = tempAttributes.END.ToString();
        wisValueText.text = tempAttributes.WIS.ToString();
        
        // Points remaining
        int pointsRemaining = totalPointsToAllocate - pointsSpent;
        pointsValueText.text = pointsRemaining.ToString();
        pointsValueText.color = pointsRemaining > 0 ? Color.green : Color.red;
        
        UpdateButtonStates();
        PreviewCombatStats();
    }
    
    private void PreviewCombatStats()
    {
        // Reuse previewStats (no new allocation!)
        // Pass null for weapon during character creation (no weapon yet)
        previewStats.Recalculate(baseStats, currentStats.Level, tempAttributes, currentStats.itemStats, null);
        
        // Combat stats
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
        
        strPlusBtn.interactable = hasPointsToSpend;
        intPlusBtn.interactable = hasPointsToSpend;
        agiPlusBtn.interactable = hasPointsToSpend;
        endPlusBtn.interactable = hasPointsToSpend;
        wisPlusBtn.interactable = hasPointsToSpend;
        
        strMinusBtn.interactable = canReduce && tempAttributes.STR > baseStats.startingSTR;
        intMinusBtn.interactable = canReduce && tempAttributes.INT > baseStats.startingINT;
        agiMinusBtn.interactable = canReduce && tempAttributes.AGI > baseStats.startingAGI;
        endMinusBtn.interactable = canReduce && tempAttributes.END > baseStats.startingEND;
        wisMinusBtn.interactable = canReduce && tempAttributes.WIS > baseStats.startingWIS;
    }

    private void OnConfirmClicked()
    {
        if (string.IsNullOrWhiteSpace(nameInput.text))
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
            Debug.LogError("[AvatarCreation] CharacterManager not found!");
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
        nameInput.text = "";
        
        UpdateUI();
    }
}