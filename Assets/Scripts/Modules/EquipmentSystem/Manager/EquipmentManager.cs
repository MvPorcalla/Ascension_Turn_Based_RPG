// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Manager\EquipmentManager.cs
// ✅ REFACTORED: Uses coordinator pattern, no direct inventory mutations
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

                #region Synchronization Validation

        /// <summary>
        /// ✅ NEW: Validates that equipment slots and inventory locations are synchronized
        /// Call this after loading saves or after critical operations
        /// </summary>
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
                    // Empty slot is fine
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

        /// <summary>
        /// ✅ NEW: Auto-repair synchronization issues
        /// Call this if ValidateEquipmentSync() returns false
        /// </summary>
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
                    // Item doesn't exist - clear the slot
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
                // Move back to previous location (or Bag if unknown)
                ItemLocation targetLocation = item.previousLocation ?? ItemLocation.Bag;
                
                Debug.LogWarning($"🔧 Repairing: Moving orphaned item {item.itemID} from Equipped to {targetLocation}");
                item.location = targetLocation;
                item.previousLocation = null;
                repairsApplied++;
            }

            Debug.Log($"[EquipmentManager] === Auto-Repair Complete: {repairsApplied} fixes applied ===");

            // Trigger UI refresh
            UpdateCharacterStats();
            OnEquipmentChanged?.Invoke();
        }

        #endregion

        #region Public API - Save/Load

        /// <summary>
        /// Load equipment from save data and validate sync
        /// </summary>
        public void LoadEquipment(EquipmentSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning("[EquipmentManager] Cannot load null save data");
                return;
            }

            Debug.Log("[EquipmentManager] Loading equipment from save...");

            // Load all slots using the new helper method
            LoadAndEquipSlot(GearSlotType.Weapon, saveData.weaponId);
            LoadAndEquipSlot(GearSlotType.Helmet, saveData.helmetId);
            LoadAndEquipSlot(GearSlotType.Chest, saveData.chestId);
            LoadAndEquipSlot(GearSlotType.Gloves, saveData.glovesId);
            LoadAndEquipSlot(GearSlotType.Boots, saveData.bootsId);
            LoadAndEquipSlot(GearSlotType.Accessory1, saveData.accessory1Id);
            LoadAndEquipSlot(GearSlotType.Accessory2, saveData.accessory2Id);

            // Validate synchronization after load
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

        /// <summary>
        /// Helper method to load and sync a single equipment slot
        /// </summary>
        private void LoadAndEquipSlot(GearSlotType slot, string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                _equippedGear.ClearSlot(slot);
                return;
            }
            
            // Set the slot ID in EquippedGear
            _equippedGear.SetSlot(slot, itemId);
            
            // ✅ CRITICAL: Update the ItemInstance location to match
            var item = _inventoryManager.Inventory.allItems
                .FirstOrDefault(i => i.itemID == itemId);
            
            if (item != null)
            {
                item.location = ItemLocation.Equipped;
                Debug.Log($"[EquipmentManager] Loaded {itemId} into slot {slot}");
            }
            else
            {
                Debug.LogError($"[EquipmentManager] Item {itemId} not found in inventory during load!");
                _equippedGear.ClearSlot(slot);  // Clear invalid slot
            }
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