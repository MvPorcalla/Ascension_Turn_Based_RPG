// ──────────────────────────────────────────────────
// InventoryManager.cs
// Manages the player's inventory and storage system
// ──────────────────────────────────────────────────

using UnityEngine;
using Ascension.GameSystem;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Database;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameDatabaseSO database;

    public BagInventory Inventory { get; private set; }
    public GameDatabaseSO Database => database;

    // Event for UI to subscribe to
    public event System.Action OnInventoryLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Inventory = new BagInventory();

        if (database == null)
            Debug.LogError("[InventoryManager] Database not assigned!");
        else
            database.Initialize();
    }


    /// <summary>
    /// Add item to player inventory
    /// </summary>
    public bool AddItem(string itemID, int quantity = 1, bool addToBag = false)
    {
        return Inventory.AddItem(itemID, quantity, addToBag, database);
    }

    /// <summary>
    /// Load inventory from save data
    /// </summary>
    public void LoadInventory(BagInventoryData data)
    {
        Inventory.allItems = data.items;
        Inventory.maxBagSlots = data.maxBagSlots;
        
        Debug.Log($"[InventoryManager] Loaded {Inventory.allItems.Count} items");
        
        // Manually trigger UI refresh after loading
        OnInventoryLoaded?.Invoke();
    }

    /// <summary>
    /// Save inventory to data
    /// </summary>
    public BagInventoryData SaveInventory()
    {
        return new BagInventoryData
        {
            items = Inventory.allItems,
            maxBagSlots = Inventory.maxBagSlots
        };
    }

    #region Debug Helpers

    [ContextMenu("Debug: Add Test Items")]
    public void DebugAddTestItems()
    {
        if (database == null)
        {
            Debug.LogError("database not assigned!");
            return;
        }

        // Add 10 of each stackable item to storage (excluding Abilities)
        foreach (var item in database.GetAllItems())
        {
            // Skip Abilities - they have their own system
            if (item.ItemType == ItemType.Ability)
                continue;

            if (item.IsStackable)
            {
                AddItem(item.ItemID, 10, false);
                Debug.Log($"Added 10x {item.ItemName} to storage");
            }
            else
            {
                AddItem(item.ItemID, 1, false);
                Debug.Log($"Added 1x {item.ItemName} to storage");
            }
        }

        // Add a few items to bag for testing
        var weapons = database.GetAllWeapons();
        if (weapons.Count > 0)
        {
            AddItem(weapons[0].ItemID, 1, true);
            Debug.Log($"Added {weapons[0].ItemName} to bag");
        }

        Debug.Log($"[InventoryManager] Test items added! Total: {Inventory.allItems.Count}");
        Debug.Log($"[InventoryManager] Abilities excluded from storage (managed separately)");
    }

    [ContextMenu("Debug: Clear All Items")]
    public void DebugClearAll()
    {
        Inventory.ClearAll();
        Debug.Log("[InventoryManager] All items cleared!");
    }

    [ContextMenu("Debug: Print Inventory")]
    public void DebugPrintInventory()
    {
        Debug.Log($"=== BAG ITEMS ({Inventory.GetBagItemCount()}/{Inventory.maxBagSlots}) ===");
        foreach (var item in Inventory.GetBagItems())
        {
            Debug.Log($"{item.itemID} x{item.quantity} {(item.isEquipped ? "[EQUIPPED]" : "")}");
        }

        Debug.Log($"\n=== STORAGE ITEMS ({Inventory.GetStorageItems().Count}) ===");
        foreach (var item in Inventory.GetStorageItems())
        {
            Debug.Log($"{item.itemID} x{item.quantity}");
        }

        Debug.Log($"\nTotal Items: {Inventory.allItems.Count}");
        Debug.Log($"Empty Bag Slots: {Inventory.GetEmptyBagSlots()}");
    }

    #endregion
}