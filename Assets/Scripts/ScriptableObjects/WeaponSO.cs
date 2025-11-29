// -------------------------------
// WeaponSO.cs (With Debug Helpers)
// -------------------------------
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Weapon")]
public class WeaponSO : ItemBaseSO
{
    [Header("Weapon Info")]
    public string weaponName;
    public WeaponType weaponType;
    public AttackRangeType attackRangeType;

    [Header("Attribute Bonuses")]
    public int bonusSTR;
    public int bonusINT;
    public int bonusAGI;
    public int bonusWIS;
    public int bonusEND;

    [Header("Derived Stat Bonuses")]
    public float bonusAD;
    public float bonusAP;
    public float bonusHP;
    public float bonusDefense;

    [Header("Percentage Stats")]
    public float bonusCritRate;
    public float bonusEvasion;
    public float bonusTenacity;

    [Header("Item-Only Bonuses")]
    public float bonusCritDamage;
    public float bonusLethality;
    [Range(0f, 100f)] public float bonusPenetration;
    [Range(0f, 100f)] public float bonusLifesteal;

    [Header("Default Weapon Skill")]
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
        info += $"{rarity} {weaponType}\n";
        info += $"{attackRangeType}\n\n";
        
        if (bonusAD > 0) info += $"+{bonusAD} AD\n";
        if (bonusAP > 0) info += $"+{bonusAP} AP\n";
        if (bonusHP > 0) info += $"+{bonusHP} HP\n";
        if (bonusDefense > 0) info += $"+{bonusDefense} Defense\n";
        if (bonusCritRate > 0) info += $"+{bonusCritRate}% Crit Rate\n";
        if (bonusCritDamage > 0) info += $"+{bonusCritDamage}% Crit Damage\n";
        if (bonusLifesteal > 0) info += $"+{bonusLifesteal}% Lifesteal\n";
        
        info += $"\n{description}";
        
        if (defaultWeaponSkill != null)
            info += $"\n\n<i>Skill: {defaultWeaponSkill.skillName}</i>";
        
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

    [ContextMenu("Print Item Info")]
    private void DebugPrintInfo()
    {
        Debug.Log($"=== {weaponName} ===");
        Debug.Log($"ID: {itemID}");
        Debug.Log($"Type: {weaponType} ({attackRangeType})");
        Debug.Log($"Rarity: {rarity}");
        Debug.Log($"Stackable: {isStackable}");
        Debug.Log($"\nStats:");
        if (bonusAD > 0) Debug.Log($"  AD: +{bonusAD}");
        if (bonusAP > 0) Debug.Log($"  AP: +{bonusAP}");
        if (bonusDefense > 0) Debug.Log($"  Defense: +{bonusDefense}");
        if (bonusCritRate > 0) Debug.Log($"  Crit Rate: +{bonusCritRate}%");
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
    Shield
}

public enum AttackRangeType
{
    Melee,
    Ranged,
    Magic
}