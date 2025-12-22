// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\Data\SaveDataMigration.cs
// ✅ CLEANED: Removed isEquipped validation
// ──────────────────────────────────────────────────

using UnityEngine;
using System.Collections.Generic;

namespace Ascension.Inventory.Data
{
    /// <summary>
    /// Utility class for migrating old save data to new format
    /// </summary>
    public static class SaveDataMigration
    {
        private const int DEFAULT_BAG_SLOTS = 12;
        private const int DEFAULT_POCKET_SLOTS = 6;
        private const int DEFAULT_STORAGE_SLOTS = 60;

        public static void MigrateInventoryData(BagInventoryData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[SaveDataMigration] Received null inventory data");
                return;
            }

            bool wasMigrated = false;

            // ═══════════════════════════════════════════════════════════
            // Step 1: Migrate slot capacities
            // ═══════════════════════════════════════════════════════════

            if (data.maxStorageSlots <= 0)
            {
                data.maxStorageSlots = DEFAULT_STORAGE_SLOTS;
                Debug.Log($"[SaveDataMigration] Added maxStorageSlots = {DEFAULT_STORAGE_SLOTS}");
                wasMigrated = true;
            }

            if (data.maxPocketSlots <= 0)
            {
                data.maxPocketSlots = DEFAULT_POCKET_SLOTS;
                Debug.Log($"[SaveDataMigration] Added maxPocketSlots = {DEFAULT_POCKET_SLOTS}");
                wasMigrated = true;
            }

            if (data.maxBagSlots <= 0)
            {
                data.maxBagSlots = DEFAULT_BAG_SLOTS;
                Debug.LogWarning("[SaveDataMigration] Fixed invalid maxBagSlots");
                wasMigrated = true;
            }

            // ═══════════════════════════════════════════════════════════
            // Step 2: Ensure items list is valid
            // ═══════════════════════════════════════════════════════════

            if (data.items == null)
            {
                data.items = new List<ItemInstance>();  // ✅ FIXED
                Debug.LogWarning("[SaveDataMigration] Fixed null items list");
                wasMigrated = true;
            }

            // ═══════════════════════════════════════════════════════════
            // Step 3: Migrate item location flags (OLD SAVES FIX)
            // ═══════════════════════════════════════════════════════════

            bool hasLocationFlags = false;
            foreach (var item in data.items)
            {
                if (item.isInBag || item.isInPocket)
                {
                    hasLocationFlags = true;
                    break;
                }
            }

            if (!hasLocationFlags && data.items.Count > 0)
            {
                Debug.LogWarning($"[SaveDataMigration] Old save detected - {data.items.Count} items have no location flags");
                Debug.LogWarning("[SaveDataMigration] All items will default to storage");
                wasMigrated = true;
            }

            // ═══════════════════════════════════════════════════════════
            // Step 4: Validation pass
            // ═══════════════════════════════════════════════════════════

            ValidateItemLocations(data.items);

            if (wasMigrated)
            {
                Debug.Log($"[SaveDataMigration] ✓ Migration complete");
                Debug.Log($"[SaveDataMigration]   Slots - Bag:{data.maxBagSlots} Pocket:{data.maxPocketSlots} Storage:{data.maxStorageSlots}");
                
                int bagCount = 0, pocketCount = 0, storageCount = 0;
                foreach (var item in data.items)
                {
                    if (item.isInBag) bagCount++;
                    else if (item.isInPocket) pocketCount++;
                    else storageCount++;
                }
                Debug.Log($"[SaveDataMigration]   Distribution - Bag:{bagCount} Pocket:{pocketCount} Storage:{storageCount}");
            }
        }

        /// <summary>
        /// ✅ CLEANED: Removed isEquipped validation
        /// </summary>
        private static void ValidateItemLocations(List<ItemInstance> items)
        {
            int fixedCount = 0;

            foreach (var item in items)
            {
                // Rule 1: An item can't be in both bag AND pocket
                if (item.isInBag && item.isInPocket)
                {
                    Debug.LogWarning($"[SaveDataMigration] Invalid: {item.itemID} in both bag and pocket - defaulting to bag");
                    item.isInPocket = false;
                    fixedCount++;
                }

                // Rule 2: Quantity must be positive
                if (item.quantity <= 0)
                {
                    Debug.LogError($"[SaveDataMigration] Invalid: {item.itemID} has quantity {item.quantity} - fixing to 1");
                    item.quantity = 1;
                    fixedCount++;
                }

                // Rule 3: ItemID must not be null/empty
                if (string.IsNullOrEmpty(item.itemID))
                {
                    Debug.LogError($"[SaveDataMigration] Invalid: Item has null/empty ID");
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
            {
                Debug.LogWarning($"[SaveDataMigration] Fixed {fixedCount} invalid item(s)");
            }
        }

        public static bool NeedsMigration(BagInventoryData data)
        {
            if (data == null) return false;
            
            return data.maxStorageSlots <= 0 || 
                   data.maxPocketSlots <= 0 || 
                   data.maxBagSlots <= 0;
        }

        public static bool HasLocationFlags(BagInventoryData data)
        {
            if (data == null || data.items == null || data.items.Count == 0)
                return true;

            foreach (var item in data.items)
            {
                if (item.isInBag || item.isInPocket)
                    return true;
            }

            return false;
        }

        public static string GetMigrationInfo(BagInventoryData data)
        {
            if (data == null)
                return "No data";

            bool needsSlotMigration = NeedsMigration(data);
            bool hasLocations = HasLocationFlags(data);

            if (!needsSlotMigration && hasLocations)
                return "No migration needed";

            string info = "Migration needed:\n";
            
            if (needsSlotMigration)
                info += "- Missing slot capacity fields\n";
            
            if (!hasLocations && data.items.Count > 0)
                info += "- Items missing location flags\n";

            return info;
        }
    }
}