// ──────────────────────────────────────────────────
// Bootstrap.cs - Entry point for game initialization
// Determines which scene to load based on save state
// ──────────────────────────────────────────────────

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Entry point of the game. Determines which scene to load based on save state.
/// Singletons (GameManager, SaveManager) are placed directly in this scene.
/// </summary>
public class Bootstrap : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string avatarCreationScene = "02_AvatarCreation";
    [SerializeField] private string mainBaseScene = "03_MainBase";
    
    [Header("Settings")]
    [SerializeField] private float minimumLoadTime = 1f; // For splash screen
    [SerializeField] private bool showDebugLogs = true;
    
    private IEnumerator Start()
    {
        float startTime = Time.time;
        
        Log("Initializing...");
        
        // Wait a frame for Awake() calls on GameManager & SaveManager
        yield return null;
        
        Log("Singletons ready");
        
        // Wait for minimum load time (splash screen)
        float elapsed = Time.time - startTime;
        if (elapsed < minimumLoadTime)
        {
            yield return new WaitForSeconds(minimumLoadTime - elapsed);
        }
        
        // Determine and load target scene
        string targetScene = DetermineTargetScene();
        Log($"Loading {targetScene}");
        
        SceneManager.LoadScene(targetScene);
    }
    
    private string DetermineTargetScene()
    {
        // Check if a save exists
        if (SaveManager.Instance.SaveExists())
        {
            // Save exists - try to load it
            if (GameManager.Instance.LoadGame())
            {
                Log($"Save found - {GameManager.Instance.CurrentPlayer.playerName}");
                return mainBaseScene;
            }
            else
            {
                // Save corrupted and backup failed
                Log("Save corrupted, starting fresh");
                return avatarCreationScene;
            }
        }
        else
        {
            // No save - new player
            Log("No save found, new game");
            GameManager.Instance.StartNewGame();
            return avatarCreationScene;
        }
    }
    
    private void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[Bootstrap] {message}");
    }
}