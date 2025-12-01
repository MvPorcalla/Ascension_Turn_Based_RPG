// ──────────────────────────────────────────────────
// ItemInstance.cs
// Represents an instance of an item in inventory/storage
// ──────────────────────────────────────────────────

using System;

[Serializable]
public class ItemInstance
{
    public string itemID; // Reference to ItemBaseSO
    public int quantity;
    public bool isEquipped;
    public bool isInBag; // true = in 12-slot bag, false = in storage
    public bool isInPocket; // true = in 6-slot pocket

    public ItemInstance(string itemID, int quantity = 1, bool isInBag = false, bool isInPocket = false)
    {
        this.itemID = itemID;
        this.quantity = quantity;
        this.isEquipped = false;
        this.isInBag = isInBag;
        this.isInPocket = isInPocket;
    }

    /// <summary>
    /// Create a copy of this instance
    /// </summary>
    public ItemInstance Clone()
    {
        return new ItemInstance(itemID, quantity, isInBag, isInPocket)
        {
            isEquipped = this.isEquipped
        };
    }

    public string GetLocation()
    {
        if (isInBag) return "Bag";
        if (isInPocket) return "Pocket";
        return "Storage";
    }
}