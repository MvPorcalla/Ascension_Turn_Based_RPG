// ════════════════════════════════════════════
// Assets/Scripts/CharacterSystem/Core/CharacterStats.cs
// ✅ FIXED: Added missing properties for SaveDataExtensions compatibility
// ════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Character;
using Ascension.Character.Runtime;

namespace Ascension.Character.Core
{
    /// <summary>
    /// Main character data container - used for both runtime and serialization
    /// NO SEPARATE CharacterData class needed - JsonUtility serializes this directly!
    /// </summary>
    [Serializable]
    public class CharacterStats
    {
        // ══════════════════════════════════════════════════════════════
        // IDENTITY & META
        // ══════════════════════════════════════════════════════════════
        
        [Header("Character Identity")]
        public string playerName;
        public string guildRank = "Unranked";

        // ══════════════════════════════════════════════════════════════
        // EQUIPMENT (Runtime Reference - NOT serialized to JSON)
        // ══════════════════════════════════════════════════════════════
        
        [NonSerialized] 
        public WeaponSO equippedWeapon; // Loaded from EquipmentManager after load

        // ══════════════════════════════════════════════════════════════
        // CORE SYSTEMS (All Serializable)
        // ══════════════════════════════════════════════════════════════
        
        [Header("Character Systems")]
        public CharacterLevelSystem levelSystem = new CharacterLevelSystem();
        public CharacterAttributes attributes = new CharacterAttributes();
        public CharacterItemStats itemStats = new CharacterItemStats();
        public CharacterDerivedStats derivedStats = new CharacterDerivedStats();
        public CharacterCombatRuntime combatRuntime = new CharacterCombatRuntime();

        // ══════════════════════════════════════════════════════════════
        // QUICK ACCESS PROPERTIES
        // ══════════════════════════════════════════════════════════════
        
        // ─── Level System ───
        public int Level => levelSystem.level;
        public int CurrentExp => levelSystem.currentEXP;
        public int AttributePoints => levelSystem.unallocatedPoints;
        public bool IsTranscended => levelSystem.isTranscended;
        public int TranscendenceLevel => levelSystem.transcendenceLevel;

        // ─── Combat Stats ───
        public float CurrentHP => combatRuntime.currentHP;
        public float CurrentMP => 0f; // ✅ TODO: Implement mana system
        public float MaxHP => derivedStats.MaxHP;
        public float MaxMP => 0f; // ✅ TODO: Implement mana system
        
        // ─── Derived Stats ───
        public float AD => derivedStats.AD;
        public float AP => derivedStats.AP;
        public float Defense => derivedStats.Defense;
        public float AttackSpeed => derivedStats.AttackSpeed;
        public float CritRate => derivedStats.CritRate;
        public float CritDamage => derivedStats.CritDamage;
        public float Evasion => derivedStats.Evasion;
        public float Tenacity => derivedStats.Tenacity;

        // ══════════════════════════════════════════════════════════════
        // INITIALIZATION
        // ══════════════════════════════════════════════════════════════
        
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

        // ══════════════════════════════════════════════════════════════
        // EXPERIENCE & LEVELING
        // ══════════════════════════════════════════════════════════════
        
        /// <summary>
        /// Add EXP and trigger level up recalculation
        /// Returns true if leveled up
        /// </summary>
        public bool AddExperience(int amount, CharacterBaseStatsSO baseStats)
        {
            int levelsGained = levelSystem.AddExperience(amount, baseStats);

            if (levelsGained > 0)
            {
                RecalculateStats(baseStats, fullHeal: false);
                combatRuntime.currentHP = derivedStats.MaxHP; // Full heal on level up
                return true;
            }

            return false;
        }

        // ══════════════════════════════════════════════════════════════
        // STAT RECALCULATION
        // ══════════════════════════════════════════════════════════════
        
        /// <summary>
        /// Recalculate all derived stats
        /// Call this after: equipment change, level up, attribute allocation
        /// </summary>
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

        // ══════════════════════════════════════════════════════════════
        // EQUIPMENT
        // ══════════════════════════════════════════════════════════════
        
        /// <summary>
        /// Equip a weapon and update stats
        /// Note: This is called by EquipmentManager, not directly by UI
        /// </summary>
        public void EquipWeapon(WeaponSO weapon, CharacterBaseStatsSO baseStats)
        {
            equippedWeapon = weapon;
            RecalculateStats(baseStats, fullHeal: false);
        }

        public void UnequipWeapon(CharacterBaseStatsSO baseStats)
        {
            equippedWeapon = null;
            RecalculateStats(baseStats, fullHeal: false);
        }

        // ══════════════════════════════════════════════════════════════
        // ATTRIBUTES
        // ══════════════════════════════════════════════════════════════
        
        /// <summary>
        /// Apply item stat bonuses (called by EquipmentManager)
        /// </summary>
        public void ApplyItemStats(CharacterItemStats newItemStats, CharacterBaseStatsSO baseStats)
        {
            itemStats = newItemStats.Clone();
            RecalculateStats(baseStats, fullHeal: false);
        }

        /// <summary>
        /// Modify a single attribute (for point allocation)
        /// </summary>
        public void ModifyAttribute(AttributeType attributeType, int amount, CharacterBaseStatsSO baseStats)
        {
            int currentValue = attributes.GetAttribute(attributeType);
            attributes.SetAttribute(attributeType, currentValue + amount);
            RecalculateStats(baseStats, fullHeal: false);
        }

        // ══════════════════════════════════════════════════════════════
        // MISC
        // ══════════════════════════════════════════════════════════════
        
        public void SetGuildRank(string rank)
        {
            guildRank = string.IsNullOrEmpty(rank) ? "Unranked" : rank;
        }

        public void MarkDirty()
        {
            derivedStats.MarkDirty();
        }
    }
}