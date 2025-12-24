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
            bool success = false;

            switch (_currentLocation)
            {
                case ItemLocation.Storage:
                    success = InventoryManager.Instance.Inventory.MoveToBag(itemInstance, 1, database);
                    if (success)
                        Debug.Log($"[StoragePopupContext] Moved {item.ItemName} to bag");
                    break;

                case ItemLocation.Bag:
                case ItemLocation.Pocket:
                    success = InventoryManager.Instance.Inventory.MoveToStorage(itemInstance, 1, database);
                    if (success)
                        Debug.Log($"[StoragePopupContext] Stored {item.ItemName}");
                    break;
            }

            return success;
        }

        public bool CanPerformAction(ItemBaseSO item, ItemInstance itemInstance)
        {
            return InventoryManager.Instance != null;
        }
    }
}