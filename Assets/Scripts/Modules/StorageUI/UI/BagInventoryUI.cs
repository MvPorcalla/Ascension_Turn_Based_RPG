// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\StorageSystem\UI\BagInventoryUI.cs
// UI component for displaying bag inventory with filtering
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
    public class BagInventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform inventoryContent;
        [SerializeField] private GameObject itemSlotPrefab;

        [Header("Buttons")]
        [SerializeField] private Button storeAllButton;

        [Header("Configuration")]
        [SerializeField] private int maxBagSlots = 12;

        // ✅ REMOVED: No more direct popup references
        // [Header("Popups")]
        // [SerializeField] private ItemPopup itemPopup;
        // [SerializeField] private PotionPopup potionPopup;

        private List<ItemSlotUI> slotCache = new List<ItemSlotUI>();
        private bool isInitialized = false;

        #region Lifecycle

        private void Start()
        {
            InitializeSlots();
            SetupButtons();
            SubscribeToEvents();
            RefreshBag();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            CleanupButtons();
        }

        #endregion

        #region Event Management

        private void SubscribeToEvents()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.Inventory.OnInventoryChanged += RefreshBag;
                InventoryManager.Instance.OnInventoryLoaded += RefreshBag;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.Inventory.OnInventoryChanged -= RefreshBag;
                InventoryManager.Instance.OnInventoryLoaded -= RefreshBag;
            }
        }

        #endregion

        #region Button Setup

        private void SetupButtons()
        {
            if (storeAllButton != null)
            {
                storeAllButton.onClick.AddListener(OnStoreAllClicked);
            }
        }

        private void CleanupButtons()
        {
            if (storeAllButton != null)
            {
                storeAllButton.onClick.RemoveListener(OnStoreAllClicked);
            }
        }

        #endregion

        #region Initialization

        private void InitializeSlots()
        {
            if (isInitialized) return;

            if (InventoryManager.Instance != null)
            {
                maxBagSlots = InventoryManager.Instance.Capacity.MaxBagSlots;
            }

            for (int i = 0; i < maxBagSlots; i++)
            {
                GameObject slotObj = Instantiate(itemSlotPrefab, inventoryContent);
                ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();

                if (slotUI == null)
                {
                    Debug.LogError("[BagInventoryUI] ItemSlotUI component missing!");
                    Destroy(slotObj);
                    continue;
                }

                slotUI.gameObject.SetActive(false);
                slotCache.Add(slotUI);
            }

            isInitialized = true;
        }

        #endregion

        #region Refresh Logic

        private void RefreshBag()
        {
            if (!isInitialized)
            {
                InitializeSlots();
            }

            var bagItems = InventoryManager.Instance.Inventory.GetBagItems();

            for (int i = 0; i < maxBagSlots; i++)
            {
                ItemSlotUI slot = slotCache[i];

                if (i < bagItems.Count)
                {
                    ItemInstance item = bagItems[i];
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
                    slot.ShowEmpty();
                    slot.gameObject.SetActive(true);
                }
            }

            UpdateStoreAllButtonState(bagItems.Count);
        }

        private void UpdateStoreAllButtonState(int bagItemCount)
        {
            if (storeAllButton != null)
            {
                storeAllButton.interactable = bagItemCount > 0;
            }
        }

        #endregion

        #region Item Click Handler

        /// <summary>
        /// ✅ REFACTORED: Uses PopupManager.ShowItemPopup() with context
        /// PopupManager handles routing to correct popup type
        /// </summary>
        private void OnItemClicked(ItemInstance item)
        {
            ItemBaseSO itemData = InventoryManager.Instance.Database.GetItem(item.itemID);

            if (itemData == null)
            {
                Debug.LogError($"[BagInventoryUI] Item data not found: {item.itemID}");
                return;
            }

            // ✅ One line - PopupManager handles everything
            PopupManager.Instance.ShowItemPopup(
                itemData, 
                item, 
                PopupContext.FromBag() // Context tells popup "this is from bag"
            );
        }

        #endregion

        #region Store All Handler

        private void OnStoreAllClicked()
        {
            if (InventoryManager.Instance?.Inventory == null)
            {
                Debug.LogError("[BagInventoryUI] InventoryManager not available!");
                return;
            }

            var equipmentManager = Ascension.Equipment.Manager.EquipmentManager.Instance;

            int bagItemCount = InventoryManager.Instance.Inventory.GetBagItemCount();
            
            if (bagItemCount == 0)
            {
                return;
            }

            InventoryManager.Instance.Inventory.StoreAllItems(
                itemId => equipmentManager?.IsItemEquipped(itemId) ?? false,
                InventoryManager.Instance.Database
            );
        }

        #endregion

        #region Public Methods

        public void ForceRefresh() => RefreshBag();
        public int GetSlotCount() => slotCache.Count;

        public void Initialize()
        {
            if (!isInitialized)
            {
                InitializeSlots();
            }
        }

        public void RefreshDisplay() => RefreshBag();

        #endregion
    }
}