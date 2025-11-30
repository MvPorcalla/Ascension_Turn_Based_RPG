// -------------------------------
// WeaponSO.cs - Direct Stats Only (Simplified)
// -------------------------------
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Weapon")]
public class WeaponSO : ItemBaseSO
{
    [Header("Weapon Info")]
    public string weaponName;
    public WeaponType weaponType;
    public AttackRangeType attackRangeType;

    [Header("Primary Stats (Main Damage Source)")]
    [Tooltip("Base physical damage - Scales with STR (1% per point)")]
    public float bonusAD;
    
    [Tooltip("Base magical damage - Scales with INT (1% per point)")]
    public float bonusAP;

    [Header("Defensive Stats")]
    [Tooltip("Bonus health")]
    public float bonusHP;
    
    [Tooltip("Bonus defense (reduces incoming damage)")]
    public float bonusDefense;

    [Header("Offensive Stats")]
    [Tooltip("Attack speed (affects turn order - higher = faster)")]
    public float bonusAttackSpeed;  // NEW: Was missing!
    
    [Tooltip("Critical strike chance %")]
    public float bonusCritRate;
    
    [Tooltip("Critical damage multiplier %")]
    public float bonusCritDamage;

    [Header("Utility Stats")]
    [Tooltip("Dodge chance %")]
    public float bonusEvasion;
    
    [Tooltip("CC resistance %")]
    public float bonusTenacity;
    
    [Tooltip("Flat armor penetration")]
    public float bonusLethality;
    
    [Tooltip("% armor penetration")]
    [Range(0f, 100f)] public float bonusPenetration;
    
    [Tooltip("Lifesteal % (heal on damage dealt)")]
    [Range(0f, 100f)] public float bonusLifesteal;

    [Header("Weapon Skill")]
    [Tooltip("Default auto-attack skill for this weapon")]
    public SkillSO defaultWeaponSkill;

    private void OnValidate()
    {
        // Auto-sync with ItemBaseSO
        itemName = weaponName;
        itemType = ItemType.Weapon;
        isStackable = false;
        
        // Auto-generate itemID if empty
        if (string.IsNullOrEmpty(itemID))
        {
            itemID = $"weapon_{name.ToLower().Replace(" ", "_")}";
        }
    }

    public override string GetInfoText()
    {
        string info = $"<b>{weaponName}</b>\n";
        info += $"<color=#888888>{rarity} {weaponType} ({attackRangeType})</color>\n\n";
        
        // Primary damage stats (colored for visibility)
        if (bonusAD > 0) info += $"<color=#ff6b6b>+{bonusAD} Attack Damage</color>\n";
        if (bonusAP > 0) info += $"<color=#4ecdc4>+{bonusAP} Ability Power</color>\n";
        
        // Defensive stats
        if (bonusHP > 0) info += $"+{bonusHP} HP\n";
        if (bonusDefense > 0) info += $"+{bonusDefense} Defense\n";
        
        // Offensive stats
        if (bonusAttackSpeed > 0) info += $"+{bonusAttackSpeed} Attack Speed\n";
        if (bonusCritRate > 0) info += $"+{bonusCritRate}% Crit Rate\n";
        if (bonusCritDamage > 0) info += $"+{bonusCritDamage}% Crit Damage\n";
        
        // Utility stats
        if (bonusEvasion > 0) info += $"+{bonusEvasion}% Evasion\n";
        if (bonusTenacity > 0) info += $"+{bonusTenacity}% Tenacity\n";
        if (bonusLethality > 0) info += $"+{bonusLethality} Lethality\n";
        if (bonusPenetration > 0) info += $"+{bonusPenetration}% Penetration\n";
        if (bonusLifesteal > 0) info += $"+{bonusLifesteal}% Lifesteal\n";
        
        // Description
        if (!string.IsNullOrEmpty(description))
            info += $"\n<i>{description}</i>";
        
        // Weapon skill
        if (defaultWeaponSkill != null)
            info += $"\n\n<color=#ffd93d>⚔ {defaultWeaponSkill.skillName}</color>";
        
        return info;
    }

    #region Debug Helpers
    
    [ContextMenu("Test: Add to Inventory (Bag)")]
    private void DebugAddToBag()
    {
        if (Application.isPlaying && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemID, 1, true);
            Debug.Log($"[WeaponSO] Added {weaponName} to bag");
        }
        else
        {
            Debug.LogWarning("[WeaponSO] Can only add items during Play Mode with InventoryManager active");
        }
    }

    [ContextMenu("Test: Add to Storage")]
    private void DebugAddToStorage()
    {
        if (Application.isPlaying && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemID, 1, false);
            Debug.Log($"[WeaponSO] Added {weaponName} to storage");
        }
        else
        {
            Debug.LogWarning("[WeaponSO] Can only add items during Play Mode with InventoryManager active");
        }
    }

    [ContextMenu("Test: Equip Weapon")]
    private void DebugEquipWeapon()
    {
        if (Application.isPlaying && GameManager.Instance != null && GameManager.Instance.CurrentPlayer != null)
        {
            GameManager.Instance.CurrentPlayer.EquipWeapon(this, GameManager.Instance.BaseStats);
            Debug.Log($"[WeaponSO] Equipped {weaponName}");
            
            // Print resulting stats
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
        Debug.Log($"=== {weaponName} ===");
        Debug.Log($"ID: {itemID}");
        Debug.Log($"Type: {weaponType} ({attackRangeType})");
        Debug.Log($"Rarity: {rarity}");
        Debug.Log($"Stackable: {isStackable}\n");
        
        Debug.Log("Primary Stats:");
        if (bonusAD > 0) Debug.Log($"  AD: +{bonusAD}");
        if (bonusAP > 0) Debug.Log($"  AP: +{bonusAP}");
        
        Debug.Log("\nDefensive Stats:");
        if (bonusHP > 0) Debug.Log($"  HP: +{bonusHP}");
        if (bonusDefense > 0) Debug.Log($"  Defense: +{bonusDefense}");
        
        Debug.Log("\nOffensive Stats:");
        if (bonusAttackSpeed > 0) Debug.Log($"  Attack Speed: +{bonusAttackSpeed}");
        if (bonusCritRate > 0) Debug.Log($"  Crit Rate: +{bonusCritRate}%");
        if (bonusCritDamage > 0) Debug.Log($"  Crit Damage: +{bonusCritDamage}%");
        
        Debug.Log("\nUtility Stats:");
        if (bonusEvasion > 0) Debug.Log($"  Evasion: +{bonusEvasion}%");
        if (bonusTenacity > 0) Debug.Log($"  Tenacity: +{bonusTenacity}%");
        if (bonusLethality > 0) Debug.Log($"  Lethality: +{bonusLethality}");
        if (bonusPenetration > 0) Debug.Log($"  Penetration: +{bonusPenetration}%");
        if (bonusLifesteal > 0) Debug.Log($"  Lifesteal: +{bonusLifesteal}%");
    }

    #endregion
}

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