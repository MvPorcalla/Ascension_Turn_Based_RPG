// ──────────────────────────────────────────────────
// PlayerData.cs
// Serializable player data for saving/loading
// ──────────────────────────────────────────────────

using System;

[Serializable]
public class PlayerData
{
    // Identity
    public string playerName;
    public string guildRank = "Unranked"; // Changed from className
    
    // Level System
    public int level;
    public int currentEXP;
    public int expToNextLevel;
    public int unallocatedPoints;
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
    public float itemAttackSpeed;
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
            guildRank = string.IsNullOrEmpty(stats.guildRank) ? "Unranked" : stats.guildRank,
            hpPercent = stats.combatRuntime.GetHPPercent(stats.derivedStats.MaxHP),
            
            // Level system
            level = stats.levelSystem.level,
            currentEXP = stats.levelSystem.currentEXP,
            expToNextLevel = stats.levelSystem.expToNextLevel,
            unallocatedPoints = stats.levelSystem.unallocatedPoints,
            transcendenceLevel = stats.levelSystem.transcendenceLevel,
            isTranscended = stats.levelSystem.isTranscended,
            
            // Attributes
            STR = stats.attributes.STR,
            INT = stats.attributes.INT,
            AGI = stats.attributes.AGI,
            END = stats.attributes.END,
            WIS = stats.attributes.WIS,
            
            // Item stats
            itemAD = stats.itemStats.AD,
            itemAP = stats.itemStats.AP,
            itemHP = stats.itemStats.HP,
            itemDefense = stats.itemStats.Defense,
            itemAttackSpeed = stats.itemStats.AttackSpeed,
            itemCritRate = stats.itemStats.CritRate,
            itemCritDamage = stats.itemStats.CritDamage,
            itemEvasion = stats.itemStats.Evasion,
            itemTenacity = stats.itemStats.Tenacity,
            itemLethality = stats.itemStats.Lethality,
            itemPenetration = stats.itemStats.Penetration,
            itemLifesteal = stats.itemStats.Lifesteal
        };
    }
    
    public PlayerStats ToPlayerStats(CharacterBaseStatsSO baseStats)
    {
        PlayerStats stats = new PlayerStats
        {
            playerName = this.playerName,
            guildRank = string.IsNullOrEmpty(this.guildRank) ? "Unranked" : this.guildRank
        };
        
        // Restore level system
        stats.levelSystem.level = this.level;
        stats.levelSystem.currentEXP = this.currentEXP;
        stats.levelSystem.expToNextLevel = this.expToNextLevel;
        stats.levelSystem.unallocatedPoints = this.unallocatedPoints;
        stats.levelSystem.transcendenceLevel = this.transcendenceLevel;
        stats.levelSystem.isTranscended = this.isTranscended;
        
        // Restore attributes
        stats.attributes.STR = this.STR;
        stats.attributes.INT = this.INT;
        stats.attributes.AGI = this.AGI;
        stats.attributes.END = this.END;
        stats.attributes.WIS = this.WIS;
        
        // Restore item stats
        stats.itemStats.AD = this.itemAD;
        stats.itemStats.AP = this.itemAP;
        stats.itemStats.HP = this.itemHP;
        stats.itemStats.Defense = this.itemDefense;
        stats.itemStats.AttackSpeed = this.itemAttackSpeed;
        stats.itemStats.CritRate = this.itemCritRate;
        stats.itemStats.CritDamage = this.itemCritDamage;
        stats.itemStats.Evasion = this.itemEvasion;
        stats.itemStats.Tenacity = this.itemTenacity;
        stats.itemStats.Lethality = this.itemLethality;
        stats.itemStats.Penetration = this.itemPenetration;
        stats.itemStats.Lifesteal = this.itemLifesteal;
        
        // Calculate derived stats
        stats.RecalculateStats(baseStats, fullHeal: false);
        
        // Restore HP from percentage
        stats.combatRuntime.currentHP = stats.derivedStats.MaxHP * this.hpPercent;
        
        // Validate EXP (check for pending level ups)
        int maxPossibleLevel = stats.levelSystem.GetMaxPossibleLevel(baseStats);
        while (stats.levelSystem.currentEXP >= stats.levelSystem.expToNextLevel && 
               stats.levelSystem.level < maxPossibleLevel)
        {
            stats.levelSystem.AddExperience(0, baseStats); // Trigger level up without adding EXP
            stats.RecalculateStats(baseStats, fullHeal: true);
        }
        
        return stats;
    }
}