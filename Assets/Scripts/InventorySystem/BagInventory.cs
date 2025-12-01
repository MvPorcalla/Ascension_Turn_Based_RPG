// -------------------------------
// BagInventory.cs - Manages player's bag and storage
// -------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BagInventory
{
    public List<ItemInstance> allItems = new List<ItemInstance>();
    
    [Header("Bag Settings")]
    public int maxBagSlots = 12; // Can be increased by equipment

    [Header("Pocket Settings")]
    public int maxPocketSlots = 6; // Fixed size for quick access items

    // Events
    public event Action OnInventoryChanged;

    /// <summary>
    /// Get all items currently in the bag (12 slots) - Excludes skills
    /// </summary>
    public List<ItemInstance> GetBagItems()
    {
        return allItems.Where(item => 
            item.isInBag && 
            !item.isInPocket &&  // 🆕 ADD THIS
            !IsSkill(item.itemID)
        ).ToList();
    }

    /// <summary>
    /// Get all items in pocket (6 slots) - Excludes skills
    /// </summary>
    public List<ItemInstance> GetPocketItems()
    {
        return allItems.Where(item => 
            item.isInPocket && 
            !IsSkill(item.itemID)
        ).ToList();
    }

    /// <summary>
    /// Get all items in storage (not in bag) - Excludes skills
    /// </summary>
    public List<ItemInstance> GetStorageItems()
    {
        return allItems.Where(item => 
            !item.isInBag && 
            !item.isInPocket &&  // 🆕 ADD THIS
            !IsSkill(item.itemID)
        ).ToList();
    }

    /// <summary>
    /// Get storage items filtered by type - Excludes skills
    /// </summary>
    public List<ItemInstance> GetStorageItemsByType(ItemType? filterType, GameDatabaseSO database)
    {
        var storageItems = GetStorageItems();
        
        if (filterType == null)
            return storageItems;
        
        return storageItems.Where(item => 
        {
            ItemBaseSO itemData = database.GetItem(item.itemID);
            return itemData != null && itemData.itemType == filterType.Value;
        }).ToList();
    }

    /// <summary>
    /// Check if an item is a skill (skills are managed separately)
    /// </summary>
    private bool IsSkill(string itemID)
    {
        // Skills shouldn't appear in storage/bag
        return itemID.StartsWith("skill_");
    }

    /// <summary>
    /// Get number of items currently in bag
    /// </summary>
    public int GetBagItemCount()
    {
        return GetBagItems().Count;
    }

    /// <summary>
    /// Get number of items currently in pocket
    /// </summary>
    public int GetPocketItemCount()
    {
        return GetPocketItems().Count;
    }

    /// <summary>
    /// Check if bag has space
    /// </summary>
    public bool HasBagSpace()
    {
        return GetBagItemCount() < maxBagSlots;
    }

    /// <summary>
    /// Check if pocket has space
    /// </summary>
    public bool HasPocketSpace()
    {
        return GetPocketItemCount() < maxPocketSlots;
    }

    /// <summary>
    /// Get number of empty slots in bag
    /// </summary>
    public int GetEmptyBagSlots()
    {
        return maxBagSlots - GetBagItemCount();
    }

    /// <summary>
    /// Get number of empty slots in pocket
    /// </summary>
    public int GetEmptyPocketSlots()
    {
        return maxPocketSlots - GetPocketItemCount();
    }


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

        // Check if item is stackable
        if (itemData.isStackable)
        {
            // Try to stack with existing item
            ItemInstance existing = allItems.FirstOrDefault(i => i.itemID == itemID && i.isInBag == addToBag);
            
            if (existing != null)
            {
                // Stack up to max, create new slots if needed
                int remainder = quantity;
                
                while (remainder > 0)
                {
                    int spaceInStack = itemData.maxStackSize - existing.quantity;
                    int amountToAdd = Mathf.Min(spaceInStack, remainder);
                    
                    existing.quantity += amountToAdd;
                    remainder -= amountToAdd;
                    
                    // If still have remainder, create new stack
                    if (remainder > 0)
                    {
                        if (addToBag && !HasBagSpace())
                        {
                            Debug.LogWarning("[BagInventory] Bag full, remaining items sent to storage");
                            addToBag = false;
                        }
                        
                        existing = new ItemInstance(itemID, 0, addToBag);
                        allItems.Add(existing);
                    }
                }
            }
            else
            {
                // Create new stack(s)
                while (quantity > 0)
                {
                    if (addToBag && !HasBagSpace())
                    {
                        Debug.LogWarning("[BagInventory] Bag full, remaining items sent to storage");
                        addToBag = false;
                    }
                    
                    int stackSize = Mathf.Min(quantity, itemData.maxStackSize);
                    allItems.Add(new ItemInstance(itemID, stackSize, addToBag));
                    quantity -= stackSize;
                }
            }
        }
        else
        {
            // Non-stackable items (weapons, armor, skills)
            for (int i = 0; i < quantity; i++)
            {
                if (addToBag && !HasBagSpace())
                {
                    Debug.LogWarning("[BagInventory] Bag full, remaining items sent to storage");
                    addToBag = false;
                }
                
                allItems.Add(new ItemInstance(itemID, 1, addToBag));
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
            return false;

        item.quantity -= quantity;

        if (item.quantity <= 0)
        {
            allItems.Remove(item);
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Move item to bag (from storage or pocket)
    /// </summary>
    public bool MoveToBag(ItemInstance item, int quantity = 1)
    {
        if (item.isInBag)
        {
            Debug.LogWarning("[BagInventory] Item already in bag");
            return false;
        }

        if (!HasBagSpace())
        {
            Debug.LogWarning("[BagInventory] Bag is full");
            return false;
        }

        // Get item data to check if stackable
        ItemBaseSO itemData = InventoryManager.Instance.Database.GetItem(item.itemID);
        
        if (itemData.isStackable)
        {
            // Try to find existing stack in bag
            ItemInstance existingInBag = allItems.FirstOrDefault(i => 
                i.itemID == item.itemID && 
                i.isInBag && 
                !i.isInPocket &&
                i.quantity < itemData.maxStackSize
            );

            if (existingInBag != null)
            {
                // Stack with existing
                int spaceInStack = itemData.maxStackSize - existingInBag.quantity;
                int amountToMove = Mathf.Min(spaceInStack, quantity);
                
                existingInBag.quantity += amountToMove;
                item.quantity -= amountToMove;
                
                // Remove source if empty
                if (item.quantity <= 0)
                {
                    allItems.Remove(item);
                }
                
                // If still have remainder and bag has space, create new stack
                if (quantity > amountToMove && HasBagSpace())
                {
                    int remainder = quantity - amountToMove;
                    item.quantity -= remainder;
                    allItems.Add(new ItemInstance(item.itemID, remainder, true, false));
                    
                    if (item.quantity <= 0)
                    {
                        allItems.Remove(item);
                    }
                }
            }
            else
            {
                // No existing stack found, move/split normally
                if (quantity < item.quantity)
                {
                    item.quantity -= quantity;
                    allItems.Add(new ItemInstance(item.itemID, quantity, true, false));
                }
                else
                {
                    item.isInBag = true;
                    item.isInPocket = false;
                }
            }
        }
        else
        {
            // Non-stackable: just change location flags
            if (quantity < item.quantity)
            {
                item.quantity -= quantity;
                allItems.Add(new ItemInstance(item.itemID, quantity, true, false));
            }
            else
            {
                item.isInBag = true;
                item.isInPocket = false;
            }
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Move item to pocket (from storage or bag)
    /// </summary>
    public bool MoveToPocket(ItemInstance item, int quantity = 1)
    {
        if (item.isInPocket)
        {
            Debug.LogWarning("[BagInventory] Item already in pocket");
            return false;
        }

        if (!HasPocketSpace())
        {
            Debug.LogWarning("[BagInventory] Pocket is full");
            return false;
        }

        // Get item data to check if stackable
        ItemBaseSO itemData = InventoryManager.Instance.Database.GetItem(item.itemID);
        
        if (itemData.isStackable)
        {
            // Try to find existing stack in pocket
            ItemInstance existingInPocket = allItems.FirstOrDefault(i => 
                i.itemID == item.itemID && 
                i.isInPocket && 
                i.quantity < itemData.maxStackSize
            );

            if (existingInPocket != null)
            {
                // Stack with existing
                int spaceInStack = itemData.maxStackSize - existingInPocket.quantity;
                int amountToMove = Mathf.Min(spaceInStack, quantity);
                
                existingInPocket.quantity += amountToMove;
                item.quantity -= amountToMove;
                
                // Remove source if empty
                if (item.quantity <= 0)
                {
                    allItems.Remove(item);
                }
                
                // If still have remainder and pocket has space, create new stack
                if (quantity > amountToMove && HasPocketSpace())
                {
                    int remainder = quantity - amountToMove;
                    item.quantity -= remainder;
                    allItems.Add(new ItemInstance(item.itemID, remainder, false, true));
                    
                    if (item.quantity <= 0)
                    {
                        allItems.Remove(item);
                    }
                }
            }
            else
            {
                // No existing stack found, move/split normally
                if (quantity < item.quantity)
                {
                    item.quantity -= quantity;
                    allItems.Add(new ItemInstance(item.itemID, quantity, false, true));
                }
                else
                {
                    item.isInBag = false;
                    item.isInPocket = true;
                }
            }
        }
        else
        {
            // Non-stackable: just change location flags
            if (quantity < item.quantity)
            {
                item.quantity -= quantity;
                allItems.Add(new ItemInstance(item.itemID, quantity, false, true));
            }
            else
            {
                item.isInBag = false;
                item.isInPocket = true;
            }
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Move item to storage (from bag or pocket)
    /// </summary>
    public bool MoveToStorage(ItemInstance item, int quantity = 1)
    {
        if (!item.isInBag && !item.isInPocket)
        {
            Debug.LogWarning("[BagInventory] Item already in storage");
            return false;
        }

        // Get item data to check if stackable
        ItemBaseSO itemData = InventoryManager.Instance.Database.GetItem(item.itemID);
        
        if (itemData.isStackable)
        {
            // Try to find existing stack in storage
            ItemInstance existingInStorage = allItems.FirstOrDefault(i => 
                i.itemID == item.itemID && 
                !i.isInBag && 
                !i.isInPocket &&
                i.quantity < itemData.maxStackSize
            );

            if (existingInStorage != null)
            {
                // Stack with existing
                int spaceInStack = itemData.maxStackSize - existingInStorage.quantity;
                int amountToMove = Mathf.Min(spaceInStack, quantity);
                
                existingInStorage.quantity += amountToMove;
                item.quantity -= amountToMove;
                
                // Remove source if empty
                if (item.quantity <= 0)
                {
                    allItems.Remove(item);
                }
                
                // If still have remainder, create new stack in storage
                if (quantity > amountToMove)
                {
                    int remainder = quantity - amountToMove;
                    item.quantity -= remainder;
                    allItems.Add(new ItemInstance(item.itemID, remainder, false, false));
                    
                    if (item.quantity <= 0)
                    {
                        allItems.Remove(item);
                    }
                }
            }
            else
            {
                // No existing stack found, move/split normally
                if (quantity < item.quantity)
                {
                    item.quantity -= quantity;
                    allItems.Add(new ItemInstance(item.itemID, quantity, false, false));
                }
                else
                {
                    item.isInBag = false;
                    item.isInPocket = false;
                }
            }
        }
        else
        {
            // Non-stackable: just change location flags
            if (quantity < item.quantity)
            {
                item.quantity -= quantity;
                allItems.Add(new ItemInstance(item.itemID, quantity, false, false));
            }
            else
            {
                item.isInBag = false;
                item.isInPocket = false;
            }
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Store all items from bag to storage
    /// </summary>
    public void StoreAllItems()
    {
        foreach (var item in GetBagItems().ToList())
        {
            if (!item.isEquipped)
            {
                item.isInBag = false;
            }
        }

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Get total count of a specific item (bag + storage)
    /// </summary>
    public int GetItemCount(string itemID)
    {
        return allItems.Where(i => i.itemID == itemID).Sum(i => i.quantity);
    }

    /// <summary>
    /// Check if player has item
    /// </summary>
    public bool HasItem(string itemID, int quantity = 1)
    {
        return GetItemCount(itemID) >= quantity;
    }

    /// <summary>
    /// Clear all items (for testing)
    /// </summary>
    public void ClearAll()
    {
        allItems.Clear();
        OnInventoryChanged?.Invoke();
    }
}