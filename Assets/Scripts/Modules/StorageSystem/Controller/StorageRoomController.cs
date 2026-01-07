// ════════════════════════════════════════════
// Assets\Scripts\Modules\StorageSystem\Controller\StorageRoomController.cs
// Main controller for the Storage Room scene
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Inventory.Manager;
using Ascension.Equipment.Manager;
using Ascension.Storage.UI;

namespace Ascension.Storage.Controller
{
    /// <summary>
    /// Storage Room only displays Bag, Storage, and Equipped Gear preview.
    /// Pocket inventory has been permanently deprecated.
    /// </summary>
    public class StorageRoomController : MonoBehaviour
    {
        [Header("Inventory Displays")]
        [SerializeField] private BagInventoryUI bagInventoryUI;
        [SerializeField] private StorageInventoryUI storageInventoryUI;

        [Header("Equipped Gear Preview")]
        [SerializeField] private EquippedGearPreviewUI equippedGearPreviewUI;

        private InventoryManager _inventoryManager;
        private EquipmentManager _equipmentManager;

        #region Lifecycle

        private void Start()
        {
            InitializeManagers();
            InitializeDisplays();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization

        private void InitializeManagers()
        {
            _inventoryManager = InventoryManager.Instance;
            _equipmentManager = EquipmentManager.Instance;

            if (_inventoryManager == null)
                Debug.LogError("[StorageRoomController] InventoryManager not found!");

            if (_equipmentManager == null)
                Debug.LogError("[StorageRoomController] EquipmentManager not found!");
        }

        private void InitializeDisplays()
        {
            bagInventoryUI?.Initialize();
            storageInventoryUI?.Initialize();

            // EquippedGearPreviewUI auto-initializes and listens to equipment events
        }

        #endregion

        #region Event Management

        private void SubscribeToEvents()
        {
            if (_inventoryManager?.Inventory == null) return;

            _inventoryManager.Inventory.OnInventoryChanged += RefreshAllDisplays;
        }

        private void UnsubscribeFromEvents()
        {
            if (_inventoryManager?.Inventory == null) return;

            _inventoryManager.Inventory.OnInventoryChanged -= RefreshAllDisplays;
        }

        #endregion

        #region Refresh Logic

        private void RefreshAllDisplays()
        {
            if (!isActiveAndEnabled) return;

            bagInventoryUI?.RefreshDisplay();
            storageInventoryUI?.RefreshDisplay();

            // EquippedGearPreviewUI listens to EquipmentManager.OnEquipmentChanged internally.
            // DO NOT refresh it here.
        }

        #endregion

        #region Debug Helpers

        [ContextMenu("Debug: Print Current State")]
        private void DebugPrintState()
        {
            if (_inventoryManager == null)
            {
                Debug.LogError("InventoryManager not found!");
                return;
            }

            Debug.Log("=== STORAGE ROOM STATE ===");
            Debug.Log($"Bag: {_inventoryManager.Inventory.GetBagItemCount()}/{_inventoryManager.Capacity.MaxBagSlots}");
            Debug.Log($"Storage: {_inventoryManager.Inventory.GetStorageItemCount()}/{_inventoryManager.Capacity.MaxStorageSlots}");
        }

        #endregion
    }
}