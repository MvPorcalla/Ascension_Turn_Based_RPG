// ══════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Controllers/PlayerInventoryPanelController.cs
// ✅ NEW: Persistent inventory panel controller
// Manages the player's inventory panel (Bag + Equipped Gear + Skills)
// Persists across scenes from 03_MainBase onwards
// ══════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using Ascension.UI.Components.Inventory;
using Ascension.Core;

namespace Ascension.Controllers
{
    /// <summary>
    /// Controls the persistent inventory panel (DontDestroyOnLoad)
    /// - Displays equipped gear (7 slots)
    /// - Displays bag inventory (12 slots)
    /// - Shows/hides on hotkey (default: 'I')
    /// - Closes when clicking background
    /// - Future: Switch between Bag and Abilities panels
    /// </summary>
    public class PlayerInventoryPanelController : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Panel References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image backgroundOverlay;

        [Header("UI Components")]
        [SerializeField] private InventoryGridUI bagGridUI;
        [SerializeField] private Transform equippedGearPreview;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button bagTabButton;
        [SerializeField] private Button abilitiesTabButton; // ✅ Placeholder for future

        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.I;
        [SerializeField] private bool closeOnBackgroundClick = true;
        #endregion

        #region Private Fields
        private bool isInitialized = false;
        private bool isPanelOpen = false;
        
        // ✅ Future: Tab switching state
        private PanelTab currentTab = PanelTab.Bag;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            // ✅ Persist across scenes (from 03_MainBase onwards)
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            HandleInput();
        }

        private void OnDestroy()
        {
            Cleanup();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            if (isInitialized)
                return;

            // Wait for ServiceContainer to be ready
            if (ServiceContainer.Instance == null || !ServiceContainer.Instance.IsInitialized)
            {
                Debug.LogWarning("[PlayerInventoryPanelController] Waiting for ServiceContainer...");
                Invoke(nameof(Initialize), 0.5f);
                return;
            }

            SetupButtons();
            SetupBackgroundClick();
            ValidateComponents();

            // Start closed
            ClosePanel();

            isInitialized = true;
            Debug.Log("[PlayerInventoryPanelController] Initialized successfully");
        }

        private void SetupButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(ClosePanel);
            }

            // ✅ Future: Tab switching
            if (bagTabButton != null)
            {
                bagTabButton.onClick.AddListener(() => SwitchTab(PanelTab.Bag));
            }

            if (abilitiesTabButton != null)
            {
                abilitiesTabButton.onClick.AddListener(() => SwitchTab(PanelTab.Abilities));
                // Disable for now (future feature)
                abilitiesTabButton.interactable = false;
            }
        }

        private void SetupBackgroundClick()
        {
            if (!closeOnBackgroundClick || backgroundOverlay == null)
                return;

            // Add button component to background for click detection
            Button bgButton = backgroundOverlay.GetComponent<Button>();
            if (bgButton == null)
            {
                bgButton = backgroundOverlay.gameObject.AddComponent<Button>();
            }

            bgButton.onClick.AddListener(OnBackgroundClicked);
            
            Debug.Log("[PlayerInventoryPanelController] Background click-to-close enabled");
        }

        private void ValidateComponents()
        {
            if (panelRoot == null)
                Debug.LogError("[PlayerInventoryPanelController] Panel root not assigned!");

            if (bagGridUI == null)
                Debug.LogWarning("[PlayerInventoryPanelController] Bag grid UI not assigned!");

            if (equippedGearPreview == null)
                Debug.LogWarning("[PlayerInventoryPanelController] Equipped gear preview not assigned!");
        }

        private void Cleanup()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();

            if (bagTabButton != null)
                bagTabButton.onClick.RemoveAllListeners();

            if (abilitiesTabButton != null)
                abilitiesTabButton.onClick.RemoveAllListeners();

            if (backgroundOverlay != null)
            {
                Button bgButton = backgroundOverlay.GetComponent<Button>();
                if (bgButton != null)
                    bgButton.onClick.RemoveAllListeners();
            }
        }
        #endregion

        #region Input Handling
        private void HandleInput()
        {
            if (!isInitialized)
                return;

            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }

            // ESC key also closes panel
            if (isPanelOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePanel();
            }
        }
        #endregion

        #region Panel Control
        /// <summary>
        /// Toggle panel open/closed
        /// </summary>
        public void TogglePanel()
        {
            if (isPanelOpen)
                ClosePanel();
            else
                OpenPanel();
        }

        /// <summary>
        /// Open inventory panel
        /// </summary>
        public void OpenPanel()
        {
            if (isPanelOpen)
                return;

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
                isPanelOpen = true;

                // Refresh bag display
                bagGridUI?.ForceRefresh();

                Debug.Log("[PlayerInventoryPanelController] Panel opened");
            }
        }

        /// <summary>
        /// Close inventory panel
        /// </summary>
        public void ClosePanel()
        {
            if (!isPanelOpen)
                return;

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
                isPanelOpen = false;

                Debug.Log("[PlayerInventoryPanelController] Panel closed");
            }
        }

        /// <summary>
        /// Handle background click (close panel)
        /// </summary>
        private void OnBackgroundClicked()
        {
            if (closeOnBackgroundClick)
            {
                ClosePanel();
            }
        }
        #endregion

        #region Tab Switching (Future Feature)
        /// <summary>
        /// ✅ PLACEHOLDER: Switch between Bag and Abilities tabs
        /// Implement this when skill system is ready
        /// </summary>
        private void SwitchTab(PanelTab newTab)
        {
            if (currentTab == newTab)
                return;

            currentTab = newTab;

            // TODO: Implement tab switching
            // - Hide Bag grid, show Abilities grid (or vice versa)
            // - Update tab button visual states
            Debug.Log($"[PlayerInventoryPanelController] ✅ PLACEHOLDER: Switch to {newTab} tab");
        }

        private enum PanelTab
        {
            Bag,
            Abilities
        }
        #endregion

        #region Public API
        /// <summary>
        /// Check if panel is currently open
        /// </summary>
        public bool IsOpen()
        {
            return isPanelOpen;
        }

        /// <summary>
        /// Force refresh all UI elements
        /// </summary>
        public void ForceRefreshAll()
        {
            bagGridUI?.ForceRefresh();
            
            // Refresh equipped gear slots
            if (equippedGearPreview != null)
            {
                var equipmentSlots = equippedGearPreview.GetComponentsInChildren<EquipmentSlotUI>();
                foreach (var slot in equipmentSlots)
                {
                    slot.ForceRefresh();
                }
            }

            Debug.Log("[PlayerInventoryPanelController] Force refreshed all components");
        }
        #endregion

        #region Editor Helpers
#if UNITY_EDITOR
        [ContextMenu("Open Panel")]
        private void EditorOpenPanel()
        {
            OpenPanel();
        }

        [ContextMenu("Close Panel")]
        private void EditorClosePanel()
        {
            ClosePanel();
        }

        [ContextMenu("Toggle Panel")]
        private void EditorTogglePanel()
        {
            TogglePanel();
        }

        [ContextMenu("Force Refresh All")]
        private void EditorForceRefresh()
        {
            if (Application.isPlaying)
            {
                ForceRefreshAll();
            }
            else
            {
                Debug.LogWarning("[PlayerInventoryPanelController] Force Refresh only works in Play Mode");
            }
        }
#endif
        #endregion
    }
}