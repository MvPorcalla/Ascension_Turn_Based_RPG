// WeaponSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName;
    public WeaponType weaponType;
    public Sprite icon;
    public Rarity rarity = Rarity.Common;
    [TextArea] public string description;

    [Header("Combat Classification")]
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
    public float bonusArmor;
    public float bonusMR;
    public float bonusCritRate;
    public float bonusEvasion;
    public float bonusTenacity;

    [Header("Item-Only Bonuses")]
    public float bonusCritDamage;
    public float bonusLethality;
    [Range(0f, 1f)] public float bonusPhysicalPen;
    [Range(0f, 1f)] public float bonusMagicPen;

    [Header("Default Weapon Skill")]
    public SkillSO defaultWeaponSkill;

    [Header("Requirements")]
    public int requiredLevel = 1;
}

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