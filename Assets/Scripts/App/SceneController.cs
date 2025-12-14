// ════════════════════════════════════════════
// Assets\Scripts\AppFlow\SceneController.cs
// Handles scene routing and transitions
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.Core;

namespace Ascension.App
{
    public class SceneController : MonoBehaviour, IGameService
    {
        #region Injected Dependencies
        private SaveController _saveController;
        private PlayerStateController _playerStateController;
        #endregion

        #region Initialization
        public void Initialize()
        {
            InjectDependencies();
        }

        private void InjectDependencies()
        {
            var container = ServiceContainer.Instance;
            
            _saveController = container.Get<SaveController>();
            _playerStateController = container.Get<PlayerStateController>();

            if (_saveController == null)
                Debug.LogError("[SceneController] SaveController not found!");
            
            if (_playerStateController == null)
                Debug.LogError("[SceneController] PlayerStateController not found!");
        }
        #endregion

        #region Public Methods - Basic Scene Loading
        public void LoadScene(string sceneName)
        {
            Debug.Log($"[SceneController] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        public void LoadSceneWithSave(string sceneName, float sessionPlayTime)
        {
            if (_saveController != null && _playerStateController.CanSave())
            {
                _saveController.SaveGame(sessionPlayTime);
            }
            
            LoadScene(sceneName);
        }
        #endregion

        #region Public Methods - Named Scene Routes
        public void GoToMainBase()
        {
            LoadScene("03_MainBase");
        }

        public void GoToAvatarCreation()
        {
            LoadScene("02_AvatarCreation");
        }

        public void ReturnToMainBase(float sessionPlayTime)
        {
            LoadSceneWithSave("03_MainBase", sessionPlayTime);
        }
        #endregion

        #region Public Methods - Flow Control
        public void ResetAndCreateNewCharacter()
        {
            if (_saveController != null)
            {
                _saveController.DeleteSave();
            }

            if (_playerStateController != null)
            {
                _playerStateController.StartNewGame();
            }

            GoToAvatarCreation();
            Debug.Log("[SceneController] Reset complete, going to avatar creation");
        }

        public void OnActivityCompleted(float sessionPlayTime)
        {
            if (_saveController != null && _playerStateController.CanSave())
            {
                _saveController.SaveGame(sessionPlayTime);
                Debug.Log("[SceneController] Progress saved after activity");
            }
        }
        #endregion
    }
}