// -------------------------------
// SkillSO.cs (With Debug Helpers)
// -------------------------------
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
public class SkillSO : ItemBaseSO
{
    [Header("Skill Info")]
    public string skillName;

    [Header("Category & Requirements")]
    public SkillCategory category;
    public WeaponType[] compatibleWeaponTypes;

    [Header("Damage & Scaling")]
    public DamageType damageType = DamageType.Physical;
    public float baseDamage;
    [Range(0f, 5f)] public float adRatio;
    [Range(0f, 5f)] public float apRatio;

    [Header("Targeting")]
    public TargetType targetType = TargetType.Single;
    [Tooltip("Only used if targetType == Multi")]
    public int maxTargets = 1;

    [Header("Cooldown")]
    public int turnCooldown = 0;

    [Header("Effects")]
    public bool canCrit = true;

    private void OnValidate()
    {
        // Auto-sync with ItemBaseSO
        itemName = skillName;
        itemType = ItemType.Skill;
        isStackable = false;

        // Auto-generate itemID if empty
        if (string.IsNullOrEmpty(itemID))
        {
            itemID = $"skill_{name.ToLower().Replace(" ", "_")}";
        }
    }

    public override string GetInfoText()
    {
        string info = $"<b>{skillName}</b>\n";
        info += $"{category} Skill\n";
        info += $"{damageType} Damage\n\n";
        
        info += $"Base Damage: {baseDamage}\n";
        if (adRatio > 0) info += $"AD Ratio: {adRatio * 100}%\n";
        if (apRatio > 0) info += $"AP Ratio: {apRatio * 100}%\n";
        info += $"Target: {targetType}\n";
        if (turnCooldown > 0) info += $"Cooldown: {turnCooldown} turns\n";
        
        info += $"\n{description}";
        
        return info;
    }

    #region Debug Helpers

    [ContextMenu("Test: Add to Inventory")]
    private void DebugAddToInventory()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[SkillSO] Enter Play Mode first!");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[SkillSO] InventoryManager not found!");
            return;
        }

        InventoryManager.Instance.AddItem(itemID, 1, false);
        Debug.Log($"[SkillSO] Added {skillName} to storage");
    }

    [ContextMenu("Print Skill Info")]
    private void DebugPrintInfo()
    {
        Debug.Log($"=== {skillName} ===");
        Debug.Log($"ID: {itemID}");
        Debug.Log($"Category: {category}");
        Debug.Log($"Damage Type: {damageType}");
        Debug.Log($"Base Damage: {baseDamage}");
        Debug.Log($"AD Ratio: {adRatio * 100}%");
        Debug.Log($"AP Ratio: {apRatio * 100}%");
        Debug.Log($"Target: {targetType}");
        Debug.Log($"Cooldown: {turnCooldown} turns");
        Debug.Log($"Can Crit: {canCrit}");
        
        if (compatibleWeaponTypes != null && compatibleWeaponTypes.Length > 0)
        {
            Debug.Log($"Compatible Weapons: {string.Join(", ", compatibleWeaponTypes)}");
        }
    }

    #endregion
}

public enum DamageType
{
    Physical,
    Magic,
    True
}

public enum SkillCategory
{
    Weapon,
    Normal,
    Ultimate
}

public enum TargetType
{
    Single,
    Multi,
    AllEnemies,
    Self,
    AllAllies
}