// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Data\EquippedGear.cs
// Pure data class for currently equipped gear
// ════════════════════════════════════════════

using System;
using Ascension.Equipment.Enums;

namespace Ascension.Equipment.Data
{
    /// <summary>
    /// Represents the player's currently equipped gear
    /// Pure data structure - no logic
    /// </summary>
    [Serializable]
    public class EquippedGear
    {
        // Gear slots
        public string weaponId = string.Empty;
        public string helmetId = string.Empty;
        public string chestId = string.Empty;
        public string glovesId = string.Empty;
        public string bootsId = string.Empty;
        public string accessory1Id = string.Empty;
        public string accessory2Id = string.Empty;

        /// <summary>
        /// Get item ID for a specific slot
        /// </summary>
        public string GetSlot(GearSlotType slotType)
        {
            return slotType switch
            {
                GearSlotType.Weapon => weaponId,
                GearSlotType.Helmet => helmetId,
                GearSlotType.Chest => chestId,
                GearSlotType.Gloves => glovesId,
                GearSlotType.Boots => bootsId,
                GearSlotType.Accessory1 => accessory1Id,
                GearSlotType.Accessory2 => accessory2Id,
                _ => string.Empty
            };
        }

        /// <summary>
        /// Set item ID for a specific slot
        /// </summary>
        public void SetSlot(GearSlotType slotType, string itemId)
        {
            switch (slotType)
            {
                case GearSlotType.Weapon:
                    weaponId = itemId;
                    break;
                case GearSlotType.Helmet:
                    helmetId = itemId;
                    break;
                case GearSlotType.Chest:
                    chestId = itemId;
                    break;
                case GearSlotType.Gloves:
                    glovesId = itemId;
                    break;
                case GearSlotType.Boots:
                    bootsId = itemId;
                    break;
                case GearSlotType.Accessory1:
                    accessory1Id = itemId;
                    break;
                case GearSlotType.Accessory2:
                    accessory2Id = itemId;
                    break;
            }
        }

        /// <summary>
        /// Check if a slot is empty
        /// </summary>
        public bool IsSlotEmpty(GearSlotType slotType)
        {
            return string.IsNullOrEmpty(GetSlot(slotType));
        }

        /// <summary>
        /// Check if an item is currently equipped
        /// </summary>
        public bool IsEquipped(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return false;

            return weaponId == itemId ||
                   helmetId == itemId ||
                   chestId == itemId ||
                   glovesId == itemId ||
                   bootsId == itemId ||
                   accessory1Id == itemId ||
                   accessory2Id == itemId;
        }

        /// <summary>
        /// Clear a specific slot
        /// </summary>
        public void ClearSlot(GearSlotType slotType)
        {
            SetSlot(slotType, string.Empty);
        }

        /// <summary>
        /// Clear all slots
        /// </summary>
        public void ClearAll()
        {
            weaponId = string.Empty;
            helmetId = string.Empty;
            chestId = string.Empty;
            glovesId = string.Empty;
            bootsId = string.Empty;
            accessory1Id = string.Empty;
            accessory2Id = string.Empty;
        }

        /// <summary>
        /// Get count of equipped items
        /// </summary>
        public int GetEquippedCount()
        {
            int count = 0;
            if (!string.IsNullOrEmpty(weaponId)) count++;
            if (!string.IsNullOrEmpty(helmetId)) count++;
            if (!string.IsNullOrEmpty(chestId)) count++;
            if (!string.IsNullOrEmpty(glovesId)) count++;
            if (!string.IsNullOrEmpty(bootsId)) count++;
            if (!string.IsNullOrEmpty(accessory1Id)) count++;
            if (!string.IsNullOrEmpty(accessory2Id)) count++;
            return count;
        }
    }
}