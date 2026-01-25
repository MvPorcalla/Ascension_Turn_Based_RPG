// ══════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Controllers/StorageRoomController.cs
// ✅ REFACTORED: Uses GameBootstrap, removed business logic
// Manages the Storage Room UI scene (Equipped + Bag + Storage grids)
// Attach to: UI_Storage → Canvas → StorageRoomPanel
// ══════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using Ascension.Core;
using Ascension.UI.Components.Inventory;

namespace Ascension.UI.Controllers
{
    /// <summary>
    /// Controls the Storage Room UI scene
    /// - Displays equipped gear (7 slots, read-only)
    /// - Displays bag inventory (12 slots, can move to storage)
    /// - Displays storage inventory (60 slots, can move to bag, has filters)
    /// Pure UI controller - delegates all logic to managers
    /// </summary>
    public class StorageRoomController : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI Components")]
        // [SerializeField] private InventoryGridUI equippedGearGrid;
        [SerializeField] private InventoryGridUI bagGrid;
        [SerializeField] private InventoryGridUI storageGrid;
        [SerializeField] private InventoryFilterBarUI storageFilterBar;

        [Header("Action Buttons")]
        [SerializeField] private Button storeAllButton;
        [SerializeField] private Button exitButton;

        [Header("Optional UI")]
        [SerializeField] private TMPro.TextMeshProUGUI bagCountText;
        [SerializeField] private TMPro.TextMeshProUGUI storageCountText;
        [SerializeField] private TMPro.TextMeshProUGUI titleText;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        #endregion

        #region Private Fields
        private bool isInitialized = false;
        #endregion

        #region Unity Callbacks
        private void OnEnable()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            else
            {
                SubscribeToEvents();
                RefreshAll();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
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

            ValidateComponents();
            ValidateManagers();
            SetupButtons();
            ConnectFilterBar();
            SubscribeToEvents();
            UpdateUI();
            RefreshAll();

            isInitialized = true;
            Log("StorageRoomController initialized");
        }

        private void ValidateComponents()
        {
            if (bagGrid == null)
                LogWarning("Bag grid not assigned!");

            if (storageGrid == null)
                LogWarning("Storage grid not assigned!");

            if (storageFilterBar == null)
                LogWarning("Storage filter bar not assigned!");

            if (storeAllButton == null)
                LogWarning("Store All button not assigned!");

            if (exitButton == null)
                LogWarning("Exit button not assigned!");
        }

        private void ValidateManagers()
        {
            if (GameBootstrap.Inventory == null)
                LogError("InventoryManager not available!");

            if (GameBootstrap.Equipment == null)
                LogError("EquipmentManager not available!");

            if (GameBootstrap.SceneFlow == null)
                LogError("SceneFlowManager not available!");
        }

        private void SetupButtons()
        {
            if (storeAllButton != null)
                storeAllButton.onClick.AddListener(OnStoreAllClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);
        }

        private void ConnectFilterBar()
        {
            if (storageGrid != null && storageFilterBar != null)
            {
                storageGrid.ConnectFilterBar(storageFilterBar);
                Log("Connected filter bar to storage grid");
            }
        }

        private void SubscribeToEvents()
        {
            // Subscribe to inventory changes
            GameEvents.OnInventoryChanged += RefreshAll;
            GameEvents.OnItemAdded += OnItemChanged;
            GameEvents.OnItemRemoved += OnItemChanged;
            GameEvents.OnItemMoved += OnItemMoved;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnInventoryChanged -= RefreshAll;
            GameEvents.OnItemAdded -= OnItemChanged;
            GameEvents.OnItemRemoved -= OnItemChanged;
            GameEvents.OnItemMoved -= OnItemMoved;
        }

        private void Cleanup()
        {
            UnsubscribeFromEvents();

            if (storeAllButton != null)
                storeAllButton.onClick.RemoveAllListeners();

            if (exitButton != null)
                exitButton.onClick.RemoveAllListeners();
        }

        private void UpdateUI()
        {
            if (titleText != null)
                titleText.text = "Storage Room";

            UpdateItemCounts();
        }
        #endregion

        #region Event Handlers
        private void OnItemChanged(Inventory.Data.ItemInstance item)
        {
            UpdateItemCounts();
        }

        private void OnItemMoved(Inventory.Data.ItemInstance item, Inventory.Enums.ItemLocation from, Inventory.Enums.ItemLocation to)
        {
            UpdateItemCounts();
        }
        #endregion

        #region Refresh Logic
        private void RefreshAll()
        {
            RefreshBag();
            RefreshStorage();
            UpdateItemCounts();
        }

        private void RefreshBag()
        {
            bagGrid?.ForceRefresh();
        }

        private void RefreshStorage()
        {
            storageGrid?.ForceRefresh();
        }

        private void UpdateItemCounts()
        {
            if (GameBootstrap.Inventory == null)
                return;

            var inventoryManager = GameBootstrap.Inventory;

            // Update bag count
            if (bagCountText != null)
            {
                int bagCount = inventoryManager.Inventory.GetBagItemCount();
                int bagMax = inventoryManager.Capacity.MaxBagSlots;
                bagCountText.text = $"{bagCount}/{bagMax}";
            }

            // Update storage count
            if (storageCountText != null)
            {
                int storageCount = inventoryManager.Inventory.GetStorageItemCount();
                int storageMax = inventoryManager.Capacity.MaxStorageSlots;
                storageCountText.text = $"{storageCount}/{storageMax}";
            }

            // Update "Store All" button state
            if (storeAllButton != null)
            {
                int bagItemCount = inventoryManager.Inventory.GetBagItemCount();
                storeAllButton.interactable = bagItemCount > 0;
            }
        }
        #endregion

        #region Button Handlers
        /// <summary>
        /// Store all items from bag to storage (excluding equipped items)
        /// ✅ Uses GameBootstrap.Inventory and GameBootstrap.Equipment
        /// </summary>
        private void OnStoreAllClicked()
        {
            if (GameBootstrap.Inventory == null || GameBootstrap.Equipment == null)
            {
                LogError("Required managers not available!");
                return;
            }

            int bagItemCount = GameBootstrap.Inventory.Inventory.GetBagItemCount();
            if (bagItemCount == 0)
            {
                Log("No items in bag to store");
                return;
            }

            Log("Storing all items from bag...");

            // Store all items (excluding equipped)
            GameBootstrap.Inventory.Inventory.StoreAllItems(
                itemId => GameBootstrap.Equipment.IsItemEquipped(itemId),
                GameBootstrap.Inventory.Database
            );

            Log("Store All completed");
        }

        /// <summary>
        /// Close storage room and return to main base
        /// ✅ Uses GameBootstrap.SceneFlow
        /// </summary>
        private void OnExitClicked()
        {
            if (GameBootstrap.SceneFlow == null)
            {
                LogError("SceneFlowManager not available!");
                return;
            }

            Log("Closing Storage Room");
            GameBootstrap.SceneFlow.CloseCurrentUIScene();
        }
        #endregion

        #region Public API
        /// <summary>
        /// Force refresh all grids (useful after external changes)
        /// </summary>
        public void ForceRefreshAll()
        {
            RefreshAll();
        }

        /// <summary>
        /// Set storage filter programmatically
        /// </summary>
        public void SetStorageFilter(Data.SO.Item.ItemType? filterType)
        {
            storageFilterBar?.SetFilter(filterType);
        }
        #endregion

        #region Logging
        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[StorageRoom] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[StorageRoom] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[StorageRoom] {message}");
        }
        #endregion

        #region Editor Helpers
#if UNITY_EDITOR
        [ContextMenu("Force Refresh All")]
        private void EditorForceRefresh()
        {
            if (Application.isPlaying)
            {
                ForceRefreshAll();
            }
            else
            {
                LogWarning("Force Refresh only works in Play Mode");
            }
        }

        [ContextMenu("Test Store All")]
        private void EditorTestStoreAll()
        {
            if (Application.isPlaying)
            {
                OnStoreAllClicked();
            }
        }
#endif
        #endregion
    }
}