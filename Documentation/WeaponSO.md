// WeaponSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Weapon")]
public class WeaponSO : ScriptableObject
{
    // ---------------------------------------------------------
    // BASIC INFO
    // ---------------------------------------------------------
    [Header("Basic Info")]
    public string weaponName;
    public WeaponType weaponType;
    public Sprite icon;
    public Rarity rarity = Rarity.Common;
    [TextArea] public string description;

    // ---------------------------------------------------------
    // COMBAT ROLE / TYPE
    // ---------------------------------------------------------
    [Header("Combat Classification")]
    public AttackRangeType attackRangeType; // Melee / Ranged / Magic

    // ---------------------------------------------------------
    // STAT BONUSES (ATTRIBUTE-DRIVEN)
    // ---------------------------------------------------------
    [Header("Attribute Bonuses")]
    public int bonusSTR;
    public int bonusINT;
    public int bonusAGI;
    public int bonusWIS;
    public int bonusEND;

    // ---------------------------------------------------------
    // ITEM-ONLY BONUSES (DIRECT TO STATS)
    // ---------------------------------------------------------
    [Header("Item-Only Bonuses")]
    [Tooltip("Flat increase to Crit Damage %")]
    public float bonusCritDamage;
    [Tooltip("Flat Lethality, reduces armor directly")]
    public float bonusLethality;
    [Tooltip("Percentage Physical Penetration (0-1)")]
    public float bonusPhysicalPen;
    [Tooltip("Percentage Magic Penetration (0-1)")]
    public float bonusMagicPen;

    // ---------------------------------------------------------
    // SKILL SYSTEM
    // ---------------------------------------------------------
    [Header("Default Weapon Skill")]
    [Tooltip("Automatically granted when equipped")]
    public SkillSO defaultWeaponSkill;

    [Header("Equipable Skill Slots")]
    [Range(1, 6)]
    public int skillSlotCount = 3;
}

// ---------------------------------------------------------
// ENUMS
// ---------------------------------------------------------
public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary,
    Mythic
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


----------------------------------------------------------------------------


// SkillSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
public class SkillSO : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public Sprite icon;
    public WeaponType requiredWeaponType;

    [Header("Category & Slot Info")]
    public SkillCategory category;    // Weapon / Normal / Ultimate

    [Header("Damage & Scaling")]
    public float baseDamage;     
    [Range(0f, 5f)]
    public float adRatio;        
    [Range(0f, 5f)]
    public float apRatio;        

    [Header("Targeting")]
    public TargetType targetType = TargetType.Single;
    [Tooltip("Only used if targetType == Multi")]
    public int maxTargets = 1;

    [Header("Cooldown")]
    [Tooltip("Number of turns the player must wait before reusing this skill")]
    public int turnCooldown = 0;
}

public enum SkillCategory
{
    Weapon,   // Can only be assigned as a default weapon skill
    Normal,   // Equipable normal skill (max 2)
    Ultimate  // Equipable ultimate skill (max 1)
}

public enum TargetType
{
    Single,
    Multi,
    AllEnemies
}
