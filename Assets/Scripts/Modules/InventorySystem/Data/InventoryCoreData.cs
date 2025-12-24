// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\Data\InventoryCoreData.cs
// Serializable data class for saving/loading BagInventory
// ──────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;

namespace Ascension.Inventory.Data
{
    [Serializable]
    public class InventoryCoreData
    {
        public List<ItemInstance> items = new List<ItemInstance>();
        public int maxBagSlots = 12;
        public int maxPocketSlots = 6;
        public int maxStorageSlots = 60; // ✅ NEW: Storage capacity

        /// <summary>
        /// ✅ UPDATED: Create from BagInventory (includes storage slots)
        /// </summary>
        public static InventoryCoreData FromInventory(InventoryCore inventory)
        {
            return new InventoryCoreData
            {
                items = new List<ItemInstance>(inventory.allItems),
                maxBagSlots = inventory.maxBagSlots,
                maxPocketSlots = inventory.maxPocketSlots,
                maxStorageSlots = inventory.maxStorageSlots // ✅ NEW
            };
        }

        /// <summary>
        /// ✅ NEW: Default constructor with safe defaults
        /// </summary>
        public InventoryCoreData()
        {
            items = new List<ItemInstance>();
            maxBagSlots = 12;
            maxPocketSlots = 6;
            maxStorageSlots = 60;
        }

        /// <summary>
        /// ✅ NEW: Constructor with custom slot capacities
        /// </summary>
        public InventoryCoreData(int bagSlots, int pocketSlots, int storageSlots)
        {
            items = new List<ItemInstance>();
            maxBagSlots = bagSlots;
            maxPocketSlots = pocketSlots;
            maxStorageSlots = storageSlots;
        }
    }
}