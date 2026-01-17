// ══════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Components/Inventory/InventoryGridUI.cs
// ✅ FIXED: Prevents memory leaks by caching manager reference
// ══════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.UI.Popups;

namespace Ascension.UI.Components.Inventory
{
    public class InventoryGridUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Grid Configuration")]
        [SerializeField] private ItemLocation location = ItemLocation.Bag;
        [SerializeField] private int maxSlots = 12;
        [SerializeField] private bool showEmptySlots = true;

        [Header("UI References")]
        [SerializeField] private Transform gridContent;
        [SerializeField] private GameObject itemSlotPrefab;

        [Header("Popup Context")]
        [SerializeField] private PopupSource popupSource = PopupSource.BagUI;

        [Header("Optional Filter")]
        [SerializeField] private InventoryFilterBarUI filterBar;
        #endregion

        #region Private Fields
        private List<ItemSlotUI> slotCache = new List<ItemSlotUI>();
        private ItemType? currentFilter = null;
        private bool isInitialized = false;
        private Coroutine refreshCoroutine;
        
        // ✅ NEW: Cache manager reference for safe unsubscribe
        private InventoryManager _cachedInventoryManager;
        #endregion

        #region Unity Callbacks
        private void OnEnable()
        {
            if (!isInitialized)
            {
                InitializeSlots();
                SubscribeToEvents();
                RefreshGrid();
            }
            else
            {
                SubscribeToEvents();
                RefreshGrid();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        // ✅ NEW: Extra safety cleanup
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            
            // Clear slot cache to free memory
            foreach (var slot in slotCache)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            slotCache.Clear();
        }
        #endregion

        #region Initialization
        private void InitializeSlots()
        {
            if (isInitialized) return;

            if (gridContent == null || itemSlotPrefab == null)
            {
                Debug.LogError($"[InventoryGridUI] Missing references on {gameObject.name}!");
                return;
            }

            // Adjust max slots based on location
            if (location == ItemLocation.Bag && maxSlots == 12)
            {
                maxSlots = InventoryManager.Instance?.Capacity.MaxBagSlots ?? 12;
            }
            else if (location == ItemLocation.Storage && maxSlots == 60)
            {
                maxSlots = InventoryManager.Instance?.Capacity.MaxStorageSlots ?? 60;
            }

            // Create slot objects
            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(itemSlotPrefab, gridContent);
                ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();

                if (slotUI == null)
                {
                    Debug.LogError($"[InventoryGridUI] ItemSlotUI component missing!");
                    Destroy(slotObj);
                    continue;
                }

                slotUI.gameObject.SetActive(false);
                slotCache.Add(slotUI);
            }

            isInitialized = true;
            Debug.Log($"[InventoryGridUI] Initialized {slotCache.Count} slots for {location}");
        }
        #endregion

        #region Event Management - FIXED
        /// <summary>
        /// ✅ FIXED: Cache manager reference for safe unsubscribe
        /// </summary>
        private void SubscribeToEvents()
        {
            // Get fresh instance
            var inventoryMgr = InventoryManager.Instance;
            if (inventoryMgr == null)
            {
                Debug.LogWarning($"[InventoryGridUI] InventoryManager not found!");
                return;
            }

            // ✅ CRITICAL: Cache the manager reference BEFORE subscribing
            _cachedInventoryManager = inventoryMgr;

            // Subscribe to events
            _cachedInventoryManager.Inventory.OnInventoryChanged += ScheduleRefresh;
            _cachedInventoryManager.OnInventoryLoaded += RefreshGrid;

            if (filterBar != null)
            {
                filterBar.OnFilterChanged += OnFilterChanged;
            }
        }

        /// <summary>
        /// ✅ FIXED: Always unsubscribe from cached manager, even if Instance is null
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            // ✅ Use cached reference instead of Instance
            // This works even if InventoryManager.Instance is null!
            if (_cachedInventoryManager != null)
            {
                _cachedInventoryManager.Inventory.OnInventoryChanged -= ScheduleRefresh;
                _cachedInventoryManager.OnInventoryLoaded -= RefreshGrid;
                _cachedInventoryManager = null; // Clear cache
            }

            if (filterBar != null)
            {
                filterBar.OnFilterChanged -= OnFilterChanged;
            }
        }

        private void ScheduleRefresh()
        {
            if (refreshCoroutine == null)
            {
                refreshCoroutine = StartCoroutine(DebouncedRefresh());
            }
        }

        private System.Collections.IEnumerator DebouncedRefresh()
        {
            yield return null;
            RefreshGrid();
            refreshCoroutine = null;
        }

        private void OnFilterChanged(ItemType? filterType)
        {
            currentFilter = filterType;
            RefreshGrid();
        }
        #endregion

        #region Refresh Logic
        private void RefreshGrid()
        {
            if (!isInitialized)
            {
                InitializeSlots();
            }

            var inventoryMgr = InventoryManager.Instance;
            if (inventoryMgr == null)
            {
                Debug.LogWarning($"[InventoryGridUI] InventoryManager not available!");
                return;
            }

            List<ItemInstance> items = GetFilteredItems(inventoryMgr);

            for (int i = 0; i < maxSlots; i++)
            {
                ItemSlotUI slot = slotCache[i];

                if (i < items.Count)
                {
                    ItemInstance item = items[i];
                    ItemBaseSO itemData = inventoryMgr.Database.GetItem(item.itemID);

                    if (itemData != null)
                    {
                        slot.Setup(itemData, item, () => OnItemClicked(item));
                        slot.gameObject.SetActive(true);
                    }
                    else
                    {
                        slot.ShowEmpty();
                        slot.gameObject.SetActive(showEmptySlots);
                    }
                }
                else
                {
                    slot.ShowEmpty();
                    slot.gameObject.SetActive(showEmptySlots);
                }
            }
        }

        private List<ItemInstance> GetFilteredItems(InventoryManager inventoryMgr)
        {
            if (currentFilter.HasValue)
            {
                if (location == ItemLocation.Storage)
                {
                    return inventoryMgr.Inventory.GetStorageItemsByType(currentFilter, inventoryMgr.Database);
                }
                else
                {
                    var allItems = inventoryMgr.Inventory.GetItemsByLocation(location);
                    return allItems.FindAll(item =>
                    {
                        var itemData = inventoryMgr.Database.GetItem(item.itemID);
                        return itemData != null && itemData.ItemType == currentFilter.Value;
                    });
                }
            }
            else
            {
                return inventoryMgr.Inventory.GetItemsByLocation(location);
            }
        }
        #endregion

        #region Item Click Handler
        private void OnItemClicked(ItemInstance item)
        {
            var inventoryMgr = InventoryManager.Instance;
            if (inventoryMgr?.Database == null)
            {
                Debug.LogError($"[InventoryGridUI] InventoryManager not available!");
                return;
            }

            ItemBaseSO itemData = inventoryMgr.Database.GetItem(item.itemID);
            if (itemData == null)
            {
                Debug.LogError($"[InventoryGridUI] Item data not found: {item.itemID}");
                return;
            }

            PopupContext context = GetPopupContext();
            PopupManager.Instance?.ShowItemPopup(itemData, item, context);
        }

        private PopupContext GetPopupContext()
        {
            return popupSource switch
            {
                PopupSource.StorageUI => PopupContext.FromStorage(),
                PopupSource.BagUI => PopupContext.FromBag(),
                PopupSource.EquippedGear => PopupContext.FromEquippedGear(),
                PopupSource.InventoryPanel => PopupContext.FromInventoryPanel(),
                _ => PopupContext.FromBag()
            };
        }
        #endregion

        #region Public API
        public void ForceRefresh() => RefreshGrid();
        
        public int GetVisibleSlotCount() => slotCache.FindAll(slot => slot.gameObject.activeSelf).Count;
        
        public int GetFilledSlotCount() => slotCache.FindAll(slot => !slot.IsEmpty() && slot.gameObject.activeSelf).Count;

        public void ConnectFilterBar(InventoryFilterBarUI newFilterBar)
        {
            if (filterBar != null)
            {
                filterBar.OnFilterChanged -= OnFilterChanged;
            }

            filterBar = newFilterBar;
            if (filterBar != null)
            {
                filterBar.OnFilterChanged += OnFilterChanged;
                currentFilter = filterBar.GetCurrentFilter();
                RefreshGrid();
            }
        }

        public void SetMaxSlots(int newMax)
        {
            if (newMax < maxSlots)
            {
                Debug.LogWarning($"[InventoryGridUI] Cannot reduce slot count");
                return;
            }

            int slotsToAdd = newMax - maxSlots;
            for (int i = 0; i < slotsToAdd; i++)
            {
                GameObject slotObj = Instantiate(itemSlotPrefab, gridContent);
                ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();

                if (slotUI != null)
                {
                    slotUI.gameObject.SetActive(false);
                    slotCache.Add(slotUI);
                }
            }

            maxSlots = newMax;
            RefreshGrid();
        }
        #endregion
    }
}