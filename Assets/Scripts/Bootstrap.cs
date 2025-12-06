// ════════════════════════════════════════════════════════════════════════
// Bootstrap.cs
// Entry point for game initialization and scene routing based on save state
// Waits for GameSystemHub before proceeding
// ════════════════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.Managers;

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

        [Header("System Initialization")]
        [SerializeField] private float systemTimeoutSeconds = 10f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        #endregion

        #region Private Fields
        private const int FRAME_WAIT_INTERVAL = 10;
        #endregion

        #region Unity Callbacks
        private IEnumerator Start()
        {
            float startTime = Time.time;
            Log("Initializing...");

            // Wait for GameSystemHub to be ready
            yield return StartCoroutine(WaitForSystemsReady());

            Log("All systems initialized");

            // Ensure minimum load time for smooth UX
            yield return EnsureMinimumLoadTime(startTime);

            // Determine and load target scene
            string targetScene = DetermineTargetScene();
            Log($"Loading scene: {targetScene}");
            SceneManager.LoadScene(targetScene);
        }
        #endregion

        #region System Initialization
        /// <summary>
        /// Wait for GameSystemHub and all critical systems to initialize
        /// </summary>
        private IEnumerator WaitForSystemsReady()
        {
            Log("Waiting for GameSystemHub...");
            
            float elapsedTime = 0f;
            int frameCount = 0;

            // Wait for Hub to exist and initialize
            while ((GameSystemHub.Instance == null || !GameSystemHub.Instance.IsInitialized) 
                   && elapsedTime < systemTimeoutSeconds)
            {
                frameCount++;
                
                // Log progress every N frames
                if (frameCount % FRAME_WAIT_INTERVAL == 0)
                {
                    Log($"Still waiting for Hub... ({elapsedTime:F1}s elapsed)");
                }

                yield return null;
                elapsedTime += Time.deltaTime;
            }

            // Check for timeout
            if (GameSystemHub.Instance == null)
            {
                LogError($"CRITICAL: GameSystemHub not found after {systemTimeoutSeconds}s!");
                yield break;
            }

            Log("GameSystemHub found, checking systems...");

            // Wait for all critical systems to be ready
            elapsedTime = 0f;
            frameCount = 0;

            while (!GameSystemHub.Instance.AreAllSystemsReady() 
                   && elapsedTime < systemTimeoutSeconds)
            {
                frameCount++;
                
                if (frameCount % FRAME_WAIT_INTERVAL == 0)
                {
                    Log($"Waiting for systems... ({elapsedTime:F1}s elapsed)");
                }

                yield return null;
                elapsedTime += Time.deltaTime;
            }

            // Final validation
            if (!GameSystemHub.Instance.AreAllSystemsReady())
            {
                LogError("CRITICAL: Systems not ready after timeout!");
                LogError(GameSystemHub.Instance.GetSystemStatus());
                yield break;
            }

            Log("✓ All systems ready!");
        }
        #endregion

        #region Scene Logic
        /// <summary>
        /// Determine which scene to load based on save state
        /// </summary>
        private string DetermineTargetScene()
        {
            // Validate managers exist
            if (!ValidateManagers())
            {
                LogError("Critical managers missing, defaulting to avatar creation");
                return avatarCreationScene;
            }

            // Check for existing save
            if (!SaveManager.Instance.SaveExists())
            {
                Log("No save found — creating new profile");
                GameManager.Instance.StartNewGame();
                return avatarCreationScene;
            }

            // Attempt to load existing save
            Log("Save found — attempting to load...");
            if (GameManager.Instance.LoadGame())
            {
                string playerName = GameManager.Instance.CurrentPlayer?.playerName ?? "Unknown";
                Log($"Save loaded — Welcome back, {playerName}!");
                return mainBaseScene;
            }

            // Save corrupted or load failed
            Log("Save corrupted or load failed — creating new profile");
            GameManager.Instance.StartNewGame();
            return avatarCreationScene;
        }

        /// <summary>
        /// Validate that critical managers are present
        /// </summary>
        private bool ValidateManagers()
        {
            bool valid = true;

            if (GameManager.Instance == null)
            {
                LogError("GameManager is null!");
                valid = false;
            }

            if (SaveManager.Instance == null)
            {
                LogError("SaveManager is null!");
                valid = false;
            }

            return valid;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Ensure minimum load time for smooth user experience
        /// </summary>
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
            if (showDebugLogs)
            {
                Debug.Log($"[Bootstrap] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[Bootstrap] {message}");
        }
        #endregion
    }
}