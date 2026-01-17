// ══════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Components/Inventory/InventoryFilterBarUI.cs
// ✅ REUSABLE: Filter buttons for inventory grids
// Used by Storage scene (has filters), NOT used by Bag (no filters)
// ══════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using UnityEngine.UI;
using Ascension.Data.SO.Item;

namespace Ascension.UI.Components.Inventory
{
    /// <summary>
    /// Reusable filter bar component
    /// Fires OnFilterChanged event when user selects a filter
    /// Connect to InventoryGridUI via event subscription
    /// </summary>
    public class InventoryFilterBarUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Filter Buttons")]
        [SerializeField] private Button allItemsButton;
        [SerializeField] private Button weaponButton;
        [SerializeField] private Button gearButton;
        [SerializeField] private Button potionButton;
        [SerializeField] private Button materialsButton;
        [SerializeField] private Button miscButton;

        [Header("Visual Feedback (Optional)")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color activeColor = Color.yellow;
        [SerializeField] private bool useColorFeedback = true;
        #endregion

        #region Events
        /// <summary>
        /// Fired when filter changes (null = show all items)
        /// </summary>
        public event Action<ItemType?> OnFilterChanged;
        #endregion

        #region Private Fields
        private ItemType? currentFilter = null;
        private Button[] allButtons;
        #endregion

        #region Unity Callbacks
        private void Start()
        {
            SetupButtons();
            SetActiveFilter(null); // Default: show all items
        }

        private void OnDestroy()
        {
            CleanupButtons();
        }
        #endregion

        #region Initialization
        private void SetupButtons()
        {
            // Store all buttons for easy iteration
            allButtons = new Button[]
            {
                allItemsButton,
                weaponButton,
                gearButton,
                potionButton,
                materialsButton,
                miscButton
            };

            // Setup click listeners
            if (allItemsButton != null)
                allItemsButton.onClick.AddListener(() => SetFilter(null));

            if (weaponButton != null)
                weaponButton.onClick.AddListener(() => SetFilter(ItemType.Weapon));

            if (gearButton != null)
                gearButton.onClick.AddListener(() => SetFilter(ItemType.Gear));

            if (potionButton != null)
                potionButton.onClick.AddListener(() => SetFilter(ItemType.Consumable));

            if (materialsButton != null)
                materialsButton.onClick.AddListener(() => SetFilter(ItemType.Material));

            if (miscButton != null)
                miscButton.onClick.AddListener(() => SetFilter(ItemType.Misc));
        }

        private void CleanupButtons()
        {
            if (allItemsButton != null)
                allItemsButton.onClick.RemoveAllListeners();

            if (weaponButton != null)
                weaponButton.onClick.RemoveAllListeners();

            if (gearButton != null)
                gearButton.onClick.RemoveAllListeners();

            if (potionButton != null)
                potionButton.onClick.RemoveAllListeners();

            if (materialsButton != null)
                materialsButton.onClick.RemoveAllListeners();

            if (miscButton != null)
                miscButton.onClick.RemoveAllListeners();
        }
        #endregion

        #region Filter Logic
        /// <summary>
        /// Set active filter and fire event
        /// </summary>
        public void SetFilter(ItemType? filterType)
        {
            if (currentFilter == filterType)
                return; // Already active

            currentFilter = filterType;
            UpdateButtonVisuals();
            OnFilterChanged?.Invoke(currentFilter);
        }

        /// <summary>
        /// Update button visual states (highlight active button)
        /// </summary>
        private void UpdateButtonVisuals()
        {
            if (!useColorFeedback)
                return;

            SetActiveFilter(currentFilter);
        }

        private void SetActiveFilter(ItemType? activeFilter)
        {
            SetButtonColor(allItemsButton, activeFilter == null);
            SetButtonColor(weaponButton, activeFilter == ItemType.Weapon);
            SetButtonColor(gearButton, activeFilter == ItemType.Gear);
            SetButtonColor(potionButton, activeFilter == ItemType.Consumable);
            SetButtonColor(materialsButton, activeFilter == ItemType.Material);
            SetButtonColor(miscButton, activeFilter == ItemType.Misc);
        }

        private void SetButtonColor(Button button, bool isActive)
        {
            if (button == null)
                return;

            var colors = button.colors;
            colors.normalColor = isActive ? activeColor : normalColor;
            button.colors = colors;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Get current active filter
        /// </summary>
        public ItemType? GetCurrentFilter()
        {
            return currentFilter;
        }

        /// <summary>
        /// Reset filter to "All Items"
        /// </summary>
        public void ResetFilter()
        {
            SetFilter(null);
        }

        /// <summary>
        /// Check if a specific filter is active
        /// </summary>
        public bool IsFilterActive(ItemType filterType)
        {
            return currentFilter == filterType;
        }
        #endregion

        #region Editor Helpers
#if UNITY_EDITOR
        [ContextMenu("Test: Set Weapon Filter")]
        private void TestWeaponFilter()
        {
            SetFilter(ItemType.Weapon);
        }

        [ContextMenu("Test: Reset Filter")]
        private void TestResetFilter()
        {
            ResetFilter();
        }
#endif
        #endregion
    }
}