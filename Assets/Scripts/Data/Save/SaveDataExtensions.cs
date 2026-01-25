// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Data/Save/SaveDataExtensions.cs
// ✅ DTO Conversion Extensions - Runtime ↔ Save Data
// ════════════════════════════════════════════════════════════════════════

using Ascension.Character.Core;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using System.Collections.Generic;

namespace Ascension.Data.Save
{
    /// <summary>
    /// Extension methods for converting between runtime objects and save DTOs
    /// Keeps conversion logic centralized and maintainable
    /// </summary>
    public static class SaveDataExtensions
    {
        // ════════════════════════════════════════════════════════════════
        // CHARACTER: Runtime → DTO
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Convert runtime CharacterStats to save DTO
        /// </summary>
        public static CharacterSaveData ToSaveData(this CharacterStats stats)
        {
            if (stats == null) return null;

            return new CharacterSaveData
            {
                // Identity
                playerName = stats.playerName,

                // Level & Progression
                level = stats.Level,
                currentExperience = stats.CurrentExp,
                attributePoints = stats.AttributePoints,

                // Core Attributes
                strength = stats.attributes.STR,
                agility = stats.attributes.AGI,
                intelligence = stats.attributes.INT,
                endurance = stats.attributes.END,
                wisdom = stats.attributes.WIS,

                // Combat Runtime State
                currentHealth = stats.CurrentHP,
                currentMana = stats.CurrentMP
            };
        }

        // ════════════════════════════════════════════════════════════════
        // CHARACTER: DTO → Runtime
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Convert save DTO to runtime CharacterStats
        /// NOTE: Derived stats (MaxHP, AD, etc.) must be recalculated after loading!
        /// </summary>
        public static CharacterStats ToRuntimeData(this CharacterSaveData saveData)
        {
            if (saveData == null) return null;

            CharacterStats stats = new CharacterStats
            {
                playerName = saveData.playerName
            };

            // Level & Progression
            stats.levelSystem.level = saveData.level;
            stats.levelSystem.currentEXP = saveData.currentExperience;
            stats.levelSystem.unallocatedPoints = saveData.attributePoints;

            // Core Attributes
            stats.attributes.STR = saveData.strength;
            stats.attributes.AGI = saveData.agility;
            stats.attributes.INT = saveData.intelligence;
            stats.attributes.END = saveData.endurance;
            stats.attributes.WIS = saveData.wisdom;

            // Combat Runtime State (will be clamped after RecalculateStats)
            stats.combatRuntime.currentHP = saveData.currentHealth;
            stats.combatRuntime.currentMP = saveData.currentMana;

            // ⚠️ IMPORTANT: Derived stats are NOT set here!
            // They must be recalculated via stats.RecalculateStats(baseStats, fullHeal)
            // This ensures formulas stay up-to-date even if they change between versions

            return stats;
        }

        // ════════════════════════════════════════════════════════════════
        // INVENTORY: Runtime → DTO
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Convert runtime ItemInstance to save DTO
        /// </summary>
        public static ItemInstanceData ToSaveData(this ItemInstance item)
        {
            if (item == null) return null;

            return new ItemInstanceData
            {
                itemId = item.itemID,
                quantity = item.quantity,
                location = (int)item.location,
                previousLocation = item.previousLocation.HasValue 
                    ? (int)item.previousLocation.Value 
                    : -1
            };
        }

        /// <summary>
        /// Convert list of runtime items to DTO array
        /// </summary>
        public static ItemInstanceData[] ToSaveDataArray(this List<ItemInstance> items)
        {
            if (items == null || items.Count == 0)
                return new ItemInstanceData[0];

            ItemInstanceData[] array = new ItemInstanceData[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                array[i] = items[i].ToSaveData();
            }

            return array;
        }

        // ════════════════════════════════════════════════════════════════
        // INVENTORY: DTO → Runtime
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Convert save DTO to runtime ItemInstance
        /// </summary>
        public static ItemInstance ToRuntimeData(this ItemInstanceData saveData)
        {
            if (saveData == null) return null;

            ItemInstance item = new ItemInstance(
                saveData.itemId,
                saveData.quantity,
                (ItemLocation)saveData.location
            );

            if (saveData.previousLocation >= 0)
            {
                item.previousLocation = (ItemLocation)saveData.previousLocation;
            }

            return item;
        }

        /// <summary>
        /// Convert DTO array to runtime item list
        /// </summary>
        public static List<ItemInstance> ToRuntimeDataList(this ItemInstanceData[] saveDataArray)
        {
            List<ItemInstance> items = new List<ItemInstance>();

            if (saveDataArray == null || saveDataArray.Length == 0)
                return items;

            foreach (var itemData in saveDataArray)
            {
                if (string.IsNullOrEmpty(itemData.itemId))
                    continue;

                ItemInstance item = itemData.ToRuntimeData();
                if (item != null)
                    items.Add(item);
            }

            return items;
        }

        // ════════════════════════════════════════════════════════════════
        // VALIDATION HELPERS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Validate CharacterSaveData for common corruption issues
        /// </summary>
        public static bool IsValid(this CharacterSaveData data, out string error)
        {
            error = null;

            if (data == null)
            {
                error = "CharacterSaveData is null";
                return false;
            }

            if (string.IsNullOrWhiteSpace(data.playerName))
            {
                error = "Player name is empty";
                return false;
            }

            if (data.level < 1 || data.level > 1000)
            {
                error = $"Invalid level: {data.level}";
                return false;
            }

            if (data.currentExperience < 0)
            {
                error = "Negative experience";
                return false;
            }

            // Validate attributes are in reasonable range
            const int MIN_ATTR = 0;
            const int MAX_ATTR = 9999;

            if (!IsInRange(data.strength, MIN_ATTR, MAX_ATTR) ||
                !IsInRange(data.agility, MIN_ATTR, MAX_ATTR) ||
                !IsInRange(data.intelligence, MIN_ATTR, MAX_ATTR) ||
                !IsInRange(data.endurance, MIN_ATTR, MAX_ATTR) ||
                !IsInRange(data.wisdom, MIN_ATTR, MAX_ATTR))
            {
                error = "One or more attributes out of valid range (0-9999)";
                return false;
            }

            return true;
        }

        private static bool IsInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Sanitize CharacterSaveData by clamping values to valid ranges
        /// Use this for graceful degradation
        /// </summary>
        public static void Sanitize(this CharacterSaveData data)
        {
            if (data == null) return;

            // Clamp level
            data.level = UnityEngine.Mathf.Clamp(data.level, 1, 1000);

            // Ensure non-negative experience
            if (data.currentExperience < 0)
                data.currentExperience = 0;

            // Ensure non-negative attribute points
            if (data.attributePoints < 0)
                data.attributePoints = 0;

            // Clamp attributes
            const int MIN_ATTR = 0;
            const int MAX_ATTR = 9999;

            data.strength = UnityEngine.Mathf.Clamp(data.strength, MIN_ATTR, MAX_ATTR);
            data.agility = UnityEngine.Mathf.Clamp(data.agility, MIN_ATTR, MAX_ATTR);
            data.intelligence = UnityEngine.Mathf.Clamp(data.intelligence, MIN_ATTR, MAX_ATTR);
            data.endurance = UnityEngine.Mathf.Clamp(data.endurance, MIN_ATTR, MAX_ATTR);
            data.wisdom = UnityEngine.Mathf.Clamp(data.wisdom, MIN_ATTR, MAX_ATTR);

            // Ensure non-negative HP/MP
            if (data.currentHealth < 0)
                data.currentHealth = 1;

            if (data.currentMana < 0)
                data.currentMana = 0;
        }
    }
}