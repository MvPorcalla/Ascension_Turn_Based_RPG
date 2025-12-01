// ──────────────────────────────────────────────────
// PotionSO.cs (FIXED)
// ScriptableObject for defining potions in the game
// ──────────────────────────────────────────────────

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Potion", menuName = "Game/Potion")]
public class PotionSO : ItemBaseSO
{
    [Header("Potion Settings")]
    public PotionType potionType;
    
    [Header("Restore Effects")]
    public float healthRestore;
    public float manaRestore;
    public float restoreDuration; // 0 = instant, >0 = over time
    
    [Header("Buffs")]
    public List<PotionBuff> buffs = new List<PotionBuff>();
    
    [Header("Usage Settings")]
    public bool canUseInCombat = true;
    public bool canUseOutOfCombat = true;
    public float cooldown = 1f; // Potion category cooldown

    // ✅ ADDED: Properties for backwards compatibility with PotionPopupUI
    public float restoreAmount => Mathf.Max(healthRestore, manaRestore);
    public float duration => restoreDuration;
    public bool grantsBuff => buffs != null && buffs.Count > 0;
    public float buffDuration => (buffs != null && buffs.Count > 0) ? buffs[0].duration : 0f;
    public BuffType buffType => (buffs != null && buffs.Count > 0) ? buffs[0].type : BuffType.AttackDamage;
    public float buffValue => (buffs != null && buffs.Count > 0) ? buffs[0].value : 0f;

    private void OnValidate()
    {
        itemType = ItemType.Consumable;
        isStackable = true;
        maxStackSize = 999;
    }

    public override string GetInfoText()
    {
        string info = $"<b>{itemName}</b>\n{rarity} Potion\n\n";
        
        // Health/Mana restore
        if (healthRestore > 0)
        {
            info += restoreDuration > 0 
                ? $"Restores {healthRestore} HP over {restoreDuration}s\n" 
                : $"Restores {healthRestore} HP instantly\n";
        }
        
        if (manaRestore > 0)
        {
            info += restoreDuration > 0 
                ? $"Restores {manaRestore} Mana over {restoreDuration}s\n" 
                : $"Restores {manaRestore} Mana instantly\n";
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

        // Cooldown
        if (cooldown > 0)
            info += $"\n<color=gray>Cooldown: {cooldown}s</color>";

        // Description
        if (!string.IsNullOrEmpty(description))
            info += $"\n\n<i>{description}</i>";

        return info;
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

        return $"{buffDesc} ({duration}s)";
    }
}

// ✅ FIXED: Added missing Antidote enum value
public enum PotionType
{
    HealthPotion,
    ManaPotion,
    Elixir,        // HP + Mana
    BuffPotion,    // Pure buffs
    Antidote,      // ✅ ADDED
    Utility        // Special effects
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