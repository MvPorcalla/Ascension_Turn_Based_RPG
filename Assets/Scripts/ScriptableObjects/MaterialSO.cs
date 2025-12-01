// ──────────────────────────────────────────────────
// MaterialSO.cs
// ScriptableObject for defining materials in the game
// ──────────────────────────────────────────────────

using UnityEngine;

[CreateAssetMenu(fileName = "New Material", menuName = "Game/Material")]
public class MaterialSO : ItemBaseSO
{
    [Header("Material Settings")]
    public MaterialCategory category;
    public int value; // sell/buy or crafting value
    public bool isCraftingMaterial = true;

    private void OnValidate()
    {
        itemType = ItemType.Material;
        isStackable = true;
        maxStackSize = 999;

        // Auto-sync itemName with asset name if empty
        if (string.IsNullOrEmpty(itemName))
            itemName = name;

        // Auto-generate itemID if empty
        if (string.IsNullOrEmpty(itemID))
            itemID = $"material_{name.ToLower().Replace(" ", "_")}";
    }

    #region Debug Helpers

    [ContextMenu("Test: Add to Inventory (Bag)")]
    private void DebugAddToBag()
    {
        if (Application.isPlaying && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemID, 5, true);
            Debug.Log($"[MaterialSO] Added 5x {itemName} to bag");
        }
    }

    [ContextMenu("Test: Add to Storage")]
    private void DebugAddToStorage()
    {
        if (Application.isPlaying && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemID, 10, false);
            Debug.Log($"[MaterialSO] Added 10x {itemName} to storage");
        }
    }

    [ContextMenu("Print Material Info")]
    private void DebugPrintInfo()
    {
        Debug.Log($"=== {itemName} ===");
        Debug.Log($"ID: {itemID}");
        Debug.Log($"Category: {category}");
        Debug.Log($"Rarity: {rarity}");
        Debug.Log($"Value: {value}");
        Debug.Log($"Crafting Material: {isCraftingMaterial}");
    }

    #endregion

}

public enum MaterialCategory
{
    Ore,
    Herb,
    Essence,
    Fabric,
    MonsterDrop,
    Misc
}
