// ════════════════════════════════════════════
// Assets\Scripts\Modules\InventorySystem\UI\StoragePopupContext.cs
// Storage Room behavior for GearPopup
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Manager;
using Ascension.SharedUI.Popups;

namespace Ascension.Storage.UI
{
    public class StoragePopupContext : IGearPopupContext
    {
        private ItemLocation _currentLocation;

        public StoragePopupContext(ItemLocation location)
        {
            _currentLocation = location;
        }

        public string GetButtonText(ItemBaseSO item, ItemInstance itemInstance)
        {
            switch (_currentLocation)
            {
                case ItemLocation.Storage:
                    return "Add to Bag";
                case ItemLocation.Bag:
                    return "Store";
                case ItemLocation.Pocket:
                    return "Store";
                default:
                    return "Action";
            }
        }

        public bool OnButtonClicked(ItemBaseSO item, ItemInstance itemInstance)
        {
            if (item == null || itemInstance == null || InventoryManager.Instance == null)
            {
                Debug.LogError("[StoragePopupContext] Missing references");
                return false;
            }

            var database = InventoryManager.Instance.Database;
            InventoryResult result; // ✅ Changed to InventoryResult

            switch (_currentLocation)
            {
                case ItemLocation.Storage:
                    result = InventoryManager.Instance.Inventory.MoveToBag(itemInstance, 1, database);
                    if (result.Success) // ✅ Check .Success property
                    {
                        Debug.Log($"[StoragePopupContext] {result.Message}");
                    }
                    else
                    {
                        Debug.LogWarning($"[StoragePopupContext] {result.Message}");
                    }
                    return result.Success; // ✅ Return .Success
                    
                case ItemLocation.Bag:
                case ItemLocation.Pocket:
                    result = InventoryManager.Instance.Inventory.MoveToStorage(itemInstance, 1, database);
                    if (result.Success) // ✅ Check .Success property
                    {
                        Debug.Log($"[StoragePopupContext] {result.Message}");
                    }
                    else
                    {
                        Debug.LogWarning($"[StoragePopupContext] {result.Message}");
                    }
                    return result.Success; // ✅ Return .Success
                    
                default:
                    return false;
            }
        }

        public bool CanPerformAction(ItemBaseSO item, ItemInstance itemInstance)
        {
            return InventoryManager.Instance != null;
        }
    }
}