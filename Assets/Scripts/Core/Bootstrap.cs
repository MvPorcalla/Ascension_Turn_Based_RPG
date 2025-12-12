using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.App;
using Ascension.Character.Manager;
using Ascension.Inventory.Manager;

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

            // Wait for ServiceContainer and all critical systems to be ready
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
        private IEnumerator WaitForSystemsReady()
        {
            Log("Waiting for ServiceContainer...");

            float elapsedTime = 0f;
            int frameCount = 0;

            // Wait for ServiceContainer to exist and initialize
            while (ServiceContainer.Instance == null || !ServiceContainer.Instance.IsInitialized)
            {
                frameCount++;
                if (frameCount % FRAME_WAIT_INTERVAL == 0)
                    Log($"Still waiting for ServiceContainer... ({elapsedTime:F1}s elapsed)");

                yield return null;
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= systemTimeoutSeconds)
                {
                    LogError($"CRITICAL: ServiceContainer not ready after {systemTimeoutSeconds}s!");
                    yield break;
                }
            }

            Log("ServiceContainer found, checking critical systems...");

            // Wait for all critical systems to be registered
            elapsedTime = 0f;
            frameCount = 0;

            while (!(ServiceContainer.Instance.Has<SaveManager>() &&
                    ServiceContainer.Instance.Has<CharacterManager>() &&
                    ServiceContainer.Instance.Has<InventoryManager>()))
            {
                frameCount++;
                if (frameCount % FRAME_WAIT_INTERVAL == 0)
                    Log($"Waiting for critical systems... ({elapsedTime:F1}s elapsed)");

                yield return null;
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= systemTimeoutSeconds)
                {
                    LogError("CRITICAL: Critical systems not ready after timeout!");
                    LogError(ServiceContainer.Instance.GetSystemStatus());
                    yield break;
                }
            }

            Log("✓ All critical systems ready!");
        }

        #endregion

        #region Scene Logic
        private string DetermineTargetScene()
        {
            if (!ValidateManagers())
            {
                LogError("Critical managers missing, defaulting to avatar creation");
                return avatarCreationScene;
            }

            if (!SaveManager.Instance.SaveExists())
            {
                Log("No save found — creating new profile");
                GameManager.Instance.StartNewGame();
                return avatarCreationScene;
            }

            Log("Save found — attempting to load...");
            if (GameManager.Instance.LoadGame())
            {
                string playerName = GameManager.Instance.CurrentPlayer?.playerName ?? "Unknown";
                Log($"Save loaded — Welcome back, {playerName}!");
                return mainBaseScene;
            }

            Log("Save corrupted or load failed — creating new profile");
            GameManager.Instance.StartNewGame();
            return avatarCreationScene;
        }

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
