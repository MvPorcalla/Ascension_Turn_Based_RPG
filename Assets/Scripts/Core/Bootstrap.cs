// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Core/Bootstrap.cs
// ✅ FIXED: Added exception handling in DetermineTargetScene() to prevent crash on corrupt saves
// ════════════════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using Ascension.App;
using Ascension.Controllers;

namespace Ascension.Core
{
    /// <summary>
    /// ⚠️ IMPORTANT: This scene loads SECOND (after Disclaimer acceptance)
    /// - Initializes all game systems
    /// - Loads PersistentUI
    /// - Determines which scene to load next (AvatarCreation or MainBase)
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        #region Scene Name Constants
        private const string SCENE_AVATAR_CREATION = "02_AvatarCreation";
        private const string SCENE_MAIN_BASE = "03_MainBase";
        #endregion
        
        #region Serialized Fields
        [Header("Persistent UI (In This Scene)")]
        [SerializeField] private GameObject persistentUICanvas;
        [SerializeField] private PersistentUIController persistentUIController;

        [Header("Splash / Load Timing")]
        [SerializeField] private float minimumLoadTime = 1f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        #endregion

        #region Unity Callbacks
        private IEnumerator Start()
        {
            float startTime = Time.time;
            Log("Bootstrap starting - initializing game systems...");

            // ✅ Setup persistent UI first
            SetupPersistentUI();

            // ✅ Wait for all managers to initialize
            yield return StartCoroutine(WaitForServiceContainer());
            Log("✓ All game systems ready");

            // ✅ Ensure minimum load time for smooth UX
            yield return EnsureMinimumLoadTime(startTime);

            // ✅ Determine which scene to load based on save state
            string targetScene = DetermineTargetScene();
            Log($"Loading initial scene: {targetScene}");
            
            // ✅ Load target scene additively
            yield return StartCoroutine(LoadSceneAdditiveAsync(targetScene));
            
            Log("✓ Bootstrap complete - staying loaded as persistent scene");
        }
        #endregion

        #region Initialization
        private void SetupPersistentUI()
        {
            if (persistentUICanvas != null)
            {
                DontDestroyOnLoad(persistentUICanvas);
                Log("✓ PersistentUICanvas set to DontDestroyOnLoad");
            }
            else
            {
                LogError("PersistentUICanvas not assigned! Assign it in Inspector.");
            }

            if (persistentUIController != null)
            {
                Log("✓ PersistentUIController found and ready");
            }
            else
            {
                LogError("PersistentUIController not assigned! Assign it in Inspector.");
            }
        }

        private IEnumerator WaitForServiceContainer()
        {
            // Wait for ServiceContainer singleton to exist
            while (ServiceContainer.Instance == null)
            {
                yield return null;
            }

            Log("ServiceContainer instance found, waiting for initialization...");

            // Wait for full initialization of all managers
            while (!ServiceContainer.Instance.IsInitialized)
            {
                yield return null;
            }

            Log("✓ ServiceContainer fully initialized");
        }
        #endregion

        #region Scene Logic
        /// <summary>
        /// ✅ CRITICAL FIX: Added exception handling to prevent crash on corrupt saves
        /// 
        /// BEHAVIOR:
        /// - No save exists → AvatarCreation
        /// - Save exists + loads successfully → MainBase
        /// - Save exists + corrupted/throws exception → AvatarCreation (fallback)
        /// 
        /// This ensures Bootstrap NEVER crashes, even if save file is malformed
        /// </summary>
        private string DetermineTargetScene()
        {
            // Validate GameManager exists
            if (GameManager.Instance == null)
            {
                LogError("GameManager not found! Going to avatar creation as fallback.");
                return SCENE_AVATAR_CREATION;
            }

            // Check if save exists
            if (!GameManager.Instance.SaveExists())
            {
                Log("No save found — going to avatar creation");
                return SCENE_AVATAR_CREATION;
            }

            // ✅ CRITICAL FIX: Wrap LoadGame() in try-catch
            // Reason: LoadGame() can throw if save file is corrupted, has missing fields,
            // or if JSON deserialization fails. Without this, Bootstrap crashes.
            Log("Save found — attempting to load...");
            
            try
            {
                if (GameManager.Instance.LoadGame())
                {
                    string playerName = GameManager.Instance.CurrentPlayer?.playerName ?? "Unknown";
                    Log($"✓ Save loaded successfully — Welcome back, {playerName}!");
                    return SCENE_MAIN_BASE;
                }
                else
                {
                    // LoadGame returned false (soft failure, no exception)
                    Log("Save load returned false — going to character creation");
                    return SCENE_AVATAR_CREATION;
                }
            }
            catch (System.Exception e)
            {
                // ✅ NEW: Catch any exceptions during load (corrupt file, missing data, etc.)
                LogError($"Save load failed with exception: {e.Message}");
                LogError($"Stack trace: {e.StackTrace}");
                Log("Falling back to character creation due to load error");
                return SCENE_AVATAR_CREATION;
            }
        }

        /// <summary>
        /// Load a scene additively and notify SceneFlowManager
        /// </summary>
        private IEnumerator LoadSceneAdditiveAsync(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            if (asyncLoad == null)
            {
                LogError($"Failed to start loading scene: {sceneName}");
                yield break;
            }
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedScene);
                Log($"✓ Scene '{sceneName}' set as active scene");
                
                // ✅ CRITICAL: Notify SceneFlowManager
                NotifySceneFlowManager(sceneName);
            }
            else
            {
                LogError($"Failed to load scene: {sceneName}");
            }
        }
        
        /// <summary>
        /// Notify SceneFlowManager about loaded scenes
        /// This ensures SceneFlowManager's state stays synchronized
        /// </summary>
        private void NotifySceneFlowManager(string sceneName)
        {
            if (SceneFlowManager.Instance == null)
            {
                LogWarning("SceneFlowManager not available - cannot notify");
                return;
            }
            
            // Determine if this is a main content scene
            if (sceneName == SCENE_MAIN_BASE || 
                sceneName == SCENE_AVATAR_CREATION ||
                sceneName.StartsWith("05_") || // Future: Town, etc.
                sceneName.StartsWith("06_"))   // Future: Dungeon, etc.
            {
                SceneFlowManager.Instance.SetCurrentMainScene(sceneName);
                Log($"✓ Notified SceneFlowManager: Main scene = {sceneName}");
            }
            else if (sceneName.StartsWith("UI_"))
            {
                SceneFlowManager.Instance.SetCurrentUIScene(sceneName);
                Log($"✓ Notified SceneFlowManager: UI scene = {sceneName}");
            }
        }
        #endregion

        #region Helper Methods
        private IEnumerator EnsureMinimumLoadTime(float startTime)
        {
            float elapsed = Time.time - startTime;

            if (elapsed < minimumLoadTime)
            {
                float remaining = minimumLoadTime - elapsed;
                Log($"Waiting {remaining:F1}s to meet minimum load time...");
                yield return new WaitForSeconds(remaining);
            }
        }

        private void Log(string message)
        {
            if (showDebugLogs) Debug.Log($"[Bootstrap] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[Bootstrap] {message}");
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[Bootstrap] {message}");
        }
        #endregion

        #region Debug Helpers
        [ContextMenu("Debug: Print Scene Constants")]
        private void DebugPrintSceneConstants()
        {
            Debug.Log("=== SCENE CONSTANTS ===");
            Debug.Log($"Avatar Creation Scene: {SCENE_AVATAR_CREATION}");
            Debug.Log($"Main Base Scene: {SCENE_MAIN_BASE}");
        }

        [ContextMenu("Debug: Print Service Container Status")]
        private void DebugPrintServiceStatus()
        {
            if (ServiceContainer.Instance != null)
            {
                Debug.Log(ServiceContainer.Instance.GetSystemStatus());
            }
            else
            {
                Debug.LogError("ServiceContainer not found!");
            }
        }
        #endregion
    }
}