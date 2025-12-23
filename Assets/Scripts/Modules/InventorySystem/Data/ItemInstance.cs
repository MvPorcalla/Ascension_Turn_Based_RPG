// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\Data\ItemInstance.cs
// Item instance data structure with enum-based location
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

        // ═══════════════════════════════════════════════════════════
        // Constructors
        // ═══════════════════════════════════════════════════════════

        public ItemInstance(string itemID, int quantity = 1, ItemLocation location = ItemLocation.Storage)
        {
            this.itemID = itemID;
            this.quantity = quantity;
            this.location = location;
        }

        // ═══════════════════════════════════════════════════════════
        // Utility Methods
        // ═══════════════════════════════════════════════════════════

        public ItemInstance Clone()
        {
            return new ItemInstance(itemID, quantity, location);
        }

        public string GetLocation()
        {
            return location switch
            {
                ItemLocation.Bag => "Bag",
                ItemLocation.Pocket => "Pocket",
                ItemLocation.Storage => "Storage",
                _ => "Unknown"
            };
        }

        public void SetLocation(ItemLocation newLocation)
        {
            location = newLocation;
        }

        public bool IsInLocation(ItemLocation checkLocation)
        {
            return location == checkLocation;
        }
    }
}