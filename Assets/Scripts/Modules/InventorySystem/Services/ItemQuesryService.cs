// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Services/ItemQueryService.cs
// 
// ══════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using System.Linq;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Database;
using Ascension.Inventory.Data;

namespace Ascension.Inventory.Services
{
    /// <summary>
    /// Service responsible for querying inventory items (read-only operations)
    /// Extracted from BagInventory.cs to follow Single Responsibility Principle
    /// </summary>
    public class ItemQueryService
    {
        /// <summary>
        /// Get all items in bag (excluding pocket and skills)
        /// </summary>
        public List<ItemInstance> GetBagItems(List<ItemInstance> allItems)
        {
            return allItems.Where(item =>
                item.isInBag &&
                !item.isInPocket &&
                !IsSkill(item.itemID)
            ).ToList();
        }

        /// <summary>
        /// Get all items in pocket
        /// </summary>
        public List<ItemInstance> GetPocketItems(List<ItemInstance> allItems)
        {
            return allItems.Where(item =>
                item.isInPocket &&
                !IsSkill(item.itemID)
            ).ToList();
        }

        /// <summary>
        /// Get all items in storage (not in bag or pocket)
        /// </summary>
        public List<ItemInstance> GetStorageItems(List<ItemInstance> allItems)
        {
            return allItems.Where(item =>
                !item.isInBag &&
                !item.isInPocket &&
                !IsSkill(item.itemID)
            ).ToList();
        }

        /// <summary>
        /// Get storage items filtered by type
        /// </summary>
        public List<ItemInstance> GetStorageItemsByType(
            List<ItemInstance> allItems,
            ItemType? filterType,
            GameDatabaseSO database)
        {
            var storageItems = GetStorageItems(allItems);

            if (filterType == null)
                return storageItems;

            return storageItems.Where(item =>
            {
                ItemBaseSO itemData = database.GetItem(item.itemID);
                return itemData != null && itemData.ItemType == filterType.Value;
            }).ToList();
        }

        /// <summary>
        /// Get total count of a specific item across all locations
        /// </summary>
        public int GetItemCount(List<ItemInstance> allItems, string itemID)
        {
            return allItems
                .Where(i => i.itemID == itemID)
                .Sum(i => i.quantity);
        }

        /// <summary>
        /// Check if inventory has specific item in required quantity
        /// </summary>
        public bool HasItem(List<ItemInstance> allItems, string itemID, int quantity = 1)
        {
            return GetItemCount(allItems, itemID) >= quantity;
        }

        /// <summary>
        /// Get number of items in bag
        /// </summary>
        public int GetBagItemCount(List<ItemInstance> allItems)
        {
            return GetBagItems(allItems).Count;
        }

        /// <summary>
        /// Get number of items in pocket
        /// </summary>
        public int GetPocketItemCount(List<ItemInstance> allItems)
        {
            return GetPocketItems(allItems).Count;
        }

        /// <summary>
        /// Check if bag has available space
        /// </summary>
        public bool HasBagSpace(List<ItemInstance> allItems, int maxBagSlots)
        {
            return GetBagItemCount(allItems) < maxBagSlots;
        }

        /// <summary>
        /// Check if pocket has available space
        /// </summary>
        public bool HasPocketSpace(List<ItemInstance> allItems, int maxPocketSlots)
        {
            return GetPocketItemCount(allItems) < maxPocketSlots;
        }

        /// <summary>
        /// Get number of empty bag slots
        /// </summary>
        public int GetEmptyBagSlots(List<ItemInstance> allItems, int maxBagSlots)
        {
            return maxBagSlots - GetBagItemCount(allItems);
        }

        /// <summary>
        /// Get number of empty pocket slots
        /// </summary>
        public int GetEmptyPocketSlots(List<ItemInstance> allItems, int maxPocketSlots)
        {
            return maxPocketSlots - GetPocketItemCount(allItems);
        }

        /// <summary>
        /// Check if item is a skill (managed separately from inventory)
        /// </summary>
        private bool IsSkill(string itemID)
        {
            return itemID.StartsWith("skill_") || itemID.StartsWith("ability_");
        }
    }
}