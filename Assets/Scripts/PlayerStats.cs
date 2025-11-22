// -------------------------------
// PlayerStats.cs
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
    public int unallocatedPoints = 0; // Points waiting to be spent

    [Header("Runtime Combat")]
    public float currentHP; // Current health (runtime + saved)

    [Header("Stat Caps")]
    private const float MAX_CRIT_RATE = 100f;
    private const float MAX_EVASION = 75f;
    private const float MAX_TENACITY = 80f;
    
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
    public float Armor;
    public float MR;
    public float CritRate;
    public float CritDamage;
    public float Evasion;
    public float Tenacity;
    
    // Item-only stats (cannot be increased through attributes or leveling)
    public float Lethality;
    public float PhysicalPenetration;
    public float MagicPenetration;
    
    [Header("Item Bonuses")]
    public float ItemAD = 0;
    public float ItemAP = 0;
    public float ItemHP = 0;
    public float ItemArmor = 0;
    public float ItemMR = 0;
    public float ItemCritRate = 0;
    public float ItemCritDamage = 0; // Only way to increase crit damage
    public float ItemEvasion = 0;
    public float ItemTenacity = 0;
    public float ItemLethality = 0; // Item-only
    public float ItemPhysicalPen = 0; // Item-only
    public float ItemMagicPen = 0; // Item-only
    
    public void Initialize(CharacterBaseStatsSO baseStats)
    {
        className = baseStats.className;
        level = 1;
        currentEXP = 0;
        expToNextLevel = 100;
        unallocatedPoints = 0;
        STR = baseStats.startingSTR;
        INT = baseStats.startingINT;
        AGI = baseStats.startingAGI;
        END = baseStats.startingEND;
        WIS = baseStats.startingWIS;
        
        RecalculateStats(baseStats, fullHeal: true); // New character = full HP
    }
    
    /// <summary>
    /// Call this when player gains a level
    /// </summary>
    public void LevelUp()
    {
        level++;
        unallocatedPoints += 5; // Give 5 points per level
        
        // Gentle exponential curve - scales well up to level 9999
        // Level 1→2: 151 EXP | Level 100→101: 6,100 EXP | Level 9999→max: ~1.35M EXP
        expToNextLevel = (int)(100 + (level * 50) + Mathf.Pow(level, 1.5f));
        currentEXP = 0;
    }

    /// <summary>
    /// Validate EXP - only use during runtime, not loading
    /// </summary>
    public void ValidateEXP(CharacterBaseStatsSO baseStats)
    {
        bool leveledUp = false;
        while (currentEXP >= expToNextLevel && level < 9999)
        {
            currentEXP -= expToNextLevel;
            LevelUp();
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
        
        bool leveledUp = false;
        while (currentEXP >= expToNextLevel)
        {
            LevelUp();
            leveledUp = true;
        }
        
        if (leveledUp)
            RecalculateStats(baseStats, fullHeal: true); // Full heal on level up
        
        return leveledUp;
    }

    /// <summary>
    /// Recalculates stats and handles HP scaling properly.
    /// fullHeal: true = restore to max HP
    /// fullHeal: false = add/subtract the HP difference (intuitive for equipment)
    /// </summary>
    public void RecalculateStats(CharacterBaseStatsSO baseStats, bool fullHeal = false)
    {
        // Store old max HP before recalculation
        float oldMaxHP = HP;
        
        // Recalculate all combat stats
        CalculateCombatStats(baseStats);
        
        if (fullHeal)
        {
            currentHP = HP;
        }
        else
        {
            // Add/subtract the difference in max HP
            float hpDifference = HP - oldMaxHP;
            currentHP += hpDifference;
            
            // Clamp to valid range
            currentHP = Mathf.Clamp(currentHP, 0, HP);
        }
    }
    
    public void CalculateCombatStats(CharacterBaseStatsSO baseStats)
    {
        // Calculate level bonuses (level - 1 because level 1 uses base stats)
        int levelBonus = level - 1;
        
        float levelHP = baseStats.HPPerLevel * levelBonus;
        // float levelAD =  0;                                                 // remove automatic AD growth
        // float levelAP =  0;                                                 // remove automatic AP growth
        float levelArmor = baseStats.ArmorPerLevel * levelBonus;
        float levelMR = baseStats.MRPerLevel * levelBonus;
        // float levelCritRate =  0;                                           // remove automatic Crit Rate growth
        float levelEvasion = baseStats.EvasionPerLevel * levelBonus;
        float levelTenacity = baseStats.TenacityPerLevel * levelBonus;
        
        // Attack Damage = (Base + Level Bonus) * (1 + STR scaling) + Items
        AD = baseStats.BaseAD * (1 + STR * 0.02f) + ItemAD;
        
        // Ability Power = (Base + Level Bonus) * (1 + INT scaling) + Items
        AP = baseStats.BaseAP * (1 + INT * 0.02f) + ItemAP;
        
        // Health Points = (Base + Level Bonus) * (1 + END scaling) + Items
        HP = (baseStats.BaseHP + levelHP) * (1 + END * 0.05f) + ItemHP;
        
        // Armor = (Base + Level Bonus) + END additive + Items
        Armor = (baseStats.BaseArmor + levelArmor) + END * 0.5f + ItemArmor;
        
        // Magic Resistance = (Base + Level Bonus) + WIS additive + Items
        MR = (baseStats.BaseMR + levelMR) + WIS * 0.5f + ItemMR;
        
        // Critical Rate = (Base + Level Bonus) + AGI additive + Items
        CritRate = baseStats.BaseCritRate + AGI * 0.5f + ItemCritRate;
        
        // Critical Damage = Base + Items ONLY (no attribute scaling)
        CritDamage = baseStats.BaseCritDamage + ItemCritDamage;
        
        // Evasion = (Base + Level Bonus) + AGI additive + Items
        Evasion = (baseStats.BaseEvasion + levelEvasion) + AGI * 0.2f + ItemEvasion;
        
        // Tenacity = (Base + Level Bonus) + WIS additive + Items
        Tenacity = (baseStats.BaseTenacity + levelTenacity) + WIS * 0.5f + ItemTenacity;
        
        // Item-only stats (no base, no level, no attributes)
        Lethality = ItemLethality;
        PhysicalPenetration = ItemPhysicalPen;
        MagicPenetration = ItemMagicPen;

        // Apply caps to percentage-based stats
        CritRate = Mathf.Min(CritRate, MAX_CRIT_RATE);
        Evasion = Mathf.Min(Evasion, MAX_EVASION);
        Tenacity = Mathf.Min(Tenacity, MAX_TENACITY);

    }
}