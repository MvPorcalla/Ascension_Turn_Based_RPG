// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/App/GameManager.cs
// ✅ UPDATED: Uses SceneConfigSO instead of hardcoded scene lists
// ════════════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Ascension.Core;
using Ascension.Character.Stat;
using Ascension.Character.Manager;
using Ascension.Data.SO;
using Ascension.Data.SO.Character;
using Ascension.Data.Config;

namespace Ascension.App
{
    public class GameManager : MonoBehaviour, IGameService
    {
        #region Singleton
        public static GameManager Instance { get; private set; }
        #endregion
        
        #region Scene Configuration
        [Header("Scene Configuration")]
        [SerializeField] private SceneManifest sceneManifest;
        #endregion
        
        #region Injected Dependencies
        private SaveManager _saveManager;
        private CharacterManager _characterManager;
        #endregion
        
        #region Runtime State
        [Header("Session Data (Read Only)")]
        [SerializeField] private float sessionPlayTime = 0f;
        [SerializeField] private bool isAvatarCreationComplete = false;
        
        #endregion
        
        #region Properties
        public CharacterStats CurrentPlayer => _characterManager?.CurrentPlayer;
        public CharacterBaseStatsSO BaseStats => _characterManager?.BaseStats;
        public bool HasActivePlayer => _characterManager != null && _characterManager.HasActivePlayer;
        public bool IsAvatarCreationComplete => isAvatarCreationComplete;
        public float SessionPlayTime => sessionPlayTime;
        public string CurrentMainScene => SceneFlowManager.Instance?.CurrentMainScene ?? "NONE";
        #endregion
        
        #region Unity Callbacks
        private void Awake()
        {
            InitializeSingleton();
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
        
        private void OnDestroy()
        {
            UnsubscribeFromManagerEvents();
        }
        #endregion
        
        #region Initialization
        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            Debug.Log("[GameManager] Singleton created");
        }

        public void Initialize()
        {
            Debug.Log("[GameManager] Initializing...");
            
            ValidateSceneConfig();
            InjectDependencies();
            ValidateDependencies();
            SubscribeToManagerEvents();
            
            Debug.Log("[GameManager] Ready");
        }

        private void ValidateSceneConfig()
        {
            if (sceneManifest == null)
            {
                Debug.LogError("[GameManager] SceneManifest is not assigned! Please assign it in the Inspector.", this);
            }
            else
            {
                Debug.Log($"[GameManager] Scene manifest loaded: {sceneManifest.name}");
            }
        }

        private void InjectDependencies()
        {
            var container = ServiceContainer.Instance;
            
            if (container == null)
            {
                Debug.LogError("[GameManager] ServiceContainer is null!");
                return;
            }

            _saveManager = container.GetRequired<SaveManager>();
            _characterManager = container.GetRequired<CharacterManager>();
        }

        private void ValidateDependencies()
        {
            if (_saveManager == null)
                throw new InvalidOperationException("SaveManager not found in ServiceContainer!");
            
            if (_characterManager == null)
                throw new InvalidOperationException("CharacterManager not found in ServiceContainer!");
        }

        private void SubscribeToManagerEvents()
        {
            if (_characterManager != null)
            {
                _characterManager.OnPlayerLoaded += HandlePlayerLoaded;
            }
        }

        private void UnsubscribeFromManagerEvents()
        {
            if (_characterManager != null)
            {
                _characterManager.OnPlayerLoaded -= HandlePlayerLoaded;
            }
        }

        private void HandlePlayerLoaded(CharacterStats stats)
        {
            GameEvents.TriggerGameLoaded(stats);
        }
        #endregion
        
        // ════════════════════════════════════════════════════════════════
        // PUBLIC API - GAME FLOW
        // ════════════════════════════════════════════════════════════════
        
        public void StartNewGame(string playerName = "Adventurer")
        {
            if (_characterManager == null)
            {
                Debug.LogError("[GameManager] Cannot start new game - CharacterManager missing!");
                return;
            }
            
            _characterManager.CreateNewPlayer(playerName);
            sessionPlayTime = 0f;
            isAvatarCreationComplete = false;
            
            GameEvents.TriggerNewGameStarted(CurrentPlayer);
            
            Debug.Log($"[GameManager] New game started: {playerName}");
        }
        
        public void SetPlayerName(string playerName)
        {
            if (!HasActivePlayer)
            {
                Debug.LogWarning("[GameManager] No active player to rename!");
                return;
            }
            
            CurrentPlayer.playerName = playerName;
            GameEvents.TriggerPlayerNameChanged(playerName);
            
            Debug.Log($"[GameManager] Player name set: {playerName}");
        }

        public void CompleteAvatarCreation()
        {
            isAvatarCreationComplete = true;
            Debug.Log("[GameManager] Avatar creation completed - saves now enabled");
        }

        public bool SaveGame()
        {
            if (!CanSave())
            {
                Debug.LogWarning("[GameManager] Cannot save - conditions not met");
                return false;
            }
            
            bool success = _saveManager.SaveGame(CurrentPlayer, sessionPlayTime);
            
            if (success)
            {
                GameEvents.TriggerGameSaved();
                Debug.Log("[GameManager] Game saved successfully");
            }
            
            return success;
        }

        public bool LoadGame()
        {
            if (_saveManager == null)
            {
                Debug.LogError("[GameManager] Cannot load - SaveManager missing!");
                return false;
            }
            
            bool success = _saveManager.LoadGame();
            
            if (success)
            {
                sessionPlayTime = 0f;
                isAvatarCreationComplete = true;
                
                Debug.Log("[GameManager] Game loaded successfully");
            }
            
            return success;
        }
        
        public bool SaveExists() => _saveManager != null && _saveManager.SaveExists();
        
        public bool DeleteSave()
        {
            if (_saveManager == null) return false;
            
            bool success = _saveManager.DeleteSave();
            
            if (success)
            {
                if (_characterManager != null)
                {
                    _characterManager.UnloadPlayer();
                }
                
                isAvatarCreationComplete = false;
                sessionPlayTime = 0f;
                
                GameEvents.TriggerSaveDeleted();
                Debug.Log("[GameManager] Save deleted");
            }
            
            return success;
        }

        public bool CanSave()
        {
            if (!HasActivePlayer)
            {
                Debug.LogWarning("[GameManager] No active player to save!");
                return false;
            }
            
            if (!isAvatarCreationComplete)
            {
                Debug.LogWarning("[GameManager] Avatar creation not complete!");
                return false;
            }
            
            return true;
        }
        
        // ════════════════════════════════════════════════════════════════
        // PUBLIC API - PLAYER ACTIONS
        // ════════════════════════════════════════════════════════════════
        
        public bool AddExperience(int amount)
        {
            if (!HasActivePlayer) return false;
            
            int oldLevel = CurrentPlayer.Level;
            _characterManager.AddExperience(amount);
            
            bool leveledUp = CurrentPlayer.Level > oldLevel;
            
            GameEvents.TriggerExperienceGained(amount, CurrentPlayer.CurrentEXP);
            
            if (leveledUp)
            {
                GameEvents.TriggerLevelUp(CurrentPlayer.Level);
                SaveGame();
            }
            
            return leveledUp;
        }

        public void Heal(float amount)
        {
            if (!HasActivePlayer) return;
            
            _characterManager.Heal(amount);
            GameEvents.TriggerHealthChanged(CurrentPlayer.CurrentHP, CurrentPlayer.MaxHP);
        }

        public void TakeDamage(float amount)
        {
            if (!HasActivePlayer) return;
            
            _characterManager.TakeDamage(amount);
            GameEvents.TriggerHealthChanged(CurrentPlayer.CurrentHP, CurrentPlayer.MaxHP);
        }

        public void FullHeal()
        {
            if (!HasActivePlayer) return;
            
            _characterManager.FullHeal();
            GameEvents.TriggerHealthChanged(CurrentPlayer.CurrentHP, CurrentPlayer.MaxHP);
        }
        
        // ════════════════════════════════════════════════════════════════
        // DEBUG HELPERS
        // ════════════════════════════════════════════════════════════════
        
        [ContextMenu("Debug: Print Loaded Scenes")]
        private void DebugPrintLoadedScenes()
        {
            Debug.Log("=== LOADED SCENES ===");
            Debug.Log($"Current Main Scene: {SceneFlowManager.Instance?.CurrentMainScene ?? "NONE"}"); // ✅ NEW
            Debug.Log($"Current UI Scene: {SceneFlowManager.Instance?.CurrentUIScene ?? "NONE"}"); // ✅ ADD THIS
            Debug.Log($"Active Scene: {SceneManager.GetActiveScene().name}");
            Debug.Log($"Total Loaded Scenes: {SceneManager.sceneCount}");
            
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                string status = scene.isLoaded ? "✓ Loaded" : "✗ Unloaded";
                string active = SceneManager.GetActiveScene() == scene ? " [ACTIVE]" : "";
                
                // Show scene type from manifest
                string type = "";
                if (sceneManifest != null)
                {
                    if (sceneManifest.IsContentScene(scene.name)) type = "[CONTENT]";
                    else if (sceneManifest.IsUIScene(scene.name)) type = "[UI]";
                    else if (sceneManifest.IsPersistentScene(scene.name)) type = "[PERSISTENT]";
                }
                
                Debug.Log($"  [{i}] {type} {scene.name}: {status}{active}");
            }
        }
    }
}