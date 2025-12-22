// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Manager\EquipmentManager.cs
// Main manager for equipment system - handles gear and stats
// ════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Core;
using Ascension.Data.SO.Database;
using Ascension.Data.SO.Item;
using Ascension.Data.Save;
using Ascension.Character.Stat;
using Ascension.Character.Manager;
using Ascension.Equipment.Data;
using Ascension.Equipment.Enums;
using Ascension.Equipment.Services;

namespace Ascension.Equipment.Manager
{
    public class EquipmentManager : MonoBehaviour, IGameService
    {
        #region Singleton
        public static EquipmentManager Instance { get; private set; }
        #endregion

        #region Serialized Fields
        [Header("References")]
        [SerializeField] private GameDatabaseSO database;
        #endregion

        #region Private Fields
        private EquippedGear _equippedGear;
        
        // Services
        private GearSlotService _slotService;
        private GearEquipService _equipService;
        private GearStatsService _statsService;
        
        // Dependencies
        private CharacterManager _characterManager;
        #endregion

        #region Properties
        public EquippedGear EquippedGear => _equippedGear;
        public GameDatabaseSO Database => database;
        #endregion

        #region Events
        public event Action<GearSlotType, string> OnGearEquipped;   // (slotType, itemId)
        public event Action<GearSlotType> OnGearUnequipped;         // (slotType)
        public event Action OnEquipmentChanged;                      // Any equipment change
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
            Debug.Log("[EquipmentManager] Initializing...");
            
            InitializeData();
            InitializeServices();
            InjectDependencies();
            
            Debug.Log("[EquipmentManager] Ready");
        }

        private void InitializeData()
        {
            _equippedGear = new EquippedGear();
        }

        private void InitializeServices()
        {
            _slotService = new GearSlotService();
            _equipService = new GearEquipService(_slotService);
            _statsService = new GearStatsService();
            
            Debug.Log("[EquipmentManager] Services initialized");
        }

        private void InjectDependencies()
        {
            var container = ServiceContainer.Instance;
            
            if (container == null)
            {
                Debug.LogError("[EquipmentManager] ServiceContainer not available!");
                return;
            }

            _characterManager = container.Get<CharacterManager>();

            if (_characterManager == null)
                Debug.LogError("[EquipmentManager] CharacterManager not found!");
            
            if (database == null)
                Debug.LogError("[EquipmentManager] GameDatabaseSO not assigned!");
        }
        #endregion

        #region Public Methods - Equip/Unequip
        /// <summary>
        /// Equip an item to a specific slot
        /// </summary>
        public bool EquipItem(string itemId, GearSlotType slotType)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("[EquipmentManager] Cannot equip null/empty item ID");
                return false;
            }

            // Equip item (returns previously equipped item if any)
            string previousItemId = _equipService.EquipItem(_equippedGear, itemId, slotType, database);

            // Update character stats
            UpdateCharacterStats();

            // Fire events
            OnGearEquipped?.Invoke(slotType, itemId);
            OnEquipmentChanged?.Invoke();

            Debug.Log($"[EquipmentManager] Equipped {itemId} to {slotType}");
            return true;
        }

        /// <summary>
        /// Unequip item from a slot
        /// </summary>
        public bool UnequipSlot(GearSlotType slotType)
        {
            bool success = _equipService.UnequipSlot(_equippedGear, slotType);

            if (success)
            {
                // Update character stats
                UpdateCharacterStats();

                // Fire events
                OnGearUnequipped?.Invoke(slotType);
                OnEquipmentChanged?.Invoke();

                Debug.Log($"[EquipmentManager] Unequipped {slotType} slot");
            }

            return success;
        }

        /// <summary>
        /// Swap accessory slots
        /// </summary>
        public bool SwapAccessorySlots()
        {
            bool success = _equipService.SwapAccessorySlots(_equippedGear);

            if (success)
            {
                OnEquipmentChanged?.Invoke();
            }

            return success;
        }
        #endregion

        #region Public Methods - Queries
        /// <summary>
        /// Check if an item is currently equipped
        /// </summary>
        public bool IsItemEquipped(string itemId)
        {
            return _equipService.IsItemEquipped(_equippedGear, itemId);
        }

        /// <summary>
        /// Get item ID in a specific slot
        /// </summary>
        public string GetEquippedItemId(GearSlotType slotType)
        {
            return _equippedGear.GetSlot(slotType);
        }

        /// <summary>
        /// Check if a slot is empty
        /// </summary>
        public bool IsSlotEmpty(GearSlotType slotType)
        {
            return _equippedGear.IsSlotEmpty(slotType);
        }

        /// <summary>
        /// Get total stats from all equipped gear
        /// </summary>
        public CharacterItemStats GetTotalItemStats()
        {
            return _statsService.CalculateTotalStats(_equippedGear, database);
        }

        /// <summary>
        /// Get currently equipped weapon
        /// </summary>
        public WeaponSO GetEquippedWeapon()
        {
            return _statsService.GetEquippedWeapon(_equippedGear, database);
        }
        #endregion

        #region Public Methods - Load/Unload
        /// <summary>
        /// Load equipment from save data
        /// </summary>
        public void LoadEquipment(EquipmentSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning("[EquipmentManager] Cannot load null save data");
                return;
            }

            _equippedGear.weaponId = saveData.weaponId ?? string.Empty;
            _equippedGear.helmetId = saveData.helmetId ?? string.Empty;
            _equippedGear.chestId = saveData.chestId ?? string.Empty;
            _equippedGear.glovesId = saveData.glovesId ?? string.Empty;
            _equippedGear.bootsId = saveData.bootsId ?? string.Empty;
            _equippedGear.accessory1Id = saveData.accessory1Id ?? string.Empty;
            _equippedGear.accessory2Id = saveData.accessory2Id ?? string.Empty;

            // Update character stats with loaded equipment
            UpdateCharacterStats();

            OnEquipmentChanged?.Invoke();
            Debug.Log($"[EquipmentManager] Loaded equipment - {_equippedGear.GetEquippedCount()} items equipped");
        }

        /// <summary>
        /// Save equipment to data structure
        /// </summary>
        public EquipmentSaveData SaveEquipment()
        {
            return new EquipmentSaveData
            {
                weaponId = _equippedGear.weaponId,
                helmetId = _equippedGear.helmetId,
                chestId = _equippedGear.chestId,
                glovesId = _equippedGear.glovesId,
                bootsId = _equippedGear.bootsId,
                accessory1Id = _equippedGear.accessory1Id,
                accessory2Id = _equippedGear.accessory2Id
            };
        }

        /// <summary>
        /// Unequip all gear
        /// </summary>
        public void UnequipAll()
        {
            _equippedGear.ClearAll();
            UpdateCharacterStats();
            OnEquipmentChanged?.Invoke();
            
            Debug.Log("[EquipmentManager] All gear unequipped");
        }
        #endregion

        #region Private Methods
        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// Update CharacterManager stats when equipment changes
        /// </summary>
        private void UpdateCharacterStats()
        {
            if (_characterManager == null)
            {
                Debug.LogWarning("[EquipmentManager] Cannot update stats - CharacterManager not found");
                return;
            }

            _characterManager.UpdateStatsFromEquipment();
        }
        
        #endregion

        #region Debug Tools
        [ContextMenu("Debug: Print Equipped Gear")]
        private void DebugPrintEquippedGear()
        {
            Debug.Log("=== EQUIPPED GEAR ===");
            Debug.Log($"Weapon: {_equippedGear.weaponId}");
            Debug.Log($"Helmet: {_equippedGear.helmetId}");
            Debug.Log($"Chest: {_equippedGear.chestId}");
            Debug.Log($"Gloves: {_equippedGear.glovesId}");
            Debug.Log($"Boots: {_equippedGear.bootsId}");
            Debug.Log($"Accessory1: {_equippedGear.accessory1Id}");
            Debug.Log($"Accessory2: {_equippedGear.accessory2Id}");
            Debug.Log($"Total Equipped: {_equippedGear.GetEquippedCount()}");
        }

        [ContextMenu("Debug: Print Total Stats")]
        private void DebugPrintTotalStats()
        {
            var stats = GetTotalItemStats();
            Debug.Log("=== TOTAL EQUIPMENT STATS ===");
            Debug.Log($"AD: {stats.AD}");
            Debug.Log($"AP: {stats.AP}");
            Debug.Log($"HP: {stats.HP}");
            Debug.Log($"Defense: {stats.Defense}");
            Debug.Log($"Crit Rate: {stats.CritRate}%");
            Debug.Log($"Attack Speed: {stats.AttackSpeed}");
        }

        [ContextMenu("Debug: Unequip All")]
        private void DebugUnequipAll()
        {
            UnequipAll();
        }
        #endregion
    }
}