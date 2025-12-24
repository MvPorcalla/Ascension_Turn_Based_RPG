// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Data/BagInventory.cs
// inventory data structure
// ══════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Database;
using Ascension.Inventory.Services;
using Ascension.Inventory.Enums;

namespace Ascension.Inventory.Data
{
    [Serializable]
    public class InventoryCore
    {
        #region Serialized Fields
        public List<ItemInstance> allItems = new List<ItemInstance>();
        public int maxBagSlots = 12;
        public int maxPocketSlots = 6;
        public int maxStorageSlots = 60;
        #endregion

        #region Events
        public event Action OnInventoryChanged;
        public event Action<int> OnMaxSlotsChanged;
        #endregion

        #region Services
        private ItemQueryService _queryService;
        private ItemStackingService _stackingService;
        private ItemLocationService _locationService;
        #endregion

        #region Constructor
        public InventoryCore()
        {
            InitializeServices();
        }

        public InventoryCore(
            ItemQueryService queryService = null,
            ItemStackingService stackingService = null,
            ItemLocationService locationService = null)
        {
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
        
        public int GetStorageItemCount()
            => _queryService.GetStorageItemCount(allItems);

        public bool HasStorageSpace()
            => _queryService.HasStorageSpace(allItems, maxStorageSlots);

        public int GetEmptyStorageSlots()
            => _queryService.GetEmptyStorageSlots(allItems, maxStorageSlots);

        #endregion

        #region Add/Remove Methods

        /// <summary>
        /// ✅ MIGRATED: Add item with enum location
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
            ItemLocation targetLocation = ItemLocation.Storage;
            
            if (addToBag)
            {
                if (HasBagSpace())
                    targetLocation = ItemLocation.Bag;
                else
                {
                    Debug.LogWarning("[InventoryCore] Bag full, sending to storage");
                    if (!HasStorageSpace())
                    {
                        Debug.LogWarning($"[InventoryCore] Storage full! ({GetStorageItemCount()}/{maxStorageSlots})");
                        return false;
                    }
                }
            }
            else if (!HasStorageSpace())
            {
                Debug.LogWarning($"[InventoryCore] Storage full! ({GetStorageItemCount()}/{maxStorageSlots})");
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
                // Add non-stackable items one by one
                for (int i = 0; i < quantity; i++)
                {
                    // Re-check space mid-loop
                    if (targetLocation == ItemLocation.Bag && !HasBagSpace())
                    {
                        Debug.LogWarning("[InventoryCore] Bag full, remaining items sent to storage");
                        targetLocation = ItemLocation.Storage;
                    }

                    if (targetLocation == ItemLocation.Storage && !HasStorageSpace())
                    {
                        Debug.LogWarning($"[InventoryCore] Storage full! Added {i}/{quantity} items");
                        OnInventoryChanged?.Invoke();
                        return false;
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

            bool success = _locationService.MoveToBag(allItems, item, quantity, maxBagSlots, itemData);

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

            bool success = _locationService.MoveToPocket(allItems, item, quantity, maxPocketSlots, itemData);

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

        #region Slot Management

        public void UpgradeBagSlots(int additionalSlots)
        {
            maxBagSlots += additionalSlots;
            OnMaxSlotsChanged?.Invoke(maxBagSlots);
            Debug.Log($"[InventoryCore] Bag upgraded to {maxBagSlots} slots");
        }

        public void UpgradePocketSlots(int additionalSlots)
        {
            maxPocketSlots += additionalSlots;
            OnMaxSlotsChanged?.Invoke(maxPocketSlots);
            Debug.Log($"[InventoryCore] Pocket upgraded to {maxPocketSlots} slots");
        }

        public void UpgradeStorageSlots(int additionalSlots)
        {
            maxStorageSlots += additionalSlots;
            OnMaxSlotsChanged?.Invoke(maxStorageSlots);
            Debug.Log($"[InventoryCore] Storage upgraded to {maxStorageSlots} slots");
        }

        public void SetSlotCapacities(int bagSlots, int pocketSlots, int storageSlots)
        {
            maxBagSlots = bagSlots;
            maxPocketSlots = pocketSlots;
            maxStorageSlots = storageSlots;
            OnMaxSlotsChanged?.Invoke(maxStorageSlots);
            Debug.Log($"[InventoryCore] Slots set - Bag: {maxBagSlots}, Pocket: {maxPocketSlots}, Storage: {maxStorageSlots}");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// ✅ MIGRATED: Store all bag items (move to storage)
        /// </summary>
        public void StoreAllItems()
        {
            foreach (var item in GetBagItems().ToList())
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
    }
}