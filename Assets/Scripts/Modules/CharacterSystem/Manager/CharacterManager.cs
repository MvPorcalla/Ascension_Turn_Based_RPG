// ════════════════════════════════════════════
// Assets\Scripts\CharacterSystem\Manager\CharacterManager.cs
// REFACTORED: Uses ServiceContainer for optional EquipmentManager dependency
// ════════════════════════════════════════════

using UnityEngine;
using System;
using Ascension.Core;
using Ascension.Manager.Model;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Character;
using Ascension.Character.Stat;

namespace Ascension.Character.Manager
{
    public class CharacterManager : MonoBehaviour
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
        
        // Optional dependency - resolved lazily when needed
        // private EquipmentManager _equipmentManager;
        #endregion

        #region Properties
        public CharacterStats CurrentPlayer => _currentPlayer;
        public CharacterBaseStatsSO BaseStats => baseStats;
        public bool HasActivePlayer => _isInitialized && _currentPlayer != null;
        #endregion

        #region Events
        public event Action<CharacterStats> OnPlayerLoaded;
        public event Action<CharacterStats> OnCharacterStatsChanged;
        public event Action<float, float> OnHealthChanged;
        public event Action<int> OnLevelUp;
        public event Action<int> OnExperienceGained;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            InitializeSingleton();
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
            
            TriggerPlayerLoadedEvents();
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
            
            TriggerPlayerLoadedEvents();
        }

        public void UnloadPlayer()
        {
            _currentPlayer = null;
            _isInitialized = false;
            Debug.Log("[CharacterManager] Player unloaded");
        }
        #endregion

        #region Equipment Integration
        /// <summary>
        /// Update player stats from equipped items
        /// Lazy-loads EquipmentManager from ServiceContainer
        /// </summary>
        public void UpdateStatsFromEquipment()
        {
            if (!HasActivePlayer) return;
            
            // TODO: Uncomment when EquipmentManager is ready
            /*
            var equipmentManager = GetEquipmentManager();
            
            if (equipmentManager != null)
            {
                CharacterItemStats equipmentStats = equipmentManager.GetTotalItemStats();
                _currentPlayer.ApplyItemStats(equipmentStats, baseStats);
                
                Debug.Log("[CharacterManager] Stats updated from equipment");
                OnCharacterStatsChanged?.Invoke(_currentPlayer);
            }
            */
            
            Debug.Log("[CharacterManager] UpdateStatsFromEquipment called (EquipmentManager not implemented yet)");
        }

        public bool EquipWeaponFromInventory(string itemID)
        {
            if (!HasActivePlayer) return false;
            
            // TODO: Implement when EquipmentManager ready
            /*
            var equipmentManager = GetEquipmentManager();
            
            if (equipmentManager == null)
            {
                Debug.LogError("[CharacterManager] EquipmentManager not found!");
                return false;
            }
            
            bool success = equipmentManager.EquipWeapon(itemID);
            
            if (success)
            {
                WeaponSO weapon = equipmentManager.GetEquippedWeapon();
                _currentPlayer.equippedWeapon = weapon;
                UpdateStatsFromEquipment();
            }
            
            return success;
            */
            
            Debug.LogWarning("[CharacterManager] EquipWeaponFromInventory not implemented yet");
            return false;
        }

        public void UnequipWeaponFromInventory()
        {
            if (!HasActivePlayer) return;
            
            // TODO: Implement when EquipmentManager ready
            /*
            var equipmentManager = GetEquipmentManager();
            
            if (equipmentManager != null)
            {
                equipmentManager.UnequipWeapon();
                _currentPlayer.equippedWeapon = null;
                UpdateStatsFromEquipment();
            }
            */
            
            Debug.LogWarning("[CharacterManager] UnequipWeaponFromInventory not implemented yet");
        }

        // TODO: Uncomment when EquipmentManager is ready
        /*
        private EquipmentManager GetEquipmentManager()
        {
            if (_equipmentManager == null && ServiceContainer.Instance != null)
            {
                _equipmentManager = ServiceContainer.Instance.Get<EquipmentManager>();
            }
            
            return _equipmentManager;
        }
        */
        #endregion

        #region Public Methods - Player Actions
        public void AddExperience(int amount)
        {
            if (!HasActivePlayer) return;

            OnExperienceGained?.Invoke(amount);

            bool leveledUp = _currentPlayer.AddExperience(amount, baseStats);

            if (leveledUp)
            {
                HandleLevelUp();
            }

            OnCharacterStatsChanged?.Invoke(_currentPlayer);
        }

        public void Heal(float amount)
        {
            if (!HasActivePlayer) return;

            float oldHP = _currentPlayer.CurrentHP;
            _currentPlayer.combatRuntime.Heal(amount, _currentPlayer.MaxHP);
            
            Debug.Log($"[CharacterManager] Healed {amount} HP ({oldHP:F0} → {_currentPlayer.CurrentHP:F0})");
            OnHealthChanged?.Invoke(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);
        }

        public void ApplyHeal(float amount)
        {
            if (!HasActivePlayer) return;
            
            float oldHP = _currentPlayer.CurrentHP;
            _currentPlayer.combatRuntime.Heal(amount, _currentPlayer.MaxHP);
            
            Debug.Log($"[CharacterManager] Applied heal {amount} HP ({oldHP:F0} → {_currentPlayer.CurrentHP:F0})");
            OnHealthChanged?.Invoke(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);
        }

        public void TakeDamage(float amount)
        {
            if (!HasActivePlayer) return;

            float oldHP = _currentPlayer.CurrentHP;
            _currentPlayer.combatRuntime.TakeDamage(amount, _currentPlayer.MaxHP);
            
            Debug.Log($"[CharacterManager] Took {amount} damage ({oldHP:F0} → {_currentPlayer.CurrentHP:F0})");
            OnHealthChanged?.Invoke(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);

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
            OnHealthChanged?.Invoke(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);
        }

        public void EquipWeapon(WeaponSO weapon)
        {
            if (!HasActivePlayer) return;

            _currentPlayer.EquipWeapon(weapon, baseStats);
            OnCharacterStatsChanged?.Invoke(_currentPlayer);
        }

        public void UnequipWeapon()
        {
            if (!HasActivePlayer) return;

            _currentPlayer.UnequipWeapon(baseStats);
            OnCharacterStatsChanged?.Invoke(_currentPlayer);
        }

        public void SetGuildRank(string rank)
        {
            if (!HasActivePlayer) return;

            _currentPlayer.SetGuildRank(rank);
            OnCharacterStatsChanged?.Invoke(_currentPlayer);
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
            OnCharacterStatsChanged?.Invoke(_currentPlayer);
            return true;
        }
        #endregion

        #region Public Methods - Helper Methods
        public void RecalculateStats()
        {
            if (!HasActivePlayer) return;

            _currentPlayer.RecalculateStats(baseStats, fullHeal: false);
            OnCharacterStatsChanged?.Invoke(_currentPlayer);
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

        private void TriggerPlayerLoadedEvents()
        {
            OnPlayerLoaded?.Invoke(_currentPlayer);
            OnCharacterStatsChanged?.Invoke(_currentPlayer);
        }

        private void HandleLevelUp()
        {
            Debug.Log($"[CharacterManager] Level up! Now level {_currentPlayer.Level}");
            OnLevelUp?.Invoke(_currentPlayer.Level);
            OnHealthChanged?.Invoke(_currentPlayer.CurrentHP, _currentPlayer.MaxHP);
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