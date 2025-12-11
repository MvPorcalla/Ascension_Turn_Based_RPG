// ──────────────────────────────────────────────────
// BagInventoryData.cs
// Serializable data class for saving/loading BagInventory
// ──────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;

namespace Ascension.Inventory.Data
{
    [Serializable]
    public class BagInventoryData
    {
        public List<ItemInstance> items = new List<ItemInstance>();
        public int maxBagSlots = 12;
        public int maxPocketSlots = 6;

        /// <summary>
        /// Create from BagInventory
        /// </summary>
        public static BagInventoryData FromInventory(BagInventory inventory)
        {
            return new BagInventoryData
            {
                items = new List<ItemInstance>(inventory.allItems),
                maxBagSlots = inventory.maxBagSlots,
                maxPocketSlots = inventory.maxPocketSlots
            };
        }
    }
}