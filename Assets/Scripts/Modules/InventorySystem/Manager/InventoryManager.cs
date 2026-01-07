// ════════════════════════════════════════════
// Assets\Scripts\Modules\InventorySystem\Manager\InventoryManager.cs
// Main manager for the Inventory Module
// ✅ REFACTORED: All pocket-related API removed
// ════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Inventory.Config;
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

            // Restore capacity settings (pocket slot count ignored)
            Capacity.SetCapacities(
                data.maxBagSlots,
                data.maxStorageSlots
            );

            // Restore inventory items
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
        [ContextMenu("Debug: Add Test Items (With Results)")]
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

                // ✅ FIXED: Use AddToStorage instead of AddItem
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

        [ContextMenu("Debug: Upgrade Storage (+10 slots)")]
        public void DebugUpgradeStorage()
        {
            UpgradeStorage(10);
        }
        #endregion
    }
}