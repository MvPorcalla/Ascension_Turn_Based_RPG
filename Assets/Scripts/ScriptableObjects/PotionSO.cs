// ──────────────────────────────────────────────────
// PotionSO.cs (Percentage-Based Update)
// ScriptableObject for defining potions in the game
// Supports percentage-based and flat healing values
// ──────────────────────────────────────────────────

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Potion", menuName = "Game/Potion")]
public class PotionSO : ItemBaseSO
{
    [Header("Potion Settings")]
    public PotionType potionType;
    
    [Header("Restore Type")]
    [Tooltip("Use percentage or flat values for healing")]
    public RestoreType restoreType = RestoreType.Percentage;
    
    [Header("Restore Effects")]
    [Tooltip("Percentage (0-100) or flat amount to restore")]
    [Range(0f, 100f)]
    public float healthRestorePercent = 20f;
    
    [Tooltip("Flat HP amount (only used if RestoreType = Flat)")]
    public float healthRestoreFlat = 50f;
    
    [Range(0f, 100f)]
    public float manaRestorePercent = 0f;
    
    [Tooltip("Flat Mana amount (only used if RestoreType = Flat)")]
    public float manaRestoreFlat = 0f;
    
    [Header("Duration Settings")]
    [Tooltip("How the restore duration is measured")]
    public DurationType durationType = DurationType.Instant;
    
    [Tooltip("Duration value (seconds for real-time, turns for turn-based)")]
    public float restoreDuration = 0f;
    
    [Header("Buffs")]
    public List<PotionBuff> buffs = new List<PotionBuff>();
    
    [Header("Usage Settings")]
    public bool canUseInCombat = true;
    public bool canUseOutOfCombat = true;

    // ✅ Calculated properties
    public float healthRestore => restoreType == RestoreType.Percentage ? healthRestorePercent : healthRestoreFlat;
    public float manaRestore => restoreType == RestoreType.Percentage ? manaRestorePercent : manaRestoreFlat;
    public float restoreAmount => Mathf.Max(healthRestore, manaRestore);
    public float duration => restoreDuration;
    public bool grantsBuff => buffs != null && buffs.Count > 0;
    public float buffDuration => (buffs != null && buffs.Count > 0) ? buffs[0].duration : 0f;
    public BuffType buffType => (buffs != null && buffs.Count > 0) ? buffs[0].type : BuffType.AttackDamage;
    public float buffValue => (buffs != null && buffs.Count > 0) ? buffs[0].value : 0f;

    // ✅ Turn-based properties
    public bool IsTurnBased => durationType == DurationType.TurnBased;
    public int TurnDuration => IsTurnBased ? Mathf.RoundToInt(restoreDuration) : 0;

    // ✅ Calculate actual heal amount for a given max HP
    public float GetActualHealAmount(float maxHP)
    {
        if (restoreType == RestoreType.Percentage)
        {
            return (healthRestorePercent / 100f) * maxHP;
        }
        else
        {
            return healthRestoreFlat;
        }
    }

    public float GetActualManaAmount(float maxMana)
    {
        if (restoreType == RestoreType.Percentage)
        {
            return (manaRestorePercent / 100f) * maxMana;
        }
        else
        {
            return manaRestoreFlat;
        }
    }

    private void OnValidate()
    {
        itemType = ItemType.Consumable;
        isStackable = true;
        maxStackSize = 999;
        
        // Auto-set duration type based on potion type
        if (potionType == PotionType.Rejuvenation)
        {
            durationType = DurationType.TurnBased;
        }
    }

    public override string GetInfoText()
    {
        string info = $"<b>{itemName}</b>\n{rarity} Potion\n\n";
        
        // Health restore
        if (healthRestore > 0)
        {
            info += GetRestoreDescription(healthRestore, "HP", restoreType);
        }
        
        // Mana restore
        if (manaRestore > 0)
        {
            info += GetRestoreDescription(manaRestore, "Mana", restoreType);
        }

        // Buffs
        if (buffs.Count > 0)
        {
            info += "\n<color=yellow>Buffs:</color>\n";
            foreach (var buff in buffs)
            {
                info += $"• {buff.GetDescription()}\n";
            }
        }

        // Description
        if (!string.IsNullOrEmpty(description))
            info += $"\n<i>{description}</i>";

        return info;
    }

    private string GetRestoreDescription(float amount, string statName, RestoreType type)
    {
        string amountText = type == RestoreType.Percentage ? $"{amount}%" : $"{amount}";
        
        switch (durationType)
        {
            case DurationType.Instant:
                return $"Restores {amountText} {statName} instantly\n";
            
            case DurationType.RealTime:
                return $"Restores {amountText} {statName} over {restoreDuration:F1}s\n";
            
            case DurationType.TurnBased:
                int turns = Mathf.RoundToInt(restoreDuration);
                float perTurn = amount / turns;
                string perTurnText = type == RestoreType.Percentage ? $"{perTurn:F1}%" : $"{perTurn:F1}";
                return $"Restores {perTurnText} {statName} per turn for {turns} turns (Total: {amountText})\n";
            
            default:
                return $"Restores {amountText} {statName}\n";
        }
    }

    /// <summary>
    /// Validates if the potion can be used in the given combat state
    /// </summary>
    public bool CanUse(bool isInCombat)
    {
        if (!canUseInCombat && isInCombat)
        {
            Debug.Log($"[PotionSO] Cannot use {itemName} in combat");
            return false;
        }

        if (!canUseOutOfCombat && !isInCombat)
        {
            Debug.Log($"[PotionSO] Can only use {itemName} in combat");
            return false;
        }

        return true;
    }

    #region Debug Helpers
    [ContextMenu("Generate Item ID")]
    private void GenerateItemID()
    {
        itemID = $"potion_{itemName.ToLower().Replace(" ", "_").Replace("'", "")}";
        Debug.Log($"Generated ID: {itemID}");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    [ContextMenu("Test: Add to Inventory")]
    private void DebugAddToInventory()
    {
        if (Application.isPlaying && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemID, 5);
            Debug.Log($"[PotionSO] Added 5x {itemName} to inventory");
        }
        else
        {
            Debug.LogWarning("Can only test in Play Mode with InventoryManager present");
        }
    }

    [ContextMenu("Print Potion Info")]
    private void DebugPrintInfo()
    {
        Debug.Log(GetInfoText());
    }

    [ContextMenu("Test Heal Amount (1000 HP)")]
    private void DebugTestHealAmount()
    {
        float testMaxHP = 1000f;
        float healAmount = GetActualHealAmount(testMaxHP);
        Debug.Log($"[{itemName}] Would heal {healAmount} HP on a player with {testMaxHP} max HP");
    }
    #endregion
}

// ──────────────────────────────────────────────────
// Supporting Classes
// ──────────────────────────────────────────────────

[System.Serializable]
public class PotionBuff
{
    public BuffType type;
    public float value;
    public float duration;
    public DurationType durationType = DurationType.RealTime;

    public string GetDescription()
    {
        string buffDesc = type switch
        {
            BuffType.AttackDamage => $"+{value} Attack Damage",
            BuffType.AbilityPower => $"+{value} Ability Power",
            BuffType.Defense => $"+{value} Defense",
            BuffType.Speed => $"+{value}% Movement Speed",
            BuffType.CritRate => $"+{value}% Crit Rate",
            BuffType.AttackSpeed => $"+{value}% Attack Speed",
            BuffType.Regeneration => $"+{value} HP/s Regeneration",
            BuffType.Resistance => $"+{value}% Damage Resistance",
            BuffType.Invisibility => "Invisibility",
            BuffType.Invulnerability => "Invulnerability",
            _ => "Unknown Buff"
        };

        string durationDesc = durationType switch
        {
            DurationType.TurnBased => $"{duration} turns",
            DurationType.RealTime => $"{duration}s",
            _ => ""
        };

        return $"{buffDesc} ({durationDesc})";
    }
}

public enum PotionType
{
    HealthPotion,
    ManaPotion,
    Elixir,           // HP + Mana instant
    BuffPotion,       // Pure buffs
    Antidote,         // Cure debuffs
    Rejuvenation,     // Heal over turns
    Utility           // Special effects
}

public enum RestoreType
{
    Percentage,       // Heal based on % of max HP/Mana
    Flat              // Heal fixed amount
}

public enum DurationType
{
    Instant,          // Applied immediately
    RealTime,         // Duration in seconds (for out-of-combat HoT)
    TurnBased         // Duration in turns (for combat HoT)
}

public enum BuffType
{
    AttackDamage,
    AbilityPower,
    Defense,
    Speed,
    CritRate,
    AttackSpeed,
    Regeneration,
    Resistance,
    Invisibility,
    Invulnerability
}