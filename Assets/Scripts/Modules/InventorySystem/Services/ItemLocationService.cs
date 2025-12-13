// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Services/ItemLocationService.cs
// STEP 3: Extract all location movement logic
// ══════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;

namespace Ascension.Inventory.Services
{
    /// <summary>
    /// Service responsible for moving items between locations (Bag/Pocket/Storage)
    /// Extracted from BagInventory.cs to follow Single Responsibility Principle
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
        /// Move item to bag (from storage or pocket)
        /// </summary>
        public bool MoveToBag(
            List<ItemInstance> allItems,
            ItemInstance item,
            int quantity,
            int maxBagSlots,
            ItemBaseSO itemData)
        {
            if (item.isInBag && !item.isInPocket)
            {
                Debug.LogWarning("[ItemLocationService] Item already in bag");
                return false;
            }

            if (!_queryService.HasBagSpace(allItems, maxBagSlots))
            {
                Debug.LogWarning("[ItemLocationService] Bag is full");
                return false;
            }

            return MoveToLocation(allItems, item, quantity, true, false, itemData);
        }

        /// <summary>
        /// Move item to pocket (from storage or bag)
        /// </summary>
        public bool MoveToPocket(
            List<ItemInstance> allItems,
            ItemInstance item,
            int quantity,
            int maxPocketSlots,
            ItemBaseSO itemData)
        {
            if (item.isInPocket)
            {
                Debug.LogWarning("[ItemLocationService] Item already in pocket");
                return false;
            }

            if (!_queryService.HasPocketSpace(allItems, maxPocketSlots))
            {
                Debug.LogWarning("[ItemLocationService] Pocket is full");
                return false;
            }

            return MoveToLocation(allItems, item, quantity, false, true, itemData);
        }

        /// <summary>
        /// Move item to storage (from bag or pocket)
        /// </summary>
        public bool MoveToStorage(
            List<ItemInstance> allItems,
            ItemInstance item,
            int quantity,
            ItemBaseSO itemData)
        {
            if (!item.isInBag && !item.isInPocket)
            {
                Debug.LogWarning("[ItemLocationService] Item already in storage");
                return false;
            }

            return MoveToLocation(allItems, item, quantity, false, false, itemData);
        }

        /// <summary>
        /// Core movement logic - handles both stackable and non-stackable items
        /// </summary>
        private bool MoveToLocation(
            List<ItemInstance> allItems,
            ItemInstance source,
            int quantity,
            bool toBag,
            bool toPocket,
            ItemBaseSO itemData)
        {
            if (itemData.IsStackable)
            {
                return MoveStackableItem(allItems, source, quantity, toBag, toPocket, itemData);
            }
            else
            {
                return MoveNonStackableItem(allItems, source, quantity, toBag, toPocket);
            }
        }

        /// <summary>
        /// Move stackable item - tries to merge with existing stacks
        /// </summary>
        private bool MoveStackableItem(
            List<ItemInstance> allItems,
            ItemInstance source,
            int quantity,
            bool toBag,
            bool toPocket,
            ItemBaseSO itemData)
        {
            // Find existing stack at destination
            var existingStack = _stackingService.FindStackWithSpace(
                allItems,
                source.itemID,
                toBag,
                toPocket,
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
                        splitStack.isInBag = toBag;
                        splitStack.isInPocket = toPocket;
                        allItems.Add(splitStack);
                    }
                }
                else
                {
                    // Move entire source stack
                    source.isInBag = toBag;
                    source.isInPocket = toPocket;
                }
            }

            return true;
        }

        /// <summary>
        /// Move non-stackable item - just changes location flags
        /// </summary>
        private bool MoveNonStackableItem(
            List<ItemInstance> allItems,
            ItemInstance source,
            int quantity,
            bool toBag,
            bool toPocket)
        {
            if (quantity < source.quantity)
            {
                // Split off items to move
                var splitStack = _stackingService.SplitStack(source, quantity);
                if (splitStack != null)
                {
                    splitStack.isInBag = toBag;
                    splitStack.isInPocket = toPocket;
                    allItems.Add(splitStack);
                }
            }
            else
            {
                // Move entire item
                source.isInBag = toBag;
                source.isInPocket = toPocket;
            }

            return true;
        }
    }
}