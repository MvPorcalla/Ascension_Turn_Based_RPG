// ════════════════════════════════════════════
// Assets/Scripts/Modules/SharedUI/Popups/PopupActionHandler.cs
// Centralized business logic handler for all popup actions
// ════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Manager;
using Ascension.Equipment.Manager;
using Ascension.Character.Manager;
using Ascension.GameSystem;

namespace Ascension.SharedUI.Popups
{
    /// <summary>
    /// Handles ALL business logic for popup actions.
    /// UI controllers subscribe to events and provide context,
    /// but don't execute business logic directly.
    /// </summary>
    public class PopupActionHandler : MonoBehaviour
    {
        #region Singleton
        public static PopupActionHandler Instance { get; private set; }
        #endregion

        #region Events
        public event Action<string> OnActionCompleted;
        public event Action<string> OnActionFailed;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        #endregion

        #region Equipment Actions

        public bool EquipItem(ItemBaseSO itemData, ItemInstance itemInstance)
        {
            var equipMgr = EquipmentManager.Instance;
            if (equipMgr == null)
            {
                NotifyFailure("Equipment system unavailable");
                return false;
            }

            bool success = equipMgr.EquipItem(itemData.ItemID);

            if (success)
            {
                NotifySuccess($"Equipped {itemData.ItemName}");
                PopupManager.Instance?.CloseCurrentPopup();
            }
            else
            {
                NotifyFailure($"Cannot equip {itemData.ItemName}");
            }

            return success;
        }

        public bool UnequipItem(ItemBaseSO itemData, ItemInstance itemInstance)
        {
            var equipMgr = EquipmentManager.Instance;
            if (equipMgr == null)
            {
                NotifyFailure("Equipment system unavailable");
                return false;
            }

            bool success = equipMgr.UnequipItem(itemData.ItemID);

            if (success)
            {
                NotifySuccess($"Unequipped {itemData.ItemName}");
                PopupManager.Instance?.CloseCurrentPopup();
            }
            else
            {
                NotifyFailure("Inventory full - cannot unequip");
            }

            return success;
        }

        #endregion

        #region Movement Actions

        public bool MoveItem(ItemInstance itemInstance, int quantity, ItemLocation targetLocation)
        {
            var inventory = InventoryManager.Instance?.Inventory;
            var database = InventoryManager.Instance?.Database;

            if (inventory == null || database == null)
            {
                NotifyFailure("Inventory system unavailable");
                return false;
            }

            InventoryResult result;

            if (targetLocation == ItemLocation.Storage)
            {
                result = inventory.MoveToStorage(itemInstance, quantity, database);
            }
            else if (targetLocation == ItemLocation.Bag)
            {
                result = inventory.MoveToBag(itemInstance, quantity, database);
            }
            else
            {
                NotifyFailure($"Invalid target location: {targetLocation}");
                return false;
            }

            if (result.Success)
            {
                NotifySuccess(result.Message);
                PopupManager.Instance?.CloseCurrentPopup();
            }
            else
            {
                NotifyFailure(result.Message);
            }

            return result.Success;
        }

        #endregion

        #region Potion Actions

        public bool UsePotion(PotionSO potionData, ItemInstance itemInstance, int quantity)
        {
            if (PotionManager.Instance == null)
            {
                NotifyFailure("Potion system unavailable");
                return false;
            }

            if (!CharacterManager.Instance.HasActivePlayer)
            {
                NotifyFailure("No active player");
                return false;
            }

            var playerStats = CharacterManager.Instance.CurrentPlayer;
            var baseStats = CharacterManager.Instance.BaseStats;

            if (playerStats == null || baseStats == null)
            {
                NotifyFailure("Player stats not initialized");
                return false;
            }

            int successfulUses = 0;
            for (int i = 0; i < quantity; i++)
            {
                bool success = PotionManager.Instance.UsePotion(potionData, playerStats, baseStats);
                
                if (success)
                {
                    successfulUses++;
                }
                else
                {
                    if (successfulUses > 0)
                    {
                        NotifyFailure($"Could only use {successfulUses}/{quantity} potions");
                    }
                    break;
                }
            }

            if (successfulUses > 0)
            {
                InventoryManager.Instance.Inventory.RemoveItem(itemInstance, successfulUses);
                NotifySuccess($"Used {successfulUses}x {potionData.ItemName}");
                
                if (successfulUses == quantity || itemInstance.quantity <= 0)
                {
                    PopupManager.Instance?.CloseCurrentPopup();
                }
                
                return true;
            }
            else
            {
                NotifyFailure($"Failed to use {potionData.ItemName}");
                return false;
            }
        }

        #endregion

        #region Shop Actions (Future)

        public bool BuyItem(ItemBaseSO itemData, int quantity, int totalCost)
        {
            // TODO: Implement when shop system is ready
            NotifyFailure("Shop system not yet implemented");
            return false;
        }

        public bool SellItem(ItemInstance itemInstance, int quantity, int sellValue)
        {
            // TODO: Implement when shop system is ready
            NotifyFailure("Shop system not yet implemented");
            return false;
        }

        #endregion

        #region Notifications

        private void NotifySuccess(string message)
        {
            Debug.Log($"[PopupActionHandler] SUCCESS: {message}");
            OnActionCompleted?.Invoke(message);
        }

        private void NotifyFailure(string message)
        {
            Debug.LogWarning($"[PopupActionHandler] FAILURE: {message}");
            OnActionFailed?.Invoke(message);
        }

        #endregion
    }
}