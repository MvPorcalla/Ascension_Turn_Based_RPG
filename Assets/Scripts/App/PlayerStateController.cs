// ════════════════════════════════════════════
// Assets\Scripts\AppFlow\PlayerStateController.cs
// Manages player state, validation, and player actions
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Core;
using Ascension.Character.Stat;
using Ascension.Character.Manager;

namespace Ascension.App
{
    public class PlayerStateController : MonoBehaviour, IGameService
    {
        #region Injected Dependencies
        private CharacterManager _characterManager;
        #endregion

        #region State Tracking
        [SerializeField] private bool isAvatarCreationComplete = false;
        #endregion

        #region Properties
        public CharacterStats CurrentPlayer => _characterManager?.CurrentPlayer;
        public bool HasActivePlayer => _characterManager != null && _characterManager.HasActivePlayer;
        public bool IsAvatarCreationComplete => isAvatarCreationComplete;
        #endregion

        #region Events
        public event System.Action<CharacterStats> OnPlayerLoaded;
        #endregion

        #region Initialization
        public void Initialize()
        {
            InjectDependencies();
            SubscribeToEvents();
        }

        private void InjectDependencies()
        {
            var container = ServiceContainer.Instance;
            _characterManager = container.Get<CharacterManager>();

            if (_characterManager == null)
                Debug.LogError("[PlayerStateController] CharacterManager not found!");
        }

        private void SubscribeToEvents()
        {
            if (_characterManager != null)
            {
                _characterManager.OnPlayerLoaded += HandlePlayerLoaded;
            }
        }

        private void OnDestroy()
        {
            if (_characterManager != null)
            {
                _characterManager.OnPlayerLoaded -= HandlePlayerLoaded;
            }
        }

        private void HandlePlayerLoaded(CharacterStats stats)
        {
            OnPlayerLoaded?.Invoke(stats);
        }
        #endregion

        #region Public Methods - Player State
        public void StartNewGame(string playerName = "Adventurer")
        {
            if (!ValidateSystemsReady()) return;

            _characterManager.CreateNewPlayer(playerName);
            isAvatarCreationComplete = false;
            
            Debug.Log($"[PlayerStateController] New game started: {playerName}");
        }

        public void SetPlayerName(string playerName)
        {
            if (!HasActivePlayer) return;
            
            CurrentPlayer.playerName = playerName;
            Debug.Log($"[PlayerStateController] Player name set: {playerName}");
        }

        public void CompleteAvatarCreation()
        {
            isAvatarCreationComplete = true;
            Debug.Log("[PlayerStateController] Avatar creation completed");
        }

        public void ResetPlayerState()
        {
            isAvatarCreationComplete = false;
            Debug.Log("[PlayerStateController] Player state reset");
        }
        #endregion

        #region Public Methods - Player Actions
        public bool AddExperience(int amount)
        {
            if (!HasActivePlayer) return false;
            
            int oldLevel = CurrentPlayer.Level;
            _characterManager.AddExperience(amount);
            
            bool leveledUp = CurrentPlayer.Level > oldLevel;
            
            if (leveledUp)
            {
                Debug.Log($"[PlayerStateController] Level up! Now level {CurrentPlayer.Level}");
            }
            
            return leveledUp;
        }

        public void Heal(float amount)
        {
            _characterManager?.Heal(amount);
        }

        public void TakeDamage(float amount)
        {
            _characterManager?.TakeDamage(amount);
        }

        public void FullHeal()
        {
            _characterManager?.FullHeal();
        }
        #endregion

        #region Public Methods - Validation
        public bool ValidateSystemsReady()
        {
            if (_characterManager == null)
            {
                Debug.LogError("[PlayerStateController] CharacterManager not initialized!");
                return false;
            }
            return true;
        }

        public bool CanSave()
        {
            if (!HasActivePlayer)
            {
                Debug.LogWarning("[PlayerStateController] No active player to save!");
                return false;
            }
            
            if (!isAvatarCreationComplete)
            {
                Debug.LogWarning("[PlayerStateController] Avatar creation not complete!");
                return false;
            }
            
            return true;
        }
        #endregion
    }
}