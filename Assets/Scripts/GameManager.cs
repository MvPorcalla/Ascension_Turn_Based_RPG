// ──────────────────────────────────────────────────
// GameManager.cs (Integrated with CharacterManager)
// Handles: Game flow, saves, scene management
// Delegates: Player stats to CharacterManager
// ──────────────────────────────────────────────────

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Runtime Data (Read Only)")]
    [SerializeField] private float sessionPlayTime = 0f;
    [SerializeField] private bool isAvatarCreationComplete = false;
    
    // ✅ Delegate to CharacterManager for player data
    public PlayerStats CurrentPlayer => CharacterManager.Instance?.CurrentPlayer;
    public CharacterBaseStatsSO BaseStats => CharacterManager.Instance?.BaseStats;
    public bool HasActivePlayer => CharacterManager.Instance != null && CharacterManager.Instance.HasActivePlayer;
    
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

    private void Start()
    {
        // Subscribe to CharacterManager events
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnPlayerLoaded += OnCharacterLoaded;
        }
        else
        {
            Debug.LogWarning("[GameManager] CharacterManager not found! Make sure it exists in the scene.");
        }
    }

    private void OnDestroy()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnPlayerLoaded -= OnCharacterLoaded;
        }
    }
    
    private void Update()
    {
        if (HasActivePlayer)
        {
            sessionPlayTime += Time.unscaledDeltaTime;
        }
    }

    private void OnCharacterLoaded(PlayerStats stats)
    {
        OnPlayerLoaded?.Invoke(stats);
    }
    
    #region Game Flow
    
    public void StartNewGame()
    {
        if (CharacterManager.Instance == null)
        {
            Debug.LogError("[GameManager] CharacterManager not found!");
            return;
        }

        // Create default player (name will be set in avatar creation)
        CharacterManager.Instance.CreateNewPlayer("Adventurer");
        
        sessionPlayTime = 0f;
        isAvatarCreationComplete = false;
        
        Debug.Log("[GameManager] New game started");
        OnNewGameStarted?.Invoke();
    }
    
    /// <summary>
    /// Update player name after avatar creation
    /// </summary>
    public void SetPlayerName(string playerName)
    {
        if (HasActivePlayer)
        {
            CurrentPlayer.playerName = playerName;
            Debug.Log($"[GameManager] Player name set: {playerName}");
        }
    }

    /// <summary>
    /// Call this when avatar creation is confirmed
    /// </summary>
    public void CompleteAvatarCreation()
    {
        isAvatarCreationComplete = true;
        Debug.Log("[GameManager] Avatar creation completed - saves now enabled");
    }
    
    /// <summary>
    /// Save game - delegates to SaveManager
    /// ONLY saves if avatar creation is complete
    /// </summary>
    public bool SaveGame()
    {
        if (!HasActivePlayer)
        {
            Debug.LogError("[GameManager] No active player to save!");
            return false;
        }
        
        // CRITICAL: Don't save during avatar creation
        if (!isAvatarCreationComplete)
        {
            Debug.LogWarning("[GameManager] Save blocked: Avatar creation not complete");
            return false;
        }
        
        // Get player data from CharacterManager
        PlayerStats playerToSave = CharacterManager.Instance.GetPlayerDataForSave();
        
        bool success = SaveManager.Instance.SaveGame(playerToSave, sessionPlayTime);
        
        if (success)
        {
            OnPlayerSaved?.Invoke();
        }
        
        return success;
    }

    /// <summary>
    /// Load game - delegates to SaveManager and CharacterManager
    /// </summary>
    public bool LoadGame()
    {
        if (CharacterManager.Instance == null)
        {
            Debug.LogError("[GameManager] CharacterManager not found!");
            return false;
        }

        SaveData saveData = SaveManager.Instance.LoadGame();
        
        if (saveData == null)
        {
            Debug.LogError("[GameManager] Failed to load save");
            return false;
        }
        
        // Load player into CharacterManager
        PlayerStats loadedPlayer = saveData.playerData.ToPlayerStats(CharacterManager.Instance.BaseStats);
        CharacterManager.Instance.LoadPlayer(loadedPlayer);
        
        sessionPlayTime = 0f;
        isAvatarCreationComplete = true; // Loaded characters are always complete
        
        // Load inventory
        if (InventoryManager.Instance != null && saveData.inventoryData != null)
        {
            InventoryManager.Instance.LoadInventory(saveData.inventoryData);
        }
        
        Debug.Log($"[GameManager] Loaded: {CurrentPlayer.playerName} (Lv.{CurrentPlayer.Level})");
        
        return true;
    }
    
    public bool SaveExists()
    {
        return SaveManager.Instance.SaveExists();
    }
    
    public bool DeleteSave()
    {
        bool success = SaveManager.Instance.DeleteSave();
        
        if (success && CharacterManager.Instance != null)
        {
            CharacterManager.Instance.UnloadPlayer();
        }
        
        return success;
    }
    
    #endregion
    
    #region Quick Access Methods (Delegate to CharacterManager)
    
    public bool AddExperience(int amount)
    {
        if (!HasActivePlayer || CharacterManager.Instance == null)
            return false;
        
        int oldLevel = CurrentPlayer.Level;
        CharacterManager.Instance.AddExperience(amount);
        
        bool leveledUp = CurrentPlayer.Level > oldLevel;
        if (leveledUp)
        {
            Debug.Log($"[GameManager] Level up! Now level {CurrentPlayer.Level}");
            SaveGame();
        }
        
        return leveledUp;
    }

    public void Heal(float amount)
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.Heal(amount);
        }
    }

    public void TakeDamage(float amount)
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.TakeDamage(amount);
        }
    }

    public void FullHeal()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.FullHeal();
        }
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
        // Only save if avatar creation is complete
        if (pauseStatus && HasActivePlayer && isAvatarCreationComplete)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationQuit()
    {
        // Only save if avatar creation is complete
        if (HasActivePlayer && isAvatarCreationComplete)
        {
            SaveGame();
        }
    }
    
    #endregion
}