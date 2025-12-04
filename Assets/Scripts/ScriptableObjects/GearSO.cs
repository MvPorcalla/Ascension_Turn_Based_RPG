// ──────────────────────────────────────────────────
// GearSO.cs
// ScriptableObject for defining armor and accessories
// ──────────────────────────────────────────────────

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Gear", menuName = "Game/Gear")]
public class GearSO : ItemBaseSO
{
    [Header("Gear Info")]
    public string gearName;
    public GearType gearType;
    
    [Header("Rarity System")]
    [Tooltip("Rarity configuration - affects stat multipliers and bonus slots")]
    public WeaponRaritySO rarityConfig;
    
    [Tooltip("Randomly rolled bonus stats (filled at craft/drop time)")]
    public List<WeaponBonusStat> bonusStats = new List<WeaponBonusStat>();
    
    [Header("Base Stats (Before Rarity Multiplier)")]
    public float baseHP;
    public float baseDefense;
    
    [Header("Offensive Stats")]
    public float baseAD;
    public float baseAP;
    public float baseAttackSpeed;
    public float baseCritRate;
    public float baseCritDamage;
    
    [Header("Utility Stats")]
    public float baseEvasion;
    public float baseTenacity;
    public float baseLethality;
    [Range(0f, 100f)] public float basePenetration;
    [Range(0f, 100f)] public float baseLifesteal;
    
    // === CALCULATED PROPERTIES (Read-Only) ===
    public float bonusHP => baseHP * GetRarityMultiplier() + GetBonusStat(BonusStatType.Health);
    public float bonusDefense => baseDefense * GetRarityMultiplier() + GetBonusStat(BonusStatType.Defense);
    public float bonusAD => baseAD * GetRarityMultiplier() + GetBonusStat(BonusStatType.AttackDamage);
    public float bonusAP => baseAP * GetRarityMultiplier() + GetBonusStat(BonusStatType.AbilityPower);
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
    
    public void RollBonusStats()
    {
        if (rarityConfig == null || rarityConfig.bonusStatSlots <= 0)
        {
            bonusStats.Clear();
            return;
        }
        
        bonusStats.Clear();
        
        var availableStats = new List<BonusStatType>((BonusStatType[])System.Enum.GetValues(typeof(BonusStatType)));
        
        for (int i = 0; i < rarityConfig.bonusStatSlots && availableStats.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableStats.Count);
            BonusStatType statType = availableStats[randomIndex];
            availableStats.RemoveAt(randomIndex);
            
            float baseValue = GetBaseStatValue(statType);
            float rollPercent = Random.Range(rarityConfig.bonusStatMinRoll, rarityConfig.bonusStatMaxRoll);
            float rolledValue = baseValue * rollPercent;
            
            bonusStats.Add(new WeaponBonusStat(statType, rolledValue));
        }
        
        Debug.Log($"[GearSO] Rolled {bonusStats.Count} bonus stats for {gearName}");
    }
    
    private float GetBaseStatValue(BonusStatType type)
    {
        switch (type)
        {
            case BonusStatType.Health: return baseHP;
            case BonusStatType.Defense: return baseDefense;
            case BonusStatType.AttackDamage: return baseAD;
            case BonusStatType.AbilityPower: return baseAP;
            case BonusStatType.AttackSpeed: return baseAttackSpeed;
            case BonusStatType.CritRate: return baseCritRate;
            case BonusStatType.CritDamage: return baseCritDamage;
            case BonusStatType.Evasion: return baseEvasion;
            case BonusStatType.Tenacity: return baseTenacity;
            case BonusStatType.Lethality: return baseLethality;
            case BonusStatType.Penetration: return basePenetration;
            case BonusStatType.Lifesteal: return baseLifesteal;
            default: return 10f;
        }
    }
    
    private void OnValidate()
    {
        itemName = gearName;
        itemType = ItemType.Gear;
        isStackable = false;
        
        if (rarityConfig != null)
        {
            rarity = (Rarity)((int)rarityConfig.tier);
        }
        
        if (string.IsNullOrEmpty(itemID))
        {
            itemID = $"gear_{gearType.ToString().ToLower()}_{name.ToLower().Replace(" ", "_")}";
        }
    }
    
    public override string GetInfoText()
    {
        string rarityName = rarityConfig != null ? rarityConfig.rarityName : "Common";
        string rarityColorHex = rarityConfig != null ? ColorUtility.ToHtmlStringRGB(rarityConfig.rarityColor) : "FFFFFF";
        
        string info = $"<b>{gearName}</b>\n";
        info += $"<color=#{rarityColorHex}>{rarityName} {gearType}</color>\n\n";
        
        if (bonusHP > 0) info += $"+{bonusHP:F0} HP\n";
        if (bonusDefense > 0) info += $"+{bonusDefense:F1} Defense\n";
        if (bonusAD > 0) info += $"<color=#ff6b6b>+{bonusAD:F1} Attack Damage</color>\n";
        if (bonusAP > 0) info += $"<color=#4ecdc4>+{bonusAP:F1} Ability Power</color>\n";
        if (bonusAttackSpeed > 0) info += $"+{bonusAttackSpeed:F1} Attack Speed\n";
        if (bonusCritRate > 0) info += $"+{bonusCritRate:F1}% Crit Rate\n";
        if (bonusCritDamage > 0) info += $"+{bonusCritDamage:F1}% Crit Damage\n";
        if (bonusEvasion > 0) info += $"+{bonusEvasion:F1}% Evasion\n";
        if (bonusTenacity > 0) info += $"+{bonusTenacity:F1}% Tenacity\n";
        if (bonusLethality > 0) info += $"+{bonusLethality:F0} Lethality\n";
        if (bonusPenetration > 0) info += $"+{bonusPenetration:F1}% Penetration\n";
        if (bonusLifesteal > 0) info += $"+{bonusLifesteal:F1}% Lifesteal\n";
        
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
        
        return info;
    }
    
    #region Debug Helpers
    
    [ContextMenu("Roll Bonus Stats")]
    private void DebugRollBonusStats()
    {
        RollBonusStats();
        Debug.Log($"[GearSO] Rolled stats for {gearName}:");
        foreach (var bonus in bonusStats)
        {
            Debug.Log($"  • {bonus.GetDisplayText()}");
        }
    }
    
    [ContextMenu("Test: Add to Storage")]
    private void DebugAddToStorage()
    {
        if (Application.isPlaying && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemID, 1, false);
            Debug.Log($"[GearSO] Added {gearName} to storage");
        }
        else
        {
            Debug.LogWarning("[GearSO] Cannot add to storage outside Play Mode or InventoryManager missing");
        }
    }
    
    #endregion
}

public enum GearType
{
    Helmet,
    ChestPlate,
    Gloves,
    Boots,
    Accessory
}