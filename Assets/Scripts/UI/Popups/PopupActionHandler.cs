// ════════════════════════════════════════════
// Assets/Scripts/UI/Popups/PopupActionHandler.cs
// ✅ FIXED: Updated to use GameBootstrap pattern and new PotionSystem namespace
// Centralized business logic handler for all popup actions
// ════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Core;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Manager;
using Ascension.Equipment.Manager;
using Ascension.Character.Manager;
using Ascension.PotionSystem.Manager;

namespace Ascension.UI.Popups
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
            // ✅ FIXED: Use GameBootstrap instead of Instance
            var equipMgr = GameBootstrap.Equipment;
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
            // ✅ FIXED: Use GameBootstrap instead of Instance
            var equipMgr = GameBootstrap.Equipment;
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
            // ✅ FIXED: Use GameBootstrap instead of Instance
            var inventoryMgr = GameBootstrap.Inventory;
            if (inventoryMgr == null)
            {
                NotifyFailure("Inventory system unavailable");
                return false;
            }

            var inventory = inventoryMgr.Inventory;
            var database = inventoryMgr.Database;

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
            // ✅ FIXED: Use GameBootstrap instead of Instance
            var potionMgr = GameBootstrap.Potion;
            var characterMgr = GameBootstrap.Character;
            var inventoryMgr = GameBootstrap.Inventory;

            if (potionMgr == null)
            {
                NotifyFailure("Potion system unavailable");
                return false;
            }

            if (characterMgr == null || !characterMgr.HasActivePlayer)
            {
                NotifyFailure("No active player");
                return false;
            }

            var playerStats = characterMgr.CurrentPlayer;
            var baseStats = characterMgr.BaseStats;

            if (playerStats == null || baseStats == null)
            {
                NotifyFailure("Player stats not initialized");
                return false;
            }

            int successfulUses = 0;
            for (int i = 0; i < quantity; i++)
            {
                // ✅ FIXED: Use new UsePotion signature (simplified)
                bool success = potionMgr.UsePotion(potionData);
                
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
                // Remove items from inventory
                if (inventoryMgr != null && inventoryMgr.Inventory != null)
                {
                    inventoryMgr.Inventory.RemoveItem(itemInstance, successfulUses);
                }

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