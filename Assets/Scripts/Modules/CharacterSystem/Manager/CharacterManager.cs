// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/CharacterSystem/Manager/CharacterManager.cs
// ✅ REFACTORED: Single attribute allocation system (batch only)
// Main manager for character lifecycle, stats, and integration with other systems
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using Ascension.Core;
using Ascension.Data.SO.Item;
using Ascension.Character.Core;
using Ascension.Data.SO.Character;

namespace Ascension.Character.Manager
{
    public class CharacterManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("References")]
        [SerializeField] private CharacterBaseStatsSO baseStats;
        #endregion

        #region Private Fields
        private CharacterStats _currentPlayer;
        private bool _isInitialized = false;
        #endregion

        #region Properties
        public CharacterStats CurrentPlayer => _currentPlayer;
        public CharacterBaseStatsSO BaseStats => baseStats;
        public bool HasActivePlayer => _isInitialized && _currentPlayer != null;
        #endregion

        #region Initialization
        /// <summary>
        /// ✅ Called by GameBootstrap during initialization
        /// </summary>
        public void Init()
        {
            if (baseStats == null)
            {
                Debug.LogError("[CharacterManager] CharacterBaseStatsSO not assigned!");
                return;
            }

            Debug.Log($"[CharacterManager] Initialized with base stats: {baseStats.name}");
        }
        #endregion

        #region Public Methods - Player Lifecycle
        public void CreateNewPlayer(string playerName)
        {
            _currentPlayer = new CharacterStats();
            _currentPlayer.playerName = playerName;
            _currentPlayer.Initialize(baseStats);
            
            _isInitialized = true;

            Debug.Log($"[CharacterManager] Created new player: {playerName}");
            
            GameEvents.TriggerNewGameStarted(_currentPlayer);
            GameEvents.TriggerStatsRecalculated(_currentPlayer);
        }

        public void LoadPlayer(CharacterStats loadedStats)
        {
            if (loadedStats == null)
            {
                Debug.LogError("[CharacterManager] Cannot load null player stats!");
                return;
            }

            _currentPlayer = loadedStats;
            _currentPlayer.RecalculateStats(baseStats, fullHeal: false);
            
            _isInitialized = true;

            Debug.Log($"[CharacterManager] Loaded player: {_currentPlayer.playerName}");
            
            GameEvents.TriggerGameLoaded(_currentPlayer);
            GameEvents.TriggerStatsRecalculated(_currentPlayer);
            
            UpdateStatsFromEquipment();
        }

        public void UnloadPlayer()
        {
            _currentPlayer = null;
            _isInitialized = false;
            Debug.Log("[CharacterManager] Player unloaded");
        }
        #endregion

        #region Equipment Integration
        public void UpdateStatsFromEquipment()
        {
            if (!HasActivePlayer) return;
            
            var equipmentManager = GameBootstrap.Equipment;
            
            if (equipmentManager != null)
            {
                CharacterItemStats equipStats = equipmentManager.GetTotalItemStats();
                _currentPlayer.ApplyItemStats(equipStats, baseStats);
                
                Debug.Log("[CharacterManager] Stats updated from equipment");
                GameEvents.TriggerStatsRecalculated(_currentPlayer);
            }
            else
            {
                Debug.LogWarning("[CharacterManager] EquipmentManager not available");
            }
        }

        public void EquipWeapon(WeaponSO weapon)
        {
            if (!HasActivePlayer) return;
            _currentPlayer.EquipWeapon(weapon, baseStats);
            GameEvents.TriggerStatsRecalculated(_currentPlayer);
        }

        public void UnequipWeapon()
        {
            if (!HasActivePlayer) return;
            _currentPlayer.UnequipWeapon(baseStats);
            GameEvents.TriggerStatsRecalculated(_currentPlayer);
        }
        #endregion

        #region Combat & Health
        public void Heal(float amount)
        {
            if (!HasActivePlayer) return;

            float oldHP = _currentPlayer.CurrentHP;
            _currentPlayer.combatRuntime.Heal(amount, _currentPlayer.MaxHP);
            
            Debug.Log($"[CharacterManager] Healed {amount} HP ({oldHP:F0} → {_currentPlayer.CurrentHP:F0})");
            GameEvents.TriggerHealthChanged(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);
        }

        public void TakeDamage(float amount)
        {
            if (!HasActivePlayer) return;

            float oldHP = _currentPlayer.CurrentHP;
            _currentPlayer.combatRuntime.TakeDamage(amount, _currentPlayer.MaxHP);
            
            Debug.Log($"[CharacterManager] Took {amount} damage ({oldHP:F0} → {_currentPlayer.CurrentHP:F0})");
            GameEvents.TriggerHealthChanged(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);

            if (_currentPlayer.CurrentHP <= 0)
            {
                HandlePlayerDeath();
            }
        }

        public void FullHeal()
        {
            if (!HasActivePlayer) return;
            _currentPlayer.combatRuntime.currentHP = _currentPlayer.MaxHP;
            Debug.Log("[CharacterManager] Full heal applied");
            GameEvents.TriggerHealthChanged(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);
        }
        #endregion

        #region Experience & Leveling
        /// <summary>
        /// Add experience points to the player
        /// Handles level-up logic and triggers appropriate events
        /// </summary>
        public void AddExperience(int amount)
        {
            if (!HasActivePlayer) return;

            bool leveledUp = _currentPlayer.AddExperience(amount, baseStats);

            GameEvents.TriggerExperienceGained(amount, _currentPlayer.CurrentExp);

            if (leveledUp)
            {
                HandleLevelUp();
            }

            GameEvents.TriggerStatsRecalculated(_currentPlayer);
        }

        private void HandleLevelUp()
        {
            Debug.Log($"[CharacterManager] Level up! Now level {_currentPlayer.Level}");
            GameEvents.TriggerLevelUp(_currentPlayer.Level);
            GameEvents.TriggerHealthChanged(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);
        }
        #endregion

        #region Attribute Allocation
        /// <summary>
        /// ✅ SINGLE SYSTEM: Apply attribute points (batch allocation)
        /// Used by LevelUpPanelUI for plan-then-confirm allocation
        /// Handles validation, application, stat recalculation, and persistence
        /// </summary>
        /// <param name="newAttributes">The new attribute distribution</param>
        /// <param name="pointsSpent">Number of points being allocated</param>
        /// <returns>True if successful, false if validation failed</returns>
        public bool ApplyAttributePoints(CharacterAttributes newAttributes, int pointsSpent)
        {
            if (!HasActivePlayer)
            {
                Debug.LogError("[CharacterManager] No active player!");
                return false;
            }

            // Validation: Check if enough points available
            if (_currentPlayer.AttributePoints < pointsSpent)
            {
                Debug.LogWarning($"[CharacterManager] Not enough points! Need {pointsSpent}, have {_currentPlayer.AttributePoints}");
                return false;
            }

            // Validation: Ensure points were actually spent
            if (pointsSpent <= 0)
            {
                Debug.LogWarning("[CharacterManager] No points to allocate!");
                return false;
            }

            // Apply new attributes
            _currentPlayer.attributes.CopyFrom(newAttributes);
            _currentPlayer.levelSystem.unallocatedPoints -= pointsSpent;
            
            // Recalculate derived stats
            RecalculateStats();
            
            // Persist changes
            SavePlayerState();
            
            Debug.Log($"[CharacterManager] Applied {pointsSpent} attribute points");
            return true;
        }

        /// <summary>
        /// Recalculate all derived stats and trigger update event
        /// </summary>
        public void RecalculateStats()
        {
            if (!HasActivePlayer) return;
            
            _currentPlayer.RecalculateStats(baseStats, fullHeal: false);
            GameEvents.TriggerStatsRecalculated(_currentPlayer);
        }

        /// <summary>
        /// Save current player state to disk
        /// Called after important state changes (attribute allocation, etc.)
        /// </summary>
        private void SavePlayerState()
        {
            GameBootstrap.Save?.SaveGame(_currentPlayer, 0f);
        }
        #endregion

        #region Future: External Attribute Bonuses
        // ═══════════════════════════════════════════════════════════════
        // ✅ FUTURE: If you need external systems to grant attribute bonuses
        // (quest rewards, consumable items, etc.), add this method:
        // ═══════════════════════════════════════════════════════════════
        
        /// <summary>
        /// ✅ FUTURE: Grant permanent attribute bonus from external source
        /// Use this for quest rewards, consumable items, etc.
        /// Does NOT consume unallocated points
        /// </summary>
        /*
        public void GrantAttributeBonus(AttributeType attributeType, int amount)
        {
            if (!HasActivePlayer) return;

            int currentValue = _currentPlayer.attributes.GetAttribute(attributeType);
            _currentPlayer.attributes.SetAttribute(attributeType, currentValue + amount);
            
            RecalculateStats();
            SavePlayerState();
            
            Debug.Log($"[CharacterManager] Granted +{amount} {attributeType} bonus");
        }
        */
        
        #endregion

        #region Misc
        public void SetGuildRank(string rank)
        {
            if (!HasActivePlayer) return;
            _currentPlayer.SetGuildRank(rank);
            GameEvents.TriggerStatsRecalculated(_currentPlayer);
        }

        public CharacterStats GetCharacterDataForSave()
        {
            return _currentPlayer;
        }
        #endregion

        #region Private Methods
        private void HandlePlayerDeath()
        {
            Debug.Log("[CharacterManager] Player has died!");
            // TODO: Implement death logic
        }
        #endregion

        #region Debug Tools
        [ContextMenu("Debug: Print Player Stats")]
        private void DebugPrintStats()
        {
            if (!HasActivePlayer)
            {
                Debug.Log("[CharacterManager] No active player");
                return;
            }

            Debug.Log("=== PLAYER STATS ===");
            Debug.Log($"Name: {_currentPlayer.playerName}");
            Debug.Log($"Level: {_currentPlayer.Level}");
            Debug.Log($"Unallocated Points: {_currentPlayer.AttributePoints}");
            Debug.Log($"HP: {_currentPlayer.CurrentHP}/{_currentPlayer.MaxHP}");
            Debug.Log($"STR: {_currentPlayer.attributes.STR}");
            Debug.Log($"INT: {_currentPlayer.attributes.INT}");
            Debug.Log($"AGI: {_currentPlayer.attributes.AGI}");
            Debug.Log($"END: {_currentPlayer.attributes.END}");
            Debug.Log($"WIS: {_currentPlayer.attributes.WIS}");
            Debug.Log($"AD: {_currentPlayer.AD}");
            Debug.Log($"AP: {_currentPlayer.AP}");
        }

        [ContextMenu("Debug: Add 100 EXP")]
        private void DebugAddExp() => AddExperience(100);

        [ContextMenu("Debug: Damage 50 HP")]
        private void DebugDamage() => TakeDamage(50);

        [ContextMenu("Debug: Heal 50 HP")]
        private void DebugHeal() => Heal(50);

        [ContextMenu("Debug: Full Heal")]
        private void DebugFullHeal() => FullHeal();
        
        [ContextMenu("Debug: Level Up")]
        private void DebugLevelUp()
        {
            if (HasActivePlayer)
            {
                int expNeeded = _currentPlayer.levelSystem.GetExpToNextLevel();
                AddExperience(expNeeded);
            }
        }
        
        [ContextMenu("Debug: Grant 5 Attribute Points")]
        private void DebugGrantPoints()
        {
            if (HasActivePlayer)
            {
                _currentPlayer.levelSystem.unallocatedPoints += 5;
                Debug.Log($"[CharacterManager] Granted 5 points. Total: {_currentPlayer.AttributePoints}");
            }
        }
        #endregion
    }
}