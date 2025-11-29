// -------------------------------
// BagInventoryData.cs - Serializable inventory data (renamed)
// -------------------------------
using System;
using System.Collections.Generic;

[Serializable]
public class BagInventoryData
{
    public List<ItemInstance> items = new List<ItemInstance>();
    public int maxBagSlots = 12;
    public int maxPocketSlots = 6;

    /// <summary>
    /// Create from BagInventory
    /// </summary>
    public static BagInventoryData FromInventory(BagInventory inventory)
    {
        return new BagInventoryData
        {
            items = new List<ItemInstance>(inventory.allItems),
            maxBagSlots = inventory.maxBagSlots,
            maxPocketSlots = inventory.maxPocketSlots
        };
    }
}