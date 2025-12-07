// ──────────────────────────────────────────────────
// EquipmentSlotUI.cs
// UI component for individual equipment slots
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Ascension.Manager;
using Ascension.Data.SO.Item;

public class EquipmentSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button slotButton;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private GameObject emptyIndicator;
    
    [Header("State")]
    private EquipmentSlotType slotType;
    private ItemBaseSO currentItem;
    private Action<EquipmentSlotUI> onClickCallback;

    private void Awake()
    {
        if (slotButton != null)
            slotButton.onClick.AddListener(OnClick);
    }

    public void Initialize(EquipmentSlotType type, Action<EquipmentSlotUI> callback)
    {
        slotType = type;
        onClickCallback = callback;
        Clear();
    }

    public void SetItem(ItemBaseSO item)
    {
        currentItem = item;
        
        if (item != null)
        {
            // Show item
            if (itemIcon != null)
            {
                itemIcon.sprite = item.Icon;
                itemIcon.enabled = true;
            }
            
            // Set rarity color
            if (rarityBorder != null)
            {
                rarityBorder.color = GetRarityColor(item.Rarity);
                rarityBorder.enabled = true;
            }
            
            if (emptyIndicator != null)
                emptyIndicator.SetActive(false);
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        currentItem = null;
        
        if (itemIcon != null)
            itemIcon.enabled = false;
        
        if (rarityBorder != null)
            rarityBorder.enabled = false;
        
        if (emptyIndicator != null)
            emptyIndicator.SetActive(true);
    }

    private void OnClick()
    {
        onClickCallback?.Invoke(this);
    }

    public bool HasItem() => currentItem != null;
    public ItemBaseSO GetItem() => currentItem;
    public EquipmentSlotType SlotType => slotType;

    private Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return new Color(0.7f, 0.7f, 0.7f); // Gray
            case Rarity.Rare: return new Color(0.2f, 0.6f, 1f); // Blue
            case Rarity.Epic: return new Color(0.7f, 0.3f, 1f); // Purple
            case Rarity.Legendary: return new Color(1f, 0.6f, 0f); // Orange
            case Rarity.Mythic: return new Color(1f, 0.2f, 0.2f); // Red
            default: return Color.white;
        }
    }
}