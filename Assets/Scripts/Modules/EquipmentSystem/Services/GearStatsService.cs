// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Services\GearStatsService.cs
// Service for calculating total stats from equipped gear
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Database;
using Ascension.Character.Core;
using Ascension.Equipment.Data;
using Ascension.Data.Enums;

namespace Ascension.Equipment.Services
{
    /// <summary>
    /// Service responsible for calculating total stats from all equipped gear
    /// Stateless, reusable logic
    /// </summary>
    public class GearStatsService
    {
        /// <summary>
        /// Calculate total item stats from all equipped gear
        /// </summary>
        public CharacterItemStats CalculateTotalStats(EquippedGear equippedGear, GameDatabaseSO database)
        {
            if (equippedGear == null || database == null)
            {
                Debug.LogWarning("[GearStatsService] Missing equipped gear or database");
                return new CharacterItemStats();
            }

            CharacterItemStats totalStats = new CharacterItemStats();

            // Add weapon stats
            if (!string.IsNullOrEmpty(equippedGear.weaponId))
            {
                AddWeaponStats(totalStats, equippedGear.weaponId, database);
            }

            // Add helmet stats
            if (!string.IsNullOrEmpty(equippedGear.helmetId))
            {
                AddGearStats(totalStats, equippedGear.helmetId, database);
            }

            // Add chest stats
            if (!string.IsNullOrEmpty(equippedGear.chestId))
            {
                AddGearStats(totalStats, equippedGear.chestId, database);
            }

            // Add gloves stats
            if (!string.IsNullOrEmpty(equippedGear.glovesId))
            {
                AddGearStats(totalStats, equippedGear.glovesId, database);
            }

            // Add boots stats
            if (!string.IsNullOrEmpty(equippedGear.bootsId))
            {
                AddGearStats(totalStats, equippedGear.bootsId, database);
            }

            // Add accessory1 stats
            if (!string.IsNullOrEmpty(equippedGear.accessory1Id))
            {
                AddGearStats(totalStats, equippedGear.accessory1Id, database);
            }

            // Add accessory2 stats
            if (!string.IsNullOrEmpty(equippedGear.accessory2Id))
            {
                AddGearStats(totalStats, equippedGear.accessory2Id, database);
            }

            return totalStats;
        }

        /// <summary>
        /// ✅ FIX: Use AddStat() instead of direct property access
        /// </summary>
        private void AddWeaponStats(CharacterItemStats totalStats, string weaponId, GameDatabaseSO database)
        {
            ItemBaseSO item = database.GetItem(weaponId);
            if (item is WeaponSO weapon)
            {
                totalStats.AddStat(BonusStatType.AttackDamage, weapon.BonusAD);
                totalStats.AddStat(BonusStatType.AbilityPower, weapon.BonusAP);
                totalStats.AddStat(BonusStatType.Health, weapon.BonusHP);
                totalStats.AddStat(BonusStatType.Defense, weapon.BonusDefense);
                totalStats.AddStat(BonusStatType.AttackSpeed, weapon.BonusAttackSpeed);
                totalStats.AddStat(BonusStatType.CritRate, weapon.BonusCritRate);
                totalStats.AddStat(BonusStatType.CritDamage, weapon.BonusCritDamage);
                totalStats.AddStat(BonusStatType.Evasion, weapon.BonusEvasion);
                totalStats.AddStat(BonusStatType.Tenacity, weapon.BonusTenacity);
                totalStats.AddStat(BonusStatType.Lethality, weapon.BonusLethality);
                totalStats.AddStat(BonusStatType.Penetration, weapon.BonusPenetration);
                totalStats.AddStat(BonusStatType.Lifesteal, weapon.BonusLifesteal);
            }
        }

        /// <summary>
        /// ✅ FIX: Use AddStat() instead of direct property access
        /// </summary>
        private void AddGearStats(CharacterItemStats totalStats, string gearId, GameDatabaseSO database)
        {
            ItemBaseSO item = database.GetItem(gearId);
            if (item is GearSO gear)
            {
                totalStats.AddStat(BonusStatType.Health, gear.BonusHP);
                totalStats.AddStat(BonusStatType.Defense, gear.BonusDefense);
                totalStats.AddStat(BonusStatType.AttackDamage, gear.BonusAD);
                totalStats.AddStat(BonusStatType.AbilityPower, gear.BonusAP);
                totalStats.AddStat(BonusStatType.AttackSpeed, gear.BonusAttackSpeed);
                totalStats.AddStat(BonusStatType.CritRate, gear.BonusCritRate);
                totalStats.AddStat(BonusStatType.CritDamage, gear.BonusCritDamage);
                totalStats.AddStat(BonusStatType.Evasion, gear.BonusEvasion);
                totalStats.AddStat(BonusStatType.Tenacity, gear.BonusTenacity);
                totalStats.AddStat(BonusStatType.Lethality, gear.BonusLethality);
                totalStats.AddStat(BonusStatType.Penetration, gear.BonusPenetration);
                totalStats.AddStat(BonusStatType.Lifesteal, gear.BonusLifesteal);
            }
        }

        /// <summary>
        /// Get weapon from equipped gear
        /// </summary>
        public WeaponSO GetEquippedWeapon(EquippedGear equippedGear, GameDatabaseSO database)
        {
            if (string.IsNullOrEmpty(equippedGear?.weaponId))
                return null;

            ItemBaseSO item = database.GetItem(equippedGear.weaponId);
            return item as WeaponSO;
        }
    }
}