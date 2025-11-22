// -------------------------------
// GameManager.cs
// -------------------------------

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager. Single source of truth for runtime data.
/// Singleton pattern - persists across scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private CharacterBaseStatsSO defaultBaseStats;
    
    [Header("Runtime Data (Read Only)")]
    [SerializeField] private float sessionPlayTime = 0f;
    
    // Current player stats - the single source of truth
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
        // Track play time
        if (HasActivePlayer)
        {
            sessionPlayTime += Time.unscaledDeltaTime;
        }
    }
    
    #region Game Flow
    
    /// <summary>
    /// Start a new game with fresh stats
    /// </summary>
    public void StartNewGame()
    {
        CurrentPlayer = new PlayerStats();
        CurrentPlayer.Initialize(defaultBaseStats);
        sessionPlayTime = 0f;
        
        Debug.Log("[GameManager] New game started");
        OnNewGameStarted?.Invoke();
    }
    
    /// <summary>
    /// Set player stats (called from AvatarCreationManager after character creation)
    /// </summary>
    public void SetPlayerStats(PlayerStats stats)
    {
        CurrentPlayer = stats;
        Debug.Log($"[GameManager] Player set: {stats.playerName}");
    }
    
    /// <summary>
    /// Save current game
    /// </summary>
    public bool SaveGame()
    {
        if (!HasActivePlayer)
        {
            Debug.LogError("[GameManager] No active player to save!");
            return false;
        }
        
        SaveData saveData = SaveData.CreateSave(CurrentPlayer);
        saveData.metaData.totalPlayTimeSeconds += sessionPlayTime;
        
        bool success = SaveManager.Instance.Save(saveData);
        
        if (success)
        {
            OnPlayerSaved?.Invoke();
        }
        
        return success;
    }
    
    /// <summary>
    /// Load game
    /// </summary>
    public bool LoadGame()
    {
        SaveData saveData = SaveManager.Instance.Load();
        
        if (saveData == null)
        {
            Debug.LogError("[GameManager] Failed to load save");
            return false;
        }
        
        // Convert to runtime PlayerStats
        CurrentPlayer = saveData.playerData.ToPlayerStats(defaultBaseStats);
        sessionPlayTime = 0f;
        
        Debug.Log($"[GameManager] Loaded: {CurrentPlayer.playerName} (Lv.{CurrentPlayer.level})");
        OnPlayerLoaded?.Invoke(CurrentPlayer);
        
        return true;
    }
    
    /// <summary>
    /// Check if save exists
    /// </summary>
    public bool SaveExists()
    {
        return SaveManager.Instance.SaveExists();
    }
    
    /// <summary>
    /// Delete save data
    /// </summary>
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
    
    /// <summary>
    /// Add experience to player and auto-save if leveled up
    /// </summary>
    public bool AddExperience(int amount)
    {
        if (!HasActivePlayer)
            return false;
        
        bool leveledUp = CurrentPlayer.AddExperience(amount, defaultBaseStats);
        
        if (leveledUp)
        {
            Debug.Log($"[GameManager] Level up! Now level {CurrentPlayer.level}");
            SaveGame(); // Auto-save on level up
        }
        
        return leveledUp;
    }
    
    #endregion
    
    #region Scene Management Helpers
    
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadSceneWithSave(string sceneName)
    {
        SaveGame();
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Go to main base (primary hub scene)
    /// </summary>
    public void GoToMainBase()
    {
        LoadScene("03_MainBase");
    }
    
    /// <summary>
    /// Reset game and go back to avatar creation
    /// </summary>
    public void ResetAndCreateNewCharacter()
    {
        DeleteSave();
        StartNewGame();
        LoadScene("02_AvatarCreation");
    }
    
    #endregion

    #region Activity Management
    
    /// <summary>
    /// Call when returning to main base from any activity
    /// </summary>
    public void ReturnToMainBase()
    {
        SaveGame(); // Auto-save before transition
        LoadScene("03_MainBase");
    }

    /// <summary>
    /// Call after completing a battle/dungeon
    /// </summary>
    public void OnActivityCompleted()
    {
        SaveGame();
        Debug.Log("[GameManager] Progress saved after activity");
    }

    #endregion
    
    #region Application Lifecycle
    
    private void OnApplicationPause(bool pauseStatus)
    {
        // Auto-save when app is paused (mobile)
        if (pauseStatus && HasActivePlayer)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationQuit()
    {
        // Auto-save on quit
        if (HasActivePlayer)
        {
            SaveGame();
        }
    }
    
    #endregion
}