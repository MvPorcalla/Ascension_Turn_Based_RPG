// ════════════════════════════════════════════
// GameManager.cs
// Central game controller - delegates player data to CharacterManager
// Handles: Game flow, saves, scene management
// ════════════════════════════════════════════

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.Data.Models;

namespace Ascension.Managers
{
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        public static GameManager Instance { get; private set; }
        #endregion
        
        #region Serialized Fields
        [Header("Runtime Data (Read Only)")]
        [SerializeField] private float sessionPlayTime = 0f;
        [SerializeField] private bool isAvatarCreationComplete = false;
        #endregion
        
        #region Properties
        public PlayerStats CurrentPlayer => CharacterManager.Instance?.CurrentPlayer;
        public CharacterBaseStatsSO BaseStats => CharacterManager.Instance?.BaseStats;
        public bool HasActivePlayer => CharacterManager.Instance != null && CharacterManager.Instance.HasActivePlayer;
        #endregion
        
        #region Events
        public event Action<PlayerStats> OnPlayerLoaded;
        public event Action OnPlayerSaved;
        public event Action OnNewGameStarted;
        #endregion
        
        #region Unity Callbacks
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
            SubscribeToCharacterManager();
        }

        private void OnDestroy()
        {
            UnsubscribeFromCharacterManager();
        }
        
        private void Update()
        {
            if (HasActivePlayer)
            {
                sessionPlayTime += Time.unscaledDeltaTime;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && CanSave())
            {
                SaveGame();
            }
        }
        
        private void OnApplicationQuit()
        {
            if (CanSave())
            {
                SaveGame();
            }
        }
        #endregion
        
        #region Public Methods - Game Flow
        public void StartNewGame()
        {
            if (CharacterManager.Instance == null)
            {
                Debug.LogError("[GameManager] CharacterManager not found!");
                return;
            }

            CharacterManager.Instance.CreateNewPlayer("Adventurer");
            
            sessionPlayTime = 0f;
            isAvatarCreationComplete = false;
            
            Debug.Log("[GameManager] New game started");
            OnNewGameStarted?.Invoke();
        }
        
        public void SetPlayerName(string playerName)
        {
            if (!HasActivePlayer) return;
            
            CurrentPlayer.playerName = playerName;
            Debug.Log($"[GameManager] Player name set: {playerName}");
        }

        public void CompleteAvatarCreation()
        {
            isAvatarCreationComplete = true;
            Debug.Log("[GameManager] Avatar creation completed - saves enabled");
        }
        
        public bool SaveGame()
        {
            if (!CanSave())
            {
                Debug.LogWarning("[GameManager] Save blocked - conditions not met");
                return false;
            }
            
            PlayerStats playerToSave = CharacterManager.Instance.GetPlayerDataForSave();
            bool success = SaveManager.Instance.SaveGame(playerToSave, sessionPlayTime);
            
            if (success)
            {
                OnPlayerSaved?.Invoke();
            }
            
            return success;
        }

        public bool LoadGame()
        {
            if (!ValidateLoadConditions())
            {
                return false;
            }

            SaveData saveData = SaveManager.Instance.LoadGame();
            
            if (saveData == null)
            {
                Debug.LogError("[GameManager] Failed to load save");
                return false;
            }
            
            LoadPlayerFromSave(saveData);
            LoadInventoryFromSave(saveData);
            
            sessionPlayTime = 0f;
            isAvatarCreationComplete = true;
            
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
        
        #region Public Methods - Player Actions (Delegate to CharacterManager)
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
            CharacterManager.Instance?.Heal(amount);
        }

        public void TakeDamage(float amount)
        {
            CharacterManager.Instance?.TakeDamage(amount);
        }

        public void FullHeal()
        {
            CharacterManager.Instance?.FullHeal();
        }
        #endregion
        
        #region Public Methods - Scene Management
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
        
        #region Private Methods
        private void SubscribeToCharacterManager()
        {
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.OnPlayerLoaded += OnCharacterLoaded;
            }
            else
            {
                Debug.LogWarning("[GameManager] CharacterManager not found! Ensure it exists in scene.");
            }
        }

        private void UnsubscribeFromCharacterManager()
        {
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.OnPlayerLoaded -= OnCharacterLoaded;
            }
        }
        
        private void OnCharacterLoaded(PlayerStats stats)
        {
            OnPlayerLoaded?.Invoke(stats);
        }
        
        private bool CanSave()
        {
            if (!HasActivePlayer)
            {
                Debug.LogError("[GameManager] No active player to save!");
                return false;
            }
            
            if (!isAvatarCreationComplete)
            {
                Debug.LogWarning("[GameManager] Save blocked: Avatar creation not complete");
                return false;
            }
            
            return true;
        }
        
        private bool ValidateLoadConditions()
        {
            if (CharacterManager.Instance == null)
            {
                Debug.LogError("[GameManager] CharacterManager not found!");
                return false;
            }
            
            return true;
        }
        
        private void LoadPlayerFromSave(SaveData saveData)
        {
            PlayerStats loadedPlayer = saveData.playerData.ToPlayerStats(
                CharacterManager.Instance.BaseStats
            );
            CharacterManager.Instance.LoadPlayer(loadedPlayer);
        }
        
        private void LoadInventoryFromSave(SaveData saveData)
        {
            if (InventoryManager.Instance != null && saveData.inventoryData != null)
            {
                InventoryManager.Instance.LoadInventory(saveData.inventoryData);
            }
        }
        #endregion
    }
}