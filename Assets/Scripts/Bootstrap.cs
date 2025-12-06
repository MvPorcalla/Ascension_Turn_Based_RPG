// ════════════════════════════════════════════════════════════════════════
// Bootstrap.cs
// Entry point for game initialization and scene routing based on save state
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

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        #endregion

        #region Unity Lifecycle
        private IEnumerator Start()
        {
            float startTime = Time.time;
            Log("Bootstrap initializing…");

            // Allow GameManager and SaveManager Awake() to execute
            yield return null;

            Log("Managers initialized");

            yield return EnsureMinimumLoadTime(startTime);

            string targetScene = DetermineTargetScene();
            Log($"Loading Scene: {targetScene}");
            SceneManager.LoadScene(targetScene);
        }
        #endregion

        #region Scene Logic
        private string DetermineTargetScene()
        {
            if (!SaveManager.Instance.SaveExists())
            {
                Log("No save found — creating new profile");
                GameManager.Instance.StartNewGame();
                return avatarCreationScene;
            }

            if (GameManager.Instance.LoadGame())
            {
                Log($"Save loaded — Welcome back {GameManager.Instance.CurrentPlayer.playerName}");
                return mainBaseScene;
            }

            Log("Save corrupted — fallback to new game");
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
                yield return new WaitForSeconds(remaining);
            }
        }

        private void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[Bootstrap] {message}");
        }
        #endregion
    }
}
