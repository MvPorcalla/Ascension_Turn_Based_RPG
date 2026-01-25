// ══════════════════════════════════════════════════════════════════
// Assets/Scripts/Modules/InventorySystem/Data/InventoryCoreData.cs
// ✅ CLEANED: Removed unused static helper method
// ══════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using Ascension.Inventory.Config;

namespace Ascension.Inventory.Data
{
    /// <summary>
    /// Serializable data container for inventory save/load operations.
    /// Used by SaveManager to persist inventory state.
    /// </summary>
    [Serializable]
    public class InventoryCoreData
    {
        /// <summary>All items across all locations</summary>
        public List<ItemInstance> items = new List<ItemInstance>();
        
        /// <summary>Current bag capacity</summary>
        public int maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS;
        
        /// <summary>Current storage capacity</summary>
        public int maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS;
    }
}