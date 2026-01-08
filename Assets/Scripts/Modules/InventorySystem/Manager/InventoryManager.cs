// ════════════════════════════════════════════
// Assets\Scripts\Modules\InventorySystem\Manager\InventoryManager.cs
// ✅ REFACTORED: Added Equipment Location API
// ════════════════════════════════════════════

using System;
using System.Linq;
using UnityEngine;
using Ascension.Inventory.Config;
using Ascension.Inventory.Enums;
using Ascension.Data.SO.Database;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Core;

namespace Ascension.Inventory.Manager
{
    public class InventoryManager : MonoBehaviour, IGameService
    {
        #region Singleton
        public static InventoryManager Instance { get; private set; }
        #endregion

        #region Serialized Fields
        [Header("References")]
        [SerializeField] private GameDatabaseSO database;
        #endregion

        #region Properties
        public InventoryCore Inventory { get; private set; }
        public SlotCapacityManager Capacity { get; private set; }
        public GameDatabaseSO Database => database;
        #endregion

        #region Events
        public event System.Action OnInventoryLoaded;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            InitializeSingleton();
        }
        #endregion

        #region IGameService Implementation
        public void Initialize()
        {
            Debug.Log("[InventoryManager] Initializing...");
            
            InitializeCapacityManager();
            InitializeInventory();
            InitializeDatabase();
            
            Debug.Log("[InventoryManager] Ready");
        }

        private void InitializeCapacityManager()
        {
            Capacity = new SlotCapacityManager(
                bagSlots: InventoryConfig.DEFAULT_BAG_SLOTS,
                storageSlots: InventoryConfig.DEFAULT_STORAGE_SLOTS
            );

            Debug.Log("[InventoryManager] Capacity manager created");
        }

        private void InitializeInventory()
        {
            Inventory = new InventoryCore(Capacity);
            Debug.Log("[InventoryManager] Inventory system created");
        }

        private void InitializeDatabase()
        {
            if (database == null)
            {
                Debug.LogError("[InventoryManager] GameDatabaseSO not assigned!");
            }
            else
            {
                database.Initialize();
                Debug.Log($"[InventoryManager] Database initialized: {database.name}");
            }
        }
        #endregion

        #region Public API - Core Operations

        /// <summary>
        /// Add item to player's bag (world loot, shop purchases)
        /// Returns BagFull error if no space - caller must handle
        /// </summary>
        public InventoryResult AddToBag(string itemID, int quantity = 1)
        {
            return Inventory.AddToBag(itemID, quantity, database);
        }

        /// <summary>
        /// Add item to storage (explicit transfers, mail, crafting)
        /// Returns StorageFull error if no space - caller must handle
        /// </summary>
        public InventoryResult AddToStorage(string itemID, int quantity = 1)
        {
            return Inventory.AddToStorage(itemID, quantity, database);
        }

        public InventoryResult RemoveItem(ItemInstance item, int quantity = 1)
        {
            return Inventory.RemoveItem(item, quantity);
        }

        public bool HasItem(string itemID, int quantity = 1)
        {
            return Inventory.HasItem(itemID, quantity);
        }

        public int GetItemCount(string itemID)
        {
            return Inventory.GetItemCount(itemID);
        }
        #endregion

        #region ✅ NEW: Equipment Location API

        /// <summary>
        /// ✅ Move item from Bag/Storage to Equipped location
        /// Tracks previousLocation for proper unequip behavior
        /// </summary>
        public InventoryResult MoveToEquipped(string itemID)
        {
            // Find item in Bag or Storage (not already equipped)
            ItemInstance item = Inventory.allItems
                .FirstOrDefault(i => i.itemID == itemID && 
                    (i.location == ItemLocation.Bag || i.location == ItemLocation.Storage));

            if (item == null)
            {
                return InventoryResult.Fail(
                    $"Item {itemID} not found in inventory (or already equipped)", 
                    InventoryErrorCode.ItemNotFound
                );
            }

            // Validate item data
            ItemBaseSO itemData = database.GetItem(itemID);
            if (itemData == null)
            {
                return InventoryResult.ItemNotFound(itemID);
            }

            // Only gear/weapons can be equipped
            if (!(itemData is WeaponSO || itemData is GearSO))
            {
                return InventoryResult.Fail(
                    $"{itemData.ItemName} cannot be equipped", 
                    InventoryErrorCode.InvalidOperation
                );
            }

            // Track where item came from
            item.previousLocation = item.location;
            
            // Move to equipped
            item.location = ItemLocation.Equipped;

            // Trigger UI refresh
            Inventory.ForceRefresh();

            Debug.Log($"[InventoryManager] Moved {itemData.ItemName} to Equipped (from {item.previousLocation})");
            return InventoryResult.Ok(item, $"Equipped {itemData.ItemName}");
        }

        /// <summary>
        /// ✅ Move item from Equipped back to original location (Bag/Storage)
        /// Uses previousLocation if available, otherwise defaults to Bag
        /// </summary>
        public InventoryResult MoveFromEquipped(string itemID)
        {
            // Find equipped item
            ItemInstance item = Inventory.allItems
                .FirstOrDefault(i => i.itemID == itemID && i.location == ItemLocation.Equipped);

            if (item == null)
            {
                return InventoryResult.Fail(
                    $"Item {itemID} is not equipped", 
                    InventoryErrorCode.ItemNotFound
                );
            }

            // Validate item data
            ItemBaseSO itemData = database.GetItem(itemID);
            if (itemData == null)
            {
                return InventoryResult.ItemNotFound(itemID);
            }

            // Determine target location (restore original or default to Bag)
            ItemLocation targetLocation = item.previousLocation ?? ItemLocation.Bag;

            // Check capacity at target location
            if (targetLocation == ItemLocation.Bag)
            {
                if (!Inventory.HasBagSpace())
                {
                    return InventoryResult.BagFull();
                }
            }
            else if (targetLocation == ItemLocation.Storage)
            {
                if (!Inventory.HasStorageSpace())
                {
                    return InventoryResult.StorageFull();
                }
            }

            // Move back to original location
            item.location = targetLocation;
            item.previousLocation = null; // Clear tracking

            // Trigger UI refresh
            Inventory.ForceRefresh();

            Debug.Log($"[InventoryManager] Moved {itemData.ItemName} from Equipped to {targetLocation}");
            return InventoryResult.Ok(item, $"Unequipped {itemData.ItemName} to {targetLocation}");
        }

        /// <summary>
        /// ✅ Check if an item can be equipped (exists in Bag/Storage, not already equipped)
        /// </summary>
        public bool CanEquipItem(string itemID)
        {
            // Must exist in Bag or Storage (not already equipped)
            ItemInstance item = Inventory.allItems
                .FirstOrDefault(i => i.itemID == itemID && 
                    (i.location == ItemLocation.Bag || i.location == ItemLocation.Storage));

            if (item == null)
                return false;

            // Must be equippable gear
            ItemBaseSO itemData = database.GetItem(itemID);
            return itemData is WeaponSO || itemData is GearSO;
        }

        /// <summary>
        /// ✅ Get item's current location (useful for debugging)
        /// </summary>
        public ItemLocation? GetItemLocation(string itemID)
        {
            ItemInstance item = Inventory.allItems.FirstOrDefault(i => i.itemID == itemID);
            return item?.location;
        }

        #endregion

        #region Public API - Capacity Management
        public void UpgradeBag(int additionalSlots)
        {
            Capacity.UpgradeBag(additionalSlots);
        }

        public void UpgradeStorage(int additionalSlots)
        {
            Capacity.UpgradeStorage(additionalSlots);
        }
        #endregion

        #region Public API - Save/Load
        public void LoadInventory(InventoryCoreData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[InventoryManager] Cannot load null data");
                return;
            }

            Capacity.SetCapacities(
                data.maxBagSlots,
                data.maxStorageSlots
            );

            Inventory.allItems = data.items;

            Debug.Log($"[InventoryManager] Loaded {Inventory.allItems.Count} items");
            Debug.Log($"[InventoryManager] Capacities - Bag: {data.maxBagSlots}, Storage: {data.maxStorageSlots}");

            OnInventoryLoaded?.Invoke();
        }

        public InventoryCoreData SaveInventory()
        {
            return new InventoryCoreData
            {
                items = new System.Collections.Generic.List<ItemInstance>(Inventory.allItems),
                maxBagSlots = Capacity.MaxBagSlots,
                maxStorageSlots = Capacity.MaxStorageSlots
            };
        }
        #endregion

        #region Private Methods
        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        #endregion

        #region Debug Helpers
        [ContextMenu("Debug: Add Test Items")]
        public void DebugAddTestItemsWithResults()
        {
            if (database == null)
            {
                Debug.LogError("Database not assigned!");
                return;
            }

            foreach (var item in database.GetAllItems())
            {
                if (item.ItemType == ItemType.Ability)
                    continue;

                var result = AddToStorage(item.ItemID, item.IsStackable ? 10 : 1);
                
                if (result.Success)
                {
                    Debug.Log($"✅ {result.Message}");
                }
                else
                {
                    Debug.LogWarning($"❌ {result.Message} (Error: {result.ErrorCode})");
                }
            }

            Debug.Log($"[InventoryManager] Test completed! Total items: {Inventory.allItems.Count}");
        }

        [ContextMenu("Debug: Clear All Items")]
        public void DebugClearAll()
        {
            Inventory.ClearAll();
            Debug.Log("[InventoryManager] All items cleared!");
        }

        [ContextMenu("Debug: Print Inventory Summary")]
        public void DebugPrintInventory()
        {
            Debug.Log($"=== INVENTORY SUMMARY ===");
            Debug.Log($"Bag: {Inventory.GetBagItemCount()}/{Capacity.MaxBagSlots}");
            Debug.Log($"Storage: {Inventory.GetStorageItemCount()}/{Capacity.MaxStorageSlots}");
            Debug.Log($"Total Items: {Inventory.allItems.Count}");
        }
        #endregion
    }
}