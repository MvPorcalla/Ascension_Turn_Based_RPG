// ════════════════════════════════════════════
// Assets\Scripts\Modules\InventorySystem\UI\StorageRoomContext.cs
// Storage Room behavior for GearPopup
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Manager;
using Ascension.SharedUI.Popups;

namespace Ascension.Inventory.UI
{
    public class StorageRoomContext : IGearPopupContext
    {
        private ItemLocation _currentLocation;

        public StorageRoomContext(ItemLocation location)
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
                Debug.LogError("[StorageRoomContext] Missing references");
                return false;
            }

            var database = InventoryManager.Instance.Database;
            bool success = false;

            switch (_currentLocation)
            {
                case ItemLocation.Storage:
                    success = InventoryManager.Instance.Inventory.MoveToBag(itemInstance, 1, database);
                    if (success)
                        Debug.Log($"[StorageRoomContext] Moved {item.ItemName} to bag");
                    break;

                case ItemLocation.Bag:
                case ItemLocation.Pocket:
                    success = InventoryManager.Instance.Inventory.MoveToStorage(itemInstance, 1, database);
                    if (success)
                        Debug.Log($"[StorageRoomContext] Stored {item.ItemName}");
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