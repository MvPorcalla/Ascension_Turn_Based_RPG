// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\Data\ItemInstance.cs
// Represents an instance of an item in inventory/storage
// ──────────────────────────────────────────────────

using System;

namespace Ascension.Inventory.Data
{
    [Serializable]
    public class ItemInstance
    {
        public string itemID;
        public int quantity;
        public bool isInBag;
        public bool isInPocket;

        public ItemInstance(string itemID, int quantity = 1, bool isInBag = false, bool isInPocket = false)
        {
            this.itemID = itemID;
            this.quantity = quantity;
            this.isInBag = isInBag;
            this.isInPocket = isInPocket;
        }

        public ItemInstance Clone()
        {
            return new ItemInstance(itemID, quantity, isInBag, isInPocket);
        }

        public string GetLocation()
        {
            if (isInBag) return "Bag";
            if (isInPocket) return "Pocket";
            return "Storage";
        }
    }
}