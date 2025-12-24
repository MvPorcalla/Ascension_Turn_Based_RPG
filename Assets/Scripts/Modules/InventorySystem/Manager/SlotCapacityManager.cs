// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Manager/SlotCapacityManager.cs
// Manages inventory slot capacities (bag, pocket, storage)
// ══════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Inventory.Enums;

namespace Ascension.Inventory.Manager
{
    /// <summary>
    /// Service responsible for managing slot capacities across all inventory locations.
    /// Separates capacity management from core inventory logic.
    /// </summary>
    public class SlotCapacityManager
    {
        #region Fields
        private int _maxBagSlots;
        private int _maxPocketSlots;
        private int _maxStorageSlots;
        #endregion

        #region Properties
        public int MaxBagSlots => _maxBagSlots;
        public int MaxPocketSlots => _maxPocketSlots;
        public int MaxStorageSlots => _maxStorageSlots;
        #endregion

        #region Events
        public event Action<ItemLocation, int> OnCapacityChanged;
        #endregion

        #region Constructor
        public SlotCapacityManager(int bagSlots = 12, int pocketSlots = 6, int storageSlots = 60)
        {
            _maxBagSlots = bagSlots;
            _maxPocketSlots = pocketSlots;
            _maxStorageSlots = storageSlots;
        }
        #endregion

        #region Public Methods - Queries
        /// <summary>
        /// Get maximum slots for a specific location
        /// </summary>
        public int GetMaxSlots(ItemLocation location)
        {
            return location switch
            {
                ItemLocation.Bag => _maxBagSlots,
                ItemLocation.Pocket => _maxPocketSlots,
                ItemLocation.Storage => _maxStorageSlots,
                _ => 0
            };
        }

        /// <summary>
        /// Check if a location has available space
        /// </summary>
        public bool HasSpace(ItemLocation location, int currentItemCount)
        {
            return currentItemCount < GetMaxSlots(location);
        }

        /// <summary>
        /// Get number of empty slots in a location
        /// </summary>
        public int GetEmptySlots(ItemLocation location, int currentItemCount)
        {
            return Mathf.Max(0, GetMaxSlots(location) - currentItemCount);
        }
        #endregion

        #region Public Methods - Upgrades
        /// <summary>
        /// Upgrade bag capacity
        /// </summary>
        public void UpgradeBag(int additionalSlots)
        {
            if (additionalSlots <= 0)
            {
                Debug.LogWarning($"[SlotCapacityManager] Invalid bag upgrade: {additionalSlots}");
                return;
            }

            _maxBagSlots += additionalSlots;
            OnCapacityChanged?.Invoke(ItemLocation.Bag, _maxBagSlots);
            Debug.Log($"[SlotCapacityManager] Bag upgraded to {_maxBagSlots} slots (+{additionalSlots})");
        }

        /// <summary>
        /// Upgrade pocket capacity
        /// </summary>
        public void UpgradePocket(int additionalSlots)
        {
            if (additionalSlots <= 0)
            {
                Debug.LogWarning($"[SlotCapacityManager] Invalid pocket upgrade: {additionalSlots}");
                return;
            }

            _maxPocketSlots += additionalSlots;
            OnCapacityChanged?.Invoke(ItemLocation.Pocket, _maxPocketSlots);
            Debug.Log($"[SlotCapacityManager] Pocket upgraded to {_maxPocketSlots} slots (+{additionalSlots})");
        }

        /// <summary>
        /// Upgrade storage capacity
        /// </summary>
        public void UpgradeStorage(int additionalSlots)
        {
            if (additionalSlots <= 0)
            {
                Debug.LogWarning($"[SlotCapacityManager] Invalid storage upgrade: {additionalSlots}");
                return;
            }

            _maxStorageSlots += additionalSlots;
            OnCapacityChanged?.Invoke(ItemLocation.Storage, _maxStorageSlots);
            Debug.Log($"[SlotCapacityManager] Storage upgraded to {_maxStorageSlots} slots (+{additionalSlots})");
        }

        /// <summary>
        /// Set all capacities at once (for loading save data)
        /// </summary>
        public void SetCapacities(int bagSlots, int pocketSlots, int storageSlots)
        {
            _maxBagSlots = bagSlots;
            _maxPocketSlots = pocketSlots;
            _maxStorageSlots = storageSlots;

            Debug.Log($"[SlotCapacityManager] Capacities set - Bag: {bagSlots}, Pocket: {pocketSlots}, Storage: {storageSlots}");
        }
        #endregion

        #region Public Methods - Validation
        /// <summary>
        /// Validate if adding items would exceed capacity
        /// </summary>
        public bool CanAddItems(ItemLocation location, int currentCount, int itemsToAdd)
        {
            int maxSlots = GetMaxSlots(location);
            return (currentCount + itemsToAdd) <= maxSlots;
        }

        /// <summary>
        /// Get overflow amount if adding items exceeds capacity
        /// </summary>
        public int GetOverflowAmount(ItemLocation location, int currentCount, int itemsToAdd)
        {
            int maxSlots = GetMaxSlots(location);
            int totalAfterAdd = currentCount + itemsToAdd;
            return Mathf.Max(0, totalAfterAdd - maxSlots);
        }
        #endregion
    }
}