// ══════════════════════════════════════════════════════════════════
// Assets\Scripts\Modules\StorageSystem\UI\EquippedGearSlotUI.cs
// ✅ INTERACTIVE: Equipped gear slot with click-to-unequip popup
// ══════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using Ascension.Equipment.Enums;
using Ascension.Data.SO.Database;
using Ascension.SharedUI.Popups;

namespace Ascension.Storage.UI
{
    /// <summary>
    /// Individual equipped gear slot - clickable to show popup with unequip option
    /// </summary>
    public class EquippedGearSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button slotButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject emptyOverlay;

        [Header("Optional Label")]
        [SerializeField] private TMPro.TextMeshProUGUI labelText;

        [Header("Visual Feedback")]
        [SerializeField] private Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color filledSlotColor = Color.white;

        private GearSlotType slotType;
        private string currentItemId;

        #region Initialization

        public void Initialize(GearSlotType type)
        {
            slotType = type;

            if (labelText != null)
                labelText.text = type.ToDisplayName();

            if (slotButton != null)
                slotButton.onClick.AddListener(OnSlotClicked);

            SetItem(null, null);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update slot display with item
        /// </summary>
        public void SetItem(string itemId, GameDatabaseSO database)
        {
            currentItemId = itemId;
            bool hasItem = !string.IsNullOrEmpty(itemId);

            // Update empty overlay
            if (emptyOverlay != null)
                emptyOverlay.SetActive(!hasItem);

            // Update background color
            if (backgroundImage != null)
                backgroundImage.color = hasItem ? filledSlotColor : emptySlotColor;

            // Update button interactivity
            if (slotButton != null)
                slotButton.interactable = hasItem;

            // Update icon
            if (!hasItem || database == null)
            {
                ClearIcon();
                return;
            }

            var itemData = database.GetItem(itemId);
            if (itemData != null && iconImage != null)
            {
                iconImage.sprite = itemData.Icon;
                iconImage.enabled = true;
                iconImage.color = Color.white;
            }
            else
            {
                ClearIcon();
            }
        }

        #endregion

        #region Private Methods

        private void ClearIcon()
        {
            if (iconImage != null)
                iconImage.enabled = false;
        }

        /// <summary>
        /// ✅ Handle slot click - show popup with unequip option
        /// </summary>
        private void OnSlotClicked()
        {
            if (string.IsNullOrEmpty(currentItemId))
            {
                Debug.LogWarning($"[EquippedGearSlotUI] Slot {slotType} is empty");
                return;
            }

            var inventoryMgr = Inventory.Manager.InventoryManager.Instance;
            if (inventoryMgr?.Database == null)
            {
                Debug.LogError("[EquippedGearSlotUI] InventoryManager not available");
                return;
            }

            var itemData = inventoryMgr.Database.GetItem(currentItemId);
            if (itemData == null)
            {
                Debug.LogError($"[EquippedGearSlotUI] Item data not found: {currentItemId}");
                return;
            }

            // Show popup for equipped item
            if (GearPopup.Instance != null)
            {
                GearPopup.Instance.ShowEquipped(itemData, currentItemId);
            }
            else
            {
                Debug.LogError("[EquippedGearSlotUI] GearPopup instance not found");
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (slotButton != null)
                slotButton.onClick.RemoveListener(OnSlotClicked);
        }

        #endregion
    }
}