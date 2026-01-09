// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\UI\StorageInventoryUI.cs
// UI component for displaying storage inventory with filtering
// ──────────────────────────────────────────────────

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.Inventory.UI;
using Ascension.SharedUI.Popups;

namespace Ascension.Storage.UI
{
    public class StorageInventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform storageContent;
        [SerializeField] private GameObject itemSlotPrefab;

        [Header("Configuration")]
        [SerializeField] private int maxStorageSlots = 60;
        [SerializeField] private bool showEmptySlots = true;

        [Header("Filter Buttons")]
        [SerializeField] private Button allItemsButton;
        [SerializeField] private Button weaponButton;
        [SerializeField] private Button gearButton;
        [SerializeField] private Button potionButton;
        [SerializeField] private Button materialsButton;
        [SerializeField] private Button miscButton;

        // ✅ REMOVED: No more direct popup references
        // [Header("Popups")]
        // [SerializeField] private ItemPopup itemPopup;
        // [SerializeField] private PotionPopup potionPopup;

        private ItemType? currentFilter = null;
        private List<ItemSlotUI> slotCache = new List<ItemSlotUI>();
        private bool isInitialized = false;

        #region Lifecycle

        private void Start()
        {
            InitializeSlots();
            SetupFilterButtons();
            SubscribeToEvents();
            RefreshStorage();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Event Management

        private void SubscribeToEvents()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.Inventory.OnInventoryChanged += RefreshStorage;
                InventoryManager.Instance.OnInventoryLoaded += RefreshStorage;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.Inventory.OnInventoryChanged -= RefreshStorage;
                InventoryManager.Instance.OnInventoryLoaded -= RefreshStorage;
            }
        }

        #endregion

        #region Initialization

        private void InitializeSlots()
        {
            if (isInitialized) return;

            for (int i = 0; i < maxStorageSlots; i++)
            {
                GameObject slotObj = Instantiate(itemSlotPrefab, storageContent);
                ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();

                if (slotUI == null)
                {
                    Debug.LogError("[StorageInventoryUI] ItemSlotUI component missing!");
                    Destroy(slotObj);
                    continue;
                }

                slotUI.gameObject.SetActive(false);
                slotCache.Add(slotUI);
            }

            isInitialized = true;
        }

        private void SetupFilterButtons()
        {
            if (allItemsButton != null)
                allItemsButton.onClick.AddListener(() => SetFilter(null));

            if (weaponButton != null)
                weaponButton.onClick.AddListener(() => SetFilter(ItemType.Weapon));

            if (gearButton != null)
                gearButton.onClick.AddListener(() => SetFilter(ItemType.Gear));

            if (potionButton != null)
                potionButton.onClick.AddListener(() => SetFilter(ItemType.Consumable));

            if (materialsButton != null)
                materialsButton.onClick.AddListener(() => SetFilter(ItemType.Material));

            if (miscButton != null)
                miscButton.onClick.AddListener(() => SetFilter(ItemType.Misc));
        }

        #endregion

        #region Filter System

        private void SetFilter(ItemType? filterType)
        {
            currentFilter = filterType;
            RefreshStorage();
            UpdateFilterButtonStates();
        }

        private void UpdateFilterButtonStates()
        {
            // TODO: Add visual feedback for active filter button
        }

        #endregion

        #region Refresh Logic

        private void RefreshStorage()
        {
            if (!isInitialized)
            {
                InitializeSlots();
            }

            var storageItems = InventoryManager.Instance.Inventory.GetStorageItemsByType(
                currentFilter,
                InventoryManager.Instance.Database
            );

            if (storageItems.Count > maxStorageSlots)
            {
                Debug.LogWarning($"[StorageInventoryUI] Storage has {storageItems.Count} items but only {maxStorageSlots} slots!");
            }

            for (int i = 0; i < maxStorageSlots; i++)
            {
                ItemSlotUI slot = slotCache[i];

                if (i < storageItems.Count)
                {
                    ItemInstance item = storageItems[i];
                    ItemBaseSO itemData = InventoryManager.Instance.Database.GetItem(item.itemID);

                    if (itemData != null)
                    {
                        slot.Setup(itemData, item, () => OnItemClicked(item));
                        slot.gameObject.SetActive(true);
                    }
                    else
                    {
                        slot.ShowEmpty();
                        slot.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (showEmptySlots)
                    {
                        slot.ShowEmpty();
                        slot.gameObject.SetActive(true);
                    }
                    else
                    {
                        slot.gameObject.SetActive(false);
                    }
                }
            }
        }

        #endregion

        #region Item Click Handler

        /// <summary>
        /// ✅ REFACTORED: Uses PopupManager.ShowItemPopup() with context
        /// </summary>
        private void OnItemClicked(ItemInstance item)
        {
            ItemBaseSO itemData = InventoryManager.Instance.Database.GetItem(item.itemID);

            if (itemData == null)
            {
                Debug.LogError($"[StorageInventoryUI] Item data not found: {item.itemID}");
                return;
            }

            // ✅ One line - PopupManager handles everything
            PopupManager.Instance.ShowItemPopup(
                itemData, 
                item, 
                PopupContext.FromStorage() // Context tells popup "this is from storage"
            );
        }

        #endregion

        #region Public Methods

        public void ForceRefresh() => RefreshStorage();
        public int GetSlotCount() => slotCache.Count;

        public int GetVisibleItemCount()
        {
            int count = 0;
            foreach (var slot in slotCache)
            {
                if (!slot.IsEmpty() && slot.gameObject.activeSelf)
                    count++;
            }
            return count;
        }

        public void SetMaxStorageSlots(int newMax)
        {
            if (newMax < maxStorageSlots)
            {
                Debug.LogWarning($"[StorageInventoryUI] Cannot reduce storage slots");
                return;
            }

            int slotsToAdd = newMax - maxStorageSlots;
            for (int i = 0; i < slotsToAdd; i++)
            {
                GameObject slotObj = Instantiate(itemSlotPrefab, storageContent);
                ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();

                if (slotUI != null)
                {
                    slotUI.gameObject.SetActive(false);
                    slotCache.Add(slotUI);
                }
            }

            maxStorageSlots = newMax;
            RefreshStorage();
        }

        public void Initialize()
        {
            if (!isInitialized)
            {
                InitializeSlots();
                SetupFilterButtons();
            }
        }

        public void RefreshDisplay() => RefreshStorage();

        #endregion
    }
}