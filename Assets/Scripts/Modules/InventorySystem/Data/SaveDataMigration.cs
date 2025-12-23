// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\Data\SaveDataMigration.cs
// ✅ MIGRATED: Boolean → Enum migration support
// ──────────────────────────────────────────────────

using UnityEngine;
using System.Collections.Generic;
using Ascension.Inventory.Enums;

namespace Ascension.Inventory.Data
{
    /// <summary>
    /// Utility class for migrating old save data to new format
    /// ✅ NOW SUPPORTS: Boolean flags → ItemLocation enum migration
    /// </summary>
    public static class SaveDataMigration
    {
        private const int DEFAULT_BAG_SLOTS = 12;
        private const int DEFAULT_POCKET_SLOTS = 6;
        private const int DEFAULT_STORAGE_SLOTS = 60;

        /// <summary>
        /// ✅ MIGRATED: Main migration entry point
        /// Now converts old boolean-based items to enum-based
        /// </summary>
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
                data.items = new List<ItemInstance>();
                Debug.LogWarning("[SaveDataMigration] Fixed null items list");
                wasMigrated = true;
            }

            // ═══════════════════════════════════════════════════════════
            // Step 3: ✅ NEW - Validate all items have correct enum location
            // ═══════════════════════════════════════════════════════════

            ValidateItemLocations(data.items);

            // ═══════════════════════════════════════════════════════════
            // Final Report
            // ═══════════════════════════════════════════════════════════

            if (wasMigrated)
            {
                Debug.Log($"[SaveDataMigration] ✓ Migration complete");
                Debug.Log($"[SaveDataMigration]   Slots - Bag:{data.maxBagSlots} Pocket:{data.maxPocketSlots} Storage:{data.maxStorageSlots}");
                
                int bagCount = 0, pocketCount = 0, storageCount = 0;
                foreach (var item in data.items)
                {
                    switch (item.location)
                    {
                        case ItemLocation.Bag: bagCount++; break;
                        case ItemLocation.Pocket: pocketCount++; break;
                        case ItemLocation.Storage: storageCount++; break;
                    }
                }
                Debug.Log($"[SaveDataMigration]   Distribution - Bag:{bagCount} Pocket:{pocketCount} Storage:{storageCount}");
            }
        }

        /// <summary>
        /// ✅ MIGRATED: Validate item locations (now uses enum)
        /// </summary>
        private static void ValidateItemLocations(List<ItemInstance> items)
        {
            int fixedCount = 0;

            foreach (var item in items)
            {
                // ✅ Enum validation: Ensure location is valid
                if (!System.Enum.IsDefined(typeof(ItemLocation), item.location))
                {
                    Debug.LogWarning($"[SaveDataMigration] Invalid location for {item.itemID}: {item.location} - defaulting to Storage");
                    item.location = ItemLocation.Storage;
                    fixedCount++;
                }

                // Rule 1: Quantity must be positive
                if (item.quantity <= 0)
                {
                    Debug.LogError($"[SaveDataMigration] Invalid: {item.itemID} has quantity {item.quantity} - fixing to 1");
                    item.quantity = 1;
                    fixedCount++;
                }

                // Rule 2: ItemID must not be null/empty
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

        // ═══════════════════════════════════════════════════════════
        // ✅ NEW: JSON Migration Helper (for manual JSON conversion)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// ✅ UTILITY: Manually migrate JSON save file from boolean → enum
        /// Call this ONCE to convert your existing save file
        /// </summary>
        [System.Obsolete("Only use this once to migrate old JSON save files")]
        public static void MigrateJsonSaveFile()
        {
            string savePath = System.IO.Path.Combine(
                UnityEngine.Application.persistentDataPath,
                "Saves/CharacterData/player_data.json"
            );

            if (!System.IO.File.Exists(savePath))
            {
                Debug.LogError($"[SaveDataMigration] Save file not found at: {savePath}");
                return;
            }

            try
            {
                // Read old JSON
                string json = System.IO.File.ReadAllText(savePath);
                
                // Parse as OldSaveData (with boolean flags)
                var oldData = JsonUtility.FromJson<OldSaveDataFormat>(json);
                
                if (oldData?.inventoryData?.items == null)
                {
                    Debug.LogError("[SaveDataMigration] Invalid save file structure");
                    return;
                }

                // Convert boolean flags → int location
                foreach (var item in oldData.inventoryData.items)
                {
                    if (item.isInPocket)
                        item.location = 1; // Pocket
                    else if (item.isInBag)
                        item.location = 2; // Bag
                    else
                        item.location = 0; // Storage
                }

                // Create backup
                string backupPath = savePath + ".backup";
                System.IO.File.Copy(savePath, backupPath, overwrite: true);
                Debug.Log($"[SaveDataMigration] Backup created: {backupPath}");

                // Save migrated JSON
                string newJson = JsonUtility.ToJson(oldData, true);
                System.IO.File.WriteAllText(savePath, newJson);

                Debug.Log("[SaveDataMigration] ✓ JSON save file migrated successfully!");
                Debug.Log($"[SaveDataMigration]   Location: {savePath}");
                Debug.Log($"[SaveDataMigration]   Backup: {backupPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveDataMigration] Migration failed: {e.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Helper Structures for JSON Migration
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// ✅ LEGACY: Old save format with boolean flags
        /// Used for one-time JSON migration only
        /// </summary>
        [System.Serializable]
        private class OldSaveDataFormat
        {
            public SaveMetaDataOld metaData;
            public CharacterSaveDataOld characterData;
            public InventorySaveDataOld inventoryData;
            public EquipmentSaveDataOld equipmentData;
            public SkillLoadoutSaveDataOld skillLoadoutData;
        }

        [System.Serializable]
        private class SaveMetaDataOld
        {
            public string saveVersion;
            public string createdTime;
            public string lastSaveTime;
            public float totalPlayTimeSeconds;
            public int saveCount;
        }

        [System.Serializable]
        private class CharacterSaveDataOld
        {
            public string playerName;
            public int level;
            public int currentExperience;
            public float currentHealth;
            public float currentMana;
            public int attributePoints;
            public int strength;
            public int agility;
            public int intelligence;
            public int endurance;
            public int wisdom;
        }

        [System.Serializable]
        private class InventorySaveDataOld
        {
            public ItemInstanceDataOld[] items;
            public int maxBagSlots;
            public int maxPocketSlots;
            public int maxStorageSlots;
        }

        [System.Serializable]
        private class ItemInstanceDataOld
        {
            public string itemId;
            public int quantity;
            
            // ✅ OLD: Boolean flags (for reading old JSON)
            public bool isInBag;
            public bool isInPocket;
            
            // ✅ NEW: Int location (for writing new JSON)
            public int location;
        }

        [System.Serializable]
        private class EquipmentSaveDataOld
        {
            public string weaponId;
            public string helmetId;
            public string chestId;
            public string glovesId;
            public string bootsId;
            public string accessory1Id;
            public string accessory2Id;
        }

        [System.Serializable]
        private class SkillLoadoutSaveDataOld
        {
            public string normalSkill1Id;
            public string normalSkill2Id;
            public string ultimateSkillId;
        }

        // ═══════════════════════════════════════════════════════════
        // Legacy Compatibility Methods (deprecated)
        // ═══════════════════════════════════════════════════════════

        public static bool NeedsMigration(BagInventoryData data)
        {
            if (data == null) return false;
            
            return data.maxStorageSlots <= 0 || 
                   data.maxPocketSlots <= 0 || 
                   data.maxBagSlots <= 0;
        }

        public static string GetMigrationInfo(BagInventoryData data)
        {
            if (data == null)
                return "No data";

            bool needsSlotMigration = NeedsMigration(data);

            if (!needsSlotMigration)
                return "No migration needed";

            string info = "Migration needed:\n";
            
            if (needsSlotMigration)
                info += "- Missing slot capacity fields\n";

            return info;
        }
    }
}