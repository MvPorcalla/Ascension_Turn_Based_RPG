// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Manager\EquipmentManager.cs
// ✅ FIXED: Load equipment without modifying inventory locations
// ════════════════════════════════════════════

using System;
using System.Linq;
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
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;

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
        private GearEquipCoordinator _coordinator;
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
            _coordinator = new GearEquipCoordinator(_slotService);
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

        public bool ToggleEquip(string itemId)
        {
            if (IsItemEquipped(itemId))
                return UnequipItem(itemId);
            else
                return EquipItem(itemId);
        }

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

        public bool IsItemEquipped(string itemId)
        {
            return _coordinator.IsItemEquipped(_equippedGear, itemId);
        }

        public string GetEquippedItemId(GearSlotType slotType)
        {
            return _equippedGear.GetSlot(slotType);
        }

        public bool IsSlotEmpty(GearSlotType slotType)
        {
            return _equippedGear.IsSlotEmpty(slotType);
        }

        public GearSlotType? FindEquippedSlot(string itemId)
        {
            return _coordinator.FindEquippedSlot(_equippedGear, itemId);
        }

        public CharacterItemStats GetTotalItemStats()
        {
            return _statsService.CalculateTotalStats(_equippedGear, database);
        }

        public WeaponSO GetEquippedWeapon()
        {
            return _statsService.GetEquippedWeapon(_equippedGear, database);
        }

        #endregion

        #region Synchronization Validation

        [ContextMenu("Validate Equipment Sync")]
        public bool ValidateEquipmentSync()
        {
            if (_inventoryManager == null)
            {
                Debug.LogError("[EquipmentManager] Cannot validate - InventoryManager not available");
                return false;
            }

            bool isValid = true;
            int errorsFound = 0;

            Debug.Log("[EquipmentManager] === Starting Equipment Sync Validation ===");

            // Check all 7 equipment slots
            foreach (GearSlotType slot in System.Enum.GetValues(typeof(GearSlotType)))
            {
                string itemId = _equippedGear.GetSlot(slot);
                
                if (string.IsNullOrEmpty(itemId))
                {
                    continue;
                }

                // Verify item exists in inventory
                ItemLocation? location = _inventoryManager.GetItemLocation(itemId);
                
                if (location == null)
                {
                    Debug.LogError($"❌ SYNC ERROR: {itemId} in slot {slot} but NOT FOUND in inventory!");
                    isValid = false;
                    errorsFound++;
                }
                else if (location != ItemLocation.Equipped)
                {
                    Debug.LogError($"❌ SYNC ERROR: {itemId} in slot {slot} but inventory location is {location}!");
                    isValid = false;
                    errorsFound++;
                }
                else
                {
                    Debug.Log($"✅ Slot {slot}: {itemId} synchronized correctly");
                }
            }

            // Check for orphaned equipped items (location=Equipped but not in any slot)
            var equippedItems = _inventoryManager.Inventory.allItems
                .Where(i => i.location == ItemLocation.Equipped)
                .ToList();

            Debug.Log($"[EquipmentManager] Found {equippedItems.Count} items with location=Equipped");

            foreach (var item in equippedItems)
            {
                if (!_equippedGear.IsEquipped(item.itemID))
                {
                    Debug.LogError($"❌ SYNC ERROR: {item.itemID} has location=Equipped but is NOT in any equipment slot!");
                    isValid = false;
                    errorsFound++;
                }
            }

            Debug.Log($"[EquipmentManager] === Validation Complete: {(isValid ? "✅ ALL OK" : $"❌ {errorsFound} ERRORS FOUND")} ===");
            
            return isValid;
        }

        public void RepairEquipmentSync()
        {
            if (_inventoryManager == null)
            {
                Debug.LogError("[EquipmentManager] Cannot repair - InventoryManager not available");
                return;
            }

            Debug.LogWarning("[EquipmentManager] === Starting Auto-Repair ===");
            int repairsApplied = 0;

            // Step 1: Fix all items in equipment slots (set their location to Equipped)
            foreach (GearSlotType slot in System.Enum.GetValues(typeof(GearSlotType)))
            {
                string itemId = _equippedGear.GetSlot(slot);
                
                if (string.IsNullOrEmpty(itemId))
                    continue;

                var item = _inventoryManager.Inventory.allItems
                    .FirstOrDefault(i => i.itemID == itemId);
                
                if (item != null && item.location != ItemLocation.Equipped)
                {
                    Debug.Log($"🔧 Repairing: Setting {itemId} location to Equipped (was {item.location})");
                    item.location = ItemLocation.Equipped;
                    repairsApplied++;
                }
                else if (item == null)
                {
                    Debug.LogWarning($"🔧 Repairing: Clearing slot {slot} (item {itemId} not found in inventory)");
                    _equippedGear.ClearSlot(slot);
                    repairsApplied++;
                }
            }

            // Step 2: Fix orphaned equipped items (location=Equipped but not in slots)
            var orphanedItems = _inventoryManager.Inventory.allItems
                .Where(i => i.location == ItemLocation.Equipped && !_equippedGear.IsEquipped(i.itemID))
                .ToList();

            foreach (var item in orphanedItems)
            {
                ItemLocation targetLocation = item.previousLocation ?? ItemLocation.Bag;
                
                Debug.LogWarning($"🔧 Repairing: Moving orphaned item {item.itemID} from Equipped to {targetLocation}");
                item.location = targetLocation;
                item.previousLocation = null;
                repairsApplied++;
            }

            Debug.Log($"[EquipmentManager] === Auto-Repair Complete: {repairsApplied} fixes applied ===");

            UpdateCharacterStats();
            OnEquipmentChanged?.Invoke();
        }

        #endregion

        #region Public API - Save/Load

        /// <summary>
        /// ✅ FIXED: Load equipment slots without modifying inventory locations
        /// Inventory locations are already correct from InventoryManager.LoadInventory()
        /// </summary>
        public void LoadEquipment(EquipmentSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning("[EquipmentManager] Cannot load null save data");
                return;
            }

            Debug.Log("[EquipmentManager] Loading equipment from save...");

            // ✅ CRITICAL FIX: Just populate slots - DON'T modify inventory locations
            // Inventory locations were already set correctly by InventoryManager.LoadInventory()
            _equippedGear.weaponId = saveData.weaponId ?? string.Empty;
            _equippedGear.helmetId = saveData.helmetId ?? string.Empty;
            _equippedGear.chestId = saveData.chestId ?? string.Empty;
            _equippedGear.glovesId = saveData.glovesId ?? string.Empty;
            _equippedGear.bootsId = saveData.bootsId ?? string.Empty;
            _equippedGear.accessory1Id = saveData.accessory1Id ?? string.Empty;
            _equippedGear.accessory2Id = saveData.accessory2Id ?? string.Empty;

            // Validate synchronization after load (read-only check)
            bool isValid = ValidateEquipmentSync();
            
            if (!isValid)
            {
                Debug.LogError("[EquipmentManager] Equipment load resulted in desync! Attempting auto-repair...");
                RepairEquipmentSync();
                
                // Validate again after repair
                if (ValidateEquipmentSync())
                {
                    Debug.Log("[EquipmentManager] ✅ Auto-repair successful!");
                }
                else
                {
                    Debug.LogError("[EquipmentManager] ❌ Auto-repair failed - manual intervention needed!");
                }
            }

            // Update stats and notify UI
            UpdateCharacterStats();
            OnEquipmentChanged?.Invoke();
            
            Debug.Log("[EquipmentManager] Equipment loaded successfully");
        }

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