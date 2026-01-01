// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\StorageSystem\Controller\StorageRoomController.cs
// Main controller for the Storage Room scene.
// ──────────────────────────────────────────────────

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;

namespace Ascension.Storage.UI
{
    /// <summary>
    /// Main controller for the Storage Room scene.
    /// Coordinates sub-panels and handles room-level actions.
    /// </summary>
    public class StorageRoomController : MonoBehaviour
    {
        [Header("Sub-Panels")]
        [SerializeField] private BagInventoryUI bagInventoryUI;
        [SerializeField] private PocketInventoryUI pocketInventoryUI;
        [SerializeField] private StorageInventoryUI storageInventoryUI;

        [Header("Quick Actions")]
        [SerializeField] private Button storeAllButton;
        [SerializeField] private Button backButton;

        [Header("Feedback (Optional)")]
        [SerializeField] private TMPro.TextMeshProUGUI feedbackText; // Optional: Show "10 items stored!"

        #region Lifecycle

        private void Start()
        {
            SetupButtons();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Event Management

        /// <summary>
        /// ✅ NEW: Subscribe to batch events for efficient updates
        /// </summary>
        private void SubscribeToEvents()
        {
            if (InventoryManager.Instance?.Inventory != null)
            {
                // ✅ Subscribe to batch move event
                InventoryManager.Instance.Inventory.OnBatchItemsMoved += HandleBatchItemsMoved;
                
                // ✅ Subscribe to individual move event (for drag & drop)
                InventoryManager.Instance.Inventory.OnItemMoved += HandleSingleItemMoved;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (InventoryManager.Instance?.Inventory != null)
            {
                InventoryManager.Instance.Inventory.OnBatchItemsMoved -= HandleBatchItemsMoved;
                InventoryManager.Instance.Inventory.OnItemMoved -= HandleSingleItemMoved;
            }
        }

        #endregion

        #region Button Setup

        private void SetupButtons()
        {
            if (storeAllButton != null)
                storeAllButton.onClick.AddListener(OnStoreAllBagClicked);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        #endregion

        #region Quick Actions

        /// <summary>
        /// ✅ IMPROVED: Now passes equipped checker to avoid storing equipped items
        /// </summary>
        private void OnStoreAllBagClicked()
        {
            if (InventoryManager.Instance?.Inventory == null)
            {
                Debug.LogError("[StorageRoomController] InventoryManager not available!");
                return;
            }

            // ✅ Pass equipped checker to avoid storing equipped gear
            InventoryManager.Instance.Inventory.StoreAllItems(
                itemID => Equipment.Manager.EquipmentManager.Instance?.IsItemEquipped(itemID) ?? false
            );

            // Note: Feedback is handled by HandleBatchItemsMoved event
        }

        private void OnBackClicked()
        {
            Debug.Log("[StorageRoomController] Back button clicked");
            gameObject.SetActive(false);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// ✅ NEW: Handle batch moves efficiently (e.g., "Store All" button)
        /// Only refreshes UI ONCE for all items instead of N times
        /// </summary>
        private void HandleBatchItemsMoved(List<ItemInstance> items, ItemLocation from, ItemLocation to)
        {
            Debug.Log($"[StorageRoomController] Batch moved {items.Count} items: {from} → {to}");

            // ✅ Refresh only the affected UIs (not individual slots)
            if (from == ItemLocation.Bag || to == ItemLocation.Bag)
            {
                bagInventoryUI?.ForceRefresh();
            }

            if (from == ItemLocation.Pocket || to == ItemLocation.Pocket)
            {
                pocketInventoryUI?.ForceRefresh();
            }

            if (from == ItemLocation.Storage || to == ItemLocation.Storage)
            {
                storageInventoryUI?.ForceRefresh();
            }

            // ✅ Optional: Show user feedback
            ShowFeedback($"Stored {items.Count} items");
        }

        /// <summary>
        /// ✅ NEW: Handle single item moves (e.g., drag & drop)
        /// Can be optimized further with targeted slot updates in the future
        /// </summary>
        private void HandleSingleItemMoved(ItemInstance item, ItemLocation from, ItemLocation to)
        {
            Debug.Log($"[StorageRoomController] Moved {item.itemID}: {from} → {to}");

            // For now, do the same as batch (full refresh)
            // TODO: Optimize to only update specific slots instead of full refresh
            if (from == ItemLocation.Bag || to == ItemLocation.Bag)
            {
                bagInventoryUI?.ForceRefresh();
            }

            if (from == ItemLocation.Pocket || to == ItemLocation.Pocket)
            {
                pocketInventoryUI?.ForceRefresh();
            }

            if (from == ItemLocation.Storage || to == ItemLocation.Storage)
            {
                storageInventoryUI?.ForceRefresh();
            }
        }

        #endregion

        #region Feedback System (Optional)

        /// <summary>
        /// ✅ Optional: Show user feedback with auto-hide
        /// </summary>
        private void ShowFeedback(string message)
        {
            if (feedbackText == null) return;

            feedbackText.text = message;
            feedbackText.gameObject.SetActive(true);

            // Auto-hide after 2 seconds
            CancelInvoke(nameof(HideFeedback));
            Invoke(nameof(HideFeedback), 2f);
        }

        private void HideFeedback()
        {
            if (feedbackText != null)
            {
                feedbackText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Public Methods (for external triggers)

        /// <summary>
        /// Manually refresh all panels (useful after loading save data)
        /// </summary>
        public void RefreshAllPanels()
        {
            bagInventoryUI?.ForceRefresh();
            pocketInventoryUI?.ForceRefresh();
            storageInventoryUI?.ForceRefresh();
        }

        #endregion
    }
}