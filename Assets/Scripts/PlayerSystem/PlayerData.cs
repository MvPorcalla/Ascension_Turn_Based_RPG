// -------------------------------
// PlayerData.cs (Updated for Attack Speed & Transcendence)
// -------------------------------

using System;

[Serializable]
public class PlayerData
{
    // Identity
    public string playerName;
    public string className;
    
    // Progression
    public int level;
    public int currentEXP;
    public int expToNextLevel;
    public int unallocatedPoints;
    
    // Transcendence
    public int transcendenceLevel;
    public bool isTranscended;

    // Runtime state
    public float hpPercent;
    
    // Attributes
    public int STR;
    public int INT;
    public int AGI;
    public int END;
    public int WIS;
    
    // Item Bonuses
    public float itemAD;
    public float itemAP;
    public float itemHP;
    public float itemDefense;
    public float itemAttackSpeed; // NEW
    public float itemCritRate;
    public float itemCritDamage;
    public float itemEvasion;
    public float itemTenacity;
    public float itemLethality;
    public float itemPenetration;
    public float itemLifesteal;
    
    public PlayerData() { }
    
    public static PlayerData FromPlayerStats(PlayerStats stats)
    {
        return new PlayerData
        {
            playerName = stats.playerName,
            className = stats.className,
            hpPercent = (stats.HP > 0) ? stats.currentHP / stats.HP : 1f,
            level = stats.level,
            currentEXP = stats.currentEXP,
            expToNextLevel = stats.expToNextLevel,
            unallocatedPoints = stats.unallocatedPoints,
            transcendenceLevel = stats.transcendenceLevel,
            isTranscended = stats.isTranscended,
            
            STR = stats.STR,
            INT = stats.INT,
            AGI = stats.AGI,
            END = stats.END,
            WIS = stats.WIS,
            
            itemAD = stats.ItemAD,
            itemAP = stats.ItemAP,
            itemHP = stats.ItemHP,
            itemDefense = stats.ItemDefense,
            itemAttackSpeed = stats.ItemAttackSpeed, // NEW
            itemCritRate = stats.ItemCritRate,
            itemCritDamage = stats.ItemCritDamage,
            itemEvasion = stats.ItemEvasion,
            itemTenacity = stats.ItemTenacity,
            itemLethality = stats.ItemLethality,
            itemPenetration = stats.ItemPenetration,
            itemLifesteal = stats.ItemLifesteal
        };
    }
    
    public PlayerStats ToPlayerStats(CharacterBaseStatsSO baseStats)
    {
        PlayerStats stats = new PlayerStats
        {
            playerName = this.playerName,
            className = this.className,
            level = this.level,
            currentEXP = this.currentEXP,
            expToNextLevel = this.expToNextLevel,
            unallocatedPoints = this.unallocatedPoints,
            transcendenceLevel = this.transcendenceLevel,
            isTranscended = this.isTranscended,
            
            STR = this.STR,
            INT = this.INT,
            AGI = this.AGI,
            END = this.END,
            WIS = this.WIS,
            
            ItemAD = this.itemAD,
            ItemAP = this.itemAP,
            ItemHP = this.itemHP,
            ItemDefense = this.itemDefense,
            ItemAttackSpeed = this.itemAttackSpeed, // NEW
            ItemCritRate = this.itemCritRate,
            ItemCritDamage = this.itemCritDamage,
            ItemEvasion = this.itemEvasion,
            ItemTenacity = this.itemTenacity,
            ItemLethality = this.itemLethality,
            ItemPenetration = this.itemPenetration,
            ItemLifesteal = this.itemLifesteal
        };
        
        // Calculate stats
        stats.CalculateCombatStats(baseStats);
        
        // Restore HP from percentage
        stats.currentHP = stats.HP * this.hpPercent;
        
        // Validate EXP without recalculating again
        int maxPossibleLevel = baseStats.maxLevel;
        if (baseStats.enableTranscendence)
            maxPossibleLevel += baseStats.maxTranscendenceLevel;
        
        while (stats.currentEXP >= stats.expToNextLevel && stats.level < maxPossibleLevel)
        {
            stats.currentEXP -= stats.expToNextLevel;
            stats.LevelUp(baseStats);
            stats.CalculateCombatStats(baseStats);
            stats.currentHP = stats.HP; // Full heal on pending level ups
        }
        
        return stats;
    }
}