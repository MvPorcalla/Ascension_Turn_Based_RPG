// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\UI\ItemSlotUI.cs
// ✅ REFACTORED: CanvasGroup visibility, reusable click handler, no allocations
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;

namespace Ascension.UI.Components.Inventory
{
    [RequireComponent(typeof(CanvasGroup))]
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
        [SerializeField] private bool useGlowEffect = false;
        [SerializeField] private float glowIntensity = 1.3f;

        // ✅ NEW: CanvasGroup for visibility (no SetActive)
        private CanvasGroup canvasGroup;

        private ItemInstance itemInstance;
        private ItemBaseSO itemData;
        
        // ✅ NEW: Store item ID to avoid lambda allocations
        private string cachedItemID;

        // Dirty Flag
        private bool isDirty;

        #region Initialization

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // ✅ NEW: Register reusable click handler (no lambda allocation)
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// ✅ OPTIMIZED: Setup slot with item data
        /// </summary>
        public void Setup(ItemBaseSO data, ItemInstance instance, Action onClick)
        {
            itemData = data;
            itemInstance = instance;
            cachedItemID = instance?.itemID;

            MarkDirty();
        }

        /// <summary>
        /// ✅ OPTIMIZED: Fast update without recreation
        /// </summary>
        public void UpdateItem(ItemBaseSO data, ItemInstance instance, Action onClick)
        {
            if (itemData == data && itemInstance == instance)
                return;

            itemData = data;
            itemInstance = instance;
            cachedItemID = instance?.itemID;

            MarkDirty();
        }

        /// <summary>
        /// ✅ OPTIMIZED: Hide slot without SetActive (uses CanvasGroup)
        /// </summary>
        public void ShowEmpty()
        {
            itemData = null;
            itemInstance = null;
            cachedItemID = null;
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
            }

            // ✅ NEW: Hide via CanvasGroup instead of SetActive
            SetVisible(false);
        }

        /// <summary>
        /// ✅ NEW: Control visibility without layout rebuild
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (canvasGroup == null) return;

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
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

            // ✅ NEW: Show slot when it has data
            SetVisible(true);

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
            
            if (itemInstance != null && Core.GameBootstrap.Equipment != null)
            {
                isEquipped = Core.GameBootstrap.Equipment.IsItemEquipped(itemInstance.itemID);
            }

            if (equippedIndicator.activeSelf != isEquipped)
            {
                equippedIndicator.SetActive(isEquipped);
            }
        }

        private void UpdateButton()
        {
            if (button == null) return;
            button.interactable = itemData != null;
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

        #region Click Handler (No Lambda Allocation)

        /// <summary>
        /// ✅ NEW: Reusable click handler - no lambda allocations
        /// Uses GameEvents to trigger popup with cached item ID
        /// </summary>
        private void OnButtonClicked()
        {
            if (string.IsNullOrEmpty(cachedItemID) || itemData == null)
                return;

            // ✅ Trigger event with item data
            Core.GameEvents.OnItemSlotClicked?.Invoke(itemData, itemInstance);
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        #endregion
    }
}