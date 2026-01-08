// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Services\GearEquipCoordinator.cs
// ✅ REFACTORED: Pure coordination service (no direct inventory mutations)
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Database;
using Ascension.Equipment.Data;
using Ascension.Equipment.Enums;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;
using Ascension.Equipment.Services;


namespace Ascension.Equipment.Coordinators
{
    /// <summary>
    /// Result type for equipment operations
    /// </summary>
    public class EquipResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string UnequippedItemId { get; set; }

        public static EquipResult Ok(string message, string unequippedItemId = null) => 
            new EquipResult { Success = true, Message = message, UnequippedItemId = unequippedItemId };
        
        public static EquipResult Fail(string message) => 
            new EquipResult { Success = false, Message = message };
    }

    /// <summary>
    /// ✅ REFACTORED: Coordinates equipment operations via InventoryManager API
    /// NO direct ItemInstance mutations - delegates to InventoryManager
    /// </summary>
    public class GearEquipCoordinator
    {
        private readonly GearSlotService _slotService;

        public GearEquipCoordinator(GearSlotService slotService)
        {
            _slotService = slotService;
        }

        #region Public API - Equipment Coordination

        /// <summary>
        /// ✅ REFACTORED: Equip item via InventoryManager API
        /// - Validates item and slot
        /// - Uses InventoryManager.MoveToEquipped() (no direct mutations)
        /// - Handles slot swapping automatically
        /// </summary>
        public EquipResult EquipFromInventory(
            EquippedGear equippedGear,
            string itemId,
            InventoryManager inventory,
            GameDatabaseSO database,
            GearSlotType? targetSlot = null)
        {
            // 1. Validate managers
            if (inventory == null)
                return EquipResult.Fail("Inventory system not available");

            if (database == null)
                return EquipResult.Fail("Database not available");

            // 2. Get item data
            ItemBaseSO itemData = database.GetItem(itemId);
            if (itemData == null)
                return EquipResult.Fail($"Item not found: {itemId}");

            // 3. Check if item can be equipped
            if (!inventory.CanEquipItem(itemId))
                return EquipResult.Fail($"{itemData.ItemName} cannot be equipped (not in inventory or already equipped)");

            // 4. Auto-detect slot if not specified
            GearSlotType slotToUse;
            if (targetSlot.HasValue)
            {
                slotToUse = targetSlot.Value;
            }
            else
            {
                GearSlotType? autoSlot = _slotService.GetSlotForItem(itemData, equippedGear);
                if (!autoSlot.HasValue)
                    return EquipResult.Fail($"Cannot determine slot for {itemData.ItemName}");
                
                slotToUse = autoSlot.Value;
            }

            // 5. Validate slot compatibility
            if (!_slotService.CanEquipInSlot(itemData, slotToUse))
                return EquipResult.Fail($"Cannot equip {itemData.ItemName} in {slotToUse} slot");

            // 6. Handle currently equipped item (if any)
            string previousItemId = equippedGear.GetSlot(slotToUse);
            if (!string.IsNullOrEmpty(previousItemId))
            {
                // Unequip old item first
                var unequipResult = UnequipViaInventory(
                    equippedGear,
                    slotToUse,
                    inventory,
                    database
                );

                if (!unequipResult.Success)
                    return EquipResult.Fail($"Cannot unequip current item: {unequipResult.Message}");
            }

            // 7. ✅ Move item to Equipped via InventoryManager API (no direct mutation)
            var moveResult = inventory.MoveToEquipped(itemId);
            if (!moveResult.Success)
                return EquipResult.Fail($"Failed to equip: {moveResult.Message}");

            // 8. Update equipment slot
            equippedGear.SetSlot(slotToUse, itemId);

            Debug.Log($"[GearEquipCoordinator] Equipped {itemData.ItemName} to {slotToUse}");
            return EquipResult.Ok(
                $"Equipped {itemData.ItemName}", 
                previousItemId
            );
        }

        /// <summary>
        /// ✅ REFACTORED: Unequip item via InventoryManager API
        /// - Removes item from equipment slot
        /// - Uses InventoryManager.MoveFromEquipped() (restores original location)
        /// </summary>
        public EquipResult UnequipViaInventory(
            EquippedGear equippedGear,
            GearSlotType slotType,
            InventoryManager inventory,
            GameDatabaseSO database)
        {
            // 1. Validate slot has item
            string itemId = equippedGear.GetSlot(slotType);
            if (string.IsNullOrEmpty(itemId))
                return EquipResult.Fail($"{slotType} slot is already empty");

            if (inventory == null)
                return EquipResult.Fail("Inventory system not available");

            if (database == null)
                return EquipResult.Fail("Database not available");

            // 2. Get item data (for display name)
            ItemBaseSO itemData = database.GetItem(itemId);
            if (itemData == null)
                return EquipResult.Fail($"Item data not found: {itemId}");

            // 3. ✅ Move item from Equipped via InventoryManager API (no direct mutation)
            var moveResult = inventory.MoveFromEquipped(itemId);
            if (!moveResult.Success)
                return EquipResult.Fail($"Failed to unequip: {moveResult.Message}");

            // 4. Clear equipment slot
            equippedGear.ClearSlot(slotType);

            Debug.Log($"[GearEquipCoordinator] Unequipped {itemData.ItemName} from {slotType}");
            return EquipResult.Ok($"Unequipped {itemData.ItemName}");
        }

        /// <summary>
        /// Find which slot an item is equipped in
        /// </summary>
        public GearSlotType? FindEquippedSlot(EquippedGear equippedGear, string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return null;

            if (equippedGear.weaponId == itemId) return GearSlotType.Weapon;
            if (equippedGear.helmetId == itemId) return GearSlotType.Helmet;
            if (equippedGear.chestId == itemId) return GearSlotType.Chest;
            if (equippedGear.glovesId == itemId) return GearSlotType.Gloves;
            if (equippedGear.bootsId == itemId) return GearSlotType.Boots;
            if (equippedGear.accessory1Id == itemId) return GearSlotType.Accessory1;
            if (equippedGear.accessory2Id == itemId) return GearSlotType.Accessory2;

            return null;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Swap items between two accessory slots
        /// </summary>
        public bool SwapAccessorySlots(EquippedGear equippedGear)
        {
            string accessory1 = equippedGear.accessory1Id;
            string accessory2 = equippedGear.accessory2Id;

            equippedGear.accessory1Id = accessory2;
            equippedGear.accessory2Id = accessory1;

            return true;
        }

        /// <summary>
        /// Check if an item is currently equipped
        /// </summary>
        public bool IsItemEquipped(EquippedGear equippedGear, string itemId)
        {
            return equippedGear.IsEquipped(itemId);
        }

        #endregion
    }
}