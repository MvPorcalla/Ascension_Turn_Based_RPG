// ════════════════════════════════════════════
// MaterialSO.cs
// ScriptableObject defining materials for crafting, inventory, and sale
// ════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;
using Ascension.Data.SO.Item;

namespace Ascension.Data.SO.Item
{
    [CreateAssetMenu(fileName = "New Material", menuName = "Items/Material")]
    public class MaterialSO : ItemBaseSO
    {
        #region Serialized Fields

        [Header("Material Settings")]
        [Tooltip("Defines the type/category of this material")]
        public MaterialCategory category;

        [Tooltip("Base value for selling, buying, or crafting")]
        public int value;

        [Tooltip("Determines if this material can be used in crafting")]
        public bool isCraftingMaterial = true;

        #endregion

        #region Unity Callbacks

        private void OnValidate()
        {
            // Ensure base Item settings are correct
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

        #endregion

        #region Debug Helpers

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
}
