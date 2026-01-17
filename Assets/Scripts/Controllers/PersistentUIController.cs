// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Controllers/PersistentUIController.cs
// ✅ FIXED: Sets visibility in Awake() to prevent flash on first frame
// Attach to: 01_Bootstrap → PersistentUICanvas
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ascension.Controllers
{
    public class PersistentUIController : MonoBehaviour
    {
        [Header("UI Layer References")]
        [SerializeField] private GameObject hudLayer;
        [SerializeField] private GameObject playerHUD;
        [SerializeField] private GameObject globalMenu;
        [SerializeField] private GameObject popupLayer;
        [SerializeField] private GameObject toastManager;
        [SerializeField] private GameObject overlayLayer;
        
        [Header("Visibility Rules")]
        [Tooltip("Scenes where HUD should be completely hidden")]
        [SerializeField] private string[] scenesWithoutHUD = 
        { 
            "00_Disclaimer",
            "01_Bootstrap",
            "02_AvatarCreation"
        };
        
        [Tooltip("Scenes where GlobalMenu should be hidden (but PlayerHUD visible)")]
        [SerializeField] private string[] scenesWithoutGlobalMenu = 
        { 
            "12_Combat"
        };

        #region Unity Callbacks
        private void Awake()
        {
            // ✅ FIXED: Set initial visibility BEFORE first frame render
            // This prevents UI flash during Bootstrap loading
            UpdateHUDVisibility("01_Bootstrap");
            
            // Subscribe to scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            Debug.Log("[PersistentUI] Initialized (managed by Bootstrap)");
        }
        
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endregion
        
        #region Scene Management
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Update HUD visibility based on newly loaded scene
            UpdateHUDVisibility(scene.name);
        }
        
        private void UpdateHUDVisibility(string sceneName)
        {
            // Check if HUD should be completely hidden
            bool shouldHideEverything = System.Array.Exists(scenesWithoutHUD, s => s == sceneName);
            
            if (shouldHideEverything)
            {
                SetHUDLayerVisible(false);
                Debug.Log($"[PersistentUI] Scene: {sceneName}, HUD Layer: HIDDEN");
                return;
            }
            
            // HUD layer should be visible
            SetHUDLayerVisible(true);
            
            // Check if GlobalMenu should be hidden (but PlayerHUD stays visible)
            bool shouldHideGlobalMenu = System.Array.Exists(scenesWithoutGlobalMenu, s => s == sceneName);
            
            SetPlayerHUDVisible(true);
            SetGlobalMenuVisible(!shouldHideGlobalMenu);
            
            string menuStatus = shouldHideGlobalMenu ? "HIDDEN" : "VISIBLE";
            Debug.Log($"[PersistentUI] Scene: {sceneName}, PlayerHUD: VISIBLE, GlobalMenu: {menuStatus}");
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
        #endregion
    }
}