// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Services\GearSlotService.cs
// Service for validating gear slot compatibility
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Equipment.Enums;

namespace Ascension.Equipment.Services
{
    /// <summary>
    /// Service responsible for validating gear slot compatibility
    /// Stateless, reusable logic
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
        /// </summary>
        public GearSlotType? GetSlotForItem(ItemBaseSO item)
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
                    GearType.Accessory => GearSlotType.Accessory1, // Default to first accessory slot
                    _ => null
                };
            }

            return null;
        }

        /// <summary>
        /// Get storage filter for a specific gear slot
        /// </summary>
        public EquipmentStorageFilter GetFilterForSlot(GearSlotType slotType)
        {
            return slotType switch
            {
                GearSlotType.Weapon => EquipmentStorageFilter.Weapons,
                GearSlotType.Helmet => EquipmentStorageFilter.Helmets,
                GearSlotType.Chest => EquipmentStorageFilter.Chests,
                GearSlotType.Gloves => EquipmentStorageFilter.Gloves,
                GearSlotType.Boots => EquipmentStorageFilter.Boots,
                GearSlotType.Accessory1 => EquipmentStorageFilter.Accessories,
                GearSlotType.Accessory2 => EquipmentStorageFilter.Accessories,
                _ => EquipmentStorageFilter.All
            };
        }

        /// <summary>
        /// ✅ FIXED: Removed Consumables filter case
        /// Validate if item matches storage filter
        /// </summary>
        public bool MatchesFilter(ItemBaseSO item, EquipmentStorageFilter filter)
        {
            if (item == null)
                return false;

            return filter switch
            {
                EquipmentStorageFilter.All => item is WeaponSO || item is GearSO || item is AbilitySO,
                EquipmentStorageFilter.Weapons => item is WeaponSO,
                EquipmentStorageFilter.Helmets => item is GearSO gear && gear.GearType == GearType.Helmet,
                EquipmentStorageFilter.Chests => item is GearSO gear && gear.GearType == GearType.ChestPlate,
                EquipmentStorageFilter.Gloves => item is GearSO gear && gear.GearType == GearType.Gloves,
                EquipmentStorageFilter.Boots => item is GearSO gear && gear.GearType == GearType.Boots,
                EquipmentStorageFilter.Accessories => item is GearSO gear && gear.GearType == GearType.Accessory,
                EquipmentStorageFilter.Abilities => item is AbilitySO,
                _ => false
            };
        }
    }
}