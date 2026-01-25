// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Controllers/MainbasePanelController.cs
// ✅ REFACTORED: Uses GameBootstrap, removed business logic
// Manages room buttons in the Mainbase scene
// Attach to: 03_MainBase → Canvas → MainBasePanel
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using Ascension.Core;

namespace Ascension.UI.Controllers
{
    /// <summary>
    /// Controls the Mainbase room selection panel
    /// Each button opens an additive UI scene (Storage, Cooking, etc.)
    /// Pure UI controller - no business logic
    /// </summary>
    public class MainbasePanelController : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Room Buttons")]
        [SerializeField] private Button storageRoomButton;
        [SerializeField] private Button equipmentRoomButton; // will be replaced as quipment room is deprecated
        [SerializeField] private Button cookingRoomButton;
        [SerializeField] private Button brewingRoomButton;
        [SerializeField] private Button craftingRoomButton;

        [Header("Optional UI")]
        [SerializeField] private TMPro.TextMeshProUGUI titleText;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        #endregion

        #region Unity Callbacks
        private void Start()
        {
            ValidateReferences();
            SetupButtons();
            UpdateUI();
            Log("MainbasePanelController initialized");
        }

        private void OnDestroy()
        {
            CleanupButtons();
        }
        #endregion

        #region Initialization
        private void ValidateReferences()
        {
            if (storageRoomButton == null) LogWarning("Storage button not assigned!");
            if (equipmentRoomButton == null) LogWarning("Equipment button not assigned!");
            if (cookingRoomButton == null) LogWarning("Cooking button not assigned!");
            if (brewingRoomButton == null) LogWarning("Brewing button not assigned!");
            if (craftingRoomButton == null) LogWarning("Crafting button not assigned!");
        }

        private void SetupButtons()
        {
            if (storageRoomButton != null)
                storageRoomButton.onClick.AddListener(OnStorageRoomClicked);

            if (equipmentRoomButton != null)
                equipmentRoomButton.onClick.AddListener(OnEquipmentRoomClicked);

            if (cookingRoomButton != null)
                cookingRoomButton.onClick.AddListener(OnCookingRoomClicked);

            if (brewingRoomButton != null)
                brewingRoomButton.onClick.AddListener(OnBrewingRoomClicked);

            if (craftingRoomButton != null)
                craftingRoomButton.onClick.AddListener(OnCraftingRoomClicked);
        }

        private void CleanupButtons()
        {
            if (storageRoomButton != null)
                storageRoomButton.onClick.RemoveAllListeners();

            if (equipmentRoomButton != null)
                equipmentRoomButton.onClick.RemoveAllListeners();

            if (cookingRoomButton != null)
                cookingRoomButton.onClick.RemoveAllListeners();

            if (brewingRoomButton != null)
                brewingRoomButton.onClick.RemoveAllListeners();

            if (craftingRoomButton != null)
                craftingRoomButton.onClick.RemoveAllListeners();
        }

        private void UpdateUI()
        {
            // Set title if available
            if (titleText != null)
            {
                titleText.text = "Main Base";
            }

            // Disable unimplemented rooms (temporary)
            if (equipmentRoomButton != null)
                equipmentRoomButton.interactable = false; // TODO: Implement

            if (cookingRoomButton != null)
                cookingRoomButton.interactable = false; // TODO: Implement

            if (brewingRoomButton != null)
                brewingRoomButton.interactable = false; // TODO: Implement

            if (craftingRoomButton != null)
                craftingRoomButton.interactable = false; // TODO: Implement
        }
        #endregion

        #region Room Button Handlers
        /// <summary>
        /// Open Storage Room UI scene
        /// ✅ Uses GameBootstrap.SceneFlow
        /// </summary>
        private void OnStorageRoomClicked()
        {
            if (GameBootstrap.SceneFlow == null)
            {
                LogError("SceneFlowManager not available!");
                return;
            }

            Log("Opening Storage Room");
            GameBootstrap.SceneFlow.OpenStorage();
        }

        /// <summary>
        /// Open Equipment Room UI scene (TODO: Implement)
        /// </summary>
        private void OnEquipmentRoomClicked()
        {
            LogWarning("Equipment Room not yet implemented");
            // TODO: Add scene and uncomment
            // GameBootstrap.SceneFlow.OpenUIScene("UI_Equipment");
        }

        /// <summary>
        /// Open Cooking Room UI scene (TODO: Implement)
        /// </summary>
        private void OnCookingRoomClicked()
        {
            LogWarning("Cooking Room not yet implemented");
            // TODO: Add scene and uncomment
            // GameBootstrap.SceneFlow.OpenCooking();
        }

        /// <summary>
        /// Open Brewing Room UI scene (TODO: Implement)
        /// </summary>
        private void OnBrewingRoomClicked()
        {
            LogWarning("Brewing Room not yet implemented");
            // TODO: Add scene and uncomment
            // GameBootstrap.SceneFlow.OpenBrewing();
        }

        /// <summary>
        /// Open Crafting Room UI scene (TODO: Implement)
        /// </summary>
        private void OnCraftingRoomClicked()
        {
            LogWarning("Crafting Room not yet implemented");
            // TODO: Add scene and uncomment
            // GameBootstrap.SceneFlow.OpenCrafting();
        }
        #endregion

        #region Logging
        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[MainbasePanel] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[MainbasePanel] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[MainbasePanel] {message}");
        }
        #endregion
    }
}