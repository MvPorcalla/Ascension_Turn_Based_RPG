// ════════════════════════════════════════════
// Assets\Scripts\AppFlow\GameManager.cs
// Central game manager handling player state, saves, and scene transitions
// ════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Core;
using Ascension.Character.Stat;
using Ascension.Data.SO.Character;

namespace Ascension.App
{
    public class GameManager : MonoBehaviour, IGameService
    {
        #region Singleton
        public static GameManager Instance { get; private set; }
        #endregion
        
        #region Injected Controllers
        private SaveController _saveController;
        private PlayerStateController _playerStateController;
        private SceneController _sceneController;
        #endregion
        
        #region Runtime State
        [Header("Session Data (Read Only)")]
        [SerializeField] private float sessionPlayTime = 0f;
        #endregion
        
        #region Properties
        public CharacterStats CurrentPlayer => _playerStateController?.CurrentPlayer;
        public CharacterBaseStatsSO BaseStats => GetBaseStatsFromCharacterManager();
        public bool HasActivePlayer => _playerStateController != null && _playerStateController.HasActivePlayer;
        #endregion
        
        #region Events
        public event Action<CharacterStats> OnPlayerLoaded;
        public event Action OnPlayerSaved;
        public event Action OnNewGameStarted;
        #endregion
        
        #region Unity Callbacks
        private void Awake()
        {
            InitializeSingleton();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
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

        /// <summary>
        /// ✅ FIXED: Called by ServiceContainer after all services are registered
        /// This ensures dependencies are available
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[GameManager] Initializing...");
            
            InjectControllers();
            ValidateControllers();
            SubscribeToControllerEvents();
            
            Debug.Log("[GameManager] Ready");
        }

        private void InjectControllers()
        {
            var container = ServiceContainer.Instance;
            
            if (container == null)
            {
                Debug.LogError("[GameManager] ServiceContainer is null during initialization!");
                return;
            }

            _saveController = container.Get<SaveController>();
            _playerStateController = container.Get<PlayerStateController>();
            _sceneController = container.Get<SceneController>();
        }

        private void ValidateControllers()
        {
            if (_saveController == null)
                Debug.LogError("[GameManager] SaveController not found!");
            
            if (_playerStateController == null)
                Debug.LogError("[GameManager] PlayerStateController not found!");
            
            if (_sceneController == null)
                Debug.LogError("[GameManager] SceneController not found!");
        }

        private void SubscribeToControllerEvents()
        {
            if (_saveController != null)
            {
                _saveController.OnSaveCompleted += HandleSaveCompleted;
            }

            if (_playerStateController != null)
            {
                _playerStateController.OnPlayerLoaded += HandlePlayerLoaded;
            }
        }

        private void HandleSaveCompleted()
        {
            OnPlayerSaved?.Invoke();
        }

        private void HandlePlayerLoaded(CharacterStats stats)
        {
            OnPlayerLoaded?.Invoke(stats);
        }

        private void UnsubscribeFromEvents()
        {
            if (_saveController != null)
            {
                _saveController.OnSaveCompleted -= HandleSaveCompleted;
            }

            if (_playerStateController != null)
            {
                _playerStateController.OnPlayerLoaded -= HandlePlayerLoaded;
            }
        }
        #endregion
        
        #region Public API - Game Flow
        public void StartNewGame()
        {
            _playerStateController?.StartNewGame("Adventurer");
            sessionPlayTime = 0f;
            
            OnNewGameStarted?.Invoke();
            Debug.Log("[GameManager] New game started");
        }
        
        public void SetPlayerName(string playerName)
        {
            _playerStateController?.SetPlayerName(playerName);
        }

        public void CompleteAvatarCreation()
        {
            _playerStateController?.CompleteAvatarCreation();
        }
        
        public bool SaveGame()
        {
            if (!CanSave()) return false;
            
            return _saveController != null && _saveController.SaveGame(sessionPlayTime);
        }

        public bool LoadGame()
        {
            bool success = _saveController != null && _saveController.LoadGame();
            
            if (success)
            {
                sessionPlayTime = 0f;
                _playerStateController?.CompleteAvatarCreation();
            }
            
            return success;
        }
        
        public bool SaveExists()
        {
            return _saveController != null && _saveController.SaveExists();
        }
        
        public bool DeleteSave()
        {
            return _saveController != null && _saveController.DeleteSave();
        }

        private bool CanSave()
        {
            return _playerStateController != null && _playerStateController.CanSave();
        }
        #endregion
        
        #region Public API - Player Actions
        public bool AddExperience(int amount)
        {
            if (_playerStateController == null) return false;
            
            bool leveledUp = _playerStateController.AddExperience(amount);
            
            if (leveledUp)
            {
                SaveGame();
            }
            
            return leveledUp;
        }

        public void Heal(float amount) => _playerStateController?.Heal(amount);
        public void TakeDamage(float amount) => _playerStateController?.TakeDamage(amount);
        public void FullHeal() => _playerStateController?.FullHeal();
        #endregion
        
        #region Public API - Scene Management
        public void LoadScene(string sceneName) => _sceneController?.LoadScene(sceneName);
        public void LoadSceneWithSave(string sceneName) => _sceneController?.LoadSceneWithSave(sceneName, sessionPlayTime);
        public void GoToMainBase() => _sceneController?.GoToMainBase();
        public void ResetAndCreateNewCharacter() => _sceneController?.ResetAndCreateNewCharacter();
        public void ReturnToMainBase() => _sceneController?.ReturnToMainBase(sessionPlayTime);
        public void OnActivityCompleted() => _sceneController?.OnActivityCompleted(sessionPlayTime);
        #endregion

        #region Private Helpers
        private CharacterBaseStatsSO GetBaseStatsFromCharacterManager()
        {
            var charManager = ServiceContainer.Instance?.Get<Ascension.Character.Manager.CharacterManager>();
            return charManager?.BaseStats;
        }
        #endregion
    }
}