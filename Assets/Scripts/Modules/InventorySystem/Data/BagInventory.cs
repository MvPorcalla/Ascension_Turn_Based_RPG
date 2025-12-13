// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Data/BagInventory.cs
// STEP 4: Refactored to use services (much cleaner!)
// ══════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Database;
using Ascension.Inventory.Services;

namespace Ascension.Inventory.Data
{
    [Serializable]
    public class BagInventory
    {
        #region Serialized Fields
        public List<ItemInstance> allItems = new List<ItemInstance>();
        public int maxBagSlots = 12;
        public int maxPocketSlots = 6;
        #endregion

        #region Events
        public event Action OnInventoryChanged;
        #endregion

        #region Services
        private ItemQueryService _queryService;
        private ItemStackingService _stackingService;
        private ItemLocationService _locationService;
        #endregion

        #region Constructor
        public BagInventory()
        {
            InitializeServices();
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

        public bool HasBagSpace()
            => _queryService.HasBagSpace(allItems, maxBagSlots);

        public bool HasPocketSpace()
            => _queryService.HasPocketSpace(allItems, maxPocketSlots);

        public int GetEmptyBagSlots()
            => _queryService.GetEmptyBagSlots(allItems, maxBagSlots);

        public int GetEmptyPocketSlots()
            => _queryService.GetEmptyPocketSlots(allItems, maxPocketSlots);

        #endregion

        #region Add/Remove Methods

        /// <summary>
        /// Add item to inventory (goes to storage by default)
        /// </summary>
        public bool AddItem(string itemID, int quantity = 1, bool addToBag = false, GameDatabaseSO database = null)
        {
            if (database == null)
            {
                Debug.LogError("[BagInventory] Database required to add items!");
                return false;
            }

            ItemBaseSO itemData = database.GetItem(itemID);
            if (itemData == null)
            {
                Debug.LogError($"[BagInventory] Item not found: {itemID}");
                return false;
            }

            // Check bag space for non-storage additions
            if (addToBag && !HasBagSpace())
            {
                Debug.LogWarning("[BagInventory] Bag full, sending to storage");
                addToBag = false;
            }

            if (itemData.IsStackable)
            {
                // Use stacking service to add stackable items
                _stackingService.AddToExistingOrCreateNew(
                    allItems,
                    itemID,
                    quantity,
                    addToBag,
                    false, // isInPocket
                    itemData.MaxStackSize
                );
            }
            else
            {
                // Non-stackable items: create individual instances
                for (int i = 0; i < quantity; i++)
                {
                    // Check space before each addition
                    if (addToBag && !HasBagSpace())
                    {
                        Debug.LogWarning("[BagInventory] Bag full, remaining items sent to storage");
                        addToBag = false;
                    }

                    allItems.Add(new ItemInstance(itemID, 1, addToBag, false));
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Remove item from inventory
        /// </summary>
        public bool RemoveItem(ItemInstance item, int quantity = 1)
        {
            if (!allItems.Contains(item))
            {
                Debug.LogWarning("[BagInventory] Item not found in inventory");
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

        /// <summary>
        /// Move item to bag (requires database for stackable check)
        /// </summary>
        public bool MoveToBag(ItemInstance item, int quantity, GameDatabaseSO database)
        {
            ItemBaseSO itemData = database.GetItem(item.itemID);
            if (itemData == null)
            {
                Debug.LogError($"[BagInventory] Item data not found: {item.itemID}");
                return false;
            }

            bool success = _locationService.MoveToBag(allItems, item, quantity, maxBagSlots, itemData);

            if (success)
                OnInventoryChanged?.Invoke();

            return success;
        }

        /// <summary>
        /// Move item to pocket (requires database for stackable check)
        /// </summary>
        public bool MoveToPocket(ItemInstance item, int quantity, GameDatabaseSO database)
        {
            ItemBaseSO itemData = database.GetItem(item.itemID);
            if (itemData == null)
            {
                Debug.LogError($"[BagInventory] Item data not found: {item.itemID}");
                return false;
            }

            bool success = _locationService.MoveToPocket(allItems, item, quantity, maxPocketSlots, itemData);

            if (success)
                OnInventoryChanged?.Invoke();

            return success;
        }

        /// <summary>
        /// Move item to storage (requires database for stackable check)
        /// </summary>
        public bool MoveToStorage(ItemInstance item, int quantity, GameDatabaseSO database)
        {
            ItemBaseSO itemData = database.GetItem(item.itemID);
            if (itemData == null)
            {
                Debug.LogError($"[BagInventory] Item data not found: {item.itemID}");
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
        /// Store all items from bag to storage (excludes equipped items)
        /// </summary>
        public void StoreAllItems()
        {
            // ToList() to avoid modification during iteration
            foreach (var item in GetBagItems().ToList())
            {
                if (!item.isEquipped)
                {
                    item.isInBag = false;
                    item.isInPocket = false;
                }
            }

            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Clear all items (for testing/reset)
        /// </summary>
        public void ClearAll()
        {
            allItems.Clear();
            OnInventoryChanged?.Invoke();
        }

        #endregion
    }
}