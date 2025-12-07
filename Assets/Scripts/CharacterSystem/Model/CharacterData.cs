// ════════════════════════════════════════════
// CharacterData.cs
// Serializable player data for saving/loading
// ════════════════════════════════════════════

using System;
using Ascension.Character.Stat;
using Ascension.Data.SO.Character;
using Ascension.Data.Enums;

namespace Ascension.Character.Model
{
    [Serializable]
    public class CharacterData
    {
        // ────────── Identity ──────────
        public string playerName;
        public string guildRank = "Unranked";

        // ────────── Level System ──────────
        public int level;
        public int currentEXP;
        public int expToNextLevel;
        public int unallocatedPoints;
        public int transcendenceLevel;
        public bool isTranscended;

        // ────────── Runtime State ──────────
        public float hpPercent;

        // ────────── Attributes ──────────
        public int STR;
        public int INT;
        public int AGI;
        public int END;
        public int WIS;

        // ────────── Item Bonuses ──────────
        public float itemAD;
        public float itemAP;
        public float itemHP;
        public float itemDefense;
        public float itemAttackSpeed;
        public float itemCritRate;
        public float itemCritDamage;
        public float itemEvasion;
        public float itemTenacity;
        public float itemLethality;
        public float itemPenetration;
        public float itemLifesteal;

        public CharacterData() { }

        // ────────── Factory Method ──────────
        public static CharacterData FromCharacterStats(CharacterStats stats)
        {
            return new CharacterData
            {
                playerName = stats.playerName,
                guildRank = string.IsNullOrEmpty(stats.guildRank) ? "Unranked" : stats.guildRank,
                hpPercent = stats.combatRuntime.GetHPPercent(stats.derivedStats.MaxHP),

                // Level system
                level = stats.levelSystem.level,
                currentEXP = stats.levelSystem.currentEXP,
                expToNextLevel = stats.levelSystem.expToNextLevel,
                unallocatedPoints = stats.levelSystem.unallocatedPoints,
                transcendenceLevel = stats.levelSystem.transcendenceLevel,
                isTranscended = stats.levelSystem.isTranscended,

                // Attributes
                STR = stats.attributes.STR,
                INT = stats.attributes.INT,
                AGI = stats.attributes.AGI,
                END = stats.attributes.END,
                WIS = stats.attributes.WIS,

                // Item stats
                itemAD = stats.itemStats.AD,
                itemAP = stats.itemStats.AP,
                itemHP = stats.itemStats.HP,
                itemDefense = stats.itemStats.Defense,
                itemAttackSpeed = stats.itemStats.AttackSpeed,
                itemCritRate = stats.itemStats.CritRate,
                itemCritDamage = stats.itemStats.CritDamage,
                itemEvasion = stats.itemStats.Evasion,
                itemTenacity = stats.itemStats.Tenacity,
                itemLethality = stats.itemStats.Lethality,
                itemPenetration = stats.itemStats.Penetration,
                itemLifesteal = stats.itemStats.Lifesteal
            };
        }

        // ────────── Restore Method ──────────
        public CharacterStats ToCharacterStats(CharacterBaseStatsSO baseStats)
        {
            var stats = new CharacterStats
            {
                playerName = playerName,
                guildRank = string.IsNullOrEmpty(guildRank) ? "Unranked" : guildRank
            };

            // Restore level system
            stats.levelSystem.level = level;
            stats.levelSystem.currentEXP = currentEXP;
            stats.levelSystem.expToNextLevel = expToNextLevel;
            stats.levelSystem.unallocatedPoints = unallocatedPoints;
            stats.levelSystem.transcendenceLevel = transcendenceLevel;
            stats.levelSystem.isTranscended = isTranscended;

            // Restore attributes
            stats.attributes.STR = STR;
            stats.attributes.INT = INT;
            stats.attributes.AGI = AGI;
            stats.attributes.END = END;
            stats.attributes.WIS = WIS;

            // Restore item stats
            stats.itemStats.AddStat(BonusStatType.AttackDamage, itemAD);
            stats.itemStats.AddStat(BonusStatType.AbilityPower, itemAP);
            stats.itemStats.AddStat(BonusStatType.Health, itemHP);
            stats.itemStats.AddStat(BonusStatType.Defense, itemDefense);
            stats.itemStats.AddStat(BonusStatType.AttackSpeed, itemAttackSpeed);
            stats.itemStats.AddStat(BonusStatType.CritRate, itemCritRate);
            stats.itemStats.AddStat(BonusStatType.CritDamage, itemCritDamage);
            stats.itemStats.AddStat(BonusStatType.Evasion, itemEvasion);
            stats.itemStats.AddStat(BonusStatType.Tenacity, itemTenacity);
            stats.itemStats.AddStat(BonusStatType.Lethality, itemLethality);
            stats.itemStats.AddStat(BonusStatType.Penetration, itemPenetration);
            stats.itemStats.AddStat(BonusStatType.Lifesteal, itemLifesteal);

            // Derived stats
            stats.RecalculateStats(baseStats, fullHeal: false);

            // Restore HP from percentage
            stats.combatRuntime.currentHP = stats.derivedStats.MaxHP * hpPercent;

            // Validate pending level-ups
            int maxLevel = stats.levelSystem.GetMaxPossibleLevel(baseStats);
            while (stats.levelSystem.currentEXP >= stats.levelSystem.expToNextLevel &&
                   stats.levelSystem.level < maxLevel)
            {
                stats.levelSystem.AddExperience(0, baseStats); // Trigger level-up without adding EXP
                stats.RecalculateStats(baseStats, fullHeal: true);
            }

            return stats;
        }
    }
}
