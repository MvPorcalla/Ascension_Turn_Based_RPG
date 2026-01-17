// ══════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Controllers/StorageRoomController.cs
// ✅ REFACTORED: Uses new reusable components
// Manages the Storage scene layout (3 grids: Equipped, Bag, Storage)
// ══════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using Ascension.UI.Components.Inventory;
using Ascension.Inventory.Manager;
using Ascension.Equipment.Manager;

namespace Ascension.Controllers
{
    /// <summary>
    /// Controls the Storage Room scene
    /// - Displays equipped gear (7 slots, read-only)
    /// - Displays bag inventory (12 slots, can move to storage)
    /// - Displays storage inventory (60 slots, can move to bag, has filters)
    /// </summary>
    public class StorageRoomController : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI Components")]
        [SerializeField] private InventoryGridUI equippedGearGrid;
        [SerializeField] private InventoryGridUI bagGrid;
        [SerializeField] private InventoryGridUI storageGrid;
        [SerializeField] private InventoryFilterBarUI storageFilterBar;

        [Header("Action Buttons")]
        [SerializeField] private Button storeAllButton;
        [SerializeField] private Button exitButton;

        [Header("Optional UI")]
        [SerializeField] private TMPro.TextMeshProUGUI bagCountText;
        [SerializeField] private TMPro.TextMeshProUGUI storageCountText;
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
                // Re-subscribe on enable (in case we were disabled)
                SubscribeToEvents();
                RefreshAll();
            }
        }

        private void OnDisable()
        {
            // Only unsubscribe, don't cleanup (we might be re-enabled)
            UnsubscribeFromEvents();
        }

        private void OnDestroy()
        {
            // Full cleanup on destroy
            Cleanup();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            if (isInitialized)
                return;

            ValidateComponents();
            SetupButtons();
            ConnectFilterBar();
            SubscribeToEvents();
            RefreshAll();

            isInitialized = true;
            Debug.Log("[StorageRoomController] Initialized successfully");
        }

        private void ValidateComponents()
        {
            if (equippedGearGrid == null)
                Debug.LogWarning("[StorageRoomController] Equipped gear grid not assigned!");

            if (bagGrid == null)
                Debug.LogWarning("[StorageRoomController] Bag grid not assigned!");

            if (storageGrid == null)
                Debug.LogWarning("[StorageRoomController] Storage grid not assigned!");

            if (storageFilterBar == null)
                Debug.LogWarning("[StorageRoomController] Storage filter bar not assigned!");
        }

        private void SetupButtons()
        {
            if (storeAllButton != null)
            {
                storeAllButton.onClick.AddListener(OnStoreAllClicked);
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnExitClicked);
            }
        }

        private void ConnectFilterBar()
        {
            // Connect filter bar to storage grid
            if (storageGrid != null && storageFilterBar != null)
            {
                storageGrid.ConnectFilterBar(storageFilterBar);
                Debug.Log("[StorageRoomController] Connected filter bar to storage grid");
            }
        }

        private void SubscribeToEvents()
        {
            var inventoryMgr = InventoryManager.Instance;
            if (inventoryMgr != null)
            {
                inventoryMgr.Inventory.OnInventoryChanged += UpdateItemCounts;
                inventoryMgr.OnInventoryLoaded += UpdateItemCounts;
            }

            var equipmentMgr = EquipmentManager.Instance;
            if (equipmentMgr != null)
            {
                equipmentMgr.OnEquipmentChanged += RefreshEquippedGear;
            }
        }

        private void UnsubscribeFromEvents()
        {
            var inventoryMgr = InventoryManager.Instance;
            if (inventoryMgr != null)
            {
                inventoryMgr.Inventory.OnInventoryChanged -= UpdateItemCounts;
                inventoryMgr.OnInventoryLoaded -= UpdateItemCounts;
            }

            var equipmentMgr = EquipmentManager.Instance;
            if (equipmentMgr != null)
            {
                equipmentMgr.OnEquipmentChanged -= RefreshEquippedGear;
            }
        }

        private void Cleanup()
        {
            UnsubscribeFromEvents();

            if (storeAllButton != null)
                storeAllButton.onClick.RemoveAllListeners();

            if (exitButton != null)
                exitButton.onClick.RemoveAllListeners();
        }
        #endregion

        #region Refresh Logic
        private void RefreshAll()
        {
            RefreshEquippedGear();
            RefreshBag();
            RefreshStorage();
            UpdateItemCounts();
        }

        private void RefreshEquippedGear()
        {
            equippedGearGrid?.ForceRefresh();
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
            var inventoryMgr = InventoryManager.Instance;
            if (inventoryMgr == null)
                return;

            // Update bag count text
            if (bagCountText != null)
            {
                int bagCount = inventoryMgr.Inventory.GetBagItemCount();
                int bagMax = inventoryMgr.Capacity.MaxBagSlots;
                bagCountText.text = $"{bagCount}/{bagMax}";
            }

            // Update storage count text
            if (storageCountText != null)
            {
                int storageCount = inventoryMgr.Inventory.GetStorageItemCount();
                int storageMax = inventoryMgr.Capacity.MaxStorageSlots;
                storageCountText.text = $"{storageCount}/{storageMax}";
            }

            // Update "Store All" button state
            if (storeAllButton != null)
            {
                int bagItemCount = inventoryMgr.Inventory.GetBagItemCount();
                storeAllButton.interactable = bagItemCount > 0;
            }
        }
        #endregion

        #region Button Handlers
        /// <summary>
        /// Store all items from bag to storage (excluding equipped items)
        /// </summary>
        private void OnStoreAllClicked()
        {
            var inventoryMgr = InventoryManager.Instance;
            var equipmentMgr = EquipmentManager.Instance;

            if (inventoryMgr?.Inventory == null)
            {
                Debug.LogError("[StorageRoomController] InventoryManager not available!");
                return;
            }

            int bagItemCount = inventoryMgr.Inventory.GetBagItemCount();
            if (bagItemCount == 0)
            {
                Debug.Log("[StorageRoomController] No items in bag to store");
                return;
            }

            // Store all items (excluding equipped)
            inventoryMgr.Inventory.StoreAllItems(
                itemId => equipmentMgr?.IsItemEquipped(itemId) ?? false,
                inventoryMgr.Database
            );

            Debug.Log("[StorageRoomController] Store All completed");
        }

        /// <summary>
        /// Exit storage room (return to previous scene or close panel)
        /// </summary>
        private void OnExitClicked()
        {
            // TODO: Implement scene transition or panel close
            Debug.Log("[StorageRoomController] Exit button clicked");
            
            // Example: Load previous scene
            // SceneController.Instance?.LoadPreviousScene();
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
                Debug.LogWarning("[StorageRoomController] Force Refresh only works in Play Mode");
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