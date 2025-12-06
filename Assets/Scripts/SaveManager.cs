// ════════════════════════════════════════════
// SaveManager.cs
// Manages game state persistence with backup system
// ════════════════════════════════════════════

using System;
using System.IO;
using UnityEngine;
using Ascension.Data.Models;

namespace Ascension.Managers
{
    public class SaveManager : MonoBehaviour
    {
        #region Singleton
        public static SaveManager Instance { get; private set; }
        #endregion
        
        #region Serialized Fields
        [Header("Settings")]
        [SerializeField] private bool prettyPrintJson = true;
        [SerializeField] private bool enableAutoBackup = true;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private int maxBackupCount = 3;
        #endregion
        
        #region Private Fields
        private const string ROOT_SAVE_FOLDER = "Saves";
        private const string PLAYER_DATA_FOLDER = "PlayerData";
        private const string BACKUP_FOLDER = "Backups";
        private const string PLAYER_DATA_FILE = "player_data.json";
        #endregion
        
        #region Properties
        private string RootSavePath => Path.Combine(Application.persistentDataPath, ROOT_SAVE_FOLDER);
        private string PlayerDataPath => Path.Combine(RootSavePath, PLAYER_DATA_FOLDER);
        private string BackupPath => Path.Combine(PlayerDataPath, BACKUP_FOLDER);
        private string PlayerDataFile => Path.Combine(PlayerDataPath, PLAYER_DATA_FILE);
        #endregion
        
        #region Events
        public event Action OnSaveCompleted;
        public event Action OnLoadCompleted;
        public event Action<string> OnSaveError;
        public event Action<string> OnLoadError;
        #endregion
        
        #region Unity Callbacks
        private void Awake()
        {
            InitializeSingleton();
            EnsureFoldersExist();
        }
        
        #if UNITY_EDITOR
        private void Update()
        {
            HandleDebugKeys();
        }
        #endif
        #endregion
        
        #region Public Methods - Save/Load API
        public bool SaveGame(PlayerStats playerStats, float playTime)
        {
            try
            {
                CreateBackupIfNeeded();
                
                SaveData saveData = BuildSaveData(playerStats, playTime);
                saveData.UpdateMetaData();
                
                WriteSaveFile(saveData);
                
                Log("Game saved successfully");
                OnSaveCompleted?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                LogError($"Save failed: {e.Message}");
                OnSaveError?.Invoke(e.Message);
                return false;
            }
        }
        
        public SaveData LoadGame()
        {
            SaveData data = TryLoadFromPath(PlayerDataFile);
            
            if (data != null)
            {
                Log("Game loaded successfully");
                OnLoadCompleted?.Invoke();
                return data;
            }
            
            return TryLoadFromBackup();
        }
        
        public bool SaveExists()
        {
            if (File.Exists(PlayerDataFile))
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
        #endregion
        
        #region Public Methods - Utility
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
        
        #region Private Methods - Core Operations
        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void EnsureFoldersExist()
        {
            CreateFolderIfMissing(RootSavePath);
            CreateFolderIfMissing(PlayerDataPath);
            CreateFolderIfMissing(BackupPath);
        }
        
        private void CreateFolderIfMissing(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        
        private SaveData BuildSaveData(PlayerStats playerStats, float playTime)
        {
            SaveData save = new SaveData
            {
                metaData = SaveMetaData.CreateNew(),
                playerData = PlayerData.FromPlayerStats(playerStats),
                inventoryData = GetInventoryData()
            };
            
            save.metaData.totalPlayTimeSeconds += playTime;
            
            return save;
        }
        
        private BagInventoryData GetInventoryData()
        {
            if (InventoryManager.Instance != null)
            {
                return InventoryManager.Instance.SaveInventory();
            }
            
            Log("InventoryManager not found, saving empty inventory");
            return CreateEmptyInventory();
        }
        
        private BagInventoryData CreateEmptyInventory()
        {
            return new BagInventoryData
            {
                items = new System.Collections.Generic.List<ItemInstance>(),
                maxBagSlots = 12
            };
        }
        
        private void WriteSaveFile(SaveData saveData)
        {
            string json = JsonUtility.ToJson(saveData, prettyPrintJson);
            File.WriteAllText(PlayerDataFile, json);
        }
        #endregion
        
        #region Private Methods - Backup System
        private void CreateBackupIfNeeded()
        {
            if (enableAutoBackup && File.Exists(PlayerDataFile))
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
                
                File.Copy(PlayerDataFile, backupFilePath, overwrite: true);
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
                SortBackupsByDate(backupFiles);
                DeleteExcessBackups(backupFiles);
            }
            catch (Exception e)
            {
                LogError($"Backup cleanup failed: {e.Message}");
            }
        }
        
        private FileInfo[] GetBackupFiles()
        {
            DirectoryInfo backupDir = new DirectoryInfo(BackupPath);
            return backupDir.GetFiles("backup_player_*.json");
        }
        
        private void SortBackupsByDate(FileInfo[] backupFiles)
        {
            Array.Sort(backupFiles, (a, b) => b.CreationTime.CompareTo(a.CreationTime));
        }
        
        private void DeleteExcessBackups(FileInfo[] backupFiles)
        {
            for (int i = maxBackupCount; i < backupFiles.Length; i++)
            {
                backupFiles[i].Delete();
                Log($"Old backup deleted: {backupFiles[i].Name}");
            }
        }
        
        private SaveData TryLoadFromBackup()
        {
            Log("Main save corrupted or missing, trying backup...");
            
            SaveData data = TryLoadNewestBackup();
            
            if (data != null)
            {
                Log("Game restored from backup");
                RestoreBackupAsMainSave(data);
                OnLoadCompleted?.Invoke();
                return data;
            }
            
            LogError("No valid save found");
            OnLoadError?.Invoke("No valid save found");
            return null;
        }
        
        private SaveData TryLoadNewestBackup()
        {
            try
            {
                FileInfo[] backupFiles = GetBackupFiles();
                
                if (backupFiles.Length == 0)
                    return null;
                
                SortBackupsByDate(backupFiles);
                
                return FindFirstValidBackup(backupFiles);
            }
            catch (Exception e)
            {
                LogError($"Backup load failed: {e.Message}");
                return null;
            }
        }
        
        private SaveData FindFirstValidBackup(FileInfo[] backupFiles)
        {
            foreach (FileInfo file in backupFiles)
            {
                SaveData data = TryLoadFromPath(file.FullName);
                if (data != null)
                {
                    Log($"Loaded from backup: {file.Name}");
                    return data;
                }
            }
            
            return null;
        }
        
        private void RestoreBackupAsMainSave(SaveData data)
        {
            string json = JsonUtility.ToJson(data, prettyPrintJson);
            File.WriteAllText(PlayerDataFile, json);
        }
        
        private bool HasBackupFiles()
        {
            if (!Directory.Exists(BackupPath))
                return false;
            
            return GetBackupFiles().Length > 0;
        }
        #endregion
        
        #region Private Methods - File Operations
        private SaveData TryLoadFromPath(string path)
        {
            if (!File.Exists(path))
                return null;
            
            try
            {
                string json = File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                
                if (!IsValidSaveData(data))
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
        
        private bool IsValidSaveData(SaveData data)
        {
            return data != null && data.playerData != null;
        }
        
        private void DeleteMainSave()
        {
            if (File.Exists(PlayerDataFile))
                File.Delete(PlayerDataFile);
        }
        
        private void DeleteAllBackups()
        {
            FileInfo[] backupFiles = GetBackupFiles();
            
            foreach (FileInfo file in backupFiles)
            {
                file.Delete();
            }
        }
        #endregion
        
        #region Private Methods - Logging
        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[SaveManager] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[SaveManager] {message}");
        }
        #endregion
        
        #region Private Methods - Debug
        #if UNITY_EDITOR
        private void HandleDebugKeys()
        {
            if (Input.GetKeyDown(KeyCode.F12))
                OpenSaveFolder();
            
            if (Input.GetKeyDown(KeyCode.F11))
                NukeSaveData();
        }
        #endif
        #endregion
    }
}