// -------------------------------
// PlayerData.cs
// -------------------------------

using System;

/// <summary>
/// Data model for player stats serialization.
/// Only contains data that needs to be saved.
/// Combat stats are recalculated on load.
/// </summary>
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

    // Runtime state
    // public float currentHP;
    public float hpPercent; // Store as ratio instead of raw value
    
    // Attributes (the core stats we need to save)
    public int STR;
    public int INT;
    public int AGI;
    public int END;
    public int WIS;
    
    // Item Bonuses (saved because they come from equipped items)
    public float itemAD;
    public float itemAP;
    public float itemHP;
    public float itemArmor;
    public float itemMR;
    public float itemCritRate;
    public float itemCritDamage;
    public float itemEvasion;
    public float itemTenacity;
    public float itemLethality;
    public float itemPhysicalPen;
    public float itemMagicPen;
    
    public PlayerData() { }
    
    /// <summary>
    /// Create PlayerData from PlayerStats (for saving)
    /// </summary>
    public static PlayerData FromPlayerStats(PlayerStats stats)
    {
        return new PlayerData
        {
            playerName = stats.playerName,
            className = stats.className,
            // currentHP = stats.currentHP,
            hpPercent = (stats.HP > 0) ? stats.currentHP / stats.HP : 1f,
            level = stats.level,
            currentEXP = stats.currentEXP,
            expToNextLevel = stats.expToNextLevel,
            unallocatedPoints = stats.unallocatedPoints,
            
            STR = stats.STR,
            INT = stats.INT,
            AGI = stats.AGI,
            END = stats.END,
            WIS = stats.WIS,
            
            itemAD = stats.ItemAD,
            itemAP = stats.ItemAP,
            itemHP = stats.ItemHP,
            itemArmor = stats.ItemArmor,
            itemMR = stats.ItemMR,
            itemCritRate = stats.ItemCritRate,
            itemCritDamage = stats.ItemCritDamage,
            itemEvasion = stats.ItemEvasion,
            itemTenacity = stats.ItemTenacity,
            itemLethality = stats.ItemLethality,
            itemPhysicalPen = stats.ItemPhysicalPen,
            itemMagicPen = stats.ItemMagicPen
        };
    }
    
    /// <summary>
    /// Convert PlayerData back to PlayerStats (for loading)
    /// </summary>
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
            
            STR = this.STR,
            INT = this.INT,
            AGI = this.AGI,
            END = this.END,
            WIS = this.WIS,
            
            ItemAD = this.itemAD,
            ItemAP = this.itemAP,
            ItemHP = this.itemHP,
            ItemArmor = this.itemArmor,
            ItemMR = this.itemMR,
            ItemCritRate = this.itemCritRate,
            ItemCritDamage = this.itemCritDamage,
            ItemEvasion = this.itemEvasion,
            ItemTenacity = this.itemTenacity,
            ItemLethality = this.itemLethality,
            ItemPhysicalPen = this.itemPhysicalPen,
            ItemMagicPen = this.itemMagicPen
        };
        
        // First calculate stats so HP is set
        stats.CalculateCombatStats(baseStats);
        
        // Then restore HP from saved percentage
        stats.currentHP = stats.HP * this.hpPercent;
        
        // Validate EXP without recalculating stats again
        while (stats.currentEXP >= stats.expToNextLevel && stats.level < 9999)
        {
            stats.currentEXP -= stats.expToNextLevel;
            stats.LevelUp();
            stats.CalculateCombatStats(baseStats);
            stats.currentHP = stats.HP; // Full heal on any pending level ups
        }
        
        return stats;
    }
}