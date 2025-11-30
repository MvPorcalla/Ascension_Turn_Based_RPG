// -------------------------------
// WeaponSO.cs - Complete System (All In One File)
// -------------------------------
using UnityEngine;
using System;
using System.Collections.Generic;

// ===============================================
// ENUMS
// ===============================================

public enum WeaponType
{
    Sword,
    Axe,
    Dagger,
    Bow,
    Staff,
    Wand,
    Hammer,
    Spear,
    Shield
}

public enum AttackRangeType
{
    Melee,
    Ranged,
    Magic
}

public enum BonusStatType
{
    AttackDamage,
    AbilityPower,
    Health,
    Defense,
    AttackSpeed,
    CritRate,
    CritDamage,
    Evasion,
    Tenacity,
    Lethality,
    Penetration,
    Lifesteal
}

public enum RarityTier
{
    Common = 0,
    Rare = 1,
    Epic = 2,
    Legendary = 3,
    Mythic = 4
}

// ===============================================
// WEAPON BONUS STAT
// ===============================================

[Serializable]
public class WeaponBonusStat
{
    public BonusStatType statType;
    public float value;
    
    public WeaponBonusStat(BonusStatType type, float val)
    {
        statType = type;
        value = val;
    }
    
    public string GetDisplayText()
    {
        switch (statType)
        {
            case BonusStatType.AttackDamage:
                return $"+{value:F1} Attack Damage";
            case BonusStatType.AbilityPower:
                return $"+{value:F1} Ability Power";
            case BonusStatType.Health:
                return $"+{value:F0} Health";
            case BonusStatType.Defense:
                return $"+{value:F1} Defense";
            case BonusStatType.AttackSpeed:
                return $"+{value:F1} Attack Speed";
            case BonusStatType.CritRate:
                return $"+{value:F1}% Crit Rate";
            case BonusStatType.CritDamage:
                return $"+{value:F1}% Crit Damage";
            case BonusStatType.Evasion:
                return $"+{value:F1}% Evasion";
            case BonusStatType.Tenacity:
                return $"+{value:F1}% Tenacity";
            case BonusStatType.Lethality:
                return $"+{value:F0} Lethality";
            case BonusStatType.Penetration:
                return $"+{value:F1}% Penetration";
            case BonusStatType.Lifesteal:
                return $"+{value:F1}% Lifesteal";
            default:
                return "";
        }
    }
}

// ===============================================
// WEAPON RARITY SO
// ===============================================

[CreateAssetMenu(fileName = "New Weapon Rarity", menuName = "Game/Weapon Rarity")]
public class WeaponRaritySO : ScriptableObject
{
    [Header("Rarity Info")]
    public string rarityName;
    public RarityTier tier;
    public Color rarityColor = Color.white;
    
    [Header("Stat Multipliers")]
    [Tooltip("Multiply all base weapon stats by this amount")]
    [Range(0.5f, 5f)]
    public float statMultiplier = 1f;
    
    [Header("Bonus Stat Slots")]
    [Tooltip("Number of random bonus stats this rarity can roll")]
    [Range(0, 6)]
    public int bonusStatSlots = 0;
    
    [Tooltip("Min value for bonus stats (% of base stat)")]
    [Range(0f, 1f)]
    public float bonusStatMinRoll = 0.1f;
    
    [Tooltip("Max value for bonus stats (% of base stat)")]
    [Range(0f, 2f)]
    public float bonusStatMaxRoll = 0.5f;
    
    [Header("Visual Effects")]
    public Sprite rarityIcon;
    public GameObject rarityParticleEffect;
    public AudioClip rarityDropSound;
    
    [Header("Crafting Requirements (Future)")]
    [Tooltip("Minimum successful hits needed in blacksmith minigame")]
    public int minCraftingHits = 0;
    
    [Tooltip("Perfect hits bonus: additional stat roll chance")]
    [Range(0f, 1f)]
    public float perfectHitBonusChance = 0f;
}

// ===============================================
// WEAPON SO
// ===============================================

[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Weapon")]
public class WeaponSO : ItemBaseSO
{
    [Header("Weapon Info")]
    public string weaponName;
    public WeaponType weaponType;
    public AttackRangeType attackRangeType;

    [Header("Rarity System")]
    [Tooltip("Rarity configuration - affects stat multipliers and bonus slots")]
    public WeaponRaritySO rarityConfig;
    
    [Tooltip("Randomly rolled bonus stats (filled at craft/drop time)")]
    public List<WeaponBonusStat> bonusStats = new List<WeaponBonusStat>();

    [Header("Base Stats (Before Rarity Multiplier)")]
    [Tooltip("Base physical damage - Multiplied by rarity, then scales with STR")]
    public float baseAD;
    
    [Tooltip("Base magical damage - Multiplied by rarity, then scales with INT")]
    public float baseAP;

    [Header("Defensive Stats")]
    public float baseHP;
    public float baseDefense;

    [Header("Offensive Stats")]
    public float baseAttackSpeed;
    public float baseCritRate;
    public float baseCritDamage;

    [Header("Utility Stats")]
    public float baseEvasion;
    public float baseTenacity;
    public float baseLethality;
    [Range(0f, 100f)] public float basePenetration;
    [Range(0f, 100f)] public float baseLifesteal;

    [Header("Weapon Skill")]
    public SkillSO defaultWeaponSkill;

    // === CALCULATED PROPERTIES (Read-Only) ===
    // These apply the rarity multiplier to base stats
    
    public float bonusAD => baseAD * GetRarityMultiplier() + GetBonusStat(BonusStatType.AttackDamage);
    public float bonusAP => baseAP * GetRarityMultiplier() + GetBonusStat(BonusStatType.AbilityPower);
    public float bonusHP => baseHP * GetRarityMultiplier() + GetBonusStat(BonusStatType.Health);
    public float bonusDefense => baseDefense * GetRarityMultiplier() + GetBonusStat(BonusStatType.Defense);
    public float bonusAttackSpeed => baseAttackSpeed * GetRarityMultiplier() + GetBonusStat(BonusStatType.AttackSpeed);
    public float bonusCritRate => baseCritRate * GetRarityMultiplier() + GetBonusStat(BonusStatType.CritRate);
    public float bonusCritDamage => baseCritDamage * GetRarityMultiplier() + GetBonusStat(BonusStatType.CritDamage);
    public float bonusEvasion => baseEvasion * GetRarityMultiplier() + GetBonusStat(BonusStatType.Evasion);
    public float bonusTenacity => baseTenacity * GetRarityMultiplier() + GetBonusStat(BonusStatType.Tenacity);
    public float bonusLethality => baseLethality * GetRarityMultiplier() + GetBonusStat(BonusStatType.Lethality);
    public float bonusPenetration => basePenetration * GetRarityMultiplier() + GetBonusStat(BonusStatType.Penetration);
    public float bonusLifesteal => baseLifesteal * GetRarityMultiplier() + GetBonusStat(BonusStatType.Lifesteal);

    private float GetRarityMultiplier()
    {
        return rarityConfig != null ? rarityConfig.statMultiplier : 1f;
    }
    
    private float GetBonusStat(BonusStatType type)
    {
        float total = 0f;
        foreach (var bonus in bonusStats)
        {
            if (bonus.statType == type)
                total += bonus.value;
        }
        return total;
    }

    /// <summary>
    /// Roll random bonus stats based on rarity (called when weapon is crafted/dropped)
    /// </summary>
    public void RollBonusStats()
    {
        if (rarityConfig == null || rarityConfig.bonusStatSlots <= 0)
        {
            bonusStats.Clear();
            return;
        }
        
        bonusStats.Clear();
        
        // Get all possible stat types
        var availableStats = new List<BonusStatType>((BonusStatType[])System.Enum.GetValues(typeof(BonusStatType)));
        
        // Roll bonus stats
        for (int i = 0; i < rarityConfig.bonusStatSlots && availableStats.Count > 0; i++)
        {
            // Pick random stat type
            int randomIndex = UnityEngine.Random.Range(0, availableStats.Count);
            BonusStatType statType = availableStats[randomIndex];
            availableStats.RemoveAt(randomIndex); // No duplicate stats
            
            // Roll value based on base stat
            float baseValue = GetBaseStatValue(statType);
            float rollPercent = UnityEngine.Random.Range(rarityConfig.bonusStatMinRoll, rarityConfig.bonusStatMaxRoll);
            float rolledValue = baseValue * rollPercent;
            
            bonusStats.Add(new WeaponBonusStat(statType, rolledValue));
        }
        
        Debug.Log($"[WeaponSO] Rolled {bonusStats.Count} bonus stats for {weaponName}");
    }
    
    private float GetBaseStatValue(BonusStatType type)
    {
        switch (type)
        {
            case BonusStatType.AttackDamage: return baseAD;
            case BonusStatType.AbilityPower: return baseAP;
            case BonusStatType.Health: return baseHP;
            case BonusStatType.Defense: return baseDefense;
            case BonusStatType.AttackSpeed: return baseAttackSpeed;
            case BonusStatType.CritRate: return baseCritRate;
            case BonusStatType.CritDamage: return baseCritDamage;
            case BonusStatType.Evasion: return baseEvasion;
            case BonusStatType.Tenacity: return baseTenacity;
            case BonusStatType.Lethality: return baseLethality;
            case BonusStatType.Penetration: return basePenetration;
            case BonusStatType.Lifesteal: return baseLifesteal;
            default: return 10f; // Fallback
        }
    }

    private void OnValidate()
    {
        itemName = weaponName;
        itemType = ItemType.Weapon;
        isStackable = false;
        
        // Sync rarity with ItemBaseSO (cast RarityTier to Rarity)
        if (rarityConfig != null)
        {
            rarity = (Rarity)((int)rarityConfig.tier);
        }
        
        if (string.IsNullOrEmpty(itemID))
        {
            itemID = $"weapon_{name.ToLower().Replace(" ", "_")}";
        }
    }

    public override string GetInfoText()
    {
        string rarityName = rarityConfig != null ? rarityConfig.rarityName : "Common";
        string rarityColorHex = rarityConfig != null ? ColorUtility.ToHtmlStringRGB(rarityConfig.rarityColor) : "FFFFFF";
        
        string info = $"<b>{weaponName}</b>\n";
        info += $"<color=#{rarityColorHex}>{rarityName} {weaponType}</color> ({attackRangeType})\n\n";
        
        // Base stats (with rarity multiplier)
        if (bonusAD > 0) info += $"<color=#ff6b6b>+{bonusAD:F1} Attack Damage</color>\n";
        if (bonusAP > 0) info += $"<color=#4ecdc4>+{bonusAP:F1} Ability Power</color>\n";
        
        if (bonusHP > 0) info += $"+{bonusHP:F0} HP\n";
        if (bonusDefense > 0) info += $"+{bonusDefense:F1} Defense\n";
        if (bonusAttackSpeed > 0) info += $"+{bonusAttackSpeed:F1} Attack Speed\n";
        if (bonusCritRate > 0) info += $"+{bonusCritRate:F1}% Crit Rate\n";
        if (bonusCritDamage > 0) info += $"+{bonusCritDamage:F1}% Crit Damage\n";
        if (bonusEvasion > 0) info += $"+{bonusEvasion:F1}% Evasion\n";
        if (bonusTenacity > 0) info += $"+{bonusTenacity:F1}% Tenacity\n";
        if (bonusLethality > 0) info += $"+{bonusLethality:F0} Lethality\n";
        if (bonusPenetration > 0) info += $"+{bonusPenetration:F1}% Penetration\n";
        if (bonusLifesteal > 0) info += $"+{bonusLifesteal:F1}% Lifesteal\n";
        
        // Bonus stats (highlighted)
        if (bonusStats.Count > 0)
        {
            info += $"\n<color=#{rarityColorHex}><b>Bonus Stats:</b></color>\n";
            foreach (var bonus in bonusStats)
            {
                info += $"<color=#ffd700>• {bonus.GetDisplayText()}</color>\n";
            }
        }
        
        if (!string.IsNullOrEmpty(description))
            info += $"\n<i>{description}</i>";
        
        if (defaultWeaponSkill != null)
            info += $"\n\n<color=#ffd93d>⚔ {defaultWeaponSkill.skillName}</color>";
        
        return info;
    }

    #region Debug Helpers
    
    [ContextMenu("Roll Bonus Stats")]
    private void DebugRollBonusStats()
    {
        RollBonusStats();
        Debug.Log($"[WeaponSO] Rolled stats for {weaponName}:");
        foreach (var bonus in bonusStats)
        {
            Debug.Log($"  • {bonus.GetDisplayText()}");
        }
    }

    [ContextMenu("Test: Equip Weapon")]
    private void DebugEquipWeapon()
    {
        if (Application.isPlaying && GameManager.Instance != null && GameManager.Instance.CurrentPlayer != null)
        {
            GameManager.Instance.CurrentPlayer.EquipWeapon(this, GameManager.Instance.BaseStats);
            Debug.Log($"[WeaponSO] Equipped {weaponName}");
            
            var stats = GameManager.Instance.CurrentPlayer;
            Debug.Log($"  AD: {stats.AD:F1} | AP: {stats.AP:F1} | Speed: {stats.AttackSpeed:F1}");
        }
        else
        {
            Debug.LogWarning("[WeaponSO] Can only equip during Play Mode with active player");
        }
    }

    [ContextMenu("Print Weapon Stats")]
    private void DebugPrintStats()
    {
        string rarityName = rarityConfig != null ? rarityConfig.rarityName : "None";
        float multiplier = GetRarityMultiplier();
        
        Debug.Log($"=== {weaponName} ===");
        Debug.Log($"ID: {itemID}");
        Debug.Log($"Type: {weaponType} ({attackRangeType})");
        Debug.Log($"Rarity: {rarityName} (x{multiplier})");
        Debug.Log($"Bonus Stat Slots: {bonusStats.Count}\n");
        
        Debug.Log("Final Stats (Base × Multiplier + Bonus):");
        if (bonusAD > 0) Debug.Log($"  AD: {baseAD} × {multiplier} + {GetBonusStat(BonusStatType.AttackDamage):F1} = {bonusAD:F1}");
        if (bonusAP > 0) Debug.Log($"  AP: {baseAP} × {multiplier} + {GetBonusStat(BonusStatType.AbilityPower):F1} = {bonusAP:F1}");
        if (bonusHP > 0) Debug.Log($"  HP: {baseHP} × {multiplier} + {GetBonusStat(BonusStatType.Health):F0} = {bonusHP:F0}");
        
        if (bonusStats.Count > 0)
        {
            Debug.Log("\nBonus Stats:");
            foreach (var bonus in bonusStats)
            {
                Debug.Log($"  • {bonus.GetDisplayText()}");
            }
        }
    }

    #endregion
}