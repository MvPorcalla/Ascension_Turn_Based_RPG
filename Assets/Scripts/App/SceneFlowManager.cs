// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/App/SceneFlowManager.cs
// ✅ FIXED: Added SetCurrentMainScene() for Bootstrap integration
// ════════════════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.Core;
using Ascension.Data.Config;

namespace Ascension.App
{
    public class SceneFlowManager : MonoBehaviour, IGameService
    {
        #region Singleton
        public static SceneFlowManager Instance { get; private set; }
        #endregion
        
        #region Scene Name Constants
        public const string SCENE_MAIN_BASE = "03_MainBase";
        public const string SCENE_AVATAR_CREATION = "02_AvatarCreation";
        public const string SCENE_TOWN = "05_Town";
        public const string SCENE_DUNGEON = "06_Dungeon";
        
        public const string SCENE_UI_WORLD_MAP = "UI_WorldMap";
        public const string SCENE_UI_PROFILE = "UI_Profile";
        public const string SCENE_UI_INVENTORY = "UI_Inventory";
        public const string SCENE_UI_QUEST = "UI_Quest";
        public const string SCENE_UI_CODEX = "UI_Codex";
        public const string SCENE_UI_STORAGE = "UI_Storage";
        public const string SCENE_UI_COOKING = "UI_Cooking";
        public const string SCENE_UI_BREWING = "UI_Brewing";
        public const string SCENE_UI_CRAFTING = "UI_Crafting";
        #endregion
        
        #region Scene Configuration
        [Header("Scene Configuration")]
        [SerializeField] private SceneManifest sceneManifest;
        #endregion
        
        #region State Management
        private string _currentUIScene = null;
        private string _currentMainScene = null;
        private bool _isTransitioning = false;
        private Coroutine _transitionCoroutine = null;
        #endregion
        
        #region Properties
        public string CurrentUIScene => _currentUIScene;
        public string CurrentMainScene => _currentMainScene;
        public bool IsTransitioning => _isTransitioning;
        public bool HasUISceneOpen => !string.IsNullOrEmpty(_currentUIScene);
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
        }
        #endregion
        
        #region IGameService
        public void Initialize()
        {
            ValidateSceneConfig();
            Debug.Log("[SceneFlowManager] Ready");
        }

        private void ValidateSceneConfig()
        {
            if (sceneManifest == null)
            {
                Debug.LogError("[SceneFlowManager] SceneManifest not assigned!", this);
            }
            else
            {
                Debug.Log($"[SceneFlowManager] Scene manifest loaded: {sceneManifest.name}");
            }
        }
        #endregion
        
        // ════════════════════════════════════════════════════════════════
        // ✅ NEW: BOOTSTRAP INTEGRATION
        // ════════════════════════════════════════════════════════════════
        
        /// <summary>
        /// ✅ CRITICAL: Called by Bootstrap after loading a main scene
        /// This ensures SceneFlowManager tracks scenes loaded during initialization
        /// </summary>
        public void SetCurrentMainScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[SceneFlow] Attempted to set empty main scene name");
                return;
            }
            
            _currentMainScene = sceneName;
            Debug.Log($"[SceneFlow] Current main scene set: {sceneName}");
        }
        
        /// <summary>
        /// ✅ CRITICAL: Called by Bootstrap after loading a UI scene
        /// </summary>
        public void SetCurrentUIScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                _currentUIScene = null;
                Debug.Log("[SceneFlow] UI scene cleared");
                return;
            }
            
            _currentUIScene = sceneName;
            Debug.Log($"[SceneFlow] Current UI scene set: {sceneName}");
        }
        
        // ════════════════════════════════════════════════════════════════
        // PUBLIC API - Additive UI Scene Loading
        // ════════════════════════════════════════════════════════════════
        
        public void OpenUIScene(string sceneName)
        {
            if (sceneManifest != null)
            {
                if (!sceneManifest.IsUIScene(sceneName))
                {
                    Debug.LogError($"[SceneFlow] '{sceneName}' is not a valid UI scene!");
                    Debug.LogError($"Valid UI scenes: {sceneManifest.GetSceneNamesFormatted(SceneCategory.UI)}");
                    return;
                }
            }
            
            if (_isTransitioning)
            {
                Debug.LogWarning($"[SceneFlow] Transition already in progress, ignoring request for '{sceneName}'");
                return;
            }
            
            if (_currentUIScene == sceneName)
            {
                Debug.LogWarning($"[SceneFlow] '{sceneName}' is already open");
                return;
            }
            
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }
            
            _transitionCoroutine = StartCoroutine(OpenUISceneCoroutine(sceneName));
        }
        
        public void CloseCurrentUIScene()
        {
            if (string.IsNullOrEmpty(_currentUIScene))
            {
                Debug.LogWarning("[SceneFlow] No UI scene currently open");
                return;
            }
            
            if (_isTransitioning)
            {
                Debug.LogWarning("[SceneFlow] Transition in progress, cannot close");
                return;
            }
            
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }
            
            _transitionCoroutine = StartCoroutine(CloseCurrentUISceneCoroutine());
        }
        
        private IEnumerator OpenUISceneCoroutine(string sceneName)
        {
            _isTransitioning = true;
            
            if (!string.IsNullOrEmpty(_currentUIScene))
            {
                Debug.Log($"[SceneFlow] Closing '{_currentUIScene}' before opening '{sceneName}'");
                yield return CloseCurrentUISceneCoroutine();
            }
            
            Debug.Log($"[SceneFlow] Loading UI scene: {sceneName}");
            
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            if (loadOp == null)
            {
                Debug.LogError($"[SceneFlow] Failed to start loading '{sceneName}'");
                _isTransitioning = false;
                yield break;
            }
            
            yield return loadOp;
            
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                Debug.LogError($"[SceneFlow] Failed to load '{sceneName}'");
                _isTransitioning = false;
                yield break;
            }
            
            _currentUIScene = sceneName;
            _isTransitioning = false;
            
            Debug.Log($"[SceneFlow] ✓ UI scene loaded: {sceneName}");
        }
        
        private IEnumerator CloseCurrentUISceneCoroutine()
        {
            if (string.IsNullOrEmpty(_currentUIScene))
            {
                yield break;
            }
            
            string sceneToClose = _currentUIScene;
            _isTransitioning = true;
            
            Debug.Log($"[SceneFlow] Unloading UI scene: {sceneToClose}");
            
            Scene scene = SceneManager.GetSceneByName(sceneToClose);
            
            if (!scene.IsValid())
            {
                Debug.LogWarning($"[SceneFlow] Scene '{sceneToClose}' not found (already unloaded?)");
                _currentUIScene = null;
                _isTransitioning = false;
                yield break;
            }
            
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(scene);
            
            if (unloadOp == null)
            {
                Debug.LogError($"[SceneFlow] Failed to start unloading '{sceneToClose}'");
                _isTransitioning = false;
                yield break;
            }
            
            yield return unloadOp;
            
            _currentUIScene = null;
            _isTransitioning = false;
            
            Debug.Log($"[SceneFlow] ✓ UI scene unloaded: {sceneToClose}");
        }
        
        // ════════════════════════════════════════════════════════════════
        // PUBLIC API - Convenience Methods
        // ════════════════════════════════════════════════════════════════
        
        public void OpenWorldMap() => OpenUIScene(SCENE_UI_WORLD_MAP);
        public void OpenProfile() => OpenUIScene(SCENE_UI_PROFILE);
        public void OpenInventory() => OpenUIScene(SCENE_UI_INVENTORY);
        public void OpenQuest() => OpenUIScene(SCENE_UI_QUEST);
        public void OpenCodex() => OpenUIScene(SCENE_UI_CODEX);
        public void OpenStorage() => OpenUIScene(SCENE_UI_STORAGE);
        public void OpenCooking() => OpenUIScene(SCENE_UI_COOKING);
        public void OpenBrewing() => OpenUIScene(SCENE_UI_BREWING);
        public void OpenCrafting() => OpenUIScene(SCENE_UI_CRAFTING);
        
        public void ToggleUIScene(string sceneName)
        {
            if (_currentUIScene == sceneName)
            {
                CloseCurrentUIScene();
            }
            else
            {
                OpenUIScene(sceneName);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // PUBLIC API - Main Scene Loading
        // ════════════════════════════════════════════════════════════════

        public void LoadMainScene(string sceneName)
        {
            if (sceneManifest != null && !sceneManifest.IsContentScene(sceneName))
            {
                Debug.LogError($"[SceneFlow] '{sceneName}' is not a content scene!");
                Debug.LogError($"Valid content scenes: {sceneManifest.GetSceneNamesFormatted(SceneCategory.Content)}");
                return;
            }
            
            if (_isTransitioning)
            {
                Debug.LogWarning($"[SceneFlow] Transition already in progress!");
                return;
            }
            
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }
            
            _transitionCoroutine = StartCoroutine(LoadMainSceneCoroutine(sceneName));
        }

        private IEnumerator LoadMainSceneCoroutine(string sceneName)
        {
            _isTransitioning = true;
            
            Debug.Log($"[SceneFlow] Loading main scene: {sceneName}");
            
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            if (loadOp == null)
            {
                Debug.LogError($"[SceneFlow] Failed to start loading '{sceneName}'");
                _isTransitioning = false;
                yield break;
            }
            
            yield return loadOp;
            
            Scene newScene = SceneManager.GetSceneByName(sceneName);
            if (newScene.IsValid() && newScene.isLoaded)
            {
                SceneManager.SetActiveScene(newScene);
                Debug.Log($"[SceneFlow] Set '{sceneName}' as active scene");
            }
            else
            {
                Debug.LogError($"[SceneFlow] Failed to load '{sceneName}'");
                _isTransitioning = false;
                yield break;
            }
            
            if (!string.IsNullOrEmpty(_currentMainScene) && _currentMainScene != sceneName)
            {
                Scene oldScene = SceneManager.GetSceneByName(_currentMainScene);
                if (oldScene.IsValid())
                {
                    Debug.Log($"[SceneFlow] Unloading previous main scene: {_currentMainScene}");
                    yield return SceneManager.UnloadSceneAsync(oldScene);
                }
            }
            
            _currentMainScene = sceneName;
            _isTransitioning = false;
            
            Debug.Log($"[SceneFlow] ✓ Main scene loaded: {sceneName}");
        }

        public void GoToMainBase() => LoadMainScene(SCENE_MAIN_BASE);
        public void GoToAvatarCreation() => LoadMainScene(SCENE_AVATAR_CREATION);
        public void GoToTown() => LoadMainScene(SCENE_TOWN);
        public void GoToDungeon() => LoadMainScene(SCENE_DUNGEON);
        
        [ContextMenu("Debug: Force Close Current UI")]
        private void DebugForceClose()
        {
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }
            
            _isTransitioning = false;
            CloseCurrentUIScene();
        }
    }
}