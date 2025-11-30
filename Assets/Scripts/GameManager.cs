// -------------------------------
// GameManager.cs (Fixed for Refactored PlayerStats)
// -------------------------------

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private CharacterBaseStatsSO defaultBaseStats;
    
    [Header("Runtime Data (Read Only)")]
    [SerializeField] private float sessionPlayTime = 0f;
    
    public PlayerStats CurrentPlayer { get; private set; }
    public CharacterBaseStatsSO BaseStats => defaultBaseStats;
    public bool HasActivePlayer => CurrentPlayer != null;
    
    // Events
    public event Action<PlayerStats> OnPlayerLoaded;
    public event Action OnPlayerSaved;
    public event Action OnNewGameStarted;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Update()
    {
        if (HasActivePlayer)
        {
            sessionPlayTime += Time.unscaledDeltaTime;
        }
    }
    
    #region Game Flow
    
    public void StartNewGame()
    {
        CurrentPlayer = new PlayerStats();
        CurrentPlayer.Initialize(defaultBaseStats);
        sessionPlayTime = 0f;
        
        Debug.Log("[GameManager] New game started");
        OnNewGameStarted?.Invoke();
    }
    
    public void SetPlayerStats(PlayerStats stats)
    {
        CurrentPlayer = stats;
        Debug.Log($"[GameManager] Player set: {stats.playerName}");
    }
    
    /// <summary>
    /// Save game - delegates to SaveManager
    /// </summary>
    public bool SaveGame()
    {
        if (!HasActivePlayer)
        {
            Debug.LogError("[GameManager] No active player to save!");
            return false;
        }
        
        // SaveManager handles ALL save logic
        bool success = SaveManager.Instance.SaveGame(CurrentPlayer, sessionPlayTime);
        
        if (success)
        {
            OnPlayerSaved?.Invoke();
        }
        
        return success;
    }

    /// <summary>
    /// Load game - delegates to SaveManager
    /// </summary>
    public bool LoadGame()
    {
        // SaveManager handles ALL load logic
        SaveData saveData = SaveManager.Instance.LoadGame();
        
        if (saveData == null)
        {
            Debug.LogError("[GameManager] Failed to load save");
            return false;
        }
        
        // Restore player stats
        CurrentPlayer = saveData.playerData.ToPlayerStats(defaultBaseStats);
        sessionPlayTime = 0f;
        
        // Restore inventory (if InventoryManager exists)
        if (InventoryManager.Instance != null && saveData.inventoryData != null)
        {
            InventoryManager.Instance.LoadInventory(saveData.inventoryData);
        }
        
        Debug.Log($"[GameManager] Loaded: {CurrentPlayer.playerName} (Lv.{CurrentPlayer.Level})");
        OnPlayerLoaded?.Invoke(CurrentPlayer);
        
        return true;
    }
    
    public bool SaveExists()
    {
        return SaveManager.Instance.SaveExists();
    }
    
    public bool DeleteSave()
    {
        bool success = SaveManager.Instance.DeleteSave();
        
        if (success)
        {
            CurrentPlayer = null;
        }
        
        return success;
    }
    
    #endregion
    
    #region Quick Access Methods
    
    public bool AddExperience(int amount)
    {
        if (!HasActivePlayer)
            return false;
        
        bool leveledUp = CurrentPlayer.AddExperience(amount, defaultBaseStats);
        
        if (leveledUp)
        {
            Debug.Log($"[GameManager] Level up! Now level {CurrentPlayer.Level}");
            SaveGame();
        }
        
        return leveledUp;
    }
    
    #endregion
    
    #region Scene Management
    
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadSceneWithSave(string sceneName)
    {
        SaveGame();
        SceneManager.LoadScene(sceneName);
    }
    
    public void GoToMainBase()
    {
        LoadScene("03_MainBase");
    }
    
    public void ResetAndCreateNewCharacter()
    {
        DeleteSave();
        StartNewGame();
        LoadScene("02_AvatarCreation");
    }
    
    public void ReturnToMainBase()
    {
        SaveGame();
        LoadScene("03_MainBase");
    }

    public void OnActivityCompleted()
    {
        SaveGame();
        Debug.Log("[GameManager] Progress saved after activity");
    }
    
    #endregion
    
    #region Application Lifecycle
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && HasActivePlayer)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationQuit()
    {
        if (HasActivePlayer)
        {
            SaveGame();
        }
    }
    
    #endregion
}