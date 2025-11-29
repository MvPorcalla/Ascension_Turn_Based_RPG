// -------------------------------
// ItemBaseSO.cs - Base class for all items
// -------------------------------
using UnityEngine;

public abstract class ItemBaseSO : ScriptableObject
{
    [Header("Basic Info")]
    public string itemID; // Unique identifier (auto-generated or manual)
    public string itemName;
    public Sprite icon;
    public Rarity rarity = Rarity.Common;
    [TextArea(2, 4)] public string description;

    [Header("Item Properties")]
    public ItemType itemType;
    public bool isStackable = false;
    public int maxStackSize = 999;

    [Header("Requirements")]
    public int requiredLevel = 1;

    /// <summary>
    /// Get formatted info text for tooltip/details
    /// </summary>
    public virtual string GetInfoText()
    {
        return $"<b>{itemName}</b>\n{rarity}\n\n{description}";
    }
}

public enum ItemType
{
    Weapon,
    Armor,
    Skill,
    Consumable,
    Material,
    Misc
}

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary,
    Mythic
}