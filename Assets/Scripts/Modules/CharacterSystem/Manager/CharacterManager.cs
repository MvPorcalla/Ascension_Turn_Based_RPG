// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/CharacterSystem/Manager/CharacterManager.cs
// ✅ FIXED: Removed duplicate events, now triggers GameEvents instead
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using System;
using Ascension.Core;
using Ascension.Data.Save;
using Ascension.Data.SO.Item;
using Ascension.Character.Stat;
using Ascension.Data.SO.Character;
using Ascension.Equipment.Manager;

namespace Ascension.Character.Manager
{
    public class CharacterManager : MonoBehaviour, IGameService
    {
        #region Singleton
        public static CharacterManager Instance { get; private set; }
        #endregion

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

        #region Events (Keep ONLY for GameManager integration)
        /// <summary>
        /// ⚠️ IMPORTANT: This event is ONLY for GameManager to forward to GameEvents
        /// UI should subscribe to GameEvents.OnGameLoaded instead
        /// </summary>
        public event Action<CharacterStats> OnPlayerLoaded;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            InitializeSingleton();
        }
        #endregion

        #region IGameService Implementation
        public void Initialize()
        {
            Debug.Log("[CharacterManager] Initializing...");
            
            ValidateBaseStats();
            
            Debug.Log("[CharacterManager] Ready");
        }

        private void ValidateBaseStats()
        {
            if (baseStats == null)
            {
                Debug.LogError("[CharacterManager] CharacterBaseStatsSO not assigned!");
            }
            else
            {
                Debug.Log($"[CharacterManager] Base stats loaded: {baseStats.name}");
            }
        }
        #endregion

        #region Public Methods - Player Initialization
        public void CreateNewPlayer(string playerName)
        {
            _currentPlayer = new CharacterStats();
            _currentPlayer.playerName = playerName;
            _currentPlayer.Initialize(baseStats);
            
            _isInitialized = true;

            Debug.Log($"[CharacterManager] Created new player: {playerName}");
            
            // ✅ Fire local event for GameManager integration
            OnPlayerLoaded?.Invoke(_currentPlayer);
            
            // ✅ Trigger GameEvents for UI consumption
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
            
            // ✅ Fire local event for GameManager integration
            OnPlayerLoaded?.Invoke(_currentPlayer);
            
            // ✅ Trigger GameEvents for UI consumption
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
            
            var equipmentManager = ServiceContainer.Instance?.Get<EquipmentManager>();
            
            if (equipmentManager != null)
            {
                CharacterItemStats equipStats = equipmentManager.GetTotalItemStats();
                _currentPlayer.ApplyItemStats(equipStats, baseStats);
                
                Debug.Log("[CharacterManager] Stats updated from equipment");
                
                // ✅ Trigger GameEvents instead of local event
                GameEvents.TriggerStatsRecalculated(_currentPlayer);
            }
            else
            {
                Debug.LogWarning("[CharacterManager] EquipmentManager not available");
            }
        }

        public bool EquipWeaponFromInventory(string itemID)
        {
            if (!HasActivePlayer) return false;
            
            Debug.LogWarning("[CharacterManager] EquipWeaponFromInventory not implemented yet");
            return false;
        }

        public void UnequipWeaponFromInventory()
        {
            if (!HasActivePlayer) return;
            
            Debug.LogWarning("[CharacterManager] UnequipWeaponFromInventory not implemented yet");
        }
        #endregion

        #region Public Methods - Player Actions

        public bool ApplyAttributePoints(CharacterAttributes newAttributes, int pointsSpent)
        {
            if (!HasActivePlayer) return false;

            if (_currentPlayer.UnallocatedPoints < pointsSpent)
            {
                Debug.LogWarning("[CharacterManager] Not enough unallocated points!");
                return false;
            }

            _currentPlayer.attributes.CopyFrom(newAttributes);
            _currentPlayer.levelSystem.unallocatedPoints -= pointsSpent;
            
            RecalculateStats();
            
            Debug.Log($"[CharacterManager] Applied {pointsSpent} attribute points");
            return true;
        }

        public void AddExperience(int amount)
        {
            if (!HasActivePlayer) return;

            bool leveledUp = _currentPlayer.AddExperience(amount, baseStats);

            // ✅ Trigger GameEvents instead of local events
            GameEvents.TriggerExperienceGained(amount, _currentPlayer.CurrentEXP);

            if (leveledUp)
            {
                HandleLevelUp();
            }

            GameEvents.TriggerStatsRecalculated(_currentPlayer);
        }

        public void Heal(float amount)
        {
            if (!HasActivePlayer) return;

            float oldHP = _currentPlayer.CurrentHP;
            _currentPlayer.combatRuntime.Heal(amount, _currentPlayer.MaxHP);
            
            Debug.Log($"[CharacterManager] Healed {amount} HP ({oldHP:F0} → {_currentPlayer.CurrentHP:F0})");
            
            // ✅ Trigger GameEvents instead of local event
            GameEvents.TriggerHealthChanged(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);
        }

        public void ApplyHeal(float amount)
        {
            if (!HasActivePlayer) return;
            
            float oldHP = _currentPlayer.CurrentHP;
            _currentPlayer.combatRuntime.Heal(amount, _currentPlayer.MaxHP);
            
            Debug.Log($"[CharacterManager] Applied heal {amount} HP ({oldHP:F0} → {_currentPlayer.CurrentHP:F0})");
            
            // ✅ Trigger GameEvents instead of local event
            GameEvents.TriggerHealthChanged(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);
        }

        public void TakeDamage(float amount)
        {
            if (!HasActivePlayer) return;

            float oldHP = _currentPlayer.CurrentHP;
            _currentPlayer.combatRuntime.TakeDamage(amount, _currentPlayer.MaxHP);
            
            Debug.Log($"[CharacterManager] Took {amount} damage ({oldHP:F0} → {_currentPlayer.CurrentHP:F0})");
            
            // ✅ Trigger GameEvents instead of local event
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
            
            // ✅ Trigger GameEvents instead of local event
            GameEvents.TriggerHealthChanged(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);
        }

        public void EquipWeapon(WeaponSO weapon)
        {
            if (!HasActivePlayer) return;

            _currentPlayer.EquipWeapon(weapon, baseStats);
            
            // ✅ Trigger GameEvents instead of local event
            GameEvents.TriggerStatsRecalculated(_currentPlayer);
        }

        public void UnequipWeapon()
        {
            if (!HasActivePlayer) return;

            _currentPlayer.UnequipWeapon(baseStats);
            
            // ✅ Trigger GameEvents instead of local event
            GameEvents.TriggerStatsRecalculated(_currentPlayer);
        }

        public void SetGuildRank(string rank)
        {
            if (!HasActivePlayer) return;

            _currentPlayer.SetGuildRank(rank);
            
            // ✅ Trigger GameEvents instead of local event
            GameEvents.TriggerStatsRecalculated(_currentPlayer);
        }

        public bool AllocateAttributePoint(string attributeName)
        {
            if (!HasActivePlayer) return false;

            if (_currentPlayer.UnallocatedPoints <= 0)
            {
                Debug.LogWarning("[CharacterManager] No unallocated points available!");
                return false;
            }

            _currentPlayer.ModifyAttribute(attributeName, 1, baseStats);
            _currentPlayer.levelSystem.unallocatedPoints--;
            
            Debug.Log($"[CharacterManager] Allocated point to {attributeName}");
            
            // ✅ Trigger GameEvents instead of local event
            GameEvents.TriggerStatsRecalculated(_currentPlayer);
            return true;
        }
        #endregion

        #region Public Methods - Helper Methods
        public void RecalculateStats()
        {
            if (!HasActivePlayer) return;

            _currentPlayer.RecalculateStats(baseStats, fullHeal: false);
            
            // ✅ Trigger GameEvents instead of local event
            GameEvents.TriggerStatsRecalculated(_currentPlayer);
        }

        public CharacterStats GetCharacterDataForSave()
        {
            return _currentPlayer;
        }
        #endregion

        #region Private Methods
        private void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void HandleLevelUp()
        {
            Debug.Log($"[CharacterManager] Level up! Now level {_currentPlayer.Level}");
            
            // ✅ Trigger GameEvents instead of local events
            GameEvents.TriggerLevelUp(_currentPlayer.Level);
            GameEvents.TriggerHealthChanged(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);
        }

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
            Debug.Log($"HP: {_currentPlayer.CurrentHP}/{_currentPlayer.MaxHP}");
            Debug.Log($"AD: {_currentPlayer.AD}");
            Debug.Log($"AP: {_currentPlayer.AP}");
            Debug.Log($"Attack Speed: {_currentPlayer.AttackSpeed}");
        }

        [ContextMenu("Debug: Add 100 EXP")]
        private void DebugAddExp() => AddExperience(100);

        [ContextMenu("Debug: Damage 50 HP")]
        private void DebugDamage() => TakeDamage(50);

        [ContextMenu("Debug: Heal 50 HP")]
        private void DebugHeal() => Heal(50);

        [ContextMenu("Debug: Full Heal")]
        private void DebugFullHeal() => FullHeal();
        #endregion
    }
}