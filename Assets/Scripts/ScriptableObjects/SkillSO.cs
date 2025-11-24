// SkillSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
public class SkillSO : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Category & Requirements")]
    public SkillCategory category;
    public WeaponType[] compatibleWeaponTypes; // Array allows skill to work with multiple weapon types

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
    
    // public StatusEffectSO[] appliedEffects; // TODO: Add when status effects are implemented
}

public enum DamageType
{
    Physical,
    Magic,
    True // Ignores armor/MR
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