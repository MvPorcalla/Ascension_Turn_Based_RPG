// -------------------------------
// CharacterBaseStatsSO.cs (Reworked with Defense)
// -------------------------------

using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "RPG/CharacterStats")]
public class CharacterBaseStatsSO : ScriptableObject
{
    [Header("Character Info")]
    public string className = "DefaultCharacterStats";
    public Sprite classIcon;
    
    [Header("Starting Attributes")]
    public int startingSTR = 1;
    public int startingINT = 1;
    public int startingAGI = 1;
    public int startingEND = 1;
    public int startingWIS = 1;
    public int bonusPointsToAllocate = 50;
    
    [Header("Base Combat Stats (Level 1)")]
    public float BaseAD = 5f;
    public float BaseAP = 5f;
    public float BaseHP = 100f;
    public float BaseDefense = 10f; // Merged from BaseArmor (5) + BaseMR (5)
    public float BaseCritRate = 5f;
    public float BaseCritDamage = 150f; // Item-only, not scaled by attributes
    public float BaseEvasion = 2f;
    public float BaseTenacity = 0f;
    
    [Header("Per Level Increases")]
    public float HPPerLevel = 15f;
    public float DefensePerLevel = 2f; // Merged from ArmorPerLevel (1) + MRPerLevel (1)
    public float EvasionPerLevel = 0.1f;
    public float TenacityPerLevel = 0.1f;
    
    [Header("Notes")]
    [TextArea(3, 5)]
    public string designNotes = 
        "Defense: Merged Armor + MR into single stat.\n" +
        "Penetration: Merged Physical Pen + Magic Pen into single %.\n" +
        "Lifesteal: New item-only stat for healing on damage dealt.\n" +
        "Lethality: Flat penetration (item-only).\n" +
        "AD/AP: Unchanged, scaled by STR/INT.\n" +
        "Crit Damage: Item-only, no attribute scaling.";
}