// ══════════════════════════════════════════════════════════════════
// Assets/Editor/JSONMigrationTool.cs
// Unity Editor tool for one-time JSON save migration
// ══════════════════════════════════════════════════════════════════

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Ascension.Editor.Tools
{
    public static class JSONMigrationTool
    {
        private const string SAVE_PATH = "Saves/CharacterData/player_data.json";

        [MenuItem("Tools/Ascension/Migrate Save File (Boolean → Enum) ✅")]
        public static void MigrateSaveFile()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, SAVE_PATH);

            if (!File.Exists(fullPath))
            {
                EditorUtility.DisplayDialog(
                    "Save Not Found",
                    $"No save file found at:\n{fullPath}",
                    "OK"
                );
                return;
            }

            // Confirm migration
            bool confirm = EditorUtility.DisplayDialog(
                "Migrate Save File",
                "This will convert your save file from boolean flags to enum-based location.\n\n" +
                "A backup will be created automatically.\n\n" +
                "Continue?",
                "Yes, Migrate",
                "Cancel"
            );

            if (!confirm)
                return;

            try
            {
                // Read JSON
                string json = File.ReadAllText(fullPath);
                
                // Parse JSON manually (Unity JsonUtility doesn't handle mixed fields well)
                var saveData = JsonUtility.FromJson<OldSaveFormat>(json);

                if (saveData?.inventoryData?.items == null)
                {
                    EditorUtility.DisplayDialog("Error", "Invalid save file structure", "OK");
                    return;
                }

                // Convert items
                int migratedCount = 0;
                foreach (var item in saveData.inventoryData.items)
                {
                    // Migration rules: Pocket > Bag > Storage
                    if (item.isInPocket)
                        item.location = 1; // Pocket
                    else if (item.isInBag)
                        item.location = 2; // Bag
                    else
                        item.location = 0; // Storage

                    migratedCount++;
                }

                // Create backup
                string backupPath = fullPath + $".backup_{System.DateTime.Now:yyyyMMdd_HHmmss}";
                File.Copy(fullPath, backupPath, overwrite: true);

                // Save migrated JSON
                string newJson = JsonUtility.ToJson(saveData, prettyPrint: true);
                File.WriteAllText(fullPath, newJson);

                // Success message
                EditorUtility.DisplayDialog(
                    "Migration Complete ✅",
                    $"Successfully migrated {migratedCount} items!\n\n" +
                    $"Save file: {fullPath}\n" +
                    $"Backup: {backupPath}\n\n" +
                    $"You can now delete the .backup file if everything works correctly.",
                    "OK"
                );

                Debug.Log($"[JSONMigrationTool] ✓ Migration complete - {migratedCount} items migrated");
                Debug.Log($"[JSONMigrationTool]   Backup: {backupPath}");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog(
                    "Migration Failed",
                    $"Error: {e.Message}\n\nCheck console for details.",
                    "OK"
                );
                Debug.LogError($"[JSONMigrationTool] Migration failed: {e}");
            }
        }

        [MenuItem("Tools/Ascension/Open Save Folder 📁")]
        public static void OpenSaveFolder()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "Saves/CharacterData");
            
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            EditorUtility.RevealInFinder(savePath);
        }

        [MenuItem("Tools/Ascension/Validate Save File 🔍")]
        public static void ValidateSaveFile()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, SAVE_PATH);

            if (!File.Exists(fullPath))
            {
                EditorUtility.DisplayDialog("Save Not Found", "No save file exists yet.", "OK");
                return;
            }

            try
            {
                string json = File.ReadAllText(fullPath);
                var saveData = JsonUtility.FromJson<OldSaveFormat>(json);

                if (saveData?.inventoryData?.items == null)
                {
                    EditorUtility.DisplayDialog("Invalid", "Save file structure is invalid", "OK");
                    return;
                }

                // Check if already migrated
                bool hasLocation = false;
                bool hasOldFlags = false;

                foreach (var item in saveData.inventoryData.items)
                {
                    if (item.location != 0 || (!item.isInBag && !item.isInPocket))
                        hasLocation = true;
                    
                    if (item.isInBag || item.isInPocket)
                        hasOldFlags = true;
                }

                string status;
                if (hasLocation && !hasOldFlags)
                    status = "✅ Already migrated to enum format";
                else if (!hasLocation && hasOldFlags)
                    status = "⚠️ Old format detected - needs migration";
                else
                    status = "❓ Mixed format detected - might need manual inspection";

                EditorUtility.DisplayDialog(
                    "Save Validation",
                    $"Items: {saveData.inventoryData.items.Length}\n" +
                    $"Status: {status}\n\n" +
                    $"Max Slots:\n" +
                    $"- Bag: {saveData.inventoryData.maxBagSlots}\n" +
                    $"- Pocket: {saveData.inventoryData.maxPocketSlots}\n" +
                    $"- Storage: {saveData.inventoryData.maxStorageSlots}",
                    "OK"
                );
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to validate: {e.Message}", "OK");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Data Structures (supports both old and new format)
        // ═══════════════════════════════════════════════════════════

        [System.Serializable]
        private class OldSaveFormat
        {
            public MetaData metaData;
            public CharData characterData;
            public InvData inventoryData;
            public EqData equipmentData;
            public SkillData skillLoadoutData;
        }

        [System.Serializable]
        private class MetaData
        {
            public string saveVersion;
            public string createdTime;
            public string lastSaveTime;
            public float totalPlayTimeSeconds;
            public int saveCount;
        }

        [System.Serializable]
        private class CharData
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
        private class InvData
        {
            public ItemData[] items;
            public int maxBagSlots;
            public int maxPocketSlots;
            public int maxStorageSlots;
        }

        [System.Serializable]
        private class ItemData
        {
            public string itemId;
            public int quantity;
            
            // Old format (for reading)
            public bool isInBag;
            public bool isInPocket;
            
            // New format (for writing)
            public int location;
        }

        [System.Serializable]
        private class EqData
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
        private class SkillData
        {
            public string normalSkill1Id;
            public string normalSkill2Id;
            public string ultimateSkillId;
        }
    }
}
#endif