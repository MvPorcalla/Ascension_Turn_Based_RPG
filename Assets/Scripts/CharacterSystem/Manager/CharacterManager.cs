// ════════════════════════════════════════════
// CharacterManager.cs
// Single source of truth for player character data
// ════════════════════════════════════════════

using UnityEngine;
using System;
using Ascension.Data;
using Ascension.Data.Model;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Character;
using Ascension.Character.Stat;

namespace Ascension.Manager
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
        private CharacterStats currentPlayer;
        private bool isInitialized = false;
        #endregion

        #region Properties
        public CharacterStats CurrentPlayer => currentPlayer;
        public CharacterBaseStatsSO BaseStats => baseStats;
        public bool HasActivePlayer => isInitialized && currentPlayer != null;
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
            currentPlayer = new CharacterStats();
            currentPlayer.playerName = playerName;
            currentPlayer.Initialize(baseStats);
            
            isInitialized = true;

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

            currentPlayer = loadedStats;
            currentPlayer.RecalculateStats(baseStats, fullHeal: false);
            
            isInitialized = true;

            Debug.Log($"[CharacterManager] Loaded player: {currentPlayer.playerName}");
            
            TriggerPlayerLoadedEvents();
        }

        public void UnloadPlayer()
        {
            currentPlayer = null;
            isInitialized = false;
            Debug.Log("[CharacterManager] Player unloaded");
        }
        #endregion

        #region Equipment Integration (NEW)

        /// <summary>
        /// Update player stats from equipped items
        /// Called by EquipmentManager when equipment changes
        /// </summary>
        public void UpdateStatsFromEquipment()
        {
            if (!HasActivePlayer) return;
            
            // Get equipment stats from EquipmentManager
            if (EquipmentManager.Instance != null)
            {
                CharacterItemStats equipmentStats = EquipmentManager.Instance.GetTotalItemStats();
                currentPlayer.ApplyItemStats(equipmentStats, baseStats);
                
                Debug.Log("[CharacterManager] Stats updated from equipment");
                OnCharacterStatsChanged?.Invoke(currentPlayer);
            }
        }

        /// <summary>
        /// Equip weapon (delegates to EquipmentManager, then updates stats)
        /// </summary>
        public bool EquipWeaponFromInventory(string itemID)
        {
            if (!HasActivePlayer) return false;
            
            if (EquipmentManager.Instance == null)
            {
                Debug.LogError("[CharacterManager] EquipmentManager not found!");
                return false;
            }
            
            bool success = EquipmentManager.Instance.EquipWeapon(itemID);
            
            if (success)
            {
                // Update CharacterStats weapon reference
                WeaponSO weapon = EquipmentManager.Instance.GetEquippedWeapon();
                currentPlayer.equippedWeapon = weapon;
                
                // Recalculate stats
                UpdateStatsFromEquipment();
            }
            
            return success;
        }

        /// <summary>
        /// Unequip weapon
        /// </summary>
        public void UnequipWeaponFromInventory()
        {
            if (!HasActivePlayer) return;
            
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.UnequipWeapon();
                currentPlayer.equippedWeapon = null;
                UpdateStatsFromEquipment();
            }
        }
        #endregion

        #region Public Methods - Player Actions
        public void AddExperience(int amount)
        {
            if (!HasActivePlayer) return;

            OnExperienceGained?.Invoke(amount);

            bool leveledUp = currentPlayer.AddExperience(amount, baseStats);

            if (leveledUp)
            {
                HandleLevelUp();
            }

            OnCharacterStatsChanged?.Invoke(currentPlayer);
        }

        public void Heal(float amount)
        {
            if (!HasActivePlayer) return;

            float oldHP = currentPlayer.CurrentHP;
            currentPlayer.combatRuntime.Heal(amount, currentPlayer.MaxHP);
            
            Debug.Log($"[CharacterManager] Healed {amount} HP ({oldHP:F0} → {currentPlayer.CurrentHP:F0})");
            OnHealthChanged?.Invoke(currentPlayer.CurrentHP, currentPlayer.MaxHP);
        }

        public void ApplyHeal(float amount)
        {
            if (!HasActivePlayer) return;
            
            float oldHP = currentPlayer.CurrentHP;
            currentPlayer.combatRuntime.Heal(amount, currentPlayer.MaxHP);
            
            Debug.Log($"[CharacterManager] Applied heal {amount} HP ({oldHP:F0} → {currentPlayer.CurrentHP:F0})");
            OnHealthChanged?.Invoke(currentPlayer.CurrentHP, currentPlayer.MaxHP);
        }

        public void TakeDamage(float amount)
        {
            if (!HasActivePlayer) return;

            float oldHP = currentPlayer.CurrentHP;
            currentPlayer.combatRuntime.TakeDamage(amount, currentPlayer.MaxHP);
            
            Debug.Log($"[CharacterManager] Took {amount} damage ({oldHP:F0} → {currentPlayer.CurrentHP:F0})");
            OnHealthChanged?.Invoke(currentPlayer.CurrentHP, currentPlayer.MaxHP);

            if (currentPlayer.CurrentHP <= 0)
            {
                HandlePlayerDeath();
            }
        }

        public void FullHeal()
        {
            if (!HasActivePlayer) return;

            currentPlayer.combatRuntime.currentHP = currentPlayer.MaxHP;
            Debug.Log("[CharacterManager] Full heal applied");
            OnHealthChanged?.Invoke(currentPlayer.CurrentHP, currentPlayer.MaxHP);
        }

        public void EquipWeapon(WeaponSO weapon)
        {
            if (!HasActivePlayer) return;

            currentPlayer.EquipWeapon(weapon, baseStats);
            OnCharacterStatsChanged?.Invoke(currentPlayer);
        }

        public void UnequipWeapon()
        {
            if (!HasActivePlayer) return;

            currentPlayer.UnequipWeapon(baseStats);
            OnCharacterStatsChanged?.Invoke(currentPlayer);
        }

        public void SetGuildRank(string rank)
        {
            if (!HasActivePlayer) return;

            currentPlayer.SetGuildRank(rank);
            OnCharacterStatsChanged?.Invoke(currentPlayer);
        }

        public bool AllocateAttributePoint(string attributeName)
        {
            if (!HasActivePlayer) return false;

            if (currentPlayer.UnallocatedPoints <= 0)
            {
                Debug.LogWarning("[CharacterManager] No unallocated points available!");
                return false;
            }

            currentPlayer.ModifyAttribute(attributeName, 1, baseStats);
            currentPlayer.levelSystem.unallocatedPoints--;
            
            Debug.Log($"[CharacterManager] Allocated point to {attributeName}");
            OnCharacterStatsChanged?.Invoke(currentPlayer);
            return true;
        }
        #endregion

        #region Public Methods - Helper Methods
        public void RecalculateStats()
        {
            if (!HasActivePlayer) return;

            currentPlayer.RecalculateStats(baseStats, fullHeal: false);
            OnCharacterStatsChanged?.Invoke(currentPlayer);
        }

        public CharacterStats GetCharacterDataForSave()
        {
            return currentPlayer;
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
            OnPlayerLoaded?.Invoke(currentPlayer);
            OnCharacterStatsChanged?.Invoke(currentPlayer);
        }

        private void HandleLevelUp()
        {
            Debug.Log($"[CharacterManager] Level up! Now level {currentPlayer.Level}");
            OnLevelUp?.Invoke(currentPlayer.Level);
            OnHealthChanged?.Invoke(currentPlayer.CurrentHP, currentPlayer.MaxHP);
        }

        private void HandlePlayerDeath()
        {
            Debug.Log("[CharacterManager] Player has died!");
            // TODO: Implement death logic (respawn, game over, etc.)
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
            Debug.Log($"Name: {currentPlayer.playerName}");
            Debug.Log($"Level: {currentPlayer.Level}");
            Debug.Log($"HP: {currentPlayer.CurrentHP}/{currentPlayer.MaxHP}");
            Debug.Log($"AD: {currentPlayer.AD}");
            Debug.Log($"AP: {currentPlayer.AP}");
            Debug.Log($"Attack Speed: {currentPlayer.AttackSpeed}");
            Debug.Log($"STR: {currentPlayer.attributes.STR}");
            Debug.Log($"INT: {currentPlayer.attributes.INT}");
            Debug.Log($"AGI: {currentPlayer.attributes.AGI}");
            Debug.Log($"END: {currentPlayer.attributes.END}");
            Debug.Log($"WIS: {currentPlayer.attributes.WIS}");
        }

        [ContextMenu("Debug: Add 100 EXP")]
        private void DebugAddExp()
        {
            AddExperience(100);
        }

        [ContextMenu("Debug: Damage 50 HP")]
        private void DebugDamage()
        {
            TakeDamage(50);
        }

        [ContextMenu("Debug: Heal 50 HP")]
        private void DebugHeal()
        {
            Heal(50);
        }

        [ContextMenu("Debug: Full Heal")]
        private void DebugFullHeal()
        {
            FullHeal();
        }
        #endregion
    }
}