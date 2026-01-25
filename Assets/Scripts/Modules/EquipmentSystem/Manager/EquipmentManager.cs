// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Modules/EquipmentSystem/Manager/EquipmentManager.cs
// ✅ SIMPLIFIED: Removed coordinator pattern, direct operations
// ════════════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Core;
using Ascension.Data.SO.Database;
using Ascension.Data.SO.Item;
using Ascension.Data.Save;
using Ascension.Character.Core;
using Ascension.Character.Manager;
using Ascension.Equipment.Data;
using Ascension.Equipment.Enums;
using Ascension.Equipment.Services;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;

namespace Ascension.Equipment.Manager
{
    public class EquipmentManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("References")]
        [SerializeField] private GameDatabaseSO database;
        #endregion

        #region Private Fields
        private EquippedGear _equippedGear;
        
        // Services (stateless helpers)
        private GearSlotService _slotService;
        private GearStatsService _statsService;
        
        // Dependencies (injected via GameBootstrap)
        private CharacterManager _characterManager;
        private InventoryManager _inventoryManager;
        #endregion

        #region Properties
        public EquippedGear EquippedGear => _equippedGear;
        public GameDatabaseSO Database => database;
        #endregion

        #region Initialization
        public void Init()
        {
            // Initialize data
            _equippedGear = new EquippedGear();
            
            // Initialize services
            _slotService = new GearSlotService();
            _statsService = new GearStatsService();
            
            // Inject dependencies
            _characterManager = GameBootstrap.Character;
            _inventoryManager = GameBootstrap.Inventory;

            // Validate
            if (_characterManager == null)
                Debug.LogError("[EquipmentManager] CharacterManager not found!");
            
            if (_inventoryManager == null)
                Debug.LogError("[EquipmentManager] InventoryManager not found!");
            
            if (database == null)
                Debug.LogError("[EquipmentManager] GameDatabaseSO not assigned!");
            else
                Debug.Log($"[EquipmentManager] Initialized with database: {database.name}");
        }
        #endregion

        #region Public API - Primary Methods

        /// <summary>
        /// ✅ SIMPLIFIED: Equip an item from inventory (auto-detects slot)
        /// </summary>
        public bool EquipItem(string itemId)
        {
            // 1. Validate managers
            if (_inventoryManager == null || database == null)
            {
                Debug.LogError("[EquipmentManager] Missing dependencies");
                return false;
            }

            // 2. Get item data
            ItemBaseSO itemData = database.GetItem(itemId);
            if (itemData == null)
            {
                Debug.LogWarning($"[EquipmentManager] Item not found: {itemId}");
                return false;
            }

            // 3. Check if item can be equipped
            if (!_inventoryManager.CanEquipItem(itemId))
            {
                Debug.LogWarning($"[EquipmentManager] {itemData.ItemName} cannot be equipped");
                return false;
            }

            // 4. Auto-detect slot
            GearSlotType? targetSlot = _slotService.GetSlotForItem(itemData, _equippedGear);
            if (!targetSlot.HasValue)
            {
                Debug.LogWarning($"[EquipmentManager] Cannot determine slot for {itemData.ItemName}");
                return false;
            }

            // 5. Validate slot compatibility
            if (!_slotService.CanEquipInSlot(itemData, targetSlot.Value))
            {
                Debug.LogWarning($"[EquipmentManager] {itemData.ItemName} cannot go in {targetSlot.Value} slot");
                return false;
            }

            // 6. Unequip current item in slot (if any)
            string previousItemId = _equippedGear.GetSlot(targetSlot.Value);
            if (!string.IsNullOrEmpty(previousItemId))
            {
                if (!UnequipSlot(targetSlot.Value))
                {
                    Debug.LogWarning($"[EquipmentManager] Failed to unequip {previousItemId}");
                    return false;
                }
            }

            // 7. Move item to equipped location (via InventoryManager)
            var moveResult = _inventoryManager.MoveToEquipped(itemId);
            if (!moveResult.Success)
            {
                Debug.LogWarning($"[EquipmentManager] Failed to equip: {moveResult.Message}");
                return false;
            }

            // 8. Update equipment slot
            _equippedGear.SetSlot(targetSlot.Value, itemId);

            // 9. Update character stats
            UpdateCharacterStats();

            // 10. Trigger events
            GameEvents.TriggerGearEquipped(targetSlot.Value, itemData);
            GameEvents.TriggerEquipmentChanged();

            Debug.Log($"[EquipmentManager] ✅ Equipped {itemData.ItemName} to {targetSlot.Value}");
            return true;
        }

        /// <summary>
        /// ✅ SIMPLIFIED: Unequip an item by item ID
        /// </summary>
        public bool UnequipItem(string itemId)
        {
            var slot = FindEquippedSlot(itemId);
            if (!slot.HasValue)
            {
                Debug.LogWarning($"[EquipmentManager] Item {itemId} not equipped");
                return false;
            }

            return UnequipSlot(slot.Value);
        }

        /// <summary>
        /// ✅ SIMPLIFIED: Unequip an item from a specific slot
        /// </summary>
        public bool UnequipSlot(GearSlotType slotType)
        {
            // 1. Validate slot has item
            string itemId = _equippedGear.GetSlot(slotType);
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning($"[EquipmentManager] {slotType} slot is already empty");
                return false;
            }

            // 2. Get item data (for logging)
            ItemBaseSO itemData = database?.GetItem(itemId);
            string itemName = itemData?.ItemName ?? itemId;

            // 3. Move item from equipped (via InventoryManager)
            var moveResult = _inventoryManager.MoveFromEquipped(itemId);
            if (!moveResult.Success)
            {
                Debug.LogWarning($"[EquipmentManager] Failed to unequip: {moveResult.Message}");
                return false;
            }

            // 4. Clear equipment slot
            _equippedGear.ClearSlot(slotType);

            // 5. Update character stats
            UpdateCharacterStats();

            // 6. Trigger events
            GameEvents.TriggerGearUnequipped(slotType);
            GameEvents.TriggerEquipmentChanged();

            Debug.Log($"[EquipmentManager] Unequipped {itemName} from {slotType}");
            return true;
        }

        #endregion

        #region Public API - Advanced Methods

        /// <summary>
        /// Toggle equip/unequip for an item
        /// </summary>
        public bool ToggleEquip(string itemId)
        {
            if (IsItemEquipped(itemId))
                return UnequipItem(itemId);
            else
                return EquipItem(itemId);
        }

        /// <summary>
        /// Unequip all gear
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

        #region Public API - Query Methods

        /// <summary>
        /// Check if an item is equipped
        /// </summary>
        public bool IsItemEquipped(string itemId)
        {
            return _equippedGear.IsEquipped(itemId);
        }

        /// <summary>
        /// Get the item ID in a specific slot
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
        /// Find which slot contains an item
        /// </summary>
        public GearSlotType? FindEquippedSlot(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return null;

            if (_equippedGear.weaponId == itemId) return GearSlotType.Weapon;
            if (_equippedGear.helmetId == itemId) return GearSlotType.Helmet;
            if (_equippedGear.chestId == itemId) return GearSlotType.Chest;
            if (_equippedGear.glovesId == itemId) return GearSlotType.Gloves;
            if (_equippedGear.bootsId == itemId) return GearSlotType.Boots;
            if (_equippedGear.accessory1Id == itemId) return GearSlotType.Accessory1;
            if (_equippedGear.accessory2Id == itemId) return GearSlotType.Accessory2;

            return null;
        }

        /// <summary>
        /// Calculate total stats from all equipped gear
        /// </summary>
        public CharacterItemStats GetTotalItemStats()
        {
            return _statsService.CalculateTotalStats(_equippedGear, database);
        }

        /// <summary>
        /// Get currently equipped weapon data
        /// </summary>
        public WeaponSO GetEquippedWeapon()
        {
            return _statsService.GetEquippedWeapon(_equippedGear, database);
        }

        #endregion

        #region Save/Load

        public void LoadEquipment(EquipmentSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning("[EquipmentManager] Cannot load null save data");
                return;
            }

            // Just populate slots - inventory locations already set by InventoryManager
            _equippedGear.weaponId = saveData.weaponId ?? string.Empty;
            _equippedGear.helmetId = saveData.helmetId ?? string.Empty;
            _equippedGear.chestId = saveData.chestId ?? string.Empty;
            _equippedGear.glovesId = saveData.glovesId ?? string.Empty;
            _equippedGear.bootsId = saveData.bootsId ?? string.Empty;
            _equippedGear.accessory1Id = saveData.accessory1Id ?? string.Empty;
            _equippedGear.accessory2Id = saveData.accessory2Id ?? string.Empty;

            UpdateCharacterStats();
            GameEvents.TriggerEquipmentChanged();

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

        #endregion

        #region Private Methods

        private void UpdateCharacterStats()
        {
            _characterManager?.UpdateStatsFromEquipment();
        }

        #endregion

        #region Debug Tools

        [ContextMenu("Debug: Print Equipment")]
        private void DebugPrintEquipment()
        {
            Debug.Log("=== EQUIPPED GEAR ===");
            Debug.Log($"Weapon: {_equippedGear.weaponId}");
            Debug.Log($"Helmet: {_equippedGear.helmetId}");
            Debug.Log($"Chest: {_equippedGear.chestId}");
            Debug.Log($"Gloves: {_equippedGear.glovesId}");
            Debug.Log($"Boots: {_equippedGear.bootsId}");
            Debug.Log($"Accessory 1: {_equippedGear.accessory1Id}");
            Debug.Log($"Accessory 2: {_equippedGear.accessory2Id}");
            Debug.Log($"Total items: {_equippedGear.GetEquippedCount()}");
        }

        [ContextMenu("Debug: Unequip All")]
        private void DebugUnequipAll() => UnequipAll();

        #endregion
    }
}