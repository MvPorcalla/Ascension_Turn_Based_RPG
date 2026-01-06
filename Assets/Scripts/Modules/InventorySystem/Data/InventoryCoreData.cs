// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\Data\InventoryCoreData.cs
// Serializable data container for inventory save/load operations.
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
        public int maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS;

        /// <summary>
        /// Create from InventoryManager runtime state
        /// </summary>
        public static InventoryCoreData FromInventoryManager(Manager.InventoryManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager), "InventoryManager cannot be null");

            return new InventoryCoreData
            {
                items = new List<ItemInstance>(manager.Inventory.allItems),
                maxBagSlots = manager.Capacity.MaxBagSlots,
                maxStorageSlots = manager.Capacity.MaxStorageSlots
            };
        }
    }
}