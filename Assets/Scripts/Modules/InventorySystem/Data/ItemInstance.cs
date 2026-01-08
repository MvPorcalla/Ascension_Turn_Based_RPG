// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\Data\ItemInstance.cs
// Serializable instance of an inventory item
// ──────────────────────────────────────────────────

using System;
using Ascension.Inventory.Enums;

namespace Ascension.Inventory.Data
{
    [Serializable]
    public class ItemInstance
    {
        public string itemID;
        public int quantity;
        public ItemLocation location;

        // ✅ NEW: Remember where item came from before equipping
        public ItemLocation? previousLocation;

        // ═══════════════════════════════════════════════════════════
        // Constructor
        // ═══════════════════════════════════════════════════════════

        public ItemInstance(string itemID, int quantity = 1, ItemLocation location = ItemLocation.Storage)
        {
            this.itemID = itemID;
            this.quantity = quantity;
            this.location = location;
            this.previousLocation = null;
        }

        // ═══════════════════════════════════════════════════════════
        // Utility Methods
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Creates a deep copy of this item instance
        /// </summary>
        public ItemInstance Clone()
        {
            return new ItemInstance(itemID, quantity, location)
            {
                previousLocation = this.previousLocation
            };
        }
    }
}