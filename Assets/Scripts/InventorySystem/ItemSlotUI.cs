// ──────────────────────────────────────────────────
// InventorySystem
// ItemSlotUI.cs
// UI component for displaying an item slot in inventory/storage
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button button;
    [SerializeField] private Image rarityBackground; // This gets colored by rarity
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject equippedIndicator;
    [SerializeField] private TMP_Text quantityText;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = new Color(0.7f, 0.7f, 0.7f, 1f); // Gray
    [SerializeField] private Color rareColor = new Color(0.2f, 0.5f, 1f, 1f); // Blue
    [SerializeField] private Color epicColor = new Color(0.6f, 0.2f, 1f, 1f); // Purple
    [SerializeField] private Color legendaryColor = new Color(1f, 0.75f, 0.1f, 1f); // Gold
    [SerializeField] private Color mythicColor = new Color(1f, 0.2f, 0.2f, 1f); // Red

    [Header("Optional Effects")]
    [SerializeField] private bool useGlowEffect = true;
    [SerializeField] private float glowIntensity = 1.3f;

    private ItemInstance itemInstance;
    private ItemBaseSO itemData;

    public void Setup(ItemBaseSO data, ItemInstance instance, Action onClick)
    {
        itemData = data;
        itemInstance = instance;

        // Set icon (CRITICAL: Clear previous icon if null to prevent caching bug)
        if (itemIcon != null)
        {
            if (data.icon != null)
            {
                itemIcon.sprite = data.icon;
                itemIcon.color = Color.white; // Keep icon at full color
                itemIcon.enabled = true;
            }
            else
            {
                // Clear previous sprite to prevent showing old icon
                itemIcon.sprite = null;
                itemIcon.enabled = false;
            }
        }

        // Apply rarity color to background
        if (rarityBackground != null)
        {
            Color rarityColor = GetRarityColor(data.rarity);

            // Optional: Add glow effect for higher rarities
            if (useGlowEffect && (data.rarity == Rarity.Epic || 
                                   data.rarity == Rarity.Legendary || 
                                   data.rarity == Rarity.Mythic))
            {
                rarityColor = rarityColor * glowIntensity;
            }

            rarityBackground.color = rarityColor;
        }
        else
        {
            Debug.LogWarning("[ItemSlotUI] RarityBackground not assigned! Rarity colors won't show.");
        }

        // Set quantity
        if (data.isStackable && instance.quantity > 1)
        {
            quantityText.gameObject.SetActive(true);
            quantityText.text = $"x{instance.quantity}";
        }
        else
        {
            quantityText.gameObject.SetActive(false);
        }

        // Set equipped indicator
        if (equippedIndicator != null)
        {
            equippedIndicator.SetActive(instance.isEquipped);
        }

        // Setup button click
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
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

    /// <summary>
    /// Show item info tooltip (placeholder for now)
    /// </summary>
    public void ShowTooltip()
    {
        // TODO: Implement tooltip system
        Debug.Log($"[ItemSlot] Tooltip: {itemData.GetInfoText()}");
    }
}