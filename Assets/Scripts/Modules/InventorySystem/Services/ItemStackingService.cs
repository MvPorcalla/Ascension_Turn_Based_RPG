// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Services/ItemStackingService.cs
// Service for item stacking logic (merge/split operations)
// ══════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;

namespace Ascension.Inventory.Services
{
    /// <summary>
    /// Service responsible for item stacking logic (merge/split operations)
    /// ✅ MIGRATED: Now uses ItemLocation enum instead of boolean flags
    /// </summary>
    public class ItemStackingService
    {
        /// <summary>
        /// ✅ MIGRATED: Find existing stack in specific location that has room
        /// </summary>
        public ItemInstance FindStackWithSpace(
            List<ItemInstance> allItems,
            string itemID,
            ItemLocation location,
            int maxStackSize)
        {
            return allItems.FirstOrDefault(i =>
                i.itemID == itemID &&
                i.location == location &&
                i.quantity < maxStackSize
            );
        }

        /// <summary>
        /// Calculate how much of a stack can fit into another stack
        /// </summary>
        public int CalculateStackSpace(ItemInstance stack, int maxStackSize)
        {
            return maxStackSize - stack.quantity;
        }

        /// <summary>
        /// Transfer items from one stack to another (returns amount transferred)
        /// </summary>
        public int TransferToStack(ItemInstance sourceStack, ItemInstance targetStack, int quantity, int maxStackSize)
        {
            int spaceInTarget = CalculateStackSpace(targetStack, maxStackSize);
            int amountToTransfer = Mathf.Min(spaceInTarget, quantity);

            targetStack.quantity += amountToTransfer;
            sourceStack.quantity -= amountToTransfer;

            return amountToTransfer;
        }

        /// <summary>
        /// ✅ MIGRATED: Create new stacks with specific location
        /// </summary>
        public List<ItemInstance> CreateNewStacks(
            string itemID,
            int totalQuantity,
            ItemLocation location,
            int maxStackSize)
        {
            List<ItemInstance> newStacks = new List<ItemInstance>();
            int remaining = totalQuantity;

            while (remaining > 0)
            {
                int stackSize = Mathf.Min(remaining, maxStackSize);
                newStacks.Add(new ItemInstance(itemID, stackSize, location));
                remaining -= stackSize;
            }

            return newStacks;
        }

        /// <summary>
        /// Split a stack into two (source loses quantity, returns new stack)
        /// </summary>
        public ItemInstance SplitStack(ItemInstance source, int quantityToSplit)
        {
            if (quantityToSplit >= source.quantity)
            {
                Debug.LogWarning("[ItemStackingService] Cannot split entire stack");
                return null;
            }

            if (quantityToSplit <= 0)
            {
                Debug.LogWarning("[ItemStackingService] Invalid split quantity");
                return null;
            }

            source.quantity -= quantityToSplit;
            return new ItemInstance(
                source.itemID,
                quantityToSplit,
                source.location  // ✅ Preserve location
            );
        }

        /// <summary>
        /// ✅ MIGRATED: Add quantity to existing stack, or create new stacks if needed
        /// </summary>
        public void AddToExistingOrCreateNew(
            List<ItemInstance> allItems,
            string itemID,
            int quantity,
            ItemLocation location,
            int maxStackSize)
        {
            int remaining = quantity;

            // Try to fill existing stacks first
            var existingStacks = allItems
                .Where(i => i.itemID == itemID &&
                           i.location == location &&
                           i.quantity < maxStackSize)
                .OrderBy(i => i.quantity) // Fill smallest stacks first
                .ToList();

            foreach (var stack in existingStacks)
            {
                if (remaining <= 0) break;

                int spaceInStack = CalculateStackSpace(stack, maxStackSize);
                int amountToAdd = Mathf.Min(spaceInStack, remaining);

                stack.quantity += amountToAdd;
                remaining -= amountToAdd;
            }

            // Create new stacks for remaining quantity
            if (remaining > 0)
            {
                var newStacks = CreateNewStacks(itemID, remaining, location, maxStackSize);
                allItems.AddRange(newStacks);
            }
        }

        /// <summary>
        /// Check if a stack should be removed (empty or invalid)
        /// </summary>
        public bool ShouldRemoveStack(ItemInstance stack)
        {
            return stack.quantity <= 0;
        }
    }
}