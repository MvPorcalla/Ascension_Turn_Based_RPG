using UnityEngine;
using Ascension.Inventory.Manager;
using Ascension.Equipment.Manager;
using Ascension.Storage.UI;

namespace Ascension.Storage.Controller
{
    // NOTE:
    // Pocket inventory has been permanently deprecated.
    // Storage Room only displays Bag, Storage, and Equipped Gear preview.
    public class StorageRoomController : MonoBehaviour
    {
        [Header("Inventory Displays")]
        [SerializeField] private BagInventoryUI bagInventoryUI;
        [SerializeField] private StorageInventoryUI storageInventoryUI;

        [Header("Equipped Gear Preview")]
        [SerializeField] private EquippedGearPreviewUI equippedGearPreviewUI;

        private InventoryManager _inventoryManager;
        private EquipmentManager _equipmentManager;

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

        private void RefreshAllDisplays()
        {
            if (!isActiveAndEnabled) return;

            bagInventoryUI?.RefreshDisplay();
            storageInventoryUI?.RefreshDisplay();

            // EquippedGearPreviewUI listens to EquipmentManager.OnEquipmentChanged internally.
            // DO NOT refresh it here.
        }

        // Store All button (excludes equipped items)
        public void OnStoreAllButtonClicked()
        {
            if (_inventoryManager?.Inventory == null) return;

            _inventoryManager.Inventory.StoreAllItems(
                itemId => _equipmentManager?.IsItemEquipped(itemId) ?? false
            );
        }
    }
}
