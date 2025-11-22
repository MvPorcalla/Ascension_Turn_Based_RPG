// -------------------------------
// CharacterBaseStatsSO.cs
// -------------------------------

using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "RPG/CharacterStats")]
public class CharacterBaseStatsSO : ScriptableObject
{
    [Header("Character Info")]
    public string className = "Default";
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
    public float BaseArmor = 5f;
    public float BaseMR = 5f;
    public float BaseCritRate = 5f;
    public float BaseCritDamage = 150f; // Item-only, not scaled by attributes
    public float BaseEvasion = 2f;
    public float BaseTenacity = 0f;
    
    [Header("Per Level Increases")]
    public float HPPerLevel = 15f;
    public float ADPerLevel = 2f;
    public float APPerLevel = 2f;
    public float ArmorPerLevel = 1f;
    public float MRPerLevel = 1f;
    public float CritRatePerLevel = 0.2f;
    public float EvasionPerLevel = 0.1f;
    public float TenacityPerLevel = 0.1f;
}