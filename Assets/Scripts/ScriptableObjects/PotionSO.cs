using UnityEngine;

[CreateAssetMenu(fileName = "New Potion", menuName = "Game/Potion")]
public class PotionSO : ItemBaseSO
{
    [Header("Potion Info")]
    public string potionName;
    public PotionType potionType;
    
    [Header("Potion Effects")]
    public float restoreAmount; // HP/Mana restored
    public float duration; // For buffs/DoTs (0 = instant)
    
    [Header("Buff Effects (Optional)")]
    public bool grantsBuff;
    public float buffDuration;
    public BuffType buffType;
    public float buffValue; // +10% speed, +50 AD, etc.
    
    [Header("Usage")]
    public bool canUseInCombat = true;
    public bool canUseOutOfCombat = true;
    public AnimationClip useAnimation; // Optional drinking animation

    private void OnValidate()
    {
        itemName = potionName;
        itemType = ItemType.Consumable;
        isStackable = true;
        maxStackSize = 999;

        if (string.IsNullOrEmpty(itemID))
        {
            itemID = $"potion_{name.ToLower().Replace(" ", "_")}";
        }
    }

    public override string GetInfoText()
    {
        string info = $"<b>{potionName}</b>\n{rarity} Potion\n\n";
        
        switch (potionType)
        {
            case PotionType.HealthPotion:
                info += duration > 0 
                    ? $"Restores {restoreAmount} HP over {duration}s\n" 
                    : $"Restores {restoreAmount} HP instantly\n";
                break;
            case PotionType.ManaPotion:
                info += duration > 0 
                    ? $"Restores {restoreAmount} Mana over {duration}s\n" 
                    : $"Restores {restoreAmount} Mana instantly\n";
                break;
            case PotionType.BuffPotion:
                info += $"Grants buff for {buffDuration}s\n{GetBuffDescription()}\n";
                break;
            case PotionType.Elixir:
                info += $"Restores {restoreAmount} HP & Mana\n";
                if (grantsBuff) info += $"+Buff: {GetBuffDescription()}\n";
                break;
        }

        if (!string.IsNullOrEmpty(description))
            info += $"\n<i>{description}</i>";

        return info;
    }

    private string GetBuffDescription()
    {
        return buffType switch
        {
            BuffType.AttackDamage => $"+{buffValue} Attack Damage",
            BuffType.AbilityPower => $"+{buffValue} Ability Power",
            BuffType.Defense => $"+{buffValue} Defense",
            BuffType.Speed => $"+{buffValue}% Movement Speed",
            BuffType.CritRate => $"+{buffValue}% Crit Rate",
            BuffType.AttackSpeed => $"+{buffValue}% Attack Speed",
            BuffType.Regeneration => $"+{buffValue} HP/s Regeneration",
            BuffType.Resistance => $"+{buffValue}% Damage Resistance",
            BuffType.Invisibility => "Grants Invisibility",
            BuffType.Invulnerability => "Grants Invulnerability",
            _ => "Unknown Buff"
        };
    }

    public bool Use(PlayerStats player)
    {
        Debug.Log($"[PotionSO] Used {potionName}");

        switch (potionType)
        {
            case PotionType.HealthPotion:
                Debug.Log($"Restored {restoreAmount} HP");
                break;
            case PotionType.ManaPotion:
                Debug.Log($"Restored {restoreAmount} Mana");
                break;
            case PotionType.BuffPotion:
                Debug.Log($"Applied {buffType} buff for {buffDuration}s");
                break;
            case PotionType.Elixir:
                Debug.Log($"Used elixir - restored HP/Mana and applied buff");
                break;
        }

        return true;
    }

    #region Debug Helpers
    [ContextMenu("Test: Add to Inventory (Bag)")]
    private void DebugAddToBag()
    {
        if (Application.isPlaying && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemID, 5, true);
            Debug.Log($"[PotionSO] Added 5x {potionName} to bag");
        }
    }

    [ContextMenu("Test: Add to Storage")]
    private void DebugAddToStorage()
    {
        if (Application.isPlaying && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemID, 10, false);
            Debug.Log($"[PotionSO] Added 10x {potionName} to storage");
        }
    }

    [ContextMenu("Print Potion Info")]
    private void DebugPrintInfo()
    {
        Debug.Log($"=== {potionName} ===");
        Debug.Log($"ID: {itemID}");
        Debug.Log($"Type: {potionType}");
        Debug.Log($"Rarity: {rarity}");
        Debug.Log($"Restore Amount: {restoreAmount}");
        Debug.Log($"Duration: {duration}s");
        if (grantsBuff)
            Debug.Log($"Buff: {buffType} (+{buffValue}) for {buffDuration}s");
    }
    #endregion
}

public enum PotionType
{
    HealthPotion,
    ManaPotion,
    BuffPotion,
    Elixir,
    Antidote,
    Utility
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
