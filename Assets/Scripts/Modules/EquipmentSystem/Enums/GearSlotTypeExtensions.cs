// ══════════════════════════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Enums\GearSlotTypeExtensions.cs
// ✅ NEW: Extension methods for GearSlotType enum (display helpers)
// ══════════════════════════════════════════════════════════════════

namespace Ascension.Equipment.Enums
{
    public static class GearSlotTypeExtensions
    {
        /// <summary>
        /// Convert GearSlotType to user-friendly display name
        /// </summary>
        public static string ToDisplayName(this GearSlotType slotType)
        {
            return slotType switch
            {
                GearSlotType.Weapon => "Weapon",
                GearSlotType.Helmet => "Helmet",
                GearSlotType.Chest => "Chest",
                GearSlotType.Gloves => "Gloves",
                GearSlotType.Boots => "Boots",
                GearSlotType.Accessory1 => "Accessory 1",
                GearSlotType.Accessory2 => "Accessory 2",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Get icon name for sprite atlas (optional)
        /// </summary>
        public static string ToIconName(this GearSlotType slotType)
        {
            return slotType switch
            {
                GearSlotType.Weapon => "slot_weapon",
                GearSlotType.Helmet => "slot_helmet",
                GearSlotType.Chest => "slot_chest",
                GearSlotType.Gloves => "slot_gloves",
                GearSlotType.Boots => "slot_boots",
                GearSlotType.Accessory1 => "slot_accessory",
                GearSlotType.Accessory2 => "slot_accessory",
                _ => "slot_empty"
            };
        }

        /// <summary>
        /// Get short label for compact UI (2-3 chars)
        /// </summary>
        public static string ToShortLabel(this GearSlotType slotType)
        {
            return slotType switch
            {
                GearSlotType.Weapon => "WPN",
                GearSlotType.Helmet => "HLM",
                GearSlotType.Chest => "CHT",
                GearSlotType.Gloves => "GLV",
                GearSlotType.Boots => "BTS",
                GearSlotType.Accessory1 => "AC1",
                GearSlotType.Accessory2 => "AC2",
                _ => "???"
            };
        }
    }
}