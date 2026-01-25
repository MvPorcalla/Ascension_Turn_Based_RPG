// ══════════════════════════════════════════════════════════════════
// Assets/Scripts/Modules/InventorySystem/Services/InventoryCapacity.cs
// ✅ NAMESPACE FIXED: Ascension.Inventory.Manager → Ascension.Inventory.Services
// ══════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Config;

namespace Ascension.Inventory.Services
{
    /// <summary>
    /// Manages inventory slot capacities for bag and storage.
    /// Handles upgrades and capacity validation.
    /// </summary>
    public class InventoryCapacity
    {
        #region Fields
        private int _maxBagSlots;
        private int _maxStorageSlots;
        #endregion

        #region Properties
        public int MaxBagSlots => _maxBagSlots;
        public int MaxStorageSlots => _maxStorageSlots;
        #endregion

        #region Events
        public event Action<ItemLocation, int> OnCapacityChanged;
        #endregion

        #region Constructor
        public InventoryCapacity(
            int bagSlots = -1,
            int storageSlots = -1)
        {
            _maxBagSlots = bagSlots > 0 ? bagSlots : InventoryConfig.DEFAULT_BAG_SLOTS;
            _maxStorageSlots = storageSlots > 0 ? storageSlots : InventoryConfig.DEFAULT_STORAGE_SLOTS;
        }
        #endregion

        #region Public Methods - Queries
        public int GetMaxSlots(ItemLocation location)
        {
            return location switch
            {
                ItemLocation.Bag => _maxBagSlots,
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
                Debug.LogWarning($"[InventoryCapacity] Invalid bag upgrade: {additionalSlots}");
                return;
            }

            _maxBagSlots += additionalSlots;
            OnCapacityChanged?.Invoke(ItemLocation.Bag, _maxBagSlots);
            Debug.Log($"[InventoryCapacity] Bag upgraded to {_maxBagSlots} slots (+{additionalSlots})");
        }

        public void UpgradeStorage(int additionalSlots)
        {
            if (additionalSlots <= 0)
            {
                Debug.LogWarning($"[InventoryCapacity] Invalid storage upgrade: {additionalSlots}");
                return;
            }

            _maxStorageSlots += additionalSlots;
            OnCapacityChanged?.Invoke(ItemLocation.Storage, _maxStorageSlots);
            Debug.Log($"[InventoryCapacity] Storage upgraded to {_maxStorageSlots} slots (+{additionalSlots})");
        }

        public void SetCapacities(int bagSlots, int storageSlots)
        {
            _maxBagSlots = bagSlots;
            _maxStorageSlots = storageSlots;

            Debug.Log($"[InventoryCapacity] Capacities set - Bag: {bagSlots}, Storage: {storageSlots}");
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