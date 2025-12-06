// ════════════════════════════════════════════
// ItemBaseSO.cs
// Base ScriptableObject for all item types
// ════════════════════════════════════════════

using UnityEngine;

namespace Ascension.Data.SO
{
    public abstract class ItemBaseSO : ScriptableObject
    {
        #region Serialized Fields
        [Header("Basic Info")]
        [SerializeField] protected string itemID; // Unique identifier
        [SerializeField] protected string itemName;
        [SerializeField] protected Sprite icon;
        [SerializeField] protected Rarity rarity = Rarity.Common;
        [TextArea(2, 4)]
        [SerializeField] protected string description;

        [Header("Item Properties")]
        [SerializeField] protected ItemType itemType;
        [SerializeField] protected bool isStackable = false;
        [SerializeField] protected int maxStackSize = 999;

        [Header("Requirements")]
        [SerializeField] protected int requiredLevel = 1;
        #endregion

        #region Properties
        public string ItemID => itemID;
        public string ItemName => itemName;
        public Sprite Icon => icon;
        public Rarity Rarity => rarity;
        public string Description => description;

        public ItemType ItemType => itemType;
        public bool IsStackable => isStackable;
        public int MaxStackSize => maxStackSize;
        public int RequiredLevel => requiredLevel;
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns formatted info text for tooltip or details
        /// </summary>
        public virtual string GetInfoText()
        {
            return $"<b>{itemName}</b>\n{rarity}\n\n{description}";
        }
        #endregion
    }

    public enum ItemType
    {
        Weapon,
        Gear,
        Ability,
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
}
