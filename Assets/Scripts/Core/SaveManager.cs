// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Core/SaveManager.cs
// ✅ FIXED: Added comprehensive validation in LoadCharacter()
// ✅ FIXED: Prevents crashes from corrupt/incomplete save files
// ════════════════════════════════════════════════════════════════════════

using System;
using System.IO;
using UnityEngine;
using Ascension.Data.Save;
using Ascension.Character.Stat;
using Ascension.Character.Manager;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Data;
using Ascension.Inventory.Config;
using Ascension.Equipment.Manager;

namespace Ascension.Core
{
    public class SaveManager : MonoBehaviour, IGameService
    {
        #region Singleton
        public static SaveManager Instance { get; private set; }
        #endregion
        
        #region Serialized Settings
        [Header("Settings")]
        [SerializeField] private bool prettyPrintJson = true;
        [SerializeField] private bool enableAutoBackup = true;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private int maxBackupCount = 3;
        #endregion
        
        #region Injected Dependencies
        private CharacterManager _characterManager;
        private InventoryManager _inventoryManager;
        private EquipmentManager _equipmentManager;
        private SkillLoadoutManager _skillLoadoutManager;
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
            InitializeSingleton();
        }
        
        #if UNITY_EDITOR
        private void Update()
        {
            HandleDebugKeys();
        }
        #endif
        #endregion

        #region IGameService Implementation
        public void Initialize()
        {
            Debug.Log("[SaveManager] Initializing...");
            
            InjectDependencies();
            EnsureFoldersExist();
            
            Debug.Log($"[SaveManager] Ready - Save path: {CharacterDataPath}");
        }

        private void InjectDependencies()
        {
            var container = ServiceContainer.Instance;
            
            _characterManager = container.GetRequired<CharacterManager>();
            _inventoryManager = container.GetRequired<InventoryManager>();
            _equipmentManager = container.GetRequired<EquipmentManager>();
            _skillLoadoutManager = container.GetRequired<SkillLoadoutManager>();
        }
        #endregion
        
        // ════════════════════════════════════════════════════════════════
        // PUBLIC API - SAVE/LOAD
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
                
                CharacterSaveData charData = ConvertCharacterToSaveData(playerStats);
                InventorySaveData invData = GatherInventoryData();
                EquipmentSaveData eqData = GatherEquipmentData();
                SkillLoadoutSaveData skillData = GatherSkillLoadoutData();
                
                SaveData saveData = BuildSaveData(charData, invData, eqData, skillData, sessionPlayTime);
                
                WriteSaveFile(saveData);
                
                Log("Game saved successfully");
                return true;
            }
            catch (Exception e)
            {
                LogError($"Save failed: {e.Message}");
                return false;
            }
        }
        
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
                LoadCharacter(saveData);
                LoadInventory(saveData);
                LoadEquipment(saveData);
                LoadSkillLoadout(saveData);
                
                Log("Game loaded successfully");
                return true;
            }
            catch (Exception e)
            {
                LogError($"Load failed: {e.Message}");
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
                return true;
            }
            catch (Exception e)
            {
                LogError($"Delete failed: {e.Message}");
                return false;
            }
        }
        
        // ════════════════════════════════════════════════════════════════
        // PRIVATE - CHARACTER CONVERSION
        // ════════════════════════════════════════════════════════════════
        
        private CharacterSaveData ConvertCharacterToSaveData(CharacterStats stats)
        {
            return new CharacterSaveData
            {
                playerName = stats.playerName,
                level = stats.Level,
                currentExperience = stats.CurrentEXP,
                currentHealth = stats.CurrentHP,
                currentMana = 0f,
                attributePoints = stats.UnallocatedPoints,
                
                strength = stats.attributes.STR,
                agility = stats.attributes.AGI,
                intelligence = stats.attributes.INT,
                endurance = stats.attributes.END,
                wisdom = stats.attributes.WIS
            };
        }

        /// <summary>
        /// ✅ FIXED: Added comprehensive validation to prevent crashes from corrupt saves
        /// 
        /// VALIDATION CHECKS:
        /// - Save data exists and is not null
        /// - Character data exists within save
        /// - Player name is valid (not null/empty)
        /// - Level is within valid range (1-100)
        /// - Attributes are within valid range (0-9999)
        /// - Health values are non-negative
        /// - Base stats are loaded
        /// 
        /// THROWS: InvalidOperationException with detailed error message if validation fails
        /// CAUGHT BY: Bootstrap.DetermineTargetScene() try-catch block
        /// </summary>
        private void LoadCharacter(SaveData saveData)
        {
            // ✅ Validation 1: Ensure save data exists
            if (saveData == null)
            {
                throw new InvalidOperationException(
                    "Save data is null! This should never happen - file was loaded but contains no data."
                );
            }

            // ✅ Validation 2: Ensure character data exists
            if (saveData.characterData == null)
            {
                throw new InvalidOperationException(
                    "Save file is missing character data section! File may be corrupted or from an old version."
                );
            }

            CharacterSaveData charData = saveData.characterData;

            // ✅ Validation 3: Player name must exist and be valid
            if (string.IsNullOrWhiteSpace(charData.playerName))
            {
                throw new InvalidOperationException(
                    "Save file has invalid or missing player name!"
                );
            }

            // ✅ Validation 4: Level must be within valid range
            if (charData.level < 1 || charData.level > 100)
            {
                throw new InvalidOperationException(
                    $"Save file has invalid level: {charData.level}. Expected 1-100."
                );
            }

            // ✅ Validation 5: Attributes must be within valid range
            const int MIN_ATTRIBUTE = 0;
            const int MAX_ATTRIBUTE = 9999;

            if (!IsAttributeValid(charData.strength, MIN_ATTRIBUTE, MAX_ATTRIBUTE) ||
                !IsAttributeValid(charData.agility, MIN_ATTRIBUTE, MAX_ATTRIBUTE) ||
                !IsAttributeValid(charData.intelligence, MIN_ATTRIBUTE, MAX_ATTRIBUTE) ||
                !IsAttributeValid(charData.endurance, MIN_ATTRIBUTE, MAX_ATTRIBUTE) ||
                !IsAttributeValid(charData.wisdom, MIN_ATTRIBUTE, MAX_ATTRIBUTE))
            {
                throw new InvalidOperationException(
                    $"Save file has invalid attributes! " +
                    $"STR={charData.strength}, AGI={charData.agility}, INT={charData.intelligence}, " +
                    $"END={charData.endurance}, WIS={charData.wisdom}. Expected {MIN_ATTRIBUTE}-{MAX_ATTRIBUTE}."
                );
            }

            // ✅ Validation 6: Health values must be non-negative
            if (charData.currentHealth < 0)
            {
                LogWarning($"Save file has negative health ({charData.currentHealth}), clamping to 0");
                charData.currentHealth = 0;
            }

            // ✅ Validation 7: Experience must be non-negative
            if (charData.currentExperience < 0)
            {
                LogWarning($"Save file has negative experience ({charData.currentExperience}), clamping to 0");
                charData.currentExperience = 0;
            }

            // ✅ Validation 8: Base stats must be loaded
            if (_characterManager.BaseStats == null)
            {
                throw new InvalidOperationException(
                    "CharacterBaseStatsSO is not loaded! Cannot restore character without base stats."
                );
            }

            // ✅ All validations passed - proceed with loading
            CharacterStats stats = new CharacterStats();
            stats.Initialize(_characterManager.BaseStats);
            
            // Restore saved values
            stats.playerName = charData.playerName;
            stats.guildRank = "Unranked";
            
            stats.levelSystem.level = charData.level;
            stats.levelSystem.currentEXP = charData.currentExperience;
            stats.levelSystem.unallocatedPoints = charData.attributePoints;
            
            stats.attributes.STR = charData.strength;
            stats.attributes.AGI = charData.agility;
            stats.attributes.INT = charData.intelligence;
            stats.attributes.END = charData.endurance;
            stats.attributes.WIS = charData.wisdom;
            
            stats.RecalculateStats(_characterManager.BaseStats, fullHeal: false);
            stats.combatRuntime.currentHP = charData.currentHealth;
            
            // ✅ Final validation: Ensure HP doesn't exceed MaxHP
            if (stats.CurrentHP > stats.MaxHP)
            {
                LogWarning($"Loaded HP ({stats.CurrentHP}) exceeds MaxHP ({stats.MaxHP}), clamping");
                stats.combatRuntime.currentHP = stats.MaxHP;
            }
            
            _characterManager.LoadPlayer(stats);
            
            Log($"✓ Character loaded: {stats.playerName} (Lv.{stats.Level}, HP: {stats.CurrentHP}/{stats.MaxHP})");
        }

        /// <summary>
        /// ✅ NEW: Helper method to validate attribute values
        /// </summary>
        private bool IsAttributeValid(int value, int min, int max)
        {
            return value >= min && value <= max;
        }
        
        // ════════════════════════════════════════════════════════════════
        // PRIVATE - INVENTORY CONVERSION
        // ════════════════════════════════════════════════════════════════
        
        private InventorySaveData GatherInventoryData()
        {
            if (_inventoryManager == null)
            {
                return new InventorySaveData 
                { 
                    items = Array.Empty<ItemInstanceData>(),
                    maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS,
                    maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS
                };
            }

            InventoryCoreData bagData = _inventoryManager.SaveInventory();

            if (bagData == null || bagData.items == null)
            {
                return new InventorySaveData 
                { 
                    items = Array.Empty<ItemInstanceData>(),
                    maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS,
                    maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS
                };
            }
            
            ItemInstanceData[] itemArray = new ItemInstanceData[bagData.items.Count];
            
            for (int i = 0; i < bagData.items.Count; i++)
            {
                ItemInstance item = bagData.items[i];
                
                itemArray[i] = new ItemInstanceData
                {
                    itemId = item.itemID,
                    quantity = item.quantity,
                    location = (int)item.location,
                    previousLocation = item.previousLocation.HasValue 
                        ? (int)item.previousLocation.Value 
                        : -1
                };
            }
            
            return new InventorySaveData
            {
                items = itemArray,
                maxBagSlots = bagData.maxBagSlots,
                maxStorageSlots = bagData.maxStorageSlots
            };
        }

        /// <summary>
        /// ✅ ENHANCED: Added validation for inventory data
        /// </summary>
        private void LoadInventory(SaveData saveData)
        {
            if (_inventoryManager == null)
            {
                LogWarning("Cannot load inventory - InventoryManager missing");
                return;
            }

            if (saveData.inventoryData == null)
            {
                LogWarning("Save file has no inventory data, initializing empty inventory");
                InventoryCoreData emptyInventory = new InventoryCoreData
                {
                    maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS,
                    maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS,
                    items = new System.Collections.Generic.List<ItemInstance>()
                };
                _inventoryManager.LoadInventory(emptyInventory);
                return;
            }

            InventoryCoreData bagData = new InventoryCoreData
            {
                maxBagSlots = saveData.inventoryData.maxBagSlots,
                maxStorageSlots = saveData.inventoryData.maxStorageSlots,
                items = new System.Collections.Generic.List<ItemInstance>()
            };
            
            // ✅ Validate items array exists
            if (saveData.inventoryData.items != null)
            {
                foreach (var itemData in saveData.inventoryData.items)
                {
                    // ✅ Validate item ID is not empty
                    if (string.IsNullOrEmpty(itemData.itemId))
                    {
                        LogWarning("Skipping item with empty ID in save file");
                        continue;
                    }

                    ItemLocation location = (ItemLocation)itemData.location;
                    
                    ItemInstance item = new ItemInstance(
                        itemData.itemId, 
                        itemData.quantity, 
                        location
                    );
                    
                    if (itemData.previousLocation >= 0)
                    {
                        item.previousLocation = (ItemLocation)itemData.previousLocation;
                    }
                    
                    bagData.items.Add(item);
                }
            }
            
            _inventoryManager.LoadInventory(bagData);

            Log($"✓ Inventory loaded: {bagData.items.Count} items");
        }
        
        // ════════════════════════════════════════════════════════════════
        // PRIVATE - EQUIPMENT CONVERSION
        // ════════════════════════════════════════════════════════════════
        
        private EquipmentSaveData GatherEquipmentData()
        {
            if (_equipmentManager != null)
            {
                return _equipmentManager.SaveEquipment();
            }

            return new EquipmentSaveData
            {
                weaponId = string.Empty,
                helmetId = string.Empty,
                chestId = string.Empty,
                glovesId = string.Empty,
                bootsId = string.Empty,
                accessory1Id = string.Empty,
                accessory2Id = string.Empty
            };
        }

        /// <summary>
        /// ✅ ENHANCED: Added validation for equipment data
        /// </summary>
        private void LoadEquipment(SaveData saveData)
        {
            if (_equipmentManager == null)
            {
                LogWarning("Cannot load equipment - EquipmentManager missing");
                return;
            }

            if (saveData.equipmentData == null)
            {
                LogWarning("Save file has no equipment data, initializing empty equipment");
                return;
            }

            _equipmentManager.LoadEquipment(saveData.equipmentData);
            
            if (_characterManager != null)
            {
                _characterManager.UpdateStatsFromEquipment();
            }
            
            Log("✓ Equipment loaded");
        }
        
        // ════════════════════════════════════════════════════════════════
        // PRIVATE - SKILL LOADOUT CONVERSION
        // ════════════════════════════════════════════════════════════════
        
        private SkillLoadoutSaveData GatherSkillLoadoutData()
        {
            if (_skillLoadoutManager != null)
            {
                return _skillLoadoutManager.SaveSkillLoadout();
            }

            return new SkillLoadoutSaveData
            {
                normalSkill1Id = string.Empty,
                normalSkill2Id = string.Empty,
                ultimateSkillId = string.Empty
            };
        }

        /// <summary>
        /// ✅ ENHANCED: Added validation for skill loadout data
        /// </summary>
        private void LoadSkillLoadout(SaveData saveData)
        {
            if (_skillLoadoutManager == null)
            {
                LogWarning("Cannot load skills - SkillLoadoutManager missing");
                return;
            }

            if (saveData.skillLoadoutData == null)
            {
                LogWarning("Save file has no skill loadout data, initializing empty loadout");
                return;
            }

            _skillLoadoutManager.LoadSkillLoadout(saveData.skillLoadoutData);
            
            Log("✓ Skill loadout loaded");
        }
        
        // ════════════════════════════════════════════════════════════════
        // PRIVATE - FILE OPERATIONS
        // ════════════════════════════════════════════════════════════════
        
        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
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
        
        private SaveData BuildSaveData(
            CharacterSaveData characterData, 
            InventorySaveData inventoryData, 
            EquipmentSaveData equipmentData,
            SkillLoadoutSaveData skillLoadoutData,
            float playTime)
        {
            SaveData existing = null;
            
            if (File.Exists(CharacterDataFile))
            {
                existing = TryLoadFromPath(CharacterDataFile);
            }
            
            if (existing != null)
            {
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
        
        /// <summary>
        /// ✅ ENHANCED: Added JSON validation before parsing
        /// </summary>
        private SaveData TryLoadFromPath(string path)
        {
            if (!File.Exists(path))
                return null;
            
            try
            {
                string json = File.ReadAllText(path);
                
                // ✅ Validate JSON is not empty or whitespace
                if (string.IsNullOrWhiteSpace(json))
                {
                    LogError($"Save file at {path} is empty!");
                    return null;
                }

                SaveData data = JsonUtility.FromJson<SaveData>(json);
                
                if (data == null || data.characterData == null)
                {
                    LogError($"Invalid save data at: {path}");
                    return null;
                }
                
                return data;
            }
            catch (Exception e)
            {
                LogError($"Failed to load {path}: {e.Message}");
                return null;
            }
        }
        
        // ════════════════════════════════════════════════════════════════
        // PRIVATE - BACKUP SYSTEM
        // ════════════════════════════════════════════════════════════════
        
        private void CreateBackupIfNeeded()
        {
            if (enableAutoBackup && File.Exists(CharacterDataFile))
            {
                CreateRollingBackup();
            }
        }
        
        private void CreateRollingBackup()
        {
            try
            {
                string backupFileName = GenerateBackupFileName();
                string backupFilePath = Path.Combine(BackupPath, backupFileName);
                
                File.Copy(CharacterDataFile, backupFilePath, overwrite: true);
                Log($"Backup created: {backupFileName}");
                
                CleanupOldBackups();
            }
            catch (Exception e)
            {
                LogError($"Backup failed: {e.Message}");
            }
        }
        
        private string GenerateBackupFileName()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"backup_player_{timestamp}.json";
        }
        
        private void CleanupOldBackups()
        {
            try
            {
                FileInfo[] backupFiles = GetBackupFiles();
                Array.Sort(backupFiles, (a, b) => b.CreationTime.CompareTo(a.CreationTime));
                
                for (int i = maxBackupCount; i < backupFiles.Length; i++)
                {
                    backupFiles[i].Delete();
                    Log($"Old backup deleted: {backupFiles[i].Name}");
                }
            }
            catch (Exception e)
            {
                LogError($"Backup cleanup failed: {e.Message}");
            }
        }
        
        private SaveData TryLoadFromBackup()
        {
            try
            {
                FileInfo[] backupFiles = GetBackupFiles();
                
                if (backupFiles.Length == 0)
                    return null;
                
                Array.Sort(backupFiles, (a, b) => b.CreationTime.CompareTo(a.CreationTime));
                
                foreach (FileInfo file in backupFiles)
                {
                    SaveData data = TryLoadFromPath(file.FullName);
                    if (data != null)
                    {
                        Log($"Loaded from backup: {file.Name}");
                        RestoreBackupAsMainSave(data);
                        return data;
                    }
                }
            }
            catch (Exception e)
            {
                LogError($"Backup load failed: {e.Message}");
            }
            
            return null;
        }
        
        private void RestoreBackupAsMainSave(SaveData data)
        {
            string json = JsonUtility.ToJson(data, prettyPrintJson);
            File.WriteAllText(CharacterDataFile, json);
        }
        
        private FileInfo[] GetBackupFiles()
        {
            DirectoryInfo backupDir = new DirectoryInfo(BackupPath);
            return backupDir.GetFiles("backup_player_*.json");
        }
        
        private bool HasBackupFiles()
        {
            if (!Directory.Exists(BackupPath))
                return false;
            
            return GetBackupFiles().Length > 0;
        }
        
        private void DeleteMainSave()
        {
            if (File.Exists(CharacterDataFile))
                File.Delete(CharacterDataFile);
        }
        
        private void DeleteAllBackups()
        {
            FileInfo[] backupFiles = GetBackupFiles();
            
            foreach (FileInfo file in backupFiles)
            {
                file.Delete();
            }
        }
        
        // ════════════════════════════════════════════════════════════════
        // PRIVATE - LOGGING & DEBUG
        // ════════════════════════════════════════════════════════════════
        
        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[SaveManager] {message}");
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
    }
}