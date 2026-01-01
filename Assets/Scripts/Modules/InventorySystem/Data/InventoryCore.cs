// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Data/InventoryCore.cs
// Core data structure for managing inventory items
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
        // Legacy event (keep for backward compatibility)
        public event Action OnInventoryChanged;

        // ✅ Granular events for UI optimization
        public event Action<ItemInstance, int> OnItemAdded;           // (item, quantity added)
        public event Action<ItemInstance, int> OnItemRemoved;         // (item, quantity removed)
        public event Action<ItemInstance, ItemLocation, ItemLocation> OnItemMoved;  // (item, from, to)
        public event Action<ItemInstance, int, int> OnQuantityChanged;  // (item, oldQty, newQty)

        /// <summary>
        /// Invoke all relevant events for an operation
        /// </summary>
        private void NotifyInventoryChanged(InventoryChangeType changeType, ItemInstance item, 
            int quantity = 0, ItemLocation? oldLocation = null)
        {
            // Fire specific event
            switch (changeType)
            {
                case InventoryChangeType.ItemAdded:
                    OnItemAdded?.Invoke(item, quantity);
                    break;
                case InventoryChangeType.ItemRemoved:
                    OnItemRemoved?.Invoke(item, quantity);
                    break;
                case InventoryChangeType.ItemMoved:
                    if (oldLocation.HasValue)
                        OnItemMoved?.Invoke(item, oldLocation.Value, item.location);
                    break;
                case InventoryChangeType.QuantityChanged:
                    OnQuantityChanged?.Invoke(item, quantity, item.quantity);
                    break;
            }

            // Always fire legacy event
            OnInventoryChanged?.Invoke();
        }

        private enum InventoryChangeType
        {
            ItemAdded,
            ItemRemoved,
            ItemMoved,
            QuantityChanged
        }
        #endregion

        #region Services & Dependencies
        private ItemQueryService _queryService;
        private ItemStackingService _stackingService;
        private ItemLocationService _locationService;
        private SlotCapacityManager _capacityManager;
        #endregion

        #region Constructors

        /// <summary>
        /// ✅ DEFAULT CONSTRUCTOR - Creates default services
        /// Use this for normal gameplay
        /// </summary>
        public InventoryCore(SlotCapacityManager capacityManager = null)
        {
            _capacityManager = capacityManager ?? new SlotCapacityManager();
            InitializeServices();
        }

        /// <summary>
        /// ✅ DEPENDENCY INJECTION CONSTRUCTOR - Accepts custom services
        /// Use this for testing or custom implementations
        /// </summary>
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
            
            // ✅ CRITICAL: Do NOT call InitializeServices() here!
            // Services are already set - calling InitializeServices() would overwrite them
        }

        /// <summary>
        /// ✅ FIXED: Only initializes services if they're null
        /// Uses null-coalescing assignment (??=) to preserve injected dependencies
        /// </summary>
        private void InitializeServices()
        {
            _queryService ??= new ItemQueryService();
            _stackingService ??= new ItemStackingService();
            _locationService ??= new ItemLocationService(_stackingService, _queryService);
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

        // Delegate to capacity manager for space queries
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
        /// Add item to inventory with location preference and capacity checks
        /// ✅ Returns InventoryResult with detailed feedback
        /// </summary>
        public InventoryResult AddItem(string itemID, int quantity = 1, bool addToBag = false, GameDatabaseSO database = null)
        {
            // Validation
            if (database == null)
                return InventoryResult.DatabaseMissing();

            ItemBaseSO itemData = database.GetItem(itemID);
            if (itemData == null)
                return InventoryResult.ItemNotFound(itemID);

            if (quantity <= 0)
                return InventoryResult.Fail("Quantity must be positive", InventoryErrorCode.InvalidOperation);

            // Determine target location
            ItemLocation targetLocation = DetermineTargetLocation(addToBag);

            if (targetLocation == ItemLocation.None)
                return InventoryResult.NoSpace(addToBag ? ItemLocation.Bag : ItemLocation.Storage);

            ItemInstance resultItem = null;

            // Add stackable items
            if (itemData.IsStackable)
            {
                resultItem = _stackingService.AddToExistingOrCreateNew(
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
                            // Partial success
                            if (resultItem != null)
                            {
                                NotifyInventoryChanged(InventoryChangeType.ItemAdded, resultItem, i);
                            }
                            return InventoryResult.Fail(
                                $"{targetLocation} full! Added {i}/{quantity} items", 
                                InventoryErrorCode.NoSpace
                            );
                        }
                    }

                    var newItem = new ItemInstance(itemID, 1, targetLocation);
                    allItems.Add(newItem);
                    resultItem = newItem;
                }
            }

            NotifyInventoryChanged(InventoryChangeType.ItemAdded, resultItem, quantity);
            return InventoryResult.Ok(resultItem, $"Added {quantity}x {itemData.ItemName}");
        }

        /// <summary>
        /// Remove item from inventory
        /// ✅ Returns InventoryResult
        /// </summary>
        public InventoryResult RemoveItem(ItemInstance item, int quantity = 1)
        {
            if (!allItems.Contains(item))
                return InventoryResult.Fail("Item not found in inventory", InventoryErrorCode.ItemNotFound);

            if (quantity > item.quantity)
                return InventoryResult.InsufficientQuantity(item.itemID, quantity, item.quantity);

            int oldQuantity = item.quantity;
            item.quantity -= quantity;

            if (_stackingService.ShouldRemoveStack(item))
            {
                allItems.Remove(item);
                NotifyInventoryChanged(InventoryChangeType.ItemRemoved, item, quantity);
            }
            else
            {
                NotifyInventoryChanged(InventoryChangeType.QuantityChanged, item, oldQuantity);
            }

            return InventoryResult.Ok(item, $"Removed {quantity}x {item.itemID}");
        }

        #endregion

        #region Location Methods (Delegate to LocationService)

        /// <summary>
        /// Move item to bag
        /// ✅ Returns InventoryResult and uses new event system
        /// </summary>
        public InventoryResult MoveToBag(ItemInstance item, int quantity, GameDatabaseSO database)
        {
            // ✅ Fast path: Check target location FIRST
            if (item?.location == ItemLocation.Bag)
                return InventoryResult.Fail("Item already in bag", InventoryErrorCode.AlreadyInLocation);

            // Validation
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

            // ✅ Capture old location BEFORE the move
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
                NotifyInventoryChanged(InventoryChangeType.ItemMoved, item, 0, oldLocation);
                return InventoryResult.Ok(item, $"Moved {quantity}x to bag");
            }

            return InventoryResult.NoSpace(ItemLocation.Bag);
        }

        /// <summary>
        /// Move item to pocket
        /// ✅ Returns InventoryResult and uses new event system
        /// </summary>
        public InventoryResult MoveToPocket(ItemInstance item, int quantity, GameDatabaseSO database)
        {
            // ✅ Fast path: Check target location FIRST
            if (item?.location == ItemLocation.Pocket)
                return InventoryResult.Fail("Item already in pocket", InventoryErrorCode.AlreadyInLocation);

            // Validation
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

            // ✅ Capture old location BEFORE the move
            ItemLocation oldLocation = item.location;

            bool success = _locationService.MoveToPocket(
                allItems, 
                item, 
                quantity, 
                _capacityManager.MaxPocketSlots, 
                itemData
            );

            if (success)
            {
                NotifyInventoryChanged(InventoryChangeType.ItemMoved, item, 0, oldLocation);
                return InventoryResult.Ok(item, $"Moved {quantity}x to pocket");
            }

            return InventoryResult.NoSpace(ItemLocation.Pocket);
        }

        /// <summary>
        /// Move item to storage
        /// ✅ Returns InventoryResult and uses new event system
        /// </summary>
        public InventoryResult MoveToStorage(ItemInstance item, int quantity, GameDatabaseSO database)
        {
            // ✅ Fast path: Check target location FIRST
            if (item?.location == ItemLocation.Storage)
                return InventoryResult.Fail("Item already in storage", InventoryErrorCode.AlreadyInLocation);

            // Validation
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

            // ✅ Capture old location BEFORE the move
            ItemLocation oldLocation = item.location;

            bool success = _locationService.MoveToStorage(allItems, item, quantity, itemData);

            if (success)
            {
                NotifyInventoryChanged(InventoryChangeType.ItemMoved, item, 0, oldLocation);
                return InventoryResult.Ok(item, $"Moved {quantity}x to storage");
            }

            return InventoryResult.Fail("Failed to move to storage", InventoryErrorCode.Unknown);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Store all bag items (excluding equipped gear)
        /// ✅ IMPROVED: Accepts delegate to decouple from EquipmentManager
        /// </summary>
        public void StoreAllItems(Func<string, bool> isEquippedChecker = null)
        {
            // Default: nothing is equipped (safe fallback)
            isEquippedChecker ??= (_) => false;

            var bagItems = GetBagItems();
            var itemsToMove = new List<ItemInstance>();

            foreach (var item in bagItems)
            {
                if (!isEquippedChecker(item.itemID))
                {
                    itemsToMove.Add(item);
                }
            }

            // Batch move
            foreach (var item in itemsToMove)
            {
                ItemLocation oldLocation = item.location;
                item.location = ItemLocation.Storage;
                NotifyInventoryChanged(InventoryChangeType.ItemMoved, item, 0, oldLocation);
            }

            // Fire legacy event once
            if (itemsToMove.Count > 0)
            {
                OnInventoryChanged?.Invoke();
            }
        }

        public void ClearAll()
        {
            allItems.Clear();
            OnInventoryChanged?.Invoke();
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Determine target location with capacity validation
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

            return ItemLocation.None;
        }

        private int GetItemCountByLocation(ItemLocation location)
        {
            return _queryService.GetItemCountByLocation(allItems, location);
        }

        #endregion
    }
}