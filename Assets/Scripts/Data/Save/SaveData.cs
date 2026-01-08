// ════════════════════════════════════════════
// Assets\Scripts\Data\Save\SaveData.cs
// ✅ FIXED: Added previousLocation tracking for equipped items
// ════════════════════════════════════════════

using System;
using Ascension.Inventory.Config;

namespace Ascension.Data.Save
{
    [Serializable]
    public class SaveData
    {
        public SaveMetaData metaData;
        public CharacterSaveData characterData;
        public InventorySaveData inventoryData;
        public EquipmentSaveData equipmentData;
        public SkillLoadoutSaveData skillLoadoutData;
    }

    [Serializable]
    public class SaveMetaData
    {
        public string saveVersion;
        public string createdTime;
        public string lastSaveTime;
        public float totalPlayTimeSeconds;
        public int saveCount;
    }

    [Serializable]
    public class CharacterSaveData
    {
        public string playerName;
        public int level;
        public int currentExperience;
        public float currentHealth;
        public float currentMana;
        public int attributePoints;
        
        public int strength;
        public int agility;
        public int intelligence;
        public int endurance;
        public int wisdom;
    }

    [Serializable]
    public class InventorySaveData
    {
        public ItemInstanceData[] items;
        public int maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS;
        public int maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS;
    }

    /// <summary>
    /// ✅ FIXED: Added previousLocation for proper unequip behavior
    /// Item instance data for serialization
    /// location values: 0=Storage, 2=Bag, 3=Equipped
    /// previousLocation: -1=None/null, 0=Storage, 2=Bag
    /// </summary>
    [Serializable]
    public class ItemInstanceData
    {
        public string itemId;
        public int quantity;
        public int location;          // Current location
        public int previousLocation;  // ✅ NEW: Where item was before equipping (-1 = none)
    }

    [Serializable]
    public class EquipmentSaveData
    {
        public string weaponId;
        public string helmetId;
        public string chestId;
        public string glovesId;
        public string bootsId;
        public string accessory1Id;
        public string accessory2Id;
    }

    [Serializable]
    public class SkillLoadoutSaveData
    {
        public string normalSkill1Id;
        public string normalSkill2Id;
        public string ultimateSkillId;
    }
}