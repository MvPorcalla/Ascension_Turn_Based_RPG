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
        /// <example>
        /// string displayName = item.location.ToDisplayName();  // "Bag"
        /// </example>
        public static string ToDisplayName(this ItemLocation location)
        {
            return location switch
            {
                ItemLocation.Bag => "Bag",
                ItemLocation.Pocket => "Pocket",
                ItemLocation.Storage => "Storage",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Convert ItemLocation enum to icon name (if using sprite atlas)
        /// </summary>
        /// <example>
        /// string iconName = item.location.ToIconName();  // "icon_bag"
        /// </example>
        public static string ToIconName(this ItemLocation location)
        {
            return location switch
            {
                ItemLocation.Bag => "icon_bag",
                ItemLocation.Pocket => "icon_pocket",
                ItemLocation.Storage => "icon_storage",
                _ => "icon_unknown"
            };
        }

        /// <summary>
        /// Get color hex for UI (optional - customize as needed)
        /// </summary>
        public static string ToColorHex(this ItemLocation location)
        {
            return location switch
            {
                ItemLocation.Bag => "#4A90E2",      // Blue
                ItemLocation.Pocket => "#7ED321",   // Green
                ItemLocation.Storage => "#F5A623",  // Orange
                _ => "#9B9B9B"                      // Gray
            };
        }
    }
}