// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Manager/SlotCapacityManager.cs
// Manages inventory slot capacities (bag, pocket, storage)
// ══════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Config;

namespace Ascension.Inventory.Manager
{
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
        /// <summary>
        /// Constructor with optional custom capacities
        /// </summary>
        public SlotCapacityManager(
            int bagSlots = -1,
            int pocketSlots = -1,
            int storageSlots = -1)
        {
            // Use config defaults if not specified
            _maxBagSlots = bagSlots > 0 ? bagSlots : InventoryConfig.DEFAULT_BAG_SLOTS;
            _maxPocketSlots = pocketSlots > 0 ? pocketSlots : InventoryConfig.DEFAULT_POCKET_SLOTS;
            _maxStorageSlots = storageSlots > 0 ? storageSlots : InventoryConfig.DEFAULT_STORAGE_SLOTS;
        }
        #endregion

        #region Public Methods - Queries
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

        public bool HasSpace(ItemLocation location, int currentItemCount)
        {
            return currentItemCount < GetMaxSlots(location);
        }

        public int GetEmptySlots(ItemLocation location, int currentItemCount)
        {
            return Mathf.Max(0, GetMaxSlots(location) - currentItemCount);
        }
        #endregion

        #region Public Methods - Upgrades
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

        public void SetCapacities(int bagSlots, int pocketSlots, int storageSlots)
        {
            _maxBagSlots = bagSlots;
            _maxPocketSlots = pocketSlots;
            _maxStorageSlots = storageSlots;

            Debug.Log($"[SlotCapacityManager] Capacities set - Bag: {bagSlots}, Pocket: {pocketSlots}, Storage: {storageSlots}");
        }
        #endregion

        #region Public Methods - Validation
        public bool CanAddItems(ItemLocation location, int currentCount, int itemsToAdd)
        {
            int maxSlots = GetMaxSlots(location);
            return (currentCount + itemsToAdd) <= maxSlots;
        }

        public int GetOverflowAmount(ItemLocation location, int currentCount, int itemsToAdd)
        {
            int maxSlots = GetMaxSlots(location);
            int totalAfterAdd = currentCount + itemsToAdd;
            return Mathf.Max(0, totalAfterAdd - maxSlots);
        }
        #endregion
    }
}