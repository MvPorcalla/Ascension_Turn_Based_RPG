// ══════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Components/Inventory/InventoryGridUI.cs
// ✅ REFACTORED: CanvasGroup visibility, no lambda allocations, removed debug logs
// ══════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using Ascension.Core;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.UI.Popups;

namespace Ascension.UI.Components.Inventory
{
    /// <summary>
    /// Displays inventory items in a grid layout
    /// Uses CanvasGroup for visibility (no SetActive layout thrashing)
    /// Uses GameEvents for click handling (no lambda allocations)
    /// </summary>
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
        
        private InventoryManager InventoryMgr => GameBootstrap.Inventory;
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
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            ClearSlotCache();
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
            if (location == ItemLocation.Bag && InventoryMgr != null)
            {
                maxSlots = InventoryMgr.Capacity?.MaxBagSlots ?? 12;
            }
            else if (location == ItemLocation.Storage && InventoryMgr != null)
            {
                maxSlots = InventoryMgr.Capacity?.MaxStorageSlots ?? 60;
            }

            // ✅ NEW: Create all slots immediately, keep them active
            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(itemSlotPrefab, gridContent);
                ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();

                if (slotUI == null)
                {
                    Debug.LogError($"[InventoryGridUI] ItemSlotUI component missing on prefab!");
                    Destroy(slotObj);
                    continue;
                }

                // ✅ CRITICAL: Keep slot active, hide via CanvasGroup
                slotObj.SetActive(true);
                slotUI.ShowEmpty(); // Initializes CanvasGroup to invisible
                
                slotCache.Add(slotUI);
            }

            isInitialized = true;
        }

        private void ClearSlotCache()
        {
            foreach (var slot in slotCache)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            slotCache.Clear();
        }
        #endregion

        #region Event Management
        private void SubscribeToEvents()
        {
            GameEvents.OnInventoryChanged += ScheduleRefresh;
            
            // ✅ NEW: Listen for item slot clicks
            GameEvents.OnItemSlotClicked += OnItemSlotClicked;

            if (filterBar != null)
            {
                filterBar.OnFilterChanged += OnFilterChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnInventoryChanged -= ScheduleRefresh;
            GameEvents.OnItemSlotClicked -= OnItemSlotClicked;

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
            yield return null; // Wait 1 frame
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
        /// <summary>
        /// ✅ OPTIMIZED: No SetActive calls, no lambda allocations
        /// </summary>
        private void RefreshGrid()
        {
            if (!isInitialized)
            {
                InitializeSlots();
            }

            if (InventoryMgr == null)
            {
                return;
            }

            List<ItemInstance> items = GetFilteredItems();

            for (int i = 0; i < maxSlots; i++)
            {
                ItemSlotUI slot = slotCache[i];

                if (i < items.Count)
                {
                    ItemInstance item = items[i];
                    ItemBaseSO itemData = InventoryMgr.Database?.GetItem(item.itemID);

                    if (itemData != null)
                    {
                        // ✅ CRITICAL: No lambda allocation, uses GameEvents
                        slot.Setup(itemData, item, null);
                    }
                    else
                    {
                        slot.ShowEmpty();
                    }
                }
                else
                {
                    // ✅ CRITICAL: ShowEmpty() uses CanvasGroup, not SetActive
                    if (showEmptySlots)
                    {
                        slot.ShowEmpty();
                    }
                    else
                    {
                        slot.SetVisible(false);
                    }
                }
            }
        }

        private List<ItemInstance> GetFilteredItems()
        {
            if (currentFilter.HasValue)
            {
                if (location == ItemLocation.Storage)
                {
                    return InventoryMgr.Inventory.GetStorageItemsByType(currentFilter, InventoryMgr.Database);
                }
                else
                {
                    var allItems = InventoryMgr.Inventory.GetItemsByLocation(location);
                    var filtered = new List<ItemInstance>(allItems.Count);
                    
                    for (int i = 0; i < allItems.Count; i++)
                    {
                        var item = allItems[i];
                        var itemData = InventoryMgr.Database?.GetItem(item.itemID);
                        if (itemData != null && itemData.ItemType == currentFilter.Value)
                        {
                            filtered.Add(item);
                        }
                    }
                    
                    return filtered;
                }
            }
            else
            {
                return InventoryMgr.Inventory.GetItemsByLocation(location);
            }
        }
        #endregion

        #region Click Handler (No Lambda)
        /// <summary>
        /// ✅ NEW: Handles clicks from GameEvents (triggered by ItemSlotUI)
        /// </summary>
        private void OnItemSlotClicked(ItemBaseSO itemData, ItemInstance itemInstance)
        {
            if (itemData == null || itemInstance == null)
                return;

            // Verify this click is for our grid's location
            if (itemInstance.location != location)
                return;

            PopupContext context = GetPopupContext();
            PopupManager.Instance?.ShowItemPopup(itemData, itemInstance, context);
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
        public void ForceRefresh()
        {
            RefreshGrid();
        }
        
        public int GetVisibleSlotCount()
        {
            int count = 0;
            for (int i = 0; i < slotCache.Count; i++)
            {
                if (slotCache[i].GetComponent<CanvasGroup>().alpha > 0f)
                    count++;
            }
            return count;
        }
        
        public int GetFilledSlotCount()
        {
            int count = 0;
            for (int i = 0; i < slotCache.Count; i++)
            {
                var slot = slotCache[i];
                if (!slot.IsEmpty() && slot.GetComponent<CanvasGroup>().alpha > 0f)
                    count++;
            }
            return count;
        }

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
                return;
            }

            int slotsToAdd = newMax - maxSlots;
            for (int i = 0; i < slotsToAdd; i++)
            {
                GameObject slotObj = Instantiate(itemSlotPrefab, gridContent);
                ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();

                if (slotUI != null)
                {
                    slotObj.SetActive(true);
                    slotUI.ShowEmpty();
                    slotCache.Add(slotUI);
                }
            }

            maxSlots = newMax;
            RefreshGrid();
        }
        #endregion

        #region Debug Tools
#if UNITY_EDITOR
        [ContextMenu("Debug: Force Refresh")]
        private void DebugForceRefresh()
        {
            if (Application.isPlaying)
            {
                ForceRefresh();
            }
        }

        [ContextMenu("Debug: Print Slot Info")]
        private void DebugPrintSlotInfo()
        {
            Debug.Log($"=== {gameObject.name} ===");
            Debug.Log($"Location: {location}");
            Debug.Log($"Max Slots: {maxSlots}");
            Debug.Log($"Visible Slots: {GetVisibleSlotCount()}");
            Debug.Log($"Filled Slots: {GetFilledSlotCount()}");
            Debug.Log($"Current Filter: {(currentFilter.HasValue ? currentFilter.Value.ToString() : "None")}");
        }
#endif
        #endregion
    }
}