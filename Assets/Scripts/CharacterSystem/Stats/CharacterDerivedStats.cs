// ════════════════════════════════════════════
// CharacterDerivedStats.cs
// Calculates and holds the player's derived stats
// ════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Data.SO;
using Ascension.GameSystem;
using Ascension.Character.Stat;
using Ascension.Data.SO.Character;

namespace Ascension.Character.Stat
{
    [Serializable]
    public class CharacterDerivedStats
    {
        // ──────────────────────────────────────────────
        // Public Fields (Calculated Stats)
        // ──────────────────────────────────────────────

        public float AD;
        public float AP;
        public float MaxHP;
        public float Defense;
        public float AttackSpeed;
        public float CritRate;
        public float CritDamage;
        public float Evasion;
        public float Tenacity;
        public float Lethality;
        public float Penetration;
        public float Lifesteal;

        // ──────────────────────────────────────────────
        // Private Fields
        // ──────────────────────────────────────────────

        [NonSerialized] private bool isDirty = true;

        // ──────────────────────────────────────────────
        // Public Methods
        // ──────────────────────────────────────────────

        public void MarkDirty() => isDirty = true;

        public bool IsDirty() => isDirty;

        /// <summary>
        /// Calculate all derived stats with weapon support and soft caps
        /// </summary>
        public void Calculate(
            CharacterBaseStatsSO baseStats,
            int level,
            CharacterAttributes attributes,
            CharacterItemStats itemStats,
            WeaponSO equippedWeapon = null)
        {
            if (!isDirty) return;

            int levelBonus = level - 1;

            // Apply soft caps
            float effectiveSTR = ApplySoftCap(attributes.STR, baseStats.attributeSoftCap, baseStats.postSoftCapEfficiency);
            float effectiveINT = ApplySoftCap(attributes.INT, baseStats.attributeSoftCap, baseStats.postSoftCapEfficiency);
            float effectiveAGI = ApplySoftCap(attributes.AGI, baseStats.attributeSoftCap, baseStats.postSoftCapEfficiency);
            float effectiveEND = ApplySoftCap(attributes.END, baseStats.attributeSoftCap, baseStats.postSoftCapEfficiency);
            float effectiveWIS = ApplySoftCap(attributes.WIS, baseStats.attributeSoftCap, baseStats.postSoftCapEfficiency);

            // === ATTACK DAMAGE ===
            float baseAD = baseStats.BaseAD;
            float weaponAD = equippedWeapon != null ? equippedWeapon.BonusAD : 0f;
            float weaponScaling = weaponAD * (effectiveSTR * baseStats.weaponSTRScaling);
            float attributeAD = effectiveSTR * baseStats.STRtoAD;
            AD = baseAD + weaponAD + weaponScaling + attributeAD + itemStats.AD;

            // === ABILITY POWER ===
            float baseAP = baseStats.BaseAP;
            float weaponAP = equippedWeapon != null ? equippedWeapon.BonusAP : 0f;
            float weaponAPScaling = weaponAP * (effectiveINT * baseStats.weaponINTScaling);
            float attributeAP = effectiveINT * baseStats.INTtoAP;
            AP = baseAP + weaponAP + weaponAPScaling + attributeAP + itemStats.AP;

            // === HEALTH ===
            float levelHP = baseStats.HPPerLevel * levelBonus;
            float attributeHP = effectiveEND * baseStats.ENDtoHP;
            float weaponHP = equippedWeapon != null ? equippedWeapon.BonusHP : 0f;
            MaxHP = baseStats.BaseHP + levelHP + attributeHP + weaponHP + itemStats.HP;

            // === DEFENSE ===
            float levelDefense = baseStats.DefensePerLevel * levelBonus;
            float weaponDefense = equippedWeapon != null ? equippedWeapon.BonusDefense : 0f;
            Defense = baseStats.BaseDefense + levelDefense +
                      (effectiveEND * baseStats.ENDtoDefense) +
                      (effectiveWIS * baseStats.WIStoDefense) +
                      weaponDefense +
                      itemStats.Defense;

            // === ATTACK SPEED ===
            AttackSpeed = baseStats.BaseAttackSpeed +
                          (effectiveAGI * baseStats.AGItoAttackSpeed) +
                          itemStats.AttackSpeed;

            // === CRITICAL RATE ===
            float levelEvasion = baseStats.EvasionPerLevel * levelBonus;
            float baseCritRate = baseStats.BaseCritRate + (effectiveAGI * baseStats.AGItoCritRate);
            if (equippedWeapon != null) baseCritRate += equippedWeapon.BonusCritRate;
            baseCritRate = Mathf.Min(baseCritRate, baseStats.baseCritRateCap);
            CritRate = Mathf.Min(baseCritRate + itemStats.CritRate, baseStats.totalCritRateCap);

            // === CRITICAL DAMAGE ===
            float weaponCritDamage = equippedWeapon != null ? equippedWeapon.BonusCritDamage : 0f;
            CritDamage = baseStats.BaseCritDamage + weaponCritDamage + itemStats.CritDamage;

            // === EVASION ===
            float baseEvasion = baseStats.BaseEvasion + levelEvasion + (effectiveAGI * baseStats.AGItoEvasion);
            if (equippedWeapon != null) baseEvasion += equippedWeapon.BonusEvasion;
            Evasion = Mathf.Min(baseEvasion + itemStats.Evasion, baseStats.totalEvasionCap);

            // === TENACITY ===
            float levelTenacity = baseStats.TenacityPerLevel * levelBonus;
            float baseTenacity = baseStats.BaseTenacity + levelTenacity + (effectiveWIS * baseStats.WIStoTenacity);
            if (equippedWeapon != null) baseTenacity += equippedWeapon.BonusTenacity;
            Tenacity = Mathf.Min(baseTenacity + itemStats.Tenacity, baseStats.totalTenacityCap);

            // === ITEM-ONLY STATS ===
            float weaponLethality = equippedWeapon != null ? equippedWeapon.BonusLethality : 0f;
            float weaponPenetration = equippedWeapon != null ? equippedWeapon.BonusPenetration : 0f;
            float weaponLifesteal = equippedWeapon != null ? equippedWeapon.BonusLifesteal : 0f;

            Lethality = weaponLethality + itemStats.Lethality;
            Penetration = Mathf.Min(weaponPenetration + itemStats.Penetration, baseStats.maxPenetration);
            Lifesteal = weaponLifesteal + itemStats.Lifesteal;

            isDirty = false;
        }

        /// <summary>
        /// Force recalculation and return self (for chaining)
        /// </summary>
        public CharacterDerivedStats Recalculate(
            CharacterBaseStatsSO baseStats,
            int level,
            CharacterAttributes attributes,
            CharacterItemStats itemStats,
            WeaponSO equippedWeapon = null)
        {
            isDirty = true;
            Calculate(baseStats, level, attributes, itemStats, equippedWeapon);
            return this;
        }

        // ──────────────────────────────────────────────
        // Private Methods
        // ──────────────────────────────────────────────

        private float ApplySoftCap(int stat, int softCap, float postCapEfficiency)
        {
            if (stat <= softCap) return stat;
            float basePortion = softCap;
            float excessPortion = (stat - softCap) * postCapEfficiency;
            return basePortion + excessPortion;
        }
    }
}
