// ════════════════════════════════════════════
// CharacterBaseStatsSO.cs
// ScriptableObject holding base character stats and scaling
// ════════════════════════════════════════════

using UnityEngine;

namespace Ascension.Data.SO.Character
{
    [CreateAssetMenu(fileName = "NewCharacterStats", menuName = "BaseStats/CharacterStats")]
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
        public float BaseAttackSpeed = 10f;
        public float BaseCritRate = 5f;
        public float BaseCritDamage = 150f;
        public float BaseEvasion = 2f;
        public float BaseTenacity = 0f;

        [Header("Per Level Increases (Reduced for Weapon-Based)")]
        public float HPPerLevel = 8f;
        public float DefensePerLevel = 0.3f;
        public float EvasionPerLevel = 0.01f;
        public float TenacityPerLevel = 0.02f;

        [Header("Attribute Scaling - FLAT BONUSES")]
        public float STRtoAD = 0.5f;
        public float INTtoAP = 0.5f;
        public float ENDtoHP = 12f;
        public float ENDtoDefense = 0.4f;
        public float WIStoDefense = 0.4f;
        public float AGItoAttackSpeed = 1.0f;
        public float AGItoCritRate = 0.15f;
        public float AGItoEvasion = 0.1f;
        public float WIStoTenacity = 0.15f;

        [Header("Weapon Damage Scaling (NEW)")]
        public float weaponSTRScaling = 0.01f;
        public float weaponINTScaling = 0.01f;

        [Header("Soft Caps (NEW)")]
        public int attributeSoftCap = 300;
        public float postSoftCapEfficiency = 0.5f;

        [Header("Percentage Stat Caps")]
        public float baseEvasionCap = 40f;
        public float totalEvasionCap = 80f;
        public float baseCritRateCap = 60f;
        public float totalCritRateCap = 100f;
        public float baseTenacityCap = 40f;
        public float totalTenacityCap = 80f;
        public float maxPenetration = 100f;

        [Header("Transcendence System")]
        public bool enableTranscendence = false;
        public int maxTranscendenceLevel = 100;
        public float transcendenceEXPMultiplier = 10f;

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
            "  = 355+ AD\n\n" +
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
}
