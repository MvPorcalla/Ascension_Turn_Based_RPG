// ════════════════════════════════════════════
// Assets\Scripts\Core\Bootstrap.cs
// Application bootstrapper handling initial scene routing
// ════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.App;

namespace Ascension.Core
{
    public class Bootstrap : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Scene Routing")]
        [SerializeField] private string avatarCreationScene = "02_AvatarCreation";
        [SerializeField] private string mainBaseScene = "03_MainBase";

        [Header("Splash / Load Timing")]
        [SerializeField] private float minimumLoadTime = 1f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        #endregion

        #region Unity Callbacks
        private IEnumerator Start()
        {
            float startTime = Time.time;
            Log("Initializing...");

            // Wait for ServiceContainer to complete initialization
            yield return StartCoroutine(WaitForServiceContainer());
            Log("ServiceContainer ready");

            // Ensure minimum load time for smooth UX
            yield return EnsureMinimumLoadTime(startTime);

            // Determine target scene based on save state
            string targetScene = DetermineTargetScene();
            Log($"Loading scene: {targetScene}");
            
            SceneManager.LoadScene(targetScene);
        }
        #endregion

        #region Initialization
        private IEnumerator WaitForServiceContainer()
        {
            // Wait for ServiceContainer singleton
            while (ServiceContainer.Instance == null)
            {
                yield return null;
            }

            Log("ServiceContainer instance found, waiting for initialization...");

            // Wait for full initialization
            while (!ServiceContainer.Instance.IsInitialized)
            {
                yield return null;
            }

            Log("✓ ServiceContainer fully initialized");
        }
        #endregion

        #region Scene Logic
        private string DetermineTargetScene()
        {
            // Validate GameManager exists
            if (GameManager.Instance == null)
            {
                LogError("GameManager not found! Going to avatar creation as fallback.");
                return avatarCreationScene;
            }

            // Check if save exists
            if (!GameManager.Instance.SaveExists())
            {
                Log("No save found — going to avatar creation");
                return avatarCreationScene;
            }

            // Save exists - try to load
            Log("Save found — attempting to load...");
            
            if (GameManager.Instance.LoadGame())
            {
                string playerName = GameManager.Instance.CurrentPlayer?.playerName ?? "Unknown";
                Log($"Save loaded successfully — Welcome back, {playerName}!");
                return mainBaseScene;
            }

            // Load failed - go to character creation
            Log("Save corrupted or load failed — going to character creation");
            return avatarCreationScene;
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
        #endregion
    }
}