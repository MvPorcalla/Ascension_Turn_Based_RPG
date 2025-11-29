// -------------------------------
// CharacterBaseStatsSO.cs (Updated: Level 1000, Attack Speed, Tiered Caps)
// -------------------------------

using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "RPG/CharacterStats")]
public class CharacterBaseStatsSO : ScriptableObject
{
    [Header("Character Info")]
    public string className = "DefaultCharacterStats";
    public Sprite classIcon;
    
    [Header("Level System")]
    [Tooltip("Maximum normal level before transcendence")]
    public int maxLevel = 1000;
    
    [Tooltip("Base EXP requirement for level 2")]
    public int baseEXPRequirement = 100;
    
    [Tooltip("Linear EXP growth per level")]
    public float expLinearGrowth = 50f;
    
    [Tooltip("Exponential EXP growth (level ^ exponent)")]
    public float expExponentGrowth = 1.5f;
    
    [Tooltip("Attribute points awarded per level")]
    public int pointsPerLevel = 5;
    
    [Header("Starting Attributes")]
    public int startingSTR = 1;
    public int startingINT = 1;
    public int startingAGI = 1;
    public int startingEND = 1;
    public int startingWIS = 1;
    
    [Tooltip("Bonus points to allocate during character creation")]
    public int bonusPointsToAllocate = 50;
    
    [Header("Base Combat Stats (Level 1)")]
    public float BaseAD = 5f;
    public float BaseAP = 5f;
    public float BaseHP = 100f;
    public float BaseDefense = 10f;
    public float BaseAttackSpeed = 100f; // NEW: Base attack speed
    public float BaseCritRate = 5f;
    public float BaseCritDamage = 150f;
    public float BaseEvasion = 2f;
    public float BaseTenacity = 0f;
    
    [Header("Per Level Increases")]
    [Tooltip("HP gained per level")]
    public float HPPerLevel = 15f;
    
    [Tooltip("Defense gained per level")]
    public float DefensePerLevel = 0.5f;
    
    [Tooltip("Evasion % gained per level")]
    public float EvasionPerLevel = 0.02f;
    
    [Tooltip("Tenacity % gained per level")]
    public float TenacityPerLevel = 0.03f;
    
    [Header("Attribute Scaling")]
    [Tooltip("AD % scaling per STR (multiplicative)")]
    public float STRtoAD = 0.025f; // 2.5% per point
    
    [Tooltip("AP % scaling per INT (multiplicative)")]
    public float INTtoAP = 0.025f; // 2.5% per point
    
    [Tooltip("HP % scaling per END (multiplicative)")]
    public float ENDtoHP = 0.06f; // 6% per point
    
    [Tooltip("Defense flat bonus per END")]
    public float ENDtoDefense = 0.6f;
    
    [Tooltip("Defense flat bonus per WIS")]
    public float WIStoDefense = 0.6f;
    
    [Tooltip("Attack Speed flat bonus per AGI")]
    public float AGItoAttackSpeed = 1.5f; // NEW: 1.5 speed per AGI
    
    [Tooltip("Crit Rate % bonus per AGI")]
    public float AGItoCritRate = 0.6f; // 0.6% per point
    
    [Tooltip("Evasion % bonus per AGI")]
    public float AGItoEvasion = 0.25f; // 0.25% per point
    
    [Tooltip("Tenacity % bonus per WIS")]
    public float WIStoTenacity = 0.6f; // 0.6% per point
    
    [Header("Percentage Stat Caps")]
    [Tooltip("Max Evasion from base stats + attributes only")]
    public float baseEvasionCap = 40f;
    
    [Tooltip("Max total Evasion including items")]
    public float totalEvasionCap = 80f;
    
    [Tooltip("Max Crit Rate from base stats + attributes only")]
    public float baseCritRateCap = 60f;
    
    [Tooltip("Max total Crit Rate including items")]
    public float totalCritRateCap = 100f;
    
    [Tooltip("Max Tenacity (no separate base cap)")]
    public float maxTenacity = 80f;
    
    [Tooltip("Max Penetration % (items only)")]
    public float maxPenetration = 100f;
    
    [Header("Transcendence System")]
    [Tooltip("Enable transcendence levels after reaching maxLevel")]
    public bool enableTranscendence = false;
    
    [Tooltip("Maximum transcendence levels (after normal maxLevel)")]
    public int maxTranscendenceLevel = 100;
    
    [Tooltip("EXP multiplier for transcendence levels")]
    public float transcendenceEXPMultiplier = 10f;
    
    [Header("Design Notes")]
    [TextArea(4, 8)]
    public string designNotes = 
        "=== MERGED STATS ===\n" +
        "Defense: Armor + MR → Single stat\n" +
        "Penetration: Physical Pen + Magic Pen → Single %\n\n" +
        "=== ITEM-ONLY STATS ===\n" +
        "Lifesteal: % healing on damage dealt\n" +
        "Lethality: Flat penetration\n" +
        "Crit Damage: No attribute scaling\n\n" +
        "=== NEW: ATTACK SPEED ===\n" +
        "Base: 100, AGI scaling: 1.5 per point\n" +
        "No cap - determines turn order\n\n" +
        "=== TIERED CAPS ===\n" +
        "Evasion: 40% base → 80% total\n" +
        "Crit Rate: 60% base → 100% total\n" +
        "Penetration: 0% base → 100% total (items only)";
    
    [Header("Quick Reference")]
    [TextArea(3, 6)]
    public string levelingReference = 
        "Level 1 → 2: ~151 EXP\n" +
        "Level 100 → 101: ~6,150 EXP\n" +
        "Level 500 → 501: ~36,205 EXP\n" +
        "Level 999 → 1000: ~81,572 EXP\n\n" +
        "Total Points at Lv.1000: ~4,995 points (5 per level)\n" +
        "Transcendence: Optional, 10x EXP multiplier";
}