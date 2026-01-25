// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Controllers/PersistentUIController.cs
// ✅ FIXED: Now uses SceneManifest + listens to SceneFlowManager
// Attach to: 01_Bootstrap → PersistentUICanvas
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.Core;
using Ascension.Data.Config;

namespace Ascension.Controllers
{
    /// <summary>
    /// ✅ FIXED: Now properly integrated with SceneManifest and SceneFlowManager
    /// This controller automatically shows/hides UI based on current scene metadata
    /// </summary>
    public class PersistentUIController : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI Layer References")]
        [SerializeField] private GameObject hudLayer;
        [SerializeField] private GameObject playerHUD;
        [SerializeField] private GameObject globalMenu;
        [SerializeField] private GameObject popupLayer;
        [SerializeField] private GameObject toastManager;
        [SerializeField] private GameObject overlayLayer;
        
        [Header("Scene Configuration")]
        [Tooltip("ScriptableObject with scene metadata (showHUD, showMenu, etc.)")]
        [SerializeField] private SceneManifest sceneManifest;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            // ✅ CRITICAL: Set initial visibility BEFORE first frame
            // Bootstrap starts with HUD hidden (since no player exists yet)
            SetHUDLayerVisible(false);
            SetPlayerHUDVisible(false);
            SetGlobalMenuVisible(false);
            
            Log("Initialized (HUD hidden until scene loads)");
        }

        private void OnEnable()
        {
            // ✅ Subscribe to Unity's scene loading events
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            // ✅ Subscribe to SceneFlowManager events (if you add them)
            // GameEvents.OnSceneChanging += OnSceneChanging;
        }
        
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            
            // GameEvents.OnSceneChanging -= OnSceneChanging;
        }
        #endregion
        
        #region Scene Event Handlers
        /// <summary>
        /// ✅ Called automatically when ANY scene loads
        /// This is where we update UI visibility based on SceneManifest
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UpdateUIVisibility(scene.name);
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            Log($"Scene unloaded: {scene.name}");
            // Could add cleanup logic here if needed
        }
        
        /// <summary>
        /// ✅ Main logic: Query SceneManifest and update UI accordingly
        /// </summary>
        private void UpdateUIVisibility(string sceneName)
        {
            if (sceneManifest == null)
            {
                LogError("SceneManifest not assigned! Using fallback visibility rules.");
                UpdateVisibilityFallback(sceneName);
                return;
            }
            
            // ✅ Query SceneManifest for metadata
            SceneMetadata sceneData = sceneManifest.GetSceneData(sceneName);
            
            if (sceneData == null)
            {
                LogWarning($"Scene '{sceneName}' not found in SceneManifest! Using fallback rules.");
                UpdateVisibilityFallback(sceneName);
                return;
            }
            
            // ✅ Apply visibility from SceneManifest
            bool shouldShowHUD = sceneData.showPlayerHUD;
            bool shouldShowMenu = sceneData.showGlobalMenu;
            
            if (!shouldShowHUD)
            {
                // Hide everything if HUD is disabled for this scene
                SetHUDLayerVisible(false);
                Log($"Scene: {sceneName} → HUD: HIDDEN (per SceneManifest)");
            }
            else
            {
                // Show HUD layer, but toggle menu individually
                SetHUDLayerVisible(true);
                SetPlayerHUDVisible(true);
                SetGlobalMenuVisible(shouldShowMenu);
                
                string menuStatus = shouldShowMenu ? "VISIBLE" : "HIDDEN";
                Log($"Scene: {sceneName} → HUD: VISIBLE, Menu: {menuStatus}");
            }
            
            // ✅ Popups/Toasts/Overlay stay always active (they manage their own visibility)
            // No need to touch them here
        }
        
        /// <summary>
        /// Fallback visibility rules if SceneManifest is missing or incomplete
        /// </summary>
        private void UpdateVisibilityFallback(string sceneName)
        {
            // Hardcoded fallback (same as your original logic)
            bool shouldHideEverything = 
                sceneName == "00_Disclaimer" ||
                sceneName == "01_Bootstrap" ||
                sceneName == "02_AvatarCreation";
            
            if (shouldHideEverything)
            {
                SetHUDLayerVisible(false);
                Log($"Scene: {sceneName} → HUD: HIDDEN (fallback)");
                return;
            }
            
            bool shouldHideMenu = sceneName == "12_Combat";
            
            SetHUDLayerVisible(true);
            SetPlayerHUDVisible(true);
            SetGlobalMenuVisible(!shouldHideMenu);
            
            string menuStatus = shouldHideMenu ? "HIDDEN" : "VISIBLE";
            Log($"Scene: {sceneName} → HUD: VISIBLE, Menu: {menuStatus} (fallback)");
        }
        #endregion
        
        #region Public API
        /// <summary>Show/hide entire HUD layer (PlayerHUD + GlobalMenu)</summary>
        public void SetHUDLayerVisible(bool visible)
        {
            if (hudLayer != null)
            {
                hudLayer.SetActive(visible);
            }
        }
        
        /// <summary>Show/hide only PlayerHUD (health, exp, name, level)</summary>
        public void SetPlayerHUDVisible(bool visible)
        {
            if (playerHUD != null)
            {
                playerHUD.SetActive(visible);
            }
        }
        
        /// <summary>Show/hide only GlobalMenu (5 navigation buttons)</summary>
        public void SetGlobalMenuVisible(bool visible)
        {
            if (globalMenu != null)
            {
                globalMenu.SetActive(visible);
            }
        }
        
        /// <summary>Check if HUD layer is visible</summary>
        public bool IsHUDLayerVisible() => hudLayer != null && hudLayer.activeSelf;
        
        /// <summary>Check if PlayerHUD is visible</summary>
        public bool IsPlayerHUDVisible() => playerHUD != null && playerHUD.activeSelf;
        
        /// <summary>Check if GlobalMenu is visible</summary>
        public bool IsGlobalMenuVisible() => globalMenu != null && globalMenu.activeSelf;
        
        /// <summary>Force refresh UI visibility based on current active scene</summary>
        [ContextMenu("Force Refresh UI Visibility")]
        public void ForceRefreshVisibility()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            UpdateUIVisibility(activeScene.name);
        }
        #endregion
        
        #region Logging
        private void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[PersistentUI] {message}");
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[PersistentUI] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[PersistentUI] {message}");
        }
        #endregion
    }
}