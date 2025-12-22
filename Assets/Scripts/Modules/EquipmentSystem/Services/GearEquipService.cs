// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Services\GearEquipService.cs
// Service for equip/unequip logic with inventory integration
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Database;
using Ascension.Equipment.Data;
using Ascension.Equipment.Enums;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;

namespace Ascension.Equipment.Services
{
    /// <summary>
    /// Service responsible for equip/unequip logic
    /// Handles inventory integration and item swapping
    /// </summary>
    public class GearEquipService
    {
        private readonly GearSlotService _slotService;

        public GearEquipService(GearSlotService slotService)
        {
            _slotService = slotService;
        }

        /// <summary>
        /// Equip an item from inventory
        /// Returns the item ID that was unequipped (if any)
        /// </summary>
        public string EquipItem(
            EquippedGear equippedGear,
            string itemId,
            GearSlotType slotType,
            GameDatabaseSO database)
        {
            // Validate item exists
            ItemBaseSO item = database.GetItem(itemId);
            if (item == null)
            {
                Debug.LogError($"[GearEquipService] Item not found: {itemId}");
                return null;
            }

            // Validate slot compatibility
            if (!_slotService.CanEquipInSlot(item, slotType))
            {
                Debug.LogWarning($"[GearEquipService] Cannot equip {item.ItemName} in {slotType} slot");
                return null;
            }

            // Get currently equipped item in this slot (if any)
            string previousItemId = equippedGear.GetSlot(slotType);

            // Equip new item
            equippedGear.SetSlot(slotType, itemId);

            Debug.Log($"[GearEquipService] Equipped {item.ItemName} to {slotType} slot");

            return previousItemId;
        }

        /// <summary>
        /// Unequip an item from a slot
        /// </summary>
        public bool UnequipSlot(EquippedGear equippedGear, GearSlotType slotType)
        {
            string itemId = equippedGear.GetSlot(slotType);

            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning($"[GearEquipService] Slot {slotType} is already empty");
                return false;
            }

            // Clear slot
            equippedGear.ClearSlot(slotType);

            Debug.Log($"[GearEquipService] Unequipped item from {slotType} slot");
            return true;
        }

        /// <summary>
        /// Swap items between two accessory slots
        /// </summary>
        public bool SwapAccessorySlots(EquippedGear equippedGear)
        {
            string accessory1 = equippedGear.accessory1Id;
            string accessory2 = equippedGear.accessory2Id;

            equippedGear.accessory1Id = accessory2;
            equippedGear.accessory2Id = accessory1;

            Debug.Log("[GearEquipService] Swapped accessory slots");
            return true;
        }

        /// <summary>
        /// Check if an item is currently equipped
        /// </summary>
        public bool IsItemEquipped(EquippedGear equippedGear, string itemId)
        {
            return equippedGear.IsEquipped(itemId);
        }

    }
}