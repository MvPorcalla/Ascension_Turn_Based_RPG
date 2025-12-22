// ════════════════════════════════════════════
// Assets\Scripts\Modules\SharedUI\Popups\IGearPopupContext.cs
// Context interface for GearPopup behavior
// ════════════════════════════════════════════

using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;

namespace Ascension.SharedUI.Popups  // ✅ FIXED: Changed from Ascension.Equipment.UI
{
    /// <summary>
    /// Defines context-specific behavior for GearPopup
    /// </summary>
    public interface IGearPopupContext
    {
        /// <summary>
        /// Get button text based on item state
        /// </summary>
        string GetButtonText(ItemBaseSO item, ItemInstance itemInstance);

        /// <summary>
        /// Handle button click action
        /// </summary>
        /// <returns>True if action succeeded and popup should close</returns>
        bool OnButtonClicked(ItemBaseSO item, ItemInstance itemInstance);

        /// <summary>
        /// Optional: Validate if action is possible
        /// </summary>
        bool CanPerformAction(ItemBaseSO item, ItemInstance itemInstance);
    }
}