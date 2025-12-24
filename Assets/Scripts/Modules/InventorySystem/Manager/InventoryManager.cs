// ════════════════════════════════════════════
// Assets\Scripts\Modules\InventorySystem\Manager\InventoryManager.cs
// ✅ REFACTORED: Clean separation of concerns
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Database;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Core;

namespace Ascension.Inventory.Manager
{
    /// <summary>
    /// Main manager for the Inventory Module.
    /// Provides a clean API for both Storage System and Equipment System.
    /// </summary>
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
        /// <summary>
        /// ✅ REFACTORED: Clean initialization
        /// </summary>
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
                bagSlots: 12,
                pocketSlots: 6,
                storageSlots: 60
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
        /// Add item to inventory
        /// </summary>
        public bool AddItem(string itemID, int quantity = 1, bool addToBag = false)
        {
            return Inventory.AddItem(itemID, quantity, addToBag, database);
        }

        /// <summary>
        /// Remove item from inventory
        /// </summary>
        public bool RemoveItem(ItemInstance item, int quantity = 1)
        {
            return Inventory.RemoveItem(item, quantity);
        }

        /// <summary>
        /// Check if inventory has item
        /// </summary>
        public bool HasItem(string itemID, int quantity = 1)
        {
            return Inventory.HasItem(itemID, quantity);
        }

        /// <summary>
        /// Get total count of specific item
        /// </summary>
        public int GetItemCount(string itemID)
        {
            return Inventory.GetItemCount(itemID);
        }
        #endregion

        #region Public API - Capacity Management
        /// <summary>
        /// Upgrade bag capacity
        /// </summary>
        public void UpgradeBag(int additionalSlots)
        {
            Capacity.UpgradeBag(additionalSlots);
        }

        /// <summary>
        /// Upgrade pocket capacity
        /// </summary>
        public void UpgradePocket(int additionalSlots)
        {
            Capacity.UpgradePocket(additionalSlots);
        }

        /// <summary>
        /// Upgrade storage capacity
        /// </summary>
        public void UpgradeStorage(int additionalSlots)
        {
            Capacity.UpgradeStorage(additionalSlots);
        }
        #endregion

        #region Public API - Save/Load
        /// <summary>
        /// ✅ REFACTORED: Load with capacity restoration
        /// </summary>
        public void LoadInventory(InventoryCoreData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[InventoryManager] Cannot load null data");
                return;
            }

            // Restore capacity settings
            Capacity.SetCapacities(
                data.maxBagSlots,
                data.maxPocketSlots,
                data.maxStorageSlots
            );

            // Restore inventory items
            Inventory.allItems = data.items;

            Debug.Log($"[InventoryManager] Loaded {Inventory.allItems.Count} items");
            Debug.Log($"[InventoryManager] Capacities - Bag: {data.maxBagSlots}, Pocket: {data.maxPocketSlots}, Storage: {data.maxStorageSlots}");

            OnInventoryLoaded?.Invoke();
        }

        /// <summary>
        /// ✅ REFACTORED: Save with capacity data
        /// </summary>
        public InventoryCoreData SaveInventory()
        {
            return new InventoryCoreData
            {
                items = new System.Collections.Generic.List<ItemInstance>(Inventory.allItems),
                maxBagSlots = Capacity.MaxBagSlots,
                maxPocketSlots = Capacity.MaxPocketSlots,
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

                AddItem(item.ItemID, item.IsStackable ? 10 : 1, false);
                Debug.Log($"Added {(item.IsStackable ? 10 : 1)}x {item.ItemName} to storage");
            }

            var weapons = database.GetAllWeapons();
            if (weapons.Count > 0)
            {
                AddItem(weapons[0].ItemID, 1, true);
                Debug.Log($"Added {weapons[0].ItemName} to bag");
            }

            Debug.Log($"[InventoryManager] Test items added! Total: {Inventory.allItems.Count}");
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
            Debug.Log($"Pocket: {Inventory.GetPocketItemCount()}/{Capacity.MaxPocketSlots}");
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