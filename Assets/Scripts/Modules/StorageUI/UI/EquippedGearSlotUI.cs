// ══════════════════════════════════════════════════════════════════
// Assets\Scripts\Modules\StorageSystem\UI\EquippedGearSlotUI.cs
// UI component for a single equipped gear slot in storage room
// ══════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using Ascension.Equipment.Enums;
using Ascension.Data.SO.Database;
using Ascension.SharedUI.Popups;
using Ascension.Inventory.Enums;
using Ascension.Inventory.Manager;
using System.Linq;

namespace Ascension.Storage.UI
{
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

        public void SetItem(string itemId, GameDatabaseSO database)
        {
            currentItemId = itemId;
            bool hasItem = !string.IsNullOrEmpty(itemId);

            if (emptyOverlay != null)
                emptyOverlay.SetActive(!hasItem);

            if (backgroundImage != null)
                backgroundImage.color = hasItem ? filledSlotColor : emptySlotColor;

            if (slotButton != null)
                slotButton.interactable = hasItem;

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
        /// Uses PopupManager with PopupContext.FromEquippedGear()
        /// </summary>
        private void OnSlotClicked()
        {
            if (string.IsNullOrEmpty(currentItemId))
            {
                Debug.LogWarning($"[EquippedGearSlotUI] Slot {slotType} is empty");
                return;
            }

            var inventoryMgr = InventoryManager.Instance;
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

            // Get ItemInstance from inventory
            var itemInstance = inventoryMgr.Inventory.allItems
                .FirstOrDefault(i => i.itemID == currentItemId && i.location == ItemLocation.Equipped);

            if (itemInstance == null)
            {
                Debug.LogError($"[EquippedGearSlotUI] ItemInstance not found: {currentItemId}");
                return;
            }

            // Use PopupManager with equipped gear context
            PopupManager.Instance.ShowItemPopup(
                itemData,
                itemInstance,
                PopupContext.FromEquippedGear() // Shows "Unequip" only
            );
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