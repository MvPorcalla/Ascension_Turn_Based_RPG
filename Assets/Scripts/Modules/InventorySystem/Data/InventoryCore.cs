// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Data/InventoryCore.cs
// ✅ REFACTORED: Pure inventory data structure (UI-agnostic)
// ══════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Database;
using Ascension.Inventory.Services;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Manager;

namespace Ascension.Inventory.Data
{
    [Serializable]
    public class InventoryCore
    {
        #region Serialized Fields
        public List<ItemInstance> allItems = new List<ItemInstance>();
        #endregion

        #region Events
        public event Action OnInventoryChanged;
        #endregion

        #region Services & Dependencies
        private ItemQueryService _queryService;
        private ItemStackingService _stackingService;
        private ItemLocationService _locationService;
        private SlotCapacityManager _capacityManager; // ✅ NEW: Injected dependency
        #endregion

        #region Constructor
        public InventoryCore(SlotCapacityManager capacityManager = null)
        {
            _capacityManager = capacityManager ?? new SlotCapacityManager();
            InitializeServices();
        }

        public InventoryCore(
            SlotCapacityManager capacityManager,
            ItemQueryService queryService,
            ItemStackingService stackingService,
            ItemLocationService locationService)
        {
            _capacityManager = capacityManager ?? new SlotCapacityManager();
            _queryService = queryService ?? new ItemQueryService();
            _stackingService = stackingService ?? new ItemStackingService();
            _locationService = locationService ?? new ItemLocationService(_stackingService, _queryService);
        }

        private void InitializeServices()
        {
            _queryService = new ItemQueryService();
            _stackingService = new ItemStackingService();
            _locationService = new ItemLocationService(_stackingService, _queryService);
        }
        #endregion

        #region Query Methods (Delegate to QueryService)
        
        public List<ItemInstance> GetBagItems()
            => _queryService.GetBagItems(allItems);

        public List<ItemInstance> GetPocketItems()
            => _queryService.GetPocketItems(allItems);

        public List<ItemInstance> GetStorageItems()
            => _queryService.GetStorageItems(allItems);

        public List<ItemInstance> GetItemsByLocation(ItemLocation location)
            => _queryService.GetItemsByLocation(allItems, location);

        public List<ItemInstance> GetStorageItemsByType(ItemType? filterType, GameDatabaseSO database)
            => _queryService.GetStorageItemsByType(allItems, filterType, database);

        public int GetItemCount(string itemID)
            => _queryService.GetItemCount(allItems, itemID);

        public bool HasItem(string itemID, int quantity = 1)
            => _queryService.HasItem(allItems, itemID, quantity);

        public int GetBagItemCount()
            => _queryService.GetBagItemCount(allItems);

        public int GetPocketItemCount()
            => _queryService.GetPocketItemCount(allItems);

        public int GetStorageItemCount()
            => _queryService.GetStorageItemCount(allItems);

        // ✅ REFACTORED: Delegate to capacity manager
        public bool HasBagSpace()
            => _capacityManager.HasSpace(ItemLocation.Bag, GetBagItemCount());

        public bool HasPocketSpace()
            => _capacityManager.HasSpace(ItemLocation.Pocket, GetPocketItemCount());

        public bool HasStorageSpace()
            => _capacityManager.HasSpace(ItemLocation.Storage, GetStorageItemCount());

        public int GetEmptyBagSlots()
            => _capacityManager.GetEmptySlots(ItemLocation.Bag, GetBagItemCount());

        public int GetEmptyPocketSlots()
            => _capacityManager.GetEmptySlots(ItemLocation.Pocket, GetPocketItemCount());

        public int GetEmptyStorageSlots()
            => _capacityManager.GetEmptySlots(ItemLocation.Storage, GetStorageItemCount());

        #endregion

        #region Add/Remove Methods

        /// <summary>
        /// ✅ REFACTORED: Add item with capacity validation
        /// </summary>
        public bool AddItem(string itemID, int quantity = 1, bool addToBag = false, GameDatabaseSO database = null)
        {
            if (database == null)
            {
                Debug.LogError("[InventoryCore] Database required to add items!");
                return false;
            }

            ItemBaseSO itemData = database.GetItem(itemID);
            if (itemData == null)
            {
                Debug.LogError($"[InventoryCore] Item not found: {itemID}");
                return false;
            }

            // Determine target location
            ItemLocation targetLocation = DetermineTargetLocation(addToBag);

            if (targetLocation == ItemLocation.None)
            {
                Debug.LogWarning("[InventoryCore] All locations full!");
                return false;
            }

            // Add stackable items
            if (itemData.IsStackable)
            {
                _stackingService.AddToExistingOrCreateNew(
                    allItems,
                    itemID,
                    quantity,
                    targetLocation,
                    itemData.MaxStackSize
                );
            }
            else
            {
                // Add non-stackable items one by one with capacity check
                for (int i = 0; i < quantity; i++)
                {
                    // Re-check space mid-loop
                    if (!_capacityManager.HasSpace(targetLocation, GetItemCountByLocation(targetLocation)))
                    {
                        if (targetLocation == ItemLocation.Bag && HasStorageSpace())
                        {
                            Debug.LogWarning("[InventoryCore] Bag full, remaining items sent to storage");
                            targetLocation = ItemLocation.Storage;
                        }
                        else
                        {
                            Debug.LogWarning($"[InventoryCore] {targetLocation} full! Added {i}/{quantity} items");
                            OnInventoryChanged?.Invoke();
                            return false;
                        }
                    }

                    allItems.Add(new ItemInstance(itemID, 1, targetLocation));
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool RemoveItem(ItemInstance item, int quantity = 1)
        {
            if (!allItems.Contains(item))
            {
                Debug.LogWarning("[InventoryCore] Item not found in inventory");
                return false;
            }

            item.quantity -= quantity;

            if (_stackingService.ShouldRemoveStack(item))
            {
                allItems.Remove(item);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        #endregion

        #region Location Methods (Delegate to LocationService)

        public bool MoveToBag(ItemInstance item, int quantity, GameDatabaseSO database)
        {
            ItemBaseSO itemData = database.GetItem(item.itemID);
            if (itemData == null)
            {
                Debug.LogError($"[InventoryCore] Item data not found: {item.itemID}");
                return false;
            }

            // ✅ Use capacity manager for validation
            bool success = _locationService.MoveToBag(
                allItems, 
                item, 
                quantity, 
                _capacityManager.MaxBagSlots, 
                itemData
            );

            if (success)
                OnInventoryChanged?.Invoke();

            return success;
        }

        public bool MoveToPocket(ItemInstance item, int quantity, GameDatabaseSO database)
        {
            ItemBaseSO itemData = database.GetItem(item.itemID);
            if (itemData == null)
            {
                Debug.LogError($"[InventoryCore] Item data not found: {item.itemID}");
                return false;
            }

            bool success = _locationService.MoveToPocket(
                allItems, 
                item, 
                quantity, 
                _capacityManager.MaxPocketSlots, 
                itemData
            );

            if (success)
                OnInventoryChanged?.Invoke();

            return success;
        }

        public bool MoveToStorage(ItemInstance item, int quantity, GameDatabaseSO database)
        {
            ItemBaseSO itemData = database.GetItem(item.itemID);
            if (itemData == null)
            {
                Debug.LogError($"[InventoryCore] Item data not found: {item.itemID}");
                return false;
            }

            bool success = _locationService.MoveToStorage(allItems, item, quantity, itemData);

            if (success)
                OnInventoryChanged?.Invoke();

            return success;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// ✅ REFACTORED: Store all bag items (excluding equipped gear)
        /// </summary>
        public void StoreAllItems()
        {
            var bagItems = GetBagItems();
            foreach (var item in bagItems)
            {
                // Query EquipmentManager for equipped state
                bool isEquipped = Equipment.Manager.EquipmentManager.Instance?.IsItemEquipped(item.itemID) ?? false;
                
                if (!isEquipped)
                {
                    item.location = ItemLocation.Storage;
                }
            }

            OnInventoryChanged?.Invoke();
        }

        public void ClearAll()
        {
            allItems.Clear();
            OnInventoryChanged?.Invoke();
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// ✅ NEW: Determine target location with capacity validation
        /// </summary>
        private ItemLocation DetermineTargetLocation(bool preferBag)
        {
            if (preferBag && HasBagSpace())
                return ItemLocation.Bag;

            if (HasStorageSpace())
                return ItemLocation.Storage;

            // If bag was preferred but full, try storage as fallback
            if (preferBag && HasStorageSpace())
            {
                Debug.LogWarning("[InventoryCore] Bag full, using storage");
                return ItemLocation.Storage;
            }

            return ItemLocation.None; // ✅ NEW: Indicate failure
        }

        private int GetItemCountByLocation(ItemLocation location)
        {
            return _queryService.GetItemCountByLocation(allItems, location);
        }

        #endregion
    }
}