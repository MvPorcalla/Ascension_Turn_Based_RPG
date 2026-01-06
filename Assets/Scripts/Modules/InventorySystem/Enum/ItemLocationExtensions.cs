// ══════════════════════════════════════════════════════════════════
// Assets\Scripts\Modules\InventorySystem\Enums\ItemLocationExtensions.cs
// Extension methods for ItemLocation enum
// ══════════════════════════════════════════════════════════════════

namespace Ascension.Inventory.Enums
{
    public static class ItemLocationExtensions
    {
        /// <summary>
        /// Convert ItemLocation enum to user-friendly display name
        /// </summary>
        public static string ToDisplayName(this ItemLocation location)
        {
            return location switch
            {
                ItemLocation.Bag => "Bag",
                ItemLocation.Storage => "Storage",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Convert ItemLocation enum to icon name (if using sprite atlas)
        /// </summary>
        public static string ToIconName(this ItemLocation location)
        {
            return location switch
            {
                ItemLocation.Bag => "icon_bag",
                ItemLocation.Storage => "icon_storage",
                _ => "icon_unknown"
            };
        }

        /// <summary>
        /// Get color hex for UI
        /// </summary>
        public static string ToColorHex(this ItemLocation location)
        {
            return location switch
            {
                ItemLocation.Bag => "#4A90E2",      // Blue
                ItemLocation.Storage => "#F5A623",  // Orange
                _ => "#9B9B9B"                      // Gray
            };
        }
    }
}