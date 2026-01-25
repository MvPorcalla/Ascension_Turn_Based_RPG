// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Core/SaveManager.cs
// ✅ FIXED: Corrected MP and EXP casing issues
// ════════════════════════════════════════════════════════════════════════

using System;
using System.IO;
using UnityEngine;
using Ascension.Character.Core;
using Ascension.Character.Manager;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;
using Ascension.Inventory.Config;
using Ascension.Equipment.Manager;
using Ascension.Skill.Manager;
using Ascension.Data.Save;

namespace Ascension.Core
{
    public class SaveManager : MonoBehaviour
    {
        #region Serialized References
        [Header("Settings")]
        [SerializeField] private bool prettyPrintJson = true;
        [SerializeField] private bool enableAutoBackup = true;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private int maxBackupCount = 3;
        
        [Header("Fallback Behavior")]
        [Tooltip("If true, corrupted saves will load with default values instead of throwing errors")]
        [SerializeField] private bool allowGracefulDegradation = true;
        #endregion

        #region File Paths
        private const string ROOT_SAVE_FOLDER = "Saves";
        private const string PLAYER_DATA_FOLDER = "CharacterData";
        private const string BACKUP_FOLDER = "Backups";
        private const string PLAYER_DATA_FILE = "player_data.json";

        private string RootSavePath => Path.Combine(Application.persistentDataPath, ROOT_SAVE_FOLDER);
        private string CharacterDataPath => Path.Combine(RootSavePath, PLAYER_DATA_FOLDER);
        private string BackupPath => Path.Combine(CharacterDataPath, BACKUP_FOLDER);
        private string CharacterDataFile => Path.Combine(CharacterDataPath, PLAYER_DATA_FILE);
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            EnsureFoldersExist();
            Log("SaveManager ready");
        }

#if UNITY_EDITOR
        private void Update()
        {
            HandleDebugKeys();
        }
#endif
        #endregion

        #region Public API - Initialization
        public void Init()
        {
            Log("SaveManager initialized");
        }

        // ════════════════════════════════════════════════════════════════
        // ✅ SAVE GAME - Runtime → DTO → JSON
        // ════════════════════════════════════════════════════════════════
        
        public bool SaveGame(CharacterStats playerStats, float sessionPlayTime)
        {
            if (playerStats == null)
            {
                LogError("Cannot save - player stats are null!");
                return false;
            }

            try
            {
                CreateBackupIfNeeded();

                // ✅ STEP 1: Convert runtime data → DTOs
                CharacterSaveData characterDTO = playerStats.ToSaveData();
                InventorySaveData inventoryDTO = GatherInventoryData();
                EquipmentSaveData equipmentDTO = GatherEquipmentData();
                SkillLoadoutSaveData skillDTO = GatherSkillLoadoutData();

                // ✅ STEP 2: Build unified save container
                SaveData saveData = BuildSaveData(characterDTO, inventoryDTO, equipmentDTO, skillDTO, sessionPlayTime);

                // ✅ STEP 3: Write to file
                WriteSaveFile(saveData);

                Log("Game saved successfully");
                GameEvents.TriggerGameSaved();
                return true;
            }
            catch (Exception e)
            {
                LogError($"Save failed: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        // ════════════════════════════════════════════════════════════════
        // ✅ LOAD GAME - JSON → DTO → Runtime
        // ════════════════════════════════════════════════════════════════
        
        public bool LoadGame()
        {
            SaveData saveData = TryLoadFromPath(CharacterDataFile);

            if (saveData == null)
            {
                Log("Main save not found, trying backup...");
                saveData = TryLoadFromBackup();
            }

            if (saveData == null)
            {
                LogError("No valid save found");
                return false;
            }

            try
            {
                LoadCharacter(saveData.characterData);
                LoadInventory(saveData.inventoryData);
                LoadEquipment(saveData.equipmentData);
                LoadSkillLoadout(saveData.skillLoadoutData);

                Log("Game loaded successfully");
                GameEvents.TriggerGameLoaded(GameBootstrap.Character.CurrentPlayer);
                return true;
            }
            catch (Exception e)
            {
                LogError($"Load failed: {e.Message}\n{e.StackTrace}");
                
                // ✅ Graceful degradation option
                if (allowGracefulDegradation)
                {
                    LogWarning("Attempting to recover with default values...");
                    return TryLoadWithDefaults();
                }
                
                return false;
            }
        }

        public bool SaveExists()
        {
            if (File.Exists(CharacterDataFile))
                return true;

            return HasBackupFiles();
        }

        public bool DeleteSave()
        {
            try
            {
                DeleteMainSave();
                DeleteAllBackups();

                Log("Save deleted");
                GameEvents.TriggerSaveDeleted();
                return true;
            }
            catch (Exception e)
            {
                LogError($"Delete failed: {e.Message}");
                return false;
            }
        }
        #endregion

        #region Private - Character Save/Load (DTO Pattern)
        
        /// <summary>
        /// ✅ REFACTORED: Load character from DTO → Runtime conversion
        /// </summary>
        private void LoadCharacter(CharacterSaveData saveData)
        {
            if (saveData == null)
            {
                if (allowGracefulDegradation)
                {
                    LogWarning("Character data missing - loading defaults");
                    LoadDefaultCharacter();
                    return;
                }
                
                throw new InvalidOperationException("Invalid save data - character data missing!");
            }

            // ✅ STEP 1: Validate DTO
            if (!saveData.IsValid(out string validationError))
            {
                if (allowGracefulDegradation)
                {
                    LogWarning($"Invalid character data: {validationError} - sanitizing...");
                    saveData.Sanitize(); // Fix corrupted values
                }
                else
                {
                    throw new InvalidOperationException($"Character save data invalid: {validationError}");
                }
            }

            // ✅ STEP 2: Convert DTO → Runtime
            CharacterStats stats = saveData.ToRuntimeData();

            if (stats == null)
            {
                throw new InvalidOperationException("Failed to convert save data to CharacterStats");
            }

            // ✅ STEP 3: Recalculate derived stats (ensures formulas are up-to-date)
            var characterManager = GameBootstrap.Character;
            if (characterManager == null || characterManager.BaseStats == null)
            {
                throw new InvalidOperationException("CharacterManager or BaseStats not available!");
            }

            stats.RecalculateStats(characterManager.BaseStats, fullHeal: false);

            // ✅ STEP 4: Clamp HP/MP to max values
            if (stats.CurrentHP > stats.MaxHP)
                stats.combatRuntime.currentHP = stats.MaxHP;

            // ✅ FIXED: MP clamping (uses placeholder currentMP field)
            if (stats.CurrentMP > stats.MaxMP)
                stats.combatRuntime.currentMP = stats.MaxMP;

            // Ensure HP is at least 1 if alive
            if (stats.CurrentHP <= 0 && allowGracefulDegradation)
            {
                LogWarning("HP was 0 or negative - setting to 1");
                stats.combatRuntime.currentHP = 1;
            }

            // ✅ STEP 5: Load into CharacterManager
            characterManager.LoadPlayer(stats);

            Log($"✓ Character loaded: {stats.playerName} (Lv.{stats.Level}, HP: {stats.CurrentHP:F0}/{stats.MaxHP:F0})");
        }

        /// <summary>
        /// ✅ Fallback method if save is completely corrupted
        /// </summary>
        private bool TryLoadWithDefaults()
        {
            try
            {
                LogWarning("Loading with default character state");
                LoadDefaultCharacter();
                
                // Empty inventory
                var inventoryManager = GameBootstrap.Inventory;
                inventoryManager?.LoadInventory(new InventoryCoreData
                {
                    items = new System.Collections.Generic.List<ItemInstance>(),
                    maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS,
                    maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS
                });
                
                // Empty equipment
                var equipmentManager = GameBootstrap.Equipment;
                equipmentManager?.LoadEquipment(new EquipmentSaveData());
                
                // Empty skills
                var skillManager = GameBootstrap.Skills;
                skillManager?.LoadSkillLoadout(new SkillLoadoutSaveData());
                
                Log("✓ Loaded with default values");
                return true;
            }
            catch (Exception e)
            {
                LogError($"Failed to load even with defaults: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// ✅ Create minimal valid character
        /// </summary>
        private void LoadDefaultCharacter()
        {
            var characterManager = GameBootstrap.Character;
            
            if (characterManager == null || characterManager.BaseStats == null)
            {
                throw new InvalidOperationException("Cannot create default character - CharacterManager not initialized");
            }

            CharacterStats defaultStats = new CharacterStats
            {
                playerName = "Adventurer",
                attributes = new CharacterAttributes
                {
                    STR = 5,
                    AGI = 5,
                    INT = 5,
                    END = 5,
                    WIS = 5
                }
            };

            // ✅ FIXED: Changed currentExp → currentEXP (correct casing)
            defaultStats.levelSystem.level = 1;
            defaultStats.levelSystem.currentEXP = 0;
            defaultStats.RecalculateStats(characterManager.BaseStats, fullHeal: true);
            
            characterManager.LoadPlayer(defaultStats);
            
            LogWarning("Default character created - save was corrupted or missing");
        }
        #endregion

        #region Private - Inventory (DTO Pattern)
        
        /// <summary>
        /// ✅ REFACTORED: Use extension method for conversion
        /// </summary>
        private InventorySaveData GatherInventoryData()
        {
            var inventoryManager = GameBootstrap.Inventory;
            
            if (inventoryManager == null)
            {
                return new InventorySaveData
                {
                    items = Array.Empty<ItemInstanceData>(),
                    maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS,
                    maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS
                };
            }

            InventoryCoreData coreData = inventoryManager.SaveInventory();

            if (coreData == null || coreData.items == null)
            {
                return new InventorySaveData
                {
                    items = Array.Empty<ItemInstanceData>(),
                    maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS,
                    maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS
                };
            }

            // ✅ Use extension method for clean conversion
            return new InventorySaveData
            {
                items = coreData.items.ToSaveDataArray(),
                maxBagSlots = coreData.maxBagSlots,
                maxStorageSlots = coreData.maxStorageSlots
            };
        }

        /// <summary>
        /// ✅ REFACTORED: Use extension method for conversion
        /// </summary>
        private void LoadInventory(InventorySaveData saveData)
        {
            var inventoryManager = GameBootstrap.Inventory;
            
            if (inventoryManager == null)
            {
                LogWarning("Cannot load inventory - InventoryManager missing");
                return;
            }

            // ✅ Use extension method for clean conversion
            InventoryCoreData coreData = new InventoryCoreData
            {
                maxBagSlots = saveData?.maxBagSlots ?? InventoryConfig.DEFAULT_BAG_SLOTS,
                maxStorageSlots = saveData?.maxStorageSlots ?? InventoryConfig.DEFAULT_STORAGE_SLOTS,
                items = saveData?.items?.ToRuntimeDataList() ?? new System.Collections.Generic.List<ItemInstance>()
            };

            inventoryManager.LoadInventory(coreData);
            Log($"✓ Inventory loaded: {coreData.items.Count} items");
        }
        #endregion

        #region Private - Equipment
        private EquipmentSaveData GatherEquipmentData()
        {
            var equipmentManager = GameBootstrap.Equipment;
            return equipmentManager?.SaveEquipment() ?? new EquipmentSaveData();
        }

        private void LoadEquipment(EquipmentSaveData saveData)
        {
            var equipmentManager = GameBootstrap.Equipment;
            var characterManager = GameBootstrap.Character;
            
            if (equipmentManager == null)
            {
                LogWarning("Cannot load equipment - EquipmentManager missing");
                return;
            }

            if (saveData == null) return;

            equipmentManager.LoadEquipment(saveData);

            if (characterManager != null)
                characterManager.UpdateStatsFromEquipment();

            Log("✓ Equipment loaded");
        }
        #endregion

        #region Private - Skill Loadout
        private SkillLoadoutSaveData GatherSkillLoadoutData()
        {
            var skillManager = GameBootstrap.Skills;
            return skillManager?.SaveSkillLoadout() ?? new SkillLoadoutSaveData();
        }

        private void LoadSkillLoadout(SkillLoadoutSaveData saveData)
        {
            var skillManager = GameBootstrap.Skills;
            
            if (skillManager == null)
            {
                LogWarning("Cannot load skills - SkillLoadoutManager missing");
                return;
            }

            if (saveData == null) return;

            skillManager.LoadSkillLoadout(saveData);
            Log("✓ Skill loadout loaded");
        }
        #endregion

        #region Private - File Operations
        
        /// <summary>
        /// ✅ Build unified save data structure
        /// </summary>
        private SaveData BuildSaveData(
            CharacterSaveData characterData,
            InventorySaveData inventoryData,
            EquipmentSaveData equipmentData,
            SkillLoadoutSaveData skillLoadoutData,
            float playTime)
        {
            SaveData existing = File.Exists(CharacterDataFile) ? TryLoadFromPath(CharacterDataFile) : null;

            if (existing != null)
            {
                // ✅ Update existing save
                existing.characterData = characterData;
                existing.inventoryData = inventoryData;
                existing.equipmentData = equipmentData;
                existing.skillLoadoutData = skillLoadoutData;

                existing.metaData.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                existing.metaData.totalPlayTimeSeconds += playTime;
                existing.metaData.saveCount++;

                return existing;
            }
            else
            {
                // ✅ Create new save
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                return new SaveData
                {
                    metaData = new SaveMetaData
                    {
                        saveVersion = Application.version,
                        createdTime = timestamp,
                        lastSaveTime = timestamp,
                        totalPlayTimeSeconds = playTime,
                        saveCount = 1
                    },
                    characterData = characterData,
                    inventoryData = inventoryData,
                    equipmentData = equipmentData,
                    skillLoadoutData = skillLoadoutData
                };
            }
        }

        private void WriteSaveFile(SaveData saveData)
        {
            string json = JsonUtility.ToJson(saveData, prettyPrintJson);
            File.WriteAllText(CharacterDataFile, json);
        }

        private SaveData TryLoadFromPath(string path)
        {
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) return null;

                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null || data.characterData == null) return null;

                return data;
            }
            catch (Exception e)
            {
                LogError($"Failed to load save from {path}: {e.Message}");
                return null;
            }
        }

        private void EnsureFoldersExist()
        {
            CreateFolderIfMissing(RootSavePath);
            CreateFolderIfMissing(CharacterDataPath);
            CreateFolderIfMissing(BackupPath);
        }

        private void CreateFolderIfMissing(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private void CreateBackupIfNeeded()
        {
            if (enableAutoBackup && File.Exists(CharacterDataFile))
                CreateRollingBackup();
        }

        private void CreateRollingBackup()
        {
            try
            {
                string backupFilePath = Path.Combine(BackupPath, GenerateBackupFileName());
                File.Copy(CharacterDataFile, backupFilePath, overwrite: true);
                CleanupOldBackups();
            }
            catch (Exception e)
            {
                LogWarning($"Backup creation failed: {e.Message}");
            }
        }

        private string GenerateBackupFileName() => $"backup_player_{DateTime.Now:yyyyMMdd_HHmmss}.json";

        private void CleanupOldBackups()
        {
            FileInfo[] backupFiles = new DirectoryInfo(BackupPath).GetFiles("backup_player_*.json");
            Array.Sort(backupFiles, (a, b) => b.CreationTime.CompareTo(a.CreationTime));

            for (int i = maxBackupCount; i < backupFiles.Length; i++)
                backupFiles[i].Delete();
        }

        private SaveData TryLoadFromBackup()
        {
            FileInfo[] backupFiles = new DirectoryInfo(BackupPath).GetFiles("backup_player_*.json");
            Array.Sort(backupFiles, (a, b) => b.CreationTime.CompareTo(a.CreationTime));

            foreach (var file in backupFiles)
            {
                SaveData data = TryLoadFromPath(file.FullName);
                if (data != null)
                {
                    Log($"Restored from backup: {file.Name}");
                    WriteSaveFile(data);
                    return data;
                }
            }

            return null;
        }

        private bool HasBackupFiles() => Directory.Exists(BackupPath) && new DirectoryInfo(BackupPath).GetFiles("backup_player_*.json").Length > 0;

        private void DeleteMainSave()
        {
            if (File.Exists(CharacterDataFile))
                File.Delete(CharacterDataFile);
        }

        private void DeleteAllBackups()
        {
            foreach (var file in new DirectoryInfo(BackupPath).GetFiles("backup_player_*.json"))
                file.Delete();
        }
        #endregion

        #region Logging & Debug
        private void Log(string message)
        {
            if (enableDebugLogs) Debug.Log($"[SaveManager] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SaveManager] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[SaveManager] {message}");
        }

#if UNITY_EDITOR
        private void HandleDebugKeys()
        {
            if (Input.GetKeyDown(KeyCode.F12))
                OpenSaveFolder();

            if (Input.GetKeyDown(KeyCode.F11))
                NukeSaveData();
        }
#endif

        [ContextMenu("Open Save Folder")]
        public void OpenSaveFolder()
        {
            EnsureFoldersExist();
            Application.OpenURL("file://" + RootSavePath);
        }

        [ContextMenu("Nuke Save Data")]
        public void NukeSaveData()
        {
            if (DeleteSave())
                Debug.Log("[SaveManager] Save data nuked!");
        }
        #endregion
    }
}