// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\Data\InventoryCoreData.cs
// Serializable data class for saving/loading inventory
// ──────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using Ascension.Inventory.Config;

namespace Ascension.Inventory.Data
{
    [Serializable]
    public class InventoryCoreData
    {
        public List<ItemInstance> items = new List<ItemInstance>();
        public int maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS;
        public int maxPocketSlots = InventoryConfig.DEFAULT_POCKET_SLOTS; 
        public int maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS;

        /// <summary>
        /// ✅ ONLY ONE default constructor
        /// </summary>
        public InventoryCoreData()
        {
            items = new List<ItemInstance>();
            maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS;
            maxPocketSlots = InventoryConfig.DEFAULT_POCKET_SLOTS;
            maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS;
        }

        /// <summary>
        /// Constructor with custom slot capacities
        /// </summary>
        public InventoryCoreData(int bagSlots, int pocketSlots, int storageSlots)
        {
            items = new List<ItemInstance>();
            maxBagSlots = bagSlots;
            maxPocketSlots = pocketSlots;
            maxStorageSlots = storageSlots;
        }

        /// <summary>
        /// Create from InventoryManager
        /// </summary>
        public static InventoryCoreData FromInventoryManager(Manager.InventoryManager manager)
        {
            return new InventoryCoreData
            {
                items = new List<ItemInstance>(manager.Inventory.allItems),
                maxBagSlots = manager.Capacity.MaxBagSlots,
                maxPocketSlots = manager.Capacity.MaxPocketSlots,
                maxStorageSlots = manager.Capacity.MaxStorageSlots
            };
        }
    }
}