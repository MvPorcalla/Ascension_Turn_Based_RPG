// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\UI\ItemSlotUI.cs
// UI component for displaying an item slot in inventory/storage/equipment
// Uses dirty-flag pattern for optimized UI updates
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;

namespace Ascension.Inventory.UI
{
    public class ItemSlotUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image rarityBackground;
        [SerializeField] private Image itemIcon;
        [SerializeField] private GameObject equippedIndicator;
        [SerializeField] private TMP_Text quantityText;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = new(0.7f, 0.7f, 0.7f, 1f);
        [SerializeField] private Color rareColor = new(0.2f, 0.5f, 1f, 1f);
        [SerializeField] private Color epicColor = new(0.6f, 0.2f, 1f, 1f);
        [SerializeField] private Color legendaryColor = new(1f, 0.75f, 0.1f, 1f);
        [SerializeField] private Color mythicColor = new(1f, 0.2f, 0.2f, 1f);

        [Header("Empty Slot")]
        [SerializeField] private Color emptySlotColor = new(0.3f, 0.3f, 0.3f, 0.5f);

        [Header("Optional Effects")]
        [SerializeField] private bool useGlowEffect = true;
        [SerializeField] private float glowIntensity = 1.3f;

        private ItemInstance itemInstance;
        private ItemBaseSO itemData;
        private Action currentOnClick;

        // ───────── Dirty Flag ─────────
        private bool isDirty;

        #region Public API

        /// <summary>
        /// Initial setup (called once when slot is assigned)
        /// </summary>
        public void Setup(ItemBaseSO data, ItemInstance instance, Action onClick)
        {
            itemData = data;
            itemInstance = instance;
            currentOnClick = onClick;

            MarkDirty();
        }

        /// <summary>
        /// Fast update without recreation
        /// </summary>
        public void UpdateItem(ItemBaseSO data, ItemInstance instance, Action onClick)
        {
            if (itemData == data && itemInstance == instance && currentOnClick == onClick)
                return;

            itemData = data;
            itemInstance = instance;
            currentOnClick = onClick;

            MarkDirty();
        }

        /// <summary>
        /// Force slot to empty state immediately
        /// </summary>
        public void ShowEmpty()
        {
            itemData = null;
            itemInstance = null;
            currentOnClick = null;
            isDirty = false;

            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.enabled = false;
            }

            if (rarityBackground != null)
            {
                rarityBackground.color = emptySlotColor;
            }

            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(false);
            }

            if (equippedIndicator != null)
            {
                equippedIndicator.SetActive(false);
            }

            if (button != null)
            {
                button.interactable = false;
                button.onClick.RemoveAllListeners();
            }
        }

        public bool IsEmpty() => itemData == null;

        #endregion

        #region Dirty Flag Logic

        public void MarkDirty()
        {
            isDirty = true;
        }

        private void LateUpdate()
        {
            if (!isDirty) return;

            UpdateVisuals();
            UpdateButton();
            isDirty = false;
        }

        #endregion

        #region Visual Updates

        private void UpdateVisuals()
        {
            if (itemData == null)
            {
                ShowEmpty();
                return;
            }

            UpdateIcon();
            UpdateRarityBackground();
            UpdateQuantity();
            UpdateEquippedIndicator();
        }

        private void UpdateIcon()
        {
            if (itemIcon == null) return;

            if (itemData.Icon != null)
            {
                itemIcon.sprite = itemData.Icon;
                itemIcon.color = Color.white;
                itemIcon.enabled = true;
            }
            else
            {
                itemIcon.sprite = null;
                itemIcon.enabled = false;
            }
        }

        private void UpdateRarityBackground()
        {
            if (rarityBackground == null) return;

            Color rarityColor = GetRarityColor(itemData.Rarity);

            if (useGlowEffect &&
                (itemData.Rarity == Rarity.Epic ||
                 itemData.Rarity == Rarity.Legendary ||
                 itemData.Rarity == Rarity.Mythic))
            {
                rarityColor *= glowIntensity;
            }

            rarityBackground.color = rarityColor;
        }

        private void UpdateQuantity()
        {
            if (quantityText == null) return;

            if (itemData.IsStackable && itemInstance != null && itemInstance.quantity > 1)
            {
                quantityText.gameObject.SetActive(true);
                quantityText.text = $"x{itemInstance.quantity}";
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }

        private void UpdateEquippedIndicator()
        {
            if (equippedIndicator == null) return;

            bool isEquipped = false;
            var equipMgr = Equipment.Manager.EquipmentManager.Instance;

            if (itemInstance != null && equipMgr != null)
            {
                isEquipped = equipMgr.IsItemEquipped(itemInstance.itemID);
            }

            equippedIndicator.SetActive(isEquipped);
        }

        private void UpdateButton()
        {
            if (button == null) return;

            button.interactable = itemData != null;
            button.onClick.RemoveAllListeners();

            if (currentOnClick != null)
            {
                button.onClick.AddListener(() => currentOnClick.Invoke());
            }
        }

        private Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => commonColor,
                Rarity.Rare => rareColor,
                Rarity.Epic => epicColor,
                Rarity.Legendary => legendaryColor,
                Rarity.Mythic => mythicColor,
                _ => commonColor
            };
        }

        #endregion

        #region Debug

        public void ShowTooltip()
        {
            if (itemData == null) return;
            Debug.Log($"[ItemSlotUI] Tooltip: {itemData.GetInfoText()}");
        }

        #endregion
    }
}
