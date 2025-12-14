// ════════════════════════════════════════════
// Assets\Scripts\Modules\InventorySystem\Manager\InventoryManager.cs
// Manages player inventory and item database
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Database;
using Ascension.Inventory.Data;
using Ascension.Core;

namespace Ascension.Inventory.Manager
{
    public class InventoryManager : MonoBehaviour, IGameService
    {
        public static InventoryManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameDatabaseSO database;

        public BagInventory Inventory { get; private set; }
        public GameDatabaseSO Database => database;

        public event System.Action OnInventoryLoaded;

        private void Awake()
        {
            InitializeSingleton();
        }

        #region IGameService Implementation
        /// <summary>
        /// ✅ FIXED: Explicit initialization called by ServiceContainer
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[InventoryManager] Initializing...");
            
            InitializeInventory();
            InitializeDatabase();
            
            Debug.Log("[InventoryManager] Ready");
        }

        private void InitializeInventory()
        {
            Inventory = new BagInventory();
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

        #region Public Methods
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
                Debug.LogError("database not assigned!");
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
}