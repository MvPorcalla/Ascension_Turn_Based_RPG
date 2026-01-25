// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Core/SceneFlowManager.cs
// ✅ REFACTORED: Singleton removed - access only via GameBootstrap
// Scene orchestration system for additive scene management
// ════════════════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.Core;
using Ascension.Data.Config;

namespace Ascension.Core
{
    /// <summary>
    /// Core system for scene orchestration
    /// Manages main scenes (content) and UI scenes (additive overlays)
    /// ✅ Access via: GameBootstrap.SceneFlow (NOT via singleton)
    /// </summary>
    public class SceneFlowManager : MonoBehaviour
    {
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
        
        [Header("Main Scene Loading Mode")]
        [Tooltip("If true, main scenes load additively and old ones are unloaded. If false, main scenes replace each other (Single mode).")]
        [SerializeField] private bool useAdditiveForMainScenes = true;

        [Header("Settings")]
        [SerializeField] private bool enableDebugLogs = true;
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

        #region Initialization
        /// <summary>
        /// ✅ Called by GameBootstrap during initialization
        /// No Awake() needed - GameBootstrap handles DontDestroyOnLoad
        /// </summary>
        public void Init()
        {
            if (!ValidateSceneConfig())
            {
                Debug.LogError("[SceneFlowManager] Initialization failed - SceneManifest invalid!", this);
                throw new System.InvalidOperationException("SceneFlowManager requires valid SceneManifest");
            }

            Log("SceneFlowManager initialized successfully");
        }

        private bool ValidateSceneConfig()
        {
            if (sceneManifest == null)
            {
                Debug.LogError("[SceneFlowManager] SceneManifest not assigned!", this);
                return false;
            }

            Log($"Scene manifest loaded: {sceneManifest.name}");
            return true;
        }
        #endregion

        #region Public API - State Management
        /// <summary>
        /// Set current main scene (called by GameBootstrap after scene loads)
        /// </summary>
        public void SetCurrentMainScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                LogWarning("Attempted to set null/empty main scene name");
                return;
            }

            _currentMainScene = sceneName;
            Log($"Current main scene set: {sceneName}");
        }

        /// <summary>
        /// Set current UI scene (internal tracking)
        /// </summary>
        public void SetCurrentUIScene(string sceneName)
        {
            _currentUIScene = string.IsNullOrEmpty(sceneName) ? null : sceneName;
            Log(_currentUIScene != null
                ? $"Current UI scene set: {sceneName}"
                : "UI scene cleared");
        }
        #endregion

        #region Public API - Main Scene Loading
        /// <summary>
        /// Load a main content scene (validates scene type)
        /// ✅ Validates scene exists in manifest and is correct type
        /// </summary>
        public void LoadMainScene(string sceneName)
        {
            // Validation 1: Not already transitioning
            if (_isTransitioning)
            {
                LogWarning($"Cannot load {sceneName} - already transitioning");
                return;
            }

            // Validation 2: Scene exists in manifest
            if (!sceneManifest.HasScene(sceneName))
            {
                Debug.LogError($"[SceneFlow] Scene '{sceneName}' not found in SceneManifest!");
                return;
            }

            // Validation 3: Not a persistent scene
            if (sceneManifest.IsPersistentScene(sceneName))
            {
                Debug.LogError($"[SceneFlow] Cannot load persistent scene '{sceneName}' as main scene!");
                return;
            }

            // Validation 4: Not a UI scene
            if (sceneManifest.IsUIScene(sceneName))
            {
                Debug.LogError($"[SceneFlow] '{sceneName}' is a UI scene - use OpenUIScene() instead!");
                return;
            }

            // ✅ All validations passed - proceed with loading
            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(LoadMainSceneCoroutine(sceneName));
        }

        // Convenience methods
        public void GoToMainBase() => LoadMainScene(SCENE_MAIN_BASE);
        public void GoToAvatarCreation() => LoadMainScene(SCENE_AVATAR_CREATION);
        public void GoToTown() => LoadMainScene(SCENE_TOWN);
        public void GoToDungeon() => LoadMainScene(SCENE_DUNGEON);
        #endregion

        #region Public API - UI Scene Loading (Convenience Methods)
        /// <summary>
        /// Open World Map UI scene
        /// </summary>
        public void OpenWorldMap() => OpenUIScene(SCENE_UI_WORLD_MAP);

        /// <summary>
        /// Open Profile UI scene
        /// </summary>
        public void OpenProfile() => OpenUIScene(SCENE_UI_PROFILE);

        /// <summary>
        /// Open Inventory UI scene
        /// </summary>
        public void OpenInventory() => OpenUIScene(SCENE_UI_INVENTORY);

        /// <summary>
        /// Open Quest UI scene
        /// </summary>
        public void OpenQuest() => OpenUIScene(SCENE_UI_QUEST);

        /// <summary>
        /// Open Codex UI scene
        /// </summary>
        public void OpenCodex() => OpenUIScene(SCENE_UI_CODEX);

        /// <summary>
        /// Open Storage UI scene
        /// </summary>
        public void OpenStorage() => OpenUIScene(SCENE_UI_STORAGE);

        /// <summary>
        /// Open Cooking UI scene
        /// </summary>
        public void OpenCooking() => OpenUIScene(SCENE_UI_COOKING);

        /// <summary>
        /// Open Brewing UI scene
        /// </summary>
        public void OpenBrewing() => OpenUIScene(SCENE_UI_BREWING);

        /// <summary>
        /// Open Crafting UI scene
        /// </summary>
        public void OpenCrafting() => OpenUIScene(SCENE_UI_CRAFTING);
        #endregion

        #region Public API - UI Scene Loading (Generic)
        /// <summary>
        /// Open a UI overlay scene (validates scene type)
        /// ✅ Validates scene exists and is correct type
        /// </summary>
        public void OpenUIScene(string sceneName)
        {
            // Validation 1: Not already transitioning
            if (_isTransitioning)
            {
                LogWarning($"Cannot open {sceneName} - already transitioning");
                return;
            }

            // Validation 2: Scene exists in manifest
            if (!sceneManifest.HasScene(sceneName))
            {
                Debug.LogError($"[SceneFlow] Scene '{sceneName}' not found in SceneManifest!");
                return;
            }

            // Validation 3: Must be a UI scene
            if (!sceneManifest.IsUIScene(sceneName))
            {
                Debug.LogError($"[SceneFlow] '{sceneName}' is not a UI scene - use LoadMainScene() instead!");
                return;
            }

            // ✅ All validations passed - proceed with loading
            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(OpenUISceneCoroutine(sceneName));
        }

        /// <summary>
        /// Close the currently open UI scene
        /// </summary>
        public void CloseCurrentUIScene()
        {
            if (string.IsNullOrEmpty(_currentUIScene))
            {
                LogWarning("No UI scene to close");
                return;
            }

            if (_isTransitioning)
            {
                LogWarning("Cannot close UI scene - already transitioning");
                return;
            }

            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(CloseCurrentUISceneCoroutine());
        }

        /// <summary>
        /// Toggle a UI scene (open if closed, close if open)
        /// </summary>
        public void ToggleUIScene(string sceneName)
        {
            if (_currentUIScene == sceneName)
                CloseCurrentUIScene();
            else
                OpenUIScene(sceneName);
        }
        #endregion

        #region Coroutines - Main Scene Loading
        /// <summary>
        /// Load main scene coroutine
        /// Supports both Single and Additive loading modes
        /// </summary>
        private IEnumerator LoadMainSceneCoroutine(string sceneName)
        {
            _isTransitioning = true;

            // ✅ Notify systems that scene is changing
            GameEvents.TriggerSceneChanging(sceneName);

            if (useAdditiveForMainScenes)
            {
                // ════════════════════════════════════════════════════════
                // ADDITIVE MODE: Load new scene, then unload old one
                // ════════════════════════════════════════════════════════
                Log($"Loading {sceneName} (Additive)");
                
                AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                
                if (loadOp == null)
                {
                    Debug.LogError($"[SceneFlow] Failed to start loading {sceneName}");
                    _isTransitioning = false;
                    yield break;
                }

                yield return loadOp;

                Scene newScene = SceneManager.GetSceneByName(sceneName);
                if (newScene.IsValid() && newScene.isLoaded)
                {
                    SceneManager.SetActiveScene(newScene);
                    Log($"✓ {sceneName} set as active scene");
                }
                else
                {
                    Debug.LogError($"[SceneFlow] Scene {sceneName} invalid after load");
                    _isTransitioning = false;
                    yield break;
                }

                // Unload previous main scene
                if (!string.IsNullOrEmpty(_currentMainScene) && _currentMainScene != sceneName)
                {
                    Log($"Unloading old scene: {_currentMainScene}");
                    Scene oldScene = SceneManager.GetSceneByName(_currentMainScene);
                    
                    if (oldScene.IsValid())
                    {
                        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(oldScene);
                        yield return unloadOp;
                        Log($"✓ {_currentMainScene} unloaded");
                    }
                }
            }
            else
            {
                // ════════════════════════════════════════════════════════
                // SINGLE MODE: Replace current scene entirely
                // ════════════════════════════════════════════════════════
                Log($"Loading {sceneName} (Single - replaces current)");
                
                AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                
                if (loadOp == null)
                {
                    Debug.LogError($"[SceneFlow] Failed to start loading {sceneName}");
                    _isTransitioning = false;
                    yield break;
                }

                yield return loadOp;

                Scene newScene = SceneManager.GetSceneByName(sceneName);
                if (newScene.IsValid())
                {
                    SceneManager.SetActiveScene(newScene);
                    Log($"✓ {sceneName} loaded and set as active");
                }
            }

            _currentMainScene = sceneName;
            _isTransitioning = false;
            
            // ✅ Trigger event for UI updates
            GameEvents.TriggerSceneLoaded(sceneName);
            Log($"=== Scene transition complete: {sceneName} ===");
        }
        #endregion

        #region Coroutines - UI Scene Loading
        /// <summary>
        /// Open UI scene coroutine
        /// UI scenes are ALWAYS loaded additively
        /// </summary>
        private IEnumerator OpenUISceneCoroutine(string sceneName)
        {
            _isTransitioning = true;
            
            // Close any existing UI scene first
            if (!string.IsNullOrEmpty(_currentUIScene))
            {
                Log($"Closing existing UI scene before opening {sceneName}");
                yield return CloseCurrentUISceneCoroutine();
            }

            Log($"Opening UI scene: {sceneName}");
            
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            if (loadOp == null)
            {
                Debug.LogError($"[SceneFlow] Failed to start loading UI scene {sceneName}");
                _isTransitioning = false;
                yield break;
            }

            yield return loadOp;

            Scene uiScene = SceneManager.GetSceneByName(sceneName);
            if (!uiScene.IsValid() || !uiScene.isLoaded)
            {
                Debug.LogError($"[SceneFlow] UI scene {sceneName} invalid after load");
                _isTransitioning = false;
                yield break;
            }

            _currentUIScene = sceneName;
            _isTransitioning = false;
            
            Log($"✓ UI scene opened: {sceneName}");
        }

        /// <summary>
        /// Close current UI scene coroutine
        /// </summary>
        private IEnumerator CloseCurrentUISceneCoroutine()
        {
            if (string.IsNullOrEmpty(_currentUIScene))
            {
                _isTransitioning = false;
                yield break;
            }

            string sceneToClose = _currentUIScene;
            Log($"Closing UI scene: {sceneToClose}");
            
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToClose);
            
            if (unloadOp == null)
            {
                Debug.LogError($"[SceneFlow] Failed to unload UI scene {sceneToClose}");
                _isTransitioning = false;
                yield break;
            }

            yield return unloadOp;

            _currentUIScene = null;
            _isTransitioning = false;
            
            Log($"✓ UI scene closed: {sceneToClose}");
        }
        #endregion

        #region Logging Helpers
        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[SceneFlow] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SceneFlow] {message}");
        }
        #endregion
    }
}