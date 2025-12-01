// ──────────────────────────────────────────────────
// SaveManager.cs
// Manages saving and loading of game state, including backups
// ──────────────────────────────────────────────────

using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private bool prettyPrintJson = true;
    [SerializeField] private bool enableAutoBackup = true;
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private int maxBackupCount = 3;
    
    // Folder structure
    private const string ROOT_SAVE_FOLDER = "Saves";
    private const string PLAYER_DATA_FOLDER = "PlayerData";
    private const string BACKUP_FOLDER = "Backups";
    private const string PLAYER_DATA_FILE = "player_data.json";
    
    // Events
    public event Action OnSaveCompleted;
    public event Action OnLoadCompleted;
    public event Action<string> OnSaveError;
    public event Action<string> OnLoadError;
    
    // Paths
    private string RootSavePath => Path.Combine(Application.persistentDataPath, ROOT_SAVE_FOLDER);
    private string PlayerDataPath => Path.Combine(RootSavePath, PLAYER_DATA_FOLDER);
    private string BackupPath => Path.Combine(PlayerDataPath, BACKUP_FOLDER);
    private string PlayerDataFile => Path.Combine(PlayerDataPath, PLAYER_DATA_FILE);
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        EnsureFoldersExist();
    }
    
    private void EnsureFoldersExist()
    {
        if (!Directory.Exists(RootSavePath))
            Directory.CreateDirectory(RootSavePath);
        
        if (!Directory.Exists(PlayerDataPath))
            Directory.CreateDirectory(PlayerDataPath);
        
        if (!Directory.Exists(BackupPath))
            Directory.CreateDirectory(BackupPath);
    }
    
    #region Public API - GameManager calls these
    
    /// <summary>
    /// Save complete game state (player + inventory + metadata)
    /// </summary>
    public bool SaveGame(PlayerStats playerStats, float playTime)
    {
        try
        {
            // Create rolling backup before saving
            if (enableAutoBackup && File.Exists(PlayerDataFile))
            {
                CreateRollingBackup();
            }
            
            // Build complete save data
            SaveData saveData = BuildSaveData(playerStats, playTime);
            
            // Update metadata
            saveData.UpdateMetaData();
            
            // Serialize and write
            string json = JsonUtility.ToJson(saveData, prettyPrintJson);
            File.WriteAllText(PlayerDataFile, json);
            
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
    
    /// <summary>
    /// Load complete game state
    /// </summary>
    public SaveData LoadGame()
    {
        // Try loading main save
        SaveData data = TryLoadFromPath(PlayerDataFile);
        
        if (data != null)
        {
            Log("Game loaded successfully");
            OnLoadCompleted?.Invoke();
            return data;
        }
        
        // Main save failed, try newest backup
        Log("Main save corrupted or missing, trying backup...");
        data = TryLoadNewestBackup();
        
        if (data != null)
        {
            Log("Game restored from backup");
            
            // Restore backup as main save
            string json = JsonUtility.ToJson(data, prettyPrintJson);
            File.WriteAllText(PlayerDataFile, json);
            
            OnLoadCompleted?.Invoke();
            return data;
        }
        
        LogError("No valid save found");
        OnLoadError?.Invoke("No valid save found");
        return null;
    }
    
    public bool SaveExists()
    {
        if (File.Exists(PlayerDataFile))
            return true;
        
        // Check if any backup exists
        if (Directory.Exists(BackupPath))
        {
            DirectoryInfo backupDir = new DirectoryInfo(BackupPath);
            return backupDir.GetFiles("backup_player_*.json").Length > 0;
        }
        
        return false;
    }
    
    public bool DeleteSave()
    {
        try
        {
            // Delete main save
            if (File.Exists(PlayerDataFile))
                File.Delete(PlayerDataFile);
            
            // Delete all backups
            DirectoryInfo backupDir = new DirectoryInfo(BackupPath);
            FileInfo[] backupFiles = backupDir.GetFiles("backup_player_*.json");
            
            foreach (FileInfo file in backupFiles)
            {
                file.Delete();
            }
            
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
    
    #region Save Data Building - SaveManager's Responsibility
    
    /// <summary>
    /// Build complete save data from all game systems
    /// This is where ALL save logic is centralized
    /// </summary>
    private SaveData BuildSaveData(PlayerStats playerStats, float playTime)
    {
        SaveData save = new SaveData
        {
            metaData = SaveMetaData.CreateNew(),
            playerData = PlayerData.FromPlayerStats(playerStats),
            inventoryData = GetInventoryData()
        };
        
        // Add play time
        save.metaData.totalPlayTimeSeconds += playTime;
        
        return save;
    }
    
    /// <summary>
    /// Safely get inventory data
    /// </summary>
    private BagInventoryData GetInventoryData()
    {
        if (InventoryManager.Instance != null)
        {
            return InventoryManager.Instance.SaveInventory();
        }
        
        // If InventoryManager doesn't exist, create empty inventory
        Log("InventoryManager not found, saving empty inventory");
        return new BagInventoryData
        {
            items = new System.Collections.Generic.List<ItemInstance>(),
            maxBagSlots = 12
        };
    }
    
    #endregion
    
    #region Backup System
    
    private void CreateRollingBackup()
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFileName = $"backup_player_{timestamp}.json";
            string backupFilePath = Path.Combine(BackupPath, backupFileName);
            
            File.Copy(PlayerDataFile, backupFilePath, true);
            Log($"Backup created: {backupFileName}");
            
            CleanupOldBackups();
        }
        catch (Exception e)
        {
            LogError($"Backup failed: {e.Message}");
        }
    }
    
    private void CleanupOldBackups()
    {
        try
        {
            DirectoryInfo backupDir = new DirectoryInfo(BackupPath);
            FileInfo[] backupFiles = backupDir.GetFiles("backup_player_*.json");
            
            System.Array.Sort(backupFiles, (a, b) => b.CreationTime.CompareTo(a.CreationTime));
            
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
    
    private SaveData TryLoadNewestBackup()
    {
        try
        {
            DirectoryInfo backupDir = new DirectoryInfo(BackupPath);
            FileInfo[] backupFiles = backupDir.GetFiles("backup_player_*.json");
            
            if (backupFiles.Length == 0)
                return null;
            
            System.Array.Sort(backupFiles, (a, b) => b.CreationTime.CompareTo(a.CreationTime));
            
            foreach (FileInfo file in backupFiles)
            {
                SaveData data = TryLoadFromPath(file.FullName);
                if (data != null)
                {
                    Log($"Loaded from backup: {file.Name}");
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
    
    #endregion
    
    #region Private Helpers
    
    private SaveData TryLoadFromPath(string path)
    {
        if (!File.Exists(path))
            return null;
        
        try
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            if (data == null || data.playerData == null)
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
    
    #region Editor Helpers
    
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

    #if UNITY_EDITOR
    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.F12))
            OpenSaveFolder();
        
        if (UnityEngine.Input.GetKeyDown(KeyCode.F11))
            NukeSaveData();
    }
    #endif
    
    #endregion
}