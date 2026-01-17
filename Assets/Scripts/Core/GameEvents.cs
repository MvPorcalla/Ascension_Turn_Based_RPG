// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Core/GameEvents.cs
// Central event hub - all game state changes flow through here
// UI components subscribe to these events for reactive updates
// ════════════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Character.Stat;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.Equipment.Enums;
using Ascension.Data.SO.Item;

namespace Ascension.Core
{
    /// <summary>
    /// Static event hub - replaces individual manager events
    /// Benefits: Single subscription point, no memory leaks from manager references
    /// </summary>
    public static class GameEvents
    {
        // ════════════════════════════════════════════════════════════════
        // GAME FLOW EVENTS
        // ════════════════════════════════════════════════════════════════
        
        /// <summary>New game started (character created)</summary>
        public static event Action<CharacterStats> OnNewGameStarted;
        
        /// <summary>Save completed successfully</summary>
        public static event Action OnGameSaved;
        
        /// <summary>Load completed successfully</summary>
        public static event Action<CharacterStats> OnGameLoaded;
        
        /// <summary>Save deleted</summary>
        public static event Action OnSaveDeleted;
        
        /// <summary>Scene is about to change</summary>
        public static event Action<string> OnSceneChanging; // sceneName
        
        // ════════════════════════════════════════════════════════════════
        // CHARACTER EVENTS
        // ════════════════════════════════════════════════════════════════
        
        /// <summary>Health changed (damage or healing)</summary>
        public static event Action<float, float> OnHealthChanged; // current, max
        
        /// <summary>Player leveled up</summary>
        public static event Action<int> OnLevelUp; // newLevel
        
        /// <summary>Experience gained</summary>
        public static event Action<int, int> OnExperienceGained; // gained, newTotal
        
        /// <summary>Stats recalculated (equipment changed, level up, etc.)</summary>
        public static event Action<CharacterStats> OnStatsRecalculated;
        
        /// <summary>Player name changed</summary>
        public static event Action<string> OnPlayerNameChanged;
        
        // ════════════════════════════════════════════════════════════════
        // INVENTORY EVENTS
        // ════════════════════════════════════════════════════════════════
        
        /// <summary>Item added to inventory</summary>
        public static event Action<ItemInstance> OnItemAdded;
        
        /// <summary>Item removed from inventory</summary>
        public static event Action<ItemInstance> OnItemRemoved;
        
        /// <summary>Item moved between locations</summary>
        public static event Action<ItemInstance, ItemLocation, ItemLocation> OnItemMoved; // item, from, to
        
        /// <summary>Inventory changed (generic refresh trigger)</summary>
        public static event Action OnInventoryChanged;
        
        // ════════════════════════════════════════════════════════════════
        // EQUIPMENT EVENTS
        // ════════════════════════════════════════════════════════════════
        
        /// <summary>Gear equipped in a slot</summary>
        public static event Action<GearSlotType, ItemBaseSO> OnGearEquipped; // slot, item
        
        /// <summary>Gear unequipped from a slot</summary>
        public static event Action<GearSlotType> OnGearUnequipped; // slot
        
        /// <summary>Equipment changed (any slot)</summary>
        public static event Action OnEquipmentChanged;
        
        // ════════════════════════════════════════════════════════════════
        // SKILL EVENTS
        // ════════════════════════════════════════════════════════════════
        
        /// <summary>Skill loadout changed</summary>
        public static event Action OnSkillLoadoutChanged;
        
        // ════════════════════════════════════════════════════════════════
        // EVENT TRIGGERS (Call these from managers/commands)
        // ════════════════════════════════════════════════════════════════
        
        public static void TriggerNewGameStarted(CharacterStats stats)
        {
            OnNewGameStarted?.Invoke(stats);
            Debug.Log($"[GameEvents] New game started: {stats.playerName}");
        }
        
        public static void TriggerGameSaved()
        {
            OnGameSaved?.Invoke();
            Debug.Log("[GameEvents] Game saved");
        }
        
        public static void TriggerGameLoaded(CharacterStats stats)
        {
            OnGameLoaded?.Invoke(stats);
            Debug.Log($"[GameEvents] Game loaded: {stats.playerName}");
        }
        
        public static void TriggerSaveDeleted()
        {
            OnSaveDeleted?.Invoke();
            Debug.Log("[GameEvents] Save deleted");
        }
        
        public static void TriggerSceneChanging(string sceneName)
        {
            OnSceneChanging?.Invoke(sceneName);
            Debug.Log($"[GameEvents] Scene changing to: {sceneName}");
        }
        
        public static void TriggerHealthChanged(float current, float max)
        {
            OnHealthChanged?.Invoke(current, max);
        }
        
        public static void TriggerLevelUp(int newLevel)
        {
            OnLevelUp?.Invoke(newLevel);
            Debug.Log($"[GameEvents] Level up! Now level {newLevel}");
        }
        
        public static void TriggerExperienceGained(int gained, int newTotal)
        {
            OnExperienceGained?.Invoke(gained, newTotal);
        }
        
        public static void TriggerStatsRecalculated(CharacterStats stats)
        {
            OnStatsRecalculated?.Invoke(stats);
        }
        
        public static void TriggerPlayerNameChanged(string newName)
        {
            OnPlayerNameChanged?.Invoke(newName);
        }
        
        public static void TriggerItemAdded(ItemInstance item)
        {
            OnItemAdded?.Invoke(item);
        }
        
        public static void TriggerItemRemoved(ItemInstance item)
        {
            OnItemRemoved?.Invoke(item);
        }
        
        public static void TriggerItemMoved(ItemInstance item, ItemLocation from, ItemLocation to)
        {
            OnItemMoved?.Invoke(item, from, to);
        }
        
        public static void TriggerInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }
        
        public static void TriggerGearEquipped(GearSlotType slot, ItemBaseSO item)
        {
            OnGearEquipped?.Invoke(slot, item);
        }
        
        public static void TriggerGearUnequipped(GearSlotType slot)
        {
            OnGearUnequipped?.Invoke(slot);
        }
        
        public static void TriggerEquipmentChanged()
        {
            OnEquipmentChanged?.Invoke();
        }
        
        public static void TriggerSkillLoadoutChanged()
        {
            OnSkillLoadoutChanged?.Invoke();
        }
    }
}