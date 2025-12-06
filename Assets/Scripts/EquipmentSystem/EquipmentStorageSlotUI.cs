// ──────────────────────────────────────────────────
// EquipmentStorageSlotUI.cs
// UI component for storage item slots in Equipment Room
// (Different from Inventory's ItemSlotUI)
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Ascension.Data.SO;
using Ascension.Managers;

public class EquipmentStorageSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private GameObject equippedIndicator;
    
    [Header("State")]
    private ItemBaseSO item;
    
    public event Action OnClick;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(() => OnClick?.Invoke());
    }

    public void SetItem(ItemBaseSO itemData)
    {
        item = itemData;
        
        if (item != null)
        {
            // Set icon
            if (itemIcon != null)
            {
                itemIcon.sprite = item.Icon;
                itemIcon.enabled = true;
            }
            
            // Set rarity
            if (rarityBorder != null)
            {
                rarityBorder.color = GetRarityColor(item.Rarity);
            }
            
            // Set quantity (only for stackable items)
            if (quantityText != null)
            {
                if (item.IsStackable)
                {
                    int quantity = GetItemQuantity(item.ItemID);
                    quantityText.text = quantity > 999 ? "999+" : quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetEquippedIndicator(bool isEquipped)
    {
        if (equippedIndicator != null)
            equippedIndicator.SetActive(isEquipped);
    }

    public ItemBaseSO GetItem() => item;

    private int GetItemQuantity(string itemID)
    {
        if (InventoryManager.Instance == null) return 0;
        
        var inventoryItem = InventoryManager.Instance.Inventory.allItems.Find(x => x.itemID == itemID);
        return inventoryItem != null ? inventoryItem.quantity : 0;
    }

    private Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return new Color(0.7f, 0.7f, 0.7f);
            case Rarity.Rare: return new Color(0.2f, 0.6f, 1f);
            case Rarity.Epic: return new Color(0.7f, 0.3f, 1f);
            case Rarity.Legendary: return new Color(1f, 0.6f, 0f);
            case Rarity.Mythic: return new Color(1f, 0.2f, 0.2f);
            default: return Color.white;
        }
    }
}