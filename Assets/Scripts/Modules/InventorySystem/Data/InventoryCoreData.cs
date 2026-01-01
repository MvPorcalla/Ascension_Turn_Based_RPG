// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\Data\InventoryCoreData.cs
// Serializable data container for inventory save/load operations.
// ──────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using Ascension.Inventory.Config;

namespace Ascension.Inventory.Data
{
    /// <summary>
    /// Serializable data container for inventory save/load operations.
    /// Use FromInventoryManager() to create from runtime state.
    /// </summary>
    [Serializable]
    public class InventoryCoreData
    {
        // ✅ Field initializers handle default values
        public List<ItemInstance> items = new List<ItemInstance>();
        public int maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS;
        public int maxPocketSlots = InventoryConfig.DEFAULT_POCKET_SLOTS; 
        public int maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS;

        // ✅ No constructor needed! Field initializers do the work.
        // The compiler generates an implicit parameterless constructor.

        /// <summary>
        /// ✅ PRIMARY API: Create from InventoryManager runtime state
        /// </summary>
        /// <example>
        /// var saveData = InventoryCoreData.FromInventoryManager(inventoryManager);
        /// </example>
        public static InventoryCoreData FromInventoryManager(Manager.InventoryManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager), "InventoryManager cannot be null");

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