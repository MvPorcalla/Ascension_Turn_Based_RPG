// ══════════════════════════════════════════════════════════════════
// Assets/Scripts/Modules/InventorySystem/Core/InventoryCore.cs
// ✅ NAMESPACE FIXED: Ascension.Inventory.Data → Ascension.Inventory.Core
// ══════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ascension.Core;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Database;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Services;

namespace Ascension.Inventory.Core
{
    /// <summary>
    /// Core inventory business logic.
    /// Triggers GameEvents directly for UI updates.
    /// </summary>
    [Serializable]
    public class InventoryCore
    {
        #region Serialized Fields
        public List<ItemInstance> allItems = new List<ItemInstance>();
        #endregion

        #region Services & Dependencies
        private ItemQueryService _queryService;
        private ItemStackingService _stackingService;
        private ItemLocationService _locationService;
        private InventoryCapacity _capacityManager;
        #endregion

        #region Constructors
        public InventoryCore(InventoryCapacity capacityManager = null)
        {
            _capacityManager = capacityManager ?? new InventoryCapacity();
            InitializeServices();
        }

        public InventoryCore(
            InventoryCapacity capacityManager,
            ItemQueryService queryService,
            ItemStackingService stackingService,
            ItemLocationService locationService)
        {
            _capacityManager = capacityManager ?? new InventoryCapacity();
            _queryService = queryService ?? new ItemQueryService();
            _stackingService = stackingService ?? new ItemStackingService();
            _locationService = locationService ?? new ItemLocationService(_stackingService, _queryService);
        }

        private void InitializeServices()
        {
            _queryService ??= new ItemQueryService();
            _stackingService ??= new ItemStackingService();
            _locationService ??= new ItemLocationService(_stackingService, _queryService);
        }
        #endregion

        #region Query Methods
        public List<ItemInstance> GetBagItems()
            => _queryService.GetBagItems(allItems);

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

        public int GetStorageItemCount()
            => _queryService.GetStorageItemCount(allItems);

        public bool HasBagSpace()
            => _capacityManager.HasSpace(ItemLocation.Bag, GetBagItemCount());

        public bool HasStorageSpace()
            => _capacityManager.HasSpace(ItemLocation.Storage, GetStorageItemCount());

        public int GetEmptyBagSlots()
            => _capacityManager.GetEmptySlots(ItemLocation.Bag, GetBagItemCount());

        public int GetEmptyStorageSlots()
            => _capacityManager.GetEmptySlots(ItemLocation.Storage, GetStorageItemCount());
        #endregion

        #region Add/Remove Methods
        public InventoryResult AddToBag(string itemID, int quantity = 1, GameDatabaseSO database = null)
        {
            if (database == null)
                return InventoryResult.DatabaseMissing();

            ItemBaseSO itemData = database.GetItem(itemID);
            if (itemData == null)
                return InventoryResult.ItemNotFound(itemID);

            if (quantity <= 0)
                return InventoryResult.Fail("Quantity must be positive", InventoryErrorCode.InvalidOperation);

            if (!CanAddToBag(itemData.IsStackable ? 1 : quantity))
                return InventoryResult.BagFull();

            ItemInstance resultItem = AddToLocationInternal(itemID, quantity, ItemLocation.Bag, itemData);

            GameEvents.TriggerItemAdded(resultItem);
            GameEvents.TriggerInventoryChanged();

            return InventoryResult.Ok(resultItem, $"Added {quantity}x {itemData.ItemName} to bag");
        }

        public InventoryResult AddToStorage(string itemID, int quantity = 1, GameDatabaseSO database = null)
        {
            if (database == null)
                return InventoryResult.DatabaseMissing();

            ItemBaseSO itemData = database.GetItem(itemID);
            if (itemData == null)
                return InventoryResult.ItemNotFound(itemID);

            if (quantity <= 0)
                return InventoryResult.Fail("Quantity must be positive", InventoryErrorCode.InvalidOperation);

            if (!CanAddToStorage(itemData.IsStackable ? 1 : quantity))
                return InventoryResult.StorageFull();

            ItemInstance resultItem = AddToLocationInternal(itemID, quantity, ItemLocation.Storage, itemData);

            GameEvents.TriggerItemAdded(resultItem);
            GameEvents.TriggerInventoryChanged();

            return InventoryResult.Ok(resultItem, $"Added {quantity}x {itemData.ItemName} to storage");
        }

        private ItemInstance AddToLocationInternal(
            string itemID, 
            int quantity, 
            ItemLocation targetLocation, 
            ItemBaseSO itemData)
        {
            if (itemData.IsStackable)
            {
                return _stackingService.AddToExistingOrCreateNew(
                    allItems,
                    itemID,
                    quantity,
                    targetLocation,
                    itemData.MaxStackSize
                );
            }
            else
            {
                ItemInstance lastItem = null;
                for (int i = 0; i < quantity; i++)
                {
                    var newItem = new ItemInstance(itemID, 1, targetLocation);
                    allItems.Add(newItem);
                    lastItem = newItem;
                }
                return lastItem;
            }
        }

        public InventoryResult RemoveItem(ItemInstance item, int quantity = 1)
        {
            if (!allItems.Contains(item))
                return InventoryResult.Fail("Item not found in inventory", InventoryErrorCode.ItemNotFound);

            if (quantity > item.quantity)
                return InventoryResult.InsufficientQuantity(item.itemID, quantity, item.quantity);

            item.quantity -= quantity;

            if (_stackingService.ShouldRemoveStack(item))
            {
                allItems.Remove(item);
            }

            GameEvents.TriggerItemRemoved(item);
            GameEvents.TriggerInventoryChanged();

            return InventoryResult.Ok(item, $"Removed {quantity}x {item.itemID}");
        }
        #endregion

        #region Location Methods
        public InventoryResult MoveToBag(ItemInstance item, int quantity, GameDatabaseSO database)
        {
            if (item?.location == ItemLocation.Bag)
                return InventoryResult.Fail("Item already in bag", InventoryErrorCode.AlreadyInLocation);

            if (item == null)
                return InventoryResult.Fail("Item is null", InventoryErrorCode.InvalidOperation);

            if (!allItems.Contains(item))
                return InventoryResult.Fail("Item not in inventory", InventoryErrorCode.ItemNotFound);

            if (database == null)
                return InventoryResult.DatabaseMissing();

            if (quantity <= 0)
                return InventoryResult.Fail("Quantity must be positive", InventoryErrorCode.InvalidOperation);

            if (quantity > item.quantity)
                return InventoryResult.InsufficientQuantity(item.itemID, quantity, item.quantity);

            ItemBaseSO itemData = database.GetItem(item.itemID);
            if (itemData == null)
                return InventoryResult.ItemNotFound(item.itemID);

            ItemLocation oldLocation = item.location;

            bool success = _locationService.MoveToBag(
                allItems, 
                item, 
                quantity, 
                _capacityManager.MaxBagSlots, 
                itemData
            );

            if (success)
            {
                GameEvents.TriggerItemMoved(item, oldLocation, ItemLocation.Bag);
                GameEvents.TriggerInventoryChanged();
                
                return InventoryResult.Ok(item, $"Moved {quantity}x to bag");
            }

            return InventoryResult.BagFull();
        }

        public InventoryResult MoveToStorage(ItemInstance item, int quantity, GameDatabaseSO database)
        {
            if (item?.location == ItemLocation.Storage)
                return InventoryResult.Fail("Item already in storage", InventoryErrorCode.AlreadyInLocation);

            if (item == null)
                return InventoryResult.Fail("Item is null", InventoryErrorCode.InvalidOperation);

            if (!allItems.Contains(item))
                return InventoryResult.Fail("Item not in inventory", InventoryErrorCode.ItemNotFound);

            if (database == null)
                return InventoryResult.DatabaseMissing();

            if (quantity <= 0)
                return InventoryResult.Fail("Quantity must be positive", InventoryErrorCode.InvalidOperation);

            if (quantity > item.quantity)
                return InventoryResult.InsufficientQuantity(item.itemID, quantity, item.quantity);

            ItemBaseSO itemData = database.GetItem(item.itemID);
            if (itemData == null)
                return InventoryResult.ItemNotFound(item.itemID);

            ItemLocation oldLocation = item.location;

            bool success = _locationService.MoveToStorage(allItems, item, quantity, itemData);

            if (success)
            {
                GameEvents.TriggerItemMoved(item, oldLocation, ItemLocation.Storage);
                GameEvents.TriggerInventoryChanged();
                
                return InventoryResult.Ok(item, $"Moved {quantity}x to storage");
            }

            return InventoryResult.StorageFull();
        }
        #endregion

        #region Utility Methods
        public void StoreAllItems(Func<string, bool> isEquippedChecker, GameDatabaseSO database)
        {
            if (database == null)
            {
                Debug.LogError("[InventoryCore] Database required for StoreAllItems");
                return;
            }

            isEquippedChecker ??= (_) => false;

            var itemsToMove = GetBagItems()
                .Where(item => !isEquippedChecker(item.itemID))
                .ToList();

            if (itemsToMove.Count == 0)
                return;

            int transferred = 0;
            int failed = 0;

            foreach (var item in itemsToMove)
            {
                var result = MoveToStorage(item, item.quantity, database);
                
                if (result.Success)
                {
                    transferred++;
                }
                else
                {
                    failed++;
                    Debug.LogWarning($"[InventoryCore] Failed to store {item.itemID}: {result.Message}");
                }
            }

            Debug.Log($"[InventoryCore] Stored {transferred} items to storage (failed: {failed})");
            
            GameEvents.TriggerInventoryChanged();
        }

        public void ClearAll()
        {
            allItems.Clear();
            GameEvents.TriggerInventoryChanged();
        }
        #endregion

        #region Private Helpers
        private bool CanAddToBag(int itemsToAdd)
        {
            return _capacityManager.CanAddItems(ItemLocation.Bag, GetBagItemCount(), itemsToAdd);
        }

        private bool CanAddToStorage(int itemsToAdd)
        {
            return _capacityManager.CanAddItems(ItemLocation.Storage, GetStorageItemCount(), itemsToAdd);
        }
        #endregion
    }
}