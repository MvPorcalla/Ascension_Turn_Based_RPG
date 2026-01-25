// ════════════════════════════════════════════
// Assets\Scripts\Modules\InventorySystem\Manager\InventoryManager.cs
// ✅ NAMESPACE FIXED: Ascension.Inventory.Manager → Ascension.Inventory.Core
// ════════════════════════════════════════════

using System.Linq;
using UnityEngine;
using Ascension.Inventory.Core;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Config;
using Ascension.Inventory.Services;
using Ascension.Data.SO.Database;
using Ascension.Data.SO.Item;
using Ascension.Core;

namespace Ascension.Inventory.Manager
{
    /// <summary>
    /// Public API for inventory operations.
    /// Delegates to InventoryCore which triggers GameEvents.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("References")]
        [SerializeField] private GameDatabaseSO database;
        #endregion

        #region Properties
        public InventoryCore Inventory { get; private set; }
        public InventoryCapacity Capacity { get; private set; }
        public GameDatabaseSO Database => database;
        #endregion

        #region Initialization
        public void Init()
        {
            Capacity = new InventoryCapacity(
                bagSlots: InventoryConfig.DEFAULT_BAG_SLOTS,
                storageSlots: InventoryConfig.DEFAULT_STORAGE_SLOTS
            );

            Inventory = new InventoryCore(Capacity);

            if (database == null)
            {
                Debug.LogError("[InventoryManager] GameDatabaseSO not assigned!");
            }
            else
            {
                database.Initialize();
                Debug.Log($"[InventoryManager] Initialized with database: {database.name}");
            }
        }
        #endregion

        #region Public API - Core Operations
        public InventoryResult AddToBag(string itemID, int quantity = 1)
            => Inventory.AddToBag(itemID, quantity, database);

        public InventoryResult AddToStorage(string itemID, int quantity = 1)
            => Inventory.AddToStorage(itemID, quantity, database);

        public InventoryResult RemoveItem(ItemInstance item, int quantity = 1)
            => Inventory.RemoveItem(item, quantity);

        public bool HasItem(string itemID, int quantity = 1)
            => Inventory.HasItem(itemID, quantity);

        public int GetItemCount(string itemID)
            => Inventory.GetItemCount(itemID);
        #endregion

        #region Equipment Location API
        public InventoryResult MoveToEquipped(string itemID)
        {
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

            ItemBaseSO itemData = database.GetItem(itemID);
            if (itemData == null)
                return InventoryResult.ItemNotFound(itemID);

            if (!(itemData is WeaponSO || itemData is GearSO))
            {
                return InventoryResult.Fail(
                    $"{itemData.ItemName} cannot be equipped", 
                    InventoryErrorCode.InvalidOperation
                );
            }

            ItemLocation originalLocation = item.location;
            item.previousLocation = item.location;
            item.location = ItemLocation.Equipped;

            GameEvents.TriggerItemMoved(item, originalLocation, ItemLocation.Equipped);
            GameEvents.TriggerInventoryChanged();

            Debug.Log($"[InventoryManager] Moved {itemData.ItemName} to Equipped (from {item.previousLocation})");
            return InventoryResult.Ok(item, $"Equipped {itemData.ItemName}");
        }

        public InventoryResult MoveFromEquipped(string itemID)
        {
            ItemInstance item = Inventory.allItems
                .FirstOrDefault(i => i.itemID == itemID && i.location == ItemLocation.Equipped);

            if (item == null)
            {
                return InventoryResult.Fail(
                    $"Item {itemID} is not equipped", 
                    InventoryErrorCode.ItemNotFound
                );
            }

            ItemBaseSO itemData = database.GetItem(itemID);
            if (itemData == null)
                return InventoryResult.ItemNotFound(itemID);

            ItemLocation targetLocation = item.previousLocation ?? ItemLocation.Bag;

            if (targetLocation == ItemLocation.Bag && !Inventory.HasBagSpace())
                return InventoryResult.BagFull();
            
            if (targetLocation == ItemLocation.Storage && !Inventory.HasStorageSpace())
                return InventoryResult.StorageFull();

            item.location = targetLocation;
            item.previousLocation = null;

            GameEvents.TriggerItemMoved(item, ItemLocation.Equipped, targetLocation);
            GameEvents.TriggerInventoryChanged();

            Debug.Log($"[InventoryManager] Moved {itemData.ItemName} from Equipped to {targetLocation}");
            return InventoryResult.Ok(item, $"Unequipped {itemData.ItemName} to {targetLocation}");
        }

        public bool CanEquipItem(string itemID)
        {
            ItemInstance item = Inventory.allItems
                .FirstOrDefault(i => i.itemID == itemID && 
                    (i.location == ItemLocation.Bag || i.location == ItemLocation.Storage));

            if (item == null)
                return false;

            ItemBaseSO itemData = database.GetItem(itemID);
            return itemData is WeaponSO || itemData is GearSO;
        }

        public ItemLocation? GetItemLocation(string itemID)
        {
            ItemInstance item = Inventory.allItems.FirstOrDefault(i => i.itemID == itemID);
            return item?.location;
        }
        #endregion

        #region Capacity Management
        public void UpgradeBag(int additionalSlots)
        {
            Capacity.UpgradeBag(additionalSlots);
            Debug.Log($"[InventoryManager] Bag upgraded to {Capacity.MaxBagSlots} slots");
            GameEvents.TriggerInventoryChanged();
        }

        public void UpgradeStorage(int additionalSlots)
        {
            Capacity.UpgradeStorage(additionalSlots);
            Debug.Log($"[InventoryManager] Storage upgraded to {Capacity.MaxStorageSlots} slots");
            GameEvents.TriggerInventoryChanged();
        }
        #endregion

        #region Save/Load
        public void LoadInventory(InventoryCoreData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[InventoryManager] Cannot load null data");
                return;
            }

            Capacity.SetCapacities(data.maxBagSlots, data.maxStorageSlots);
            Inventory.allItems = data.items;

            Debug.Log($"[InventoryManager] Loaded {Inventory.allItems.Count} items");
            Debug.Log($"[InventoryManager] Capacities - Bag: {data.maxBagSlots}, Storage: {data.maxStorageSlots}");

            GameEvents.TriggerInventoryChanged();
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

        #region Debug Helpers
        [ContextMenu("Debug: Add Test Items")]
        public void DebugAddTestItems()
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
                    Debug.Log($"✅ {result.Message}");
                else
                    Debug.LogWarning($"❌ {result.Message} (Error: {result.ErrorCode})");
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