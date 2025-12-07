// ════════════════════════════════════════════
// CharacterStats.cs
// Core character stats wrapper and recalculation pipeline
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.GameSystem;
using Ascension.Data.SO.Item;
using Ascension.Character.Runtime;
using Ascension.Data.SO.Character;

namespace Ascension.Character.Stat
{
    [System.Serializable]
    public class CharacterStats
    {
        // ──────────────────────────────────────────────────
        // Serialized Fields
        // ──────────────────────────────────────────────────

        [Header("Character Identity")]
        public string playerName;
        public string guildRank = "Unranked";

        [Header("Equipment")]
        public WeaponSO equippedWeapon;

        [Header("Core Systems")]
        public CharacterLevelSystem levelSystem = new CharacterLevelSystem();
        public CharacterAttributes attributes = new CharacterAttributes();
        public CharacterItemStats itemStats = new CharacterItemStats();
        public CharacterDerivedStats derivedStats = new CharacterDerivedStats();
        public CharacterCombatRuntime combatRuntime = new CharacterCombatRuntime();

        // ──────────────────────────────────────────────────
        // Properties (Quick Access)
        // ──────────────────────────────────────────────────

        public int Level => levelSystem.level;
        public int CurrentEXP => levelSystem.currentEXP;
        public int UnallocatedPoints => levelSystem.unallocatedPoints;
        public bool IsTranscended => levelSystem.isTranscended;
        public int TranscendenceLevel => levelSystem.transcendenceLevel;

        public float CurrentHP => combatRuntime.currentHP;
        public float MaxHP => derivedStats.MaxHP;
        public float AD => derivedStats.AD;
        public float AP => derivedStats.AP;
        public float AttackSpeed => derivedStats.AttackSpeed;

        // ──────────────────────────────────────────────────
        // Public Methods
        // ──────────────────────────────────────────────────

        public void Initialize(CharacterBaseStatsSO baseStats)
        {
            guildRank = "Unranked";

            levelSystem.Initialize(baseStats);

            attributes = new CharacterAttributes(
                baseStats.startingSTR,
                baseStats.startingINT,
                baseStats.startingAGI,
                baseStats.startingEND,
                baseStats.startingWIS
            );

            itemStats = new CharacterItemStats();

            RecalculateStats(baseStats, fullHeal: true);
        }

        /// <summary> Add EXP and trigger level up recalculation </summary>
        public bool AddExperience(int amount, CharacterBaseStatsSO baseStats)
        {
            float oldMaxHP = derivedStats.MaxHP;
            int levelsGained = levelSystem.AddExperience(amount, baseStats);

            if (levelsGained > 0)
            {
                RecalculateStats(baseStats, fullHeal: false);
                combatRuntime.currentHP = derivedStats.MaxHP;
                return true;
            }

            return false;
        }

        /// <summary> Recalculate full derived stats </summary>
        public void RecalculateStats(CharacterBaseStatsSO baseStats, bool fullHeal = false)
        {
            float oldMaxHP = derivedStats.MaxHP;

            derivedStats.Recalculate(
                baseStats,
                levelSystem.level,
                attributes,
                itemStats,
                equippedWeapon
            );

            combatRuntime.OnMaxHPChanged(oldMaxHP, derivedStats.MaxHP, fullHeal);
        }

        /// <summary> Equip a weapon and update stats </summary>
        public void EquipWeapon(WeaponSO weapon, CharacterBaseStatsSO baseStats)
        {
            equippedWeapon = weapon;
            RecalculateStats(baseStats, fullHeal: false);
            Debug.Log($"[CharacterStats] Equipped: {weapon.WeaponName}");
        }

        public void UnequipWeapon(CharacterBaseStatsSO baseStats)
        {
            equippedWeapon = null;
            RecalculateStats(baseStats, fullHeal: false);
            Debug.Log("[CharacterStats] Weapon unequipped");
        }

        public void SetGuildRank(string rank)
        {
            guildRank = string.IsNullOrEmpty(rank) ? "Unranked" : rank;
            Debug.Log($"[CharacterStats] Guild rank updated to: {guildRank}");
        }

        public void MarkDirty()
        {
            derivedStats.MarkDirty();
        }

        public void ApplyItemStats(CharacterItemStats newItemStats, CharacterBaseStatsSO baseStats)
        {
            itemStats = newItemStats.Clone();
            RecalculateStats(baseStats, fullHeal: false);
        }

        public void ModifyAttribute(string attributeName, int amount, CharacterBaseStatsSO baseStats)
        {
            switch (attributeName.ToUpper())
            {
                case "STR": attributes.STR += amount; break;
                case "INT": attributes.INT += amount; break;
                case "AGI": attributes.AGI += amount; break;
                case "END": attributes.END += amount; break;
                case "WIS": attributes.WIS += amount; break;
            }

            RecalculateStats(baseStats, fullHeal: false);
        }
    }
}
