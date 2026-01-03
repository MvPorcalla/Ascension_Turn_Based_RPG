// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Services/ItemLocationService.cs
// Service for moving items between locations
// ✅ FIXED: Removed redundant space checks (caller validates)
// ══════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;

namespace Ascension.Inventory.Services
{
    /// <summary>
    /// Service responsible for moving items between locations.
    /// Assumes caller (InventoryCore) has already validated capacity.
    /// </summary>
    public class ItemLocationService
    {
        private readonly ItemStackingService _stackingService;
        private readonly ItemQueryService _queryService;

        public ItemLocationService(ItemStackingService stackingService, ItemQueryService queryService)
        {
            _stackingService = stackingService;
            _queryService = queryService;
        }

        /// <summary>
        /// Move item to bag.
        /// ✅ FIXED: No space check - InventoryCore already validated
        /// </summary>
        public bool MoveToBag(
            List<ItemInstance> allItems,
            ItemInstance item,
            int quantity,
            int maxBagSlots,
            ItemBaseSO itemData)
        {
            // Note: maxBagSlots parameter kept for future use, but not validated here
            return MoveToLocation(allItems, item, quantity, ItemLocation.Bag, itemData);
        }

        /// <summary>
        /// Move item to pocket.
        /// ✅ FIXED: No space check - InventoryCore already validated
        /// </summary>
        public bool MoveToPocket(
            List<ItemInstance> allItems,
            ItemInstance item,
            int quantity,
            int maxPocketSlots,
            ItemBaseSO itemData)
        {
            // Note: maxPocketSlots parameter kept for future use, but not validated here
            return MoveToLocation(allItems, item, quantity, ItemLocation.Pocket, itemData);
        }

        /// <summary>
        /// Move item to storage.
        /// Storage has no capacity limit, so no validation needed.
        /// </summary>
        public bool MoveToStorage(
            List<ItemInstance> allItems,
            ItemInstance item,
            int quantity,
            ItemBaseSO itemData)
        {
            return MoveToLocation(allItems, item, quantity, ItemLocation.Storage, itemData);
        }

        /// <summary>
        /// Core movement logic - handles both stackable and non-stackable items
        /// </summary>
        private bool MoveToLocation(
            List<ItemInstance> allItems,
            ItemInstance source,
            int quantity,
            ItemLocation targetLocation,
            ItemBaseSO itemData)
        {
            if (itemData.IsStackable)
            {
                return MoveStackableItem(allItems, source, quantity, targetLocation, itemData);
            }
            else
            {
                return MoveNonStackableItem(allItems, source, quantity, targetLocation);
            }
        }

        /// <summary>
        /// Move stackable item - tries to merge with existing stacks
        /// </summary>
        private bool MoveStackableItem(
            List<ItemInstance> allItems,
            ItemInstance source,
            int quantity,
            ItemLocation targetLocation,
            ItemBaseSO itemData)
        {
            // Find existing stack at destination
            var existingStack = _stackingService.FindStackWithSpace(
                allItems,
                source.itemID,
                targetLocation,
                itemData.MaxStackSize
            );

            int remaining = quantity;

            if (existingStack != null)
            {
                // Transfer to existing stack
                int transferred = _stackingService.TransferToStack(
                    source,
                    existingStack,
                    quantity,
                    itemData.MaxStackSize
                );

                remaining -= transferred;

                // Remove source if empty
                if (_stackingService.ShouldRemoveStack(source))
                {
                    allItems.Remove(source);
                }
            }

            // If still have items to move, create new stack or move entire source
            if (remaining > 0)
            {
                if (remaining < source.quantity)
                {
                    // Split off the quantity we want to move
                    var splitStack = _stackingService.SplitStack(source, remaining);
                    if (splitStack != null)
                    {
                        splitStack.location = targetLocation;
                        allItems.Add(splitStack);
                    }
                }
                else
                {
                    // Move entire source stack
                    source.location = targetLocation;
                }
            }

            return true;
        }

        /// <summary>
        /// Move non-stackable item - just changes location
        /// </summary>
        private bool MoveNonStackableItem(
            List<ItemInstance> allItems,
            ItemInstance source,
            int quantity,
            ItemLocation targetLocation)
        {
            if (quantity < source.quantity)
            {
                // Split off items to move
                var splitStack = _stackingService.SplitStack(source, quantity);
                if (splitStack != null)
                {
                    splitStack.location = targetLocation;
                    allItems.Add(splitStack);
                }
            }
            else
            {
                // Move entire item
                source.location = targetLocation;
            }

            return true;
        }
    }
}