// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Controllers/GlobalMenuController.cs
// ✅ REFACTORED: Uses GameBootstrap instead of Instance pattern
// Controls the 5 persistent navigation buttons (WorldMap, Profile, Inventory, Quest, Codex)
// Attach to: 02_UIPersistent → PersistentUICanvas → HUDLayer → GlobalMenu
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using Ascension.Core;

namespace Ascension.UI.Controllers
{
    /// <summary>
    /// Controls the 5 persistent navigation buttons visible in all gameplay scenes
    /// These buttons open additive UI scenes via SceneFlowManager
    /// NOTE: This entire GameObject is hidden during combat (handled by PersistentUIController)
    /// </summary>
    public class GlobalMenuController : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Navigation Buttons")]
        [SerializeField] private Button worldMapButton;
        [SerializeField] private Button profileButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button questButton;
        [SerializeField] private Button codexButton;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        #endregion

        #region Unity Callbacks
        private void Start()
        {
            ValidateReferences();
            SetupButtons();
            Log("GlobalMenuController initialized");
        }

        private void OnDestroy()
        {
            CleanupButtons();
        }
        #endregion

        #region Initialization
        private void ValidateReferences()
        {
            if (worldMapButton == null) LogWarning("WorldMap button not assigned!");
            if (profileButton == null) LogWarning("Profile button not assigned!");
            if (inventoryButton == null) LogWarning("Inventory button not assigned!");
            if (questButton == null) LogWarning("Quest button not assigned!");
            if (codexButton == null) LogWarning("Codex button not assigned!");
        }

        private void SetupButtons()
        {
            if (worldMapButton != null)
                worldMapButton.onClick.AddListener(OnWorldMapClicked);

            if (profileButton != null)
                profileButton.onClick.AddListener(OnProfileClicked);

            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(OnInventoryClicked);

            if (questButton != null)
                questButton.onClick.AddListener(OnQuestClicked);

            if (codexButton != null)
                codexButton.onClick.AddListener(OnCodexClicked);
        }

        private void CleanupButtons()
        {
            if (worldMapButton != null)
                worldMapButton.onClick.RemoveAllListeners();

            if (profileButton != null)
                profileButton.onClick.RemoveAllListeners();

            if (inventoryButton != null)
                inventoryButton.onClick.RemoveAllListeners();

            if (questButton != null)
                questButton.onClick.RemoveAllListeners();

            if (codexButton != null)
                codexButton.onClick.RemoveAllListeners();
        }
        #endregion

        #region Button Handlers
        /// <summary>
        /// Open World Map UI scene
        /// ✅ Uses GameBootstrap.SceneFlow (no singleton!)
        /// </summary>
        private void OnWorldMapClicked()
        {
            if (GameBootstrap.SceneFlow == null)
            {
                LogError("SceneFlowManager not available!");
                return;
            }

            Log("Opening World Map");
            GameBootstrap.SceneFlow.OpenWorldMap();
        }

        /// <summary>
        /// Open Profile UI scene
        /// </summary>
        private void OnProfileClicked()
        {
            if (GameBootstrap.SceneFlow == null)
            {
                LogError("SceneFlowManager not available!");
                return;
            }

            Log("Opening Profile");
            GameBootstrap.SceneFlow.OpenProfile();
        }

        /// <summary>
        /// Open Inventory UI scene
        /// </summary>
        private void OnInventoryClicked()
        {
            if (GameBootstrap.SceneFlow == null)
            {
                LogError("SceneFlowManager not available!");
                return;
            }

            Log("Opening Inventory");
            GameBootstrap.SceneFlow.OpenInventory();
        }

        /// <summary>
        /// Open Quest UI scene
        /// </summary>
        private void OnQuestClicked()
        {
            if (GameBootstrap.SceneFlow == null)
            {
                LogError("SceneFlowManager not available!");
                return;
            }

            Log("Opening Quest");
            GameBootstrap.SceneFlow.OpenQuest();
        }

        /// <summary>
        /// Open Codex UI scene
        /// </summary>
        private void OnCodexClicked()
        {
            if (GameBootstrap.SceneFlow == null)
            {
                LogError("SceneFlowManager not available!");
                return;
            }

            Log("Opening Codex");
            GameBootstrap.SceneFlow.OpenCodex();
        }
        #endregion

        #region Logging
        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[GlobalMenu] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[GlobalMenu] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[GlobalMenu] {message}");
        }
        #endregion
    }
}