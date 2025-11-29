// -------------------------------
// PlayerStats.cs (Updated for Level 1000 + Attack Speed + Tiered Caps)
// -------------------------------

using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    [Header("Character Identity")]
    public string playerName;
    public string className;
    public int level = 1;
    public int currentEXP = 0;
    public int expToNextLevel = 100;
    public int unallocatedPoints = 0;
    
    [Header("Transcendence (Future)")]
    public int transcendenceLevel = 0; // 0 = normal, 1+ = transcended
    public bool isTranscended = false;

    [Header("Runtime Combat")]
    public float currentHP;
    
    [Header("Attributes")]
    public int STR;
    public int INT;
    public int AGI;
    public int END;
    public int WIS;
    
    [Header("Derived Combat Stats")]
    public float AD;
    public float AP;
    public float HP;
    public float Defense;
    public float AttackSpeed; // NEW
    public float CritRate;
    public float CritDamage;
    public float Evasion;
    public float Tenacity;
    
    // Item-only stats
    public float Lethality;
    public float Penetration;
    public float Lifesteal;
    
    [Header("Item Bonuses")]
    public float ItemAD = 0;
    public float ItemAP = 0;
    public float ItemHP = 0;
    public float ItemDefense = 0;
    public float ItemAttackSpeed = 0; // NEW
    public float ItemCritRate = 0;
    public float ItemCritDamage = 0;
    public float ItemEvasion = 0;
    public float ItemTenacity = 0;
    public float ItemLethality = 0;
    public float ItemPenetration = 0;
    public float ItemLifesteal = 0;
    
    public void Initialize(CharacterBaseStatsSO baseStats)
    {
        className = baseStats.className;
        level = 1;
        currentEXP = 0;
        expToNextLevel = CalculateEXPForLevel(2, baseStats);
        unallocatedPoints = 0;
        transcendenceLevel = 0;
        isTranscended = false;
        
        STR = baseStats.startingSTR;
        INT = baseStats.startingINT;
        AGI = baseStats.startingAGI;
        END = baseStats.startingEND;
        WIS = baseStats.startingWIS;
        
        RecalculateStats(baseStats, fullHeal: true);
    }
    
    /// <summary>
    /// Calculate EXP requirement for a specific level
    /// Formula: base + (level * linear) + (level ^ exponent)
    /// </summary>
    private int CalculateEXPForLevel(int targetLevel, CharacterBaseStatsSO baseStats)
    {
        if (targetLevel <= 1) return 0;
        
        // Normal leveling
        if (targetLevel <= baseStats.maxLevel)
        {
            return (int)(baseStats.baseEXPRequirement + 
                        (targetLevel * baseStats.expLinearGrowth) + 
                        Mathf.Pow(targetLevel, baseStats.expExponentGrowth));
        }
        
        // Transcendence leveling (if enabled)
        if (baseStats.enableTranscendence && targetLevel <= baseStats.maxLevel + baseStats.maxTranscendenceLevel)
        {
            int baseReq = CalculateEXPForLevel(baseStats.maxLevel, baseStats);
            int transcendLevel = targetLevel - baseStats.maxLevel;
            return (int)(baseReq * baseStats.transcendenceEXPMultiplier * transcendLevel);
        }
        
        return int.MaxValue; // Cap reached
    }
    
    /// <summary>
    /// Call this when player gains a level
    /// </summary>
    public void LevelUp(CharacterBaseStatsSO baseStats)
    {
        level++;
        unallocatedPoints += baseStats.pointsPerLevel;
        
        // Check for transcendence
        if (level > baseStats.maxLevel && baseStats.enableTranscendence)
        {
            if (!isTranscended)
            {
                isTranscended = true;
                transcendenceLevel = 1;
                Debug.Log($"[PlayerStats] TRANSCENDENCE UNLOCKED! Transcendence Level: {transcendenceLevel}");
            }
            else
            {
                transcendenceLevel++;
            }
        }
        
        expToNextLevel = CalculateEXPForLevel(level + 1, baseStats);
        currentEXP = 0;
    }

    /// <summary>
    /// Validate EXP - only use during runtime, not loading
    /// </summary>
    public void ValidateEXP(CharacterBaseStatsSO baseStats)
    {
        int maxPossibleLevel = baseStats.maxLevel;
        if (baseStats.enableTranscendence)
            maxPossibleLevel += baseStats.maxTranscendenceLevel;
        
        bool leveledUp = false;
        while (currentEXP >= expToNextLevel && level < maxPossibleLevel)
        {
            currentEXP -= expToNextLevel;
            LevelUp(baseStats);
            leveledUp = true;
        }
        
        if (leveledUp)
            RecalculateStats(baseStats, fullHeal: true);
    }
    
    /// <summary>
    /// Add EXP and check for level up
    /// </summary>
    public bool AddExperience(int amount, CharacterBaseStatsSO baseStats)
    {
        currentEXP += amount;
        
        int maxPossibleLevel = baseStats.maxLevel;
        if (baseStats.enableTranscendence)
            maxPossibleLevel += baseStats.maxTranscendenceLevel;
        
        bool leveledUp = false;
        while (currentEXP >= expToNextLevel && level < maxPossibleLevel)
        {
            LevelUp(baseStats);
            leveledUp = true;
        }
        
        if (leveledUp)
            RecalculateStats(baseStats, fullHeal: true);
        
        return leveledUp;
    }

    /// <summary>
    /// Recalculates stats and handles HP scaling properly.
    /// </summary>
    public void RecalculateStats(CharacterBaseStatsSO baseStats, bool fullHeal = false)
    {
        float oldMaxHP = HP;
        CalculateCombatStats(baseStats);
        
        if (fullHeal)
        {
            currentHP = HP;
        }
        else
        {
            float hpDifference = HP - oldMaxHP;
            currentHP += hpDifference;
            currentHP = Mathf.Clamp(currentHP, 0, HP);
        }
    }
    
    public void CalculateCombatStats(CharacterBaseStatsSO baseStats)
    {
        // Calculate level bonuses
        int levelBonus = level - 1;
        
        float levelHP = baseStats.HPPerLevel * levelBonus;
        float levelDefense = baseStats.DefensePerLevel * levelBonus;
        float levelEvasion = baseStats.EvasionPerLevel * levelBonus;
        float levelTenacity = baseStats.TenacityPerLevel * levelBonus;
        
        // === ATTACK DAMAGE ===
        AD = baseStats.BaseAD * (1 + STR * baseStats.STRtoAD) + ItemAD;
        
        // === ABILITY POWER ===
        AP = baseStats.BaseAP * (1 + INT * baseStats.INTtoAP) + ItemAP;
        
        // === HEALTH POINTS ===
        HP = (baseStats.BaseHP + levelHP) * (1 + END * baseStats.ENDtoHP) + ItemHP;
        
        // === DEFENSE ===
        Defense = (baseStats.BaseDefense + levelDefense) + 
                  (END * baseStats.ENDtoDefense) + 
                  (WIS * baseStats.WIStoDefense) + 
                  ItemDefense;
        
        // === ATTACK SPEED (NEW) ===
        AttackSpeed = baseStats.BaseAttackSpeed + (AGI * baseStats.AGItoAttackSpeed) + ItemAttackSpeed;
        // No cap on attack speed
        
        // === CRITICAL RATE (Tiered Cap) ===
        float baseCritRate = baseStats.BaseCritRate + (AGI * baseStats.AGItoCritRate);
        baseCritRate = Mathf.Min(baseCritRate, baseStats.baseCritRateCap); // Cap base at 60%
        
        CritRate = baseCritRate + ItemCritRate;
        CritRate = Mathf.Min(CritRate, baseStats.totalCritRateCap); // Cap total at 100%
        
        // === CRITICAL DAMAGE ===
        CritDamage = baseStats.BaseCritDamage + ItemCritDamage;
        
        // === EVASION (Tiered Cap) ===
        float baseEvasion = baseStats.BaseEvasion + levelEvasion + (AGI * baseStats.AGItoEvasion);
        baseEvasion = Mathf.Min(baseEvasion, baseStats.baseEvasionCap); // Cap base at 40%
        
        Evasion = baseEvasion + ItemEvasion;
        Evasion = Mathf.Min(Evasion, baseStats.totalEvasionCap); // Cap total at 80%
        
        // === TENACITY ===
        Tenacity = (baseStats.BaseTenacity + levelTenacity) + (WIS * baseStats.WIStoTenacity) + ItemTenacity;
        Tenacity = Mathf.Min(Tenacity, baseStats.maxTenacity); // Cap at 80%
        
        // === ITEM-ONLY STATS ===
        Lethality = ItemLethality;
        Penetration = Mathf.Min(ItemPenetration, baseStats.maxPenetration); // Cap at 100%
        Lifesteal = ItemLifesteal; // No cap on lifesteal
    }
}