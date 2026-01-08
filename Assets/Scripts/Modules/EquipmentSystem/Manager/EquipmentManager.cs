// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Manager\EquipmentManager.cs
// ✅ REFACTORED: Uses coordinator pattern, no direct inventory mutations
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
using Ascension.Equipment.Coordinators;
using Ascension.Inventory.Manager;

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
        private GearEquipCoordinator _coordinator; // ✅ Renamed from GearEquipService
        private GearStatsService _statsService;
        
        // Dependencies
        private CharacterManager _characterManager;
        private InventoryManager _inventoryManager;
        #endregion

        #region Properties
        public EquippedGear EquippedGear => _equippedGear;
        public GameDatabaseSO Database => database;
        #endregion

        #region Events
        public event Action<GearSlotType, string> OnGearEquipped;
        public event Action<GearSlotType, string> OnGearUnequipped;
        public event Action OnEquipmentChanged;
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
            _coordinator = new GearEquipCoordinator(_slotService); // ✅ Use coordinator
            _statsService = new GearStatsService();
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
            _inventoryManager = container.Get<InventoryManager>();

            if (_characterManager == null)
                Debug.LogError("[EquipmentManager] CharacterManager not found!");
            
            if (_inventoryManager == null)
                Debug.LogError("[EquipmentManager] InventoryManager not found!");
            
            if (database == null)
                Debug.LogError("[EquipmentManager] GameDatabaseSO not assigned!");
        }
        #endregion

        #region Public API - Primary Methods

        /// <summary>
        /// ✅ REFACTORED: Equip item via coordinator (no direct inventory mutations)
        /// </summary>
        public bool EquipItem(string itemId)
        {
            var result = _coordinator.EquipFromInventory(
                _equippedGear,
                itemId,
                _inventoryManager,
                database
            );

            if (result.Success)
            {
                // Find which slot was equipped
                var slot = _coordinator.FindEquippedSlot(_equippedGear, itemId);
                if (slot.HasValue)
                {
                    UpdateCharacterStats();
                    OnGearEquipped?.Invoke(slot.Value, itemId);
                    
                    if (!string.IsNullOrEmpty(result.UnequippedItemId))
                        OnGearUnequipped?.Invoke(slot.Value, result.UnequippedItemId);
                    
                    OnEquipmentChanged?.Invoke();
                }
                
                Debug.Log($"[EquipmentManager] {result.Message}");
            }
            else
            {
                Debug.LogWarning($"[EquipmentManager] {result.Message}");
            }

            return result.Success;
        }

        /// <summary>
        /// ✅ REFACTORED: Unequip item by ID (via coordinator)
        /// </summary>
        public bool UnequipItem(string itemId)
        {
            var slot = _coordinator.FindEquippedSlot(_equippedGear, itemId);
            if (!slot.HasValue)
            {
                Debug.LogWarning($"[EquipmentManager] Item {itemId} not equipped");
                return false;
            }

            return UnequipSlot(slot.Value);
        }

        /// <summary>
        /// ✅ REFACTORED: Unequip specific slot (via coordinator)
        /// </summary>
        public bool UnequipSlot(GearSlotType slotType)
        {
            var result = _coordinator.UnequipViaInventory(
                _equippedGear,
                slotType,
                _inventoryManager,
                database
            );

            if (result.Success)
            {
                UpdateCharacterStats();
                OnGearUnequipped?.Invoke(slotType, result.Message);
                OnEquipmentChanged?.Invoke();
                
                Debug.Log($"[EquipmentManager] {result.Message}");
            }
            else
            {
                Debug.LogWarning($"[EquipmentManager] {result.Message}");
            }

            return result.Success;
        }

        #endregion

        #region Public API - Advanced Methods

        /// <summary>
        /// Equip to specific slot (for manual slot selection)
        /// </summary>
        public bool EquipItemToSlot(string itemId, GearSlotType slotType)
        {
            var result = _coordinator.EquipFromInventory(
                _equippedGear,
                itemId,
                _inventoryManager,
                database,
                slotType
            );

            if (result.Success)
            {
                UpdateCharacterStats();
                OnGearEquipped?.Invoke(slotType, itemId);
                
                if (!string.IsNullOrEmpty(result.UnequippedItemId))
                    OnGearUnequipped?.Invoke(slotType, result.UnequippedItemId);
                
                OnEquipmentChanged?.Invoke();
            }

            return result.Success;
        }

        /// <summary>
        /// Toggle equip/unequip (for button that switches state)
        /// </summary>
        public bool ToggleEquip(string itemId)
        {
            if (IsItemEquipped(itemId))
                return UnequipItem(itemId);
            else
                return EquipItem(itemId);
        }

        /// <summary>
        /// Swap accessory slots
        /// </summary>
        public bool SwapAccessorySlots()
        {
            bool success = _coordinator.SwapAccessorySlots(_equippedGear);

            if (success)
            {
                OnEquipmentChanged?.Invoke();
            }

            return success;
        }

        #endregion

        #region Public API - Query Methods

        /// <summary>
        /// Check if an item is currently equipped
        /// </summary>
        public bool IsItemEquipped(string itemId)
        {
            return _coordinator.IsItemEquipped(_equippedGear, itemId);
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
        /// Find which slot an item is equipped in
        /// </summary>
        public GearSlotType? FindEquippedSlot(string itemId)
        {
            return _coordinator.FindEquippedSlot(_equippedGear, itemId);
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

        #region Public API - Save/Load

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

            UpdateCharacterStats();
            OnEquipmentChanged?.Invoke();
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
        /// Unequip all gear (returns to bag/storage)
        /// </summary>
        public void UnequipAll()
        {
            foreach (GearSlotType slot in Enum.GetValues(typeof(GearSlotType)))
            {
                if (!IsSlotEmpty(slot))
                    UnequipSlot(slot);
            }
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
    }
}