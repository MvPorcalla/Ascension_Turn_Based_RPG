// ──────────────────────────────────────────────────
// CharacterBaseStatsSO.cs
// ScriptableObject holding base character stats and scaling
// ──────────────────────────────────────────────────

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
    
    [Header("Base Combat Stats (Level 1, No Weapon)")]
    public float BaseAD = 5f;
    public float BaseAP = 5f;
    public float BaseHP = 100f;
    public float BaseDefense = 10f;
    public float BaseAttackSpeed = 10f;    // CHANGED: 100 → 10
    public float BaseCritRate = 5f;
    public float BaseCritDamage = 150f;
    public float BaseEvasion = 2f;
    public float BaseTenacity = 0f;

    // ---------------------------------------------------------------------
    
    [Header("Per Level Increases (Reduced for Weapon-Based)")]
    [Tooltip("HP gained per level")]
    public float HPPerLevel = 8f;          // CHANGED: 20 → 8
    
    [Tooltip("Defense gained per level")]
    public float DefensePerLevel = 0.3f;   // CHANGED: 0.5 → 0.3
    
    [Tooltip("Evasion % gained per level")]
    public float EvasionPerLevel = 0.01f;  // CHANGED: 0.02 → 0.01
    
    [Tooltip("Tenacity % gained per level")]
    public float TenacityPerLevel = 0.02f; // CHANGED: 0.03 → 0.02

    // ---------------------------------------------------------------------
    
    [Header("Attribute Scaling - FLAT BONUSES")]
    [Tooltip("Flat AD bonus per STR")]
    public float STRtoAD = 0.5f;           // CHANGED: 0.025 → 0.5 (FLAT, not %)
    
    [Tooltip("Flat AP bonus per INT")]
    public float INTtoAP = 0.5f;           // CHANGED: 0.025 → 0.5 (FLAT, not %)
    
    [Tooltip("Flat HP bonus per END")]
    public float ENDtoHP = 12f;            // CHANGED: Formula changed from % to flat
    
    [Tooltip("Defense flat bonus per END")]
    public float ENDtoDefense = 0.4f;      // CHANGED: 0.6 → 0.4
    
    [Tooltip("Defense flat bonus per WIS")]
    public float WIStoDefense = 0.4f;      // CHANGED: 0.6 → 0.4
    
    [Tooltip("Attack Speed flat bonus per AGI")]
    public float AGItoAttackSpeed = 1.0f;  // CHANGED: 1.5 → 1.0
    
    [Tooltip("Crit Rate % bonus per AGI")]
    public float AGItoCritRate = 0.15f;    // CHANGED: 0.6 → 0.15
    
    [Tooltip("Evasion % bonus per AGI")]
    public float AGItoEvasion = 0.1f;      // CHANGED: 0.25 → 0.1
    
    [Tooltip("Tenacity % bonus per WIS")]
    public float WIStoTenacity = 0.15f;    // CHANGED: 0.6 → 0.15

    // ---------------------------------------------------------------------
    
    [Header("Weapon Damage Scaling (NEW)")]
    [Tooltip("Weapon damage % scaling from STR (multiplicative)")]
    public float weaponSTRScaling = 0.01f; // NEW: 1% per STR
    
    [Tooltip("Weapon damage % scaling from INT (multiplicative)")]
    public float weaponINTScaling = 0.01f; // NEW: 1% per INT
    
    [Header("Soft Caps (NEW)")]
    [Tooltip("Diminishing returns start after this threshold")]
    public int attributeSoftCap = 300;     // NEW: First 300 points = 100% value
    
    [Tooltip("Efficiency after soft cap")]
    public float postSoftCapEfficiency = 0.5f; // NEW: 50% value after cap

    // ---------------------------------------------------------------------

    [Header("Percentage Stat Caps")]
    [Tooltip("Max Evasion from base stats + attributes only")]
    public float baseEvasionCap = 40f;
    
    [Tooltip("Max total Evasion including items")]
    public float totalEvasionCap = 80f;
    
    [Tooltip("Max Crit Rate from base stats + attributes only")]
    public float baseCritRateCap = 60f;
    
    [Tooltip("Max total Crit Rate including items")]
    public float totalCritRateCap = 100f;
    
    [Tooltip("Max Tenacity from base stats + attributes only")] // NEW
    public float baseTenacityCap = 40f;                          // NEW

    [Tooltip("Max total Tenacity including items")]
    public float totalTenacityCap = 80f;  
    
    [Tooltip("Max Penetration % (items only)")]
    public float maxPenetration = 100f;

    // ---------------------------------------------------------------------
    
    [Header("Transcendence System")]
    [Tooltip("Enable transcendence levels after reaching maxLevel")]
    public bool enableTranscendence = false;
    
    [Tooltip("Maximum transcendence levels (after normal maxLevel)")]
    public int maxTranscendenceLevel = 100;
    
    [Tooltip("EXP multiplier for transcendence levels")]
    public float transcendenceEXPMultiplier = 10f;

    // ---------------------------------------------------------------------
    
    [Header("Design Notes")]
    [TextArea(4, 12)]
    public string designNotes = 
        "=== TURN-BASED RPG SCALING ===\n" +
        "Weapons are PRIMARY damage source\n" +
        "Attributes give moderate FLAT bonuses\n" +
        "Soft caps at 300 encourage balanced builds\n\n" +
        "=== DAMAGE FORMULA ===\n" +
        "AD = BaseAD + WeaponAD + (WeaponAD × STR × 0.01) + (STR × 0.5) + ItemAD\n" +
        "Example at Lv100, 150 weapon, 100 STR:\n" +
        "  = 5 + 150 + (150×100×0.01) + (100×0.5) + items\n" +
        "  = 5 + 150 + 150 + 50 + items = 355+ AD\n\n" +
        "=== ATTACK SPEED ===\n" +
        "Base: 10, AGI: 1.0 per point\n" +
        "Determines turn order in battle (higher = faster)\n\n" +
        "=== SOFT CAPS ===\n" +
        "First 300 points: 100% efficiency\n" +
        "After 300: 50% efficiency\n" +
        "Encourages 2-3 main stats instead of 1-stat stacking";
    
    [Header("Quick Reference")]
    [TextArea(3, 8)]
    public string levelingReference = 
        "=== LEVEL PROGRESSION ===\n" +
        "Level 1 → 2: ~151 EXP\n" +
        "Level 100 → 101: ~6,150 EXP\n" +
        "Level 500 → 501: ~36,205 EXP\n" +
        "Level 999 → 1000: ~81,572 EXP\n\n" +
        "Points at Lv.1000: ~4,995 (5 per level)\n" +
        "Optimal: 300 in 2-3 stats (soft cap)\n" +
        "Example: 300 STR + 300 AGI + 300 END = 900 points\n" +
        "Remaining 4,095 points for specialization";
}