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

    [Header("Equipment")] // NEW
    public WeaponSO equippedWeapon; // NEW: Reference to equipped weapon
    
    [Header("Core Systems")]
    public PlayerLevelSystem levelSystem = new PlayerLevelSystem();
    public PlayerAttributes attributes = new PlayerAttributes();
    public PlayerItemStats itemStats = new PlayerItemStats();
    public PlayerDerivedStats derivedStats = new PlayerDerivedStats();
    public PlayerCombatRuntime combatRuntime = new PlayerCombatRuntime();
    
    // Quick accessors for backwards compatibility
    public int Level => levelSystem.level;
    public int CurrentEXP => levelSystem.currentEXP;
    public int UnallocatedPoints => levelSystem.unallocatedPoints;
    public bool IsTranscended => levelSystem.isTranscended;
    public int TranscendenceLevel => levelSystem.transcendenceLevel;
    
    public float CurrentHP => combatRuntime.currentHP;
    public float MaxHP => derivedStats.MaxHP;
    public float AD => derivedStats.AD;
    public float AP => derivedStats.AP;
    public float AttackSpeed => derivedStats.AttackSpeed;
    
    public void Initialize(CharacterBaseStatsSO baseStats)
    {
        className = baseStats.className;
        
        levelSystem.Initialize(baseStats);
        
        attributes = new PlayerAttributes(
            baseStats.startingSTR,
            baseStats.startingINT,
            baseStats.startingAGI,
            baseStats.startingEND,
            baseStats.startingWIS
        );
        
        itemStats = new PlayerItemStats();
        
        RecalculateStats(baseStats, fullHeal: true);
    }
    
    /// <summary>
    /// Add EXP and handle level ups
    /// Returns true if leveled up
    /// </summary>
    public bool AddExperience(int amount, CharacterBaseStatsSO baseStats)
    {
        float oldMaxHP = derivedStats.MaxHP;
        
        int levelsGained = levelSystem.AddExperience(amount, baseStats);
        
        if (levelsGained > 0)
        {
            RecalculateStats(baseStats, fullHeal: false);
            
            // Full heal on level up
            combatRuntime.currentHP = derivedStats.MaxHP;
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Recalculate all derived stats
    /// </summary>
    public void RecalculateStats(CharacterBaseStatsSO baseStats, bool fullHeal = false)
    {
        float oldMaxHP = derivedStats.MaxHP;
        
        // Pass equipped weapon to calculation
        derivedStats.Recalculate(baseStats, levelSystem.level, attributes, itemStats, equippedWeapon);
        
        combatRuntime.OnMaxHPChanged(oldMaxHP, derivedStats.MaxHP, fullHeal);
    }

    /// <summary>
    /// Equip a weapon and recalculate stats
    /// </summary>
    public void EquipWeapon(WeaponSO weapon, CharacterBaseStatsSO baseStats)
    {
        equippedWeapon = weapon;
        RecalculateStats(baseStats, fullHeal: false);
        Debug.Log($"[PlayerStats] Equipped: {weapon.weaponName}");
    }

    /// <summary>
    /// Unequip weapon
    /// </summary>
    public void UnequipWeapon(CharacterBaseStatsSO baseStats)
    {
        equippedWeapon = null;
        RecalculateStats(baseStats, fullHeal: false);
        Debug.Log("[PlayerStats] Weapon unequipped");
    }
    
    /// <summary>
    /// Mark stats as dirty (needs recalculation)
    /// Call this when attributes or items change
    /// </summary>
    public void MarkDirty()
    {
        derivedStats.MarkDirty();
    }
    
    /// <summary>
    /// Apply item stat changes and recalculate
    /// </summary>
    public void ApplyItemStats(PlayerItemStats newItemStats, CharacterBaseStatsSO baseStats)
    {
        itemStats = newItemStats.Clone();
        RecalculateStats(baseStats, fullHeal: false);
    }
    
    /// <summary>
    /// Quick access for attribute modification
    /// </summary>
    public void ModifyAttribute(string attributeName, int amount, CharacterBaseStatsSO baseStats)
    {
        switch (attributeName.ToUpper())
        {
            case "STR": attributes.STR += amount; break;
            case "INT": attributes.INT += amount; break;
            case "AGI": attributes.AGI += amount; break;
            case "END": attributes.END += amount; break;
            case "WIS": attributes.WIS += amount; break;
        }
        
        RecalculateStats(baseStats, fullHeal: false);
    }
}