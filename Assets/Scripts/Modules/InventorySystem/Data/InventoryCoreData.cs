// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\Data\InventoryCoreData.cs
// Serializable data class for saving/loading inventory
// ──────────────────────────────────────────────────

using System;
using System.Collections.Generic;

namespace Ascension.Inventory.Data
{
    [Serializable]
    public class InventoryCoreData
    {
        public List<ItemInstance> items = new List<ItemInstance>();
        public int maxBagSlots = 12;
        public int maxPocketSlots = 6;
        public int maxStorageSlots = 60;

        /// <summary>
        /// ✅ FIXED: Create from InventoryManager (not InventoryCore)
        /// </summary>
        public static InventoryCoreData FromInventoryManager(Manager.InventoryManager manager)
        {
            return new InventoryCoreData
            {
                items = new List<ItemInstance>(manager.Inventory.allItems),
                maxBagSlots = manager.Capacity.MaxBagSlots,      // ✅ FIXED
                maxPocketSlots = manager.Capacity.MaxPocketSlots, // ✅ FIXED
                maxStorageSlots = manager.Capacity.MaxStorageSlots // ✅ FIXED
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