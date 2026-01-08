// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Services\GearSlotService.cs
// ✅ CLEANED: Removed UI filter logic (moved to StorageSystem)
// Service for validating gear slot compatibility
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Equipment.Enums;
using Ascension.Equipment.Data;

namespace Ascension.Equipment.Services
{
    /// <summary>
    /// Service responsible for validating gear slot compatibility
    /// Stateless, reusable logic - ZERO UI concerns
    /// </summary>
    public class GearSlotService
    {
        /// <summary>
        /// Check if an item can be equipped in a specific slot
        /// </summary>
        public bool CanEquipInSlot(ItemBaseSO item, GearSlotType slotType)
        {
            if (item == null)
            {
                Debug.LogWarning("[GearSlotService] Item is null");
                return false;
            }

            // Weapons can only go in weapon slot
            if (item is WeaponSO)
            {
                return slotType == GearSlotType.Weapon;
            }

            // Gear pieces must match their slot type
            if (item is GearSO gear)
            {
                return slotType switch
                {
                    GearSlotType.Helmet => gear.GearType == GearType.Helmet,
                    GearSlotType.Chest => gear.GearType == GearType.ChestPlate,
                    GearSlotType.Gloves => gear.GearType == GearType.Gloves,
                    GearSlotType.Boots => gear.GearType == GearType.Boots,
                    GearSlotType.Accessory1 => gear.GearType == GearType.Accessory,
                    GearSlotType.Accessory2 => gear.GearType == GearType.Accessory,
                    _ => false
                };
            }

            Debug.LogWarning($"[GearSlotService] {item.ItemName} is not equippable gear");
            return false;
        }

        /// <summary>
        /// Get the appropriate slot type for an item
        /// Returns null if item cannot be equipped
        /// </summary>
        public GearSlotType? GetSlotForItem(ItemBaseSO item, EquippedGear equippedGear = null)
        {
            if (item == null)
                return null;

            if (item is WeaponSO)
                return GearSlotType.Weapon;

            if (item is GearSO gear)
            {
                return gear.GearType switch
                {
                    GearType.Helmet => GearSlotType.Helmet,
                    GearType.ChestPlate => GearSlotType.Chest,
                    GearType.Gloves => GearSlotType.Gloves,
                    GearType.Boots => GearSlotType.Boots,
                    GearType.Accessory => GetAvailableAccessorySlot(equippedGear),
                    _ => null
                };
            }

            return null;
        }

        /// <summary>
        /// ✅ NEW: Get the first available accessory slot (or Accessory1 if both full)
        /// </summary>
        private GearSlotType GetAvailableAccessorySlot(EquippedGear equippedGear)
        {
            if (equippedGear == null)
                return GearSlotType.Accessory1; // Default to first slot

            // Check if Accessory1 is empty
            if (equippedGear.IsSlotEmpty(GearSlotType.Accessory1))
                return GearSlotType.Accessory1;

            // Check if Accessory2 is empty
            if (equippedGear.IsSlotEmpty(GearSlotType.Accessory2))
                return GearSlotType.Accessory2;

            // Both full - default to Accessory1 (will trigger swap)
            return GearSlotType.Accessory1;
        }

        /// <summary>
        /// Check if an item is equippable gear/weapon
        /// </summary>
        public bool IsEquippable(ItemBaseSO item)
        {
            return item is WeaponSO || item is GearSO;
        }

        /// <summary>
        /// Validate slot compatibility between two items (for swapping)
        /// </summary>
        public bool CanSwapItems(ItemBaseSO item1, GearSlotType slot1, ItemBaseSO item2, GearSlotType slot2)
        {
            return CanEquipInSlot(item1, slot2) && CanEquipInSlot(item2, slot1);
        }
    }
}