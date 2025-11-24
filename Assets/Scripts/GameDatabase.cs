// -------------------------------
// GameDatabase.cs
// -------------------------------

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameDatabase", menuName = "Game/Database")]
public class GameDatabase : ScriptableObject
{
    [Header("All Weapons")]
    public List<WeaponSO> allWeapons = new List<WeaponSO>();

    [Header("All Skills")]
    public List<SkillSO> allSkills = new List<SkillSO>();

    // Cached dictionaries for fast lookup
    private Dictionary<string, WeaponSO> weaponLookup;
    private Dictionary<string, SkillSO> skillLookup;

    public void Initialize()
    {
        weaponLookup = new Dictionary<string, WeaponSO>();
        foreach (var weapon in allWeapons)
        {
            if (weapon != null)
                weaponLookup[weapon.name] = weapon;
        }

        skillLookup = new Dictionary<string, SkillSO>();
        foreach (var skill in allSkills)
        {
            if (skill != null)
                skillLookup[skill.name] = skill;
        }
    }

    public WeaponSO GetWeapon(string weaponName)
    {
        if (weaponLookup == null) Initialize();
        return weaponLookup.TryGetValue(weaponName, out var weapon) ? weapon : null;
    }

    public SkillSO GetSkill(string skillName)
    {
        if (skillLookup == null) Initialize();
        return skillLookup.TryGetValue(skillName, out var skill) ? skill : null;
    }

    public List<SkillSO> GetSkillsForWeapon(WeaponType weaponType, SkillCategory category)
    {
        List<SkillSO> result = new List<SkillSO>();

        foreach (var skill in allSkills)
        {
            if (skill.category != category) continue;

            foreach (var compatibleType in skill.compatibleWeaponTypes)
            {
                if (compatibleType == weaponType)
                {
                    result.Add(skill);
                    break;
                }
            }
        }

        return result;
    }

    [ContextMenu("Test Database")]
    public void TestDatabase()
    {
        Initialize();
        
        // Test all weapons load
        Debug.Log($"=== WEAPONS ({allWeapons.Count}) ===");
        foreach (var weapon in allWeapons)
        {
            Debug.Log($"{weapon.weaponName} ({weapon.weaponType}) - AD:{weapon.bonusAD} | Skill: {weapon.defaultWeaponSkill?.skillName}");
        }
        
        // Test all skills load
        Debug.Log($"=== SKILLS ({allSkills.Count}) ===");
        foreach (var skill in allSkills)
        {
            Debug.Log($"{skill.skillName} ({skill.category}) - {skill.damageType} | Base:{skill.baseDamage}");
        }
        
        // Test filtering works
        Debug.Log("=== SWORD COMPATIBLE NORMAL SKILLS ===");
        var swordSkills = GetSkillsForWeapon(WeaponType.Sword, SkillCategory.Normal);
        foreach (var skill in swordSkills)
        {
            Debug.Log($"  - {skill.skillName}");
        }
    }
}