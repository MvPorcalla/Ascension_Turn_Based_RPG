// ════════════════════════════════════════════
// Assets\Scripts\Data\Save\SaveData.cs
// Save data structures for serialization
// ════════════════════════════════════════════

using System;
using Ascension.Inventory.Config;

namespace Ascension.Data.Save
{
    /// <summary>
    /// Root save data container
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public SaveMetaData metaData;
        public CharacterSaveData characterData;
        public InventorySaveData inventoryData;
        public EquipmentSaveData equipmentData;
        public SkillLoadoutSaveData skillLoadoutData;
    }

    // ════════════════════════════════════════════
    // SaveMetaData - Pure data, no logic
    // ════════════════════════════════════════════

    [Serializable]
    public class SaveMetaData
    {
        public string saveVersion;
        public string createdTime;
        public string lastSaveTime;
        public float totalPlayTimeSeconds;
        public int saveCount;
    }

    // ════════════════════════════════════════════
    // CharacterSaveData - Pure serializable data
    // ════════════════════════════════════════════

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

    // ════════════════════════════════════════════
    // InventorySaveData
    // ════════════════════════════════════════════

    [Serializable]
    public class InventorySaveData
    {
        public ItemInstanceData[] items;
        public int maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS;
        public int maxPocketSlots = InventoryConfig.DEFAULT_POCKET_SLOTS;
        public int maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS;
    }

    // ════════════════════════════════════════════
    // ItemInstanceData - ✅ MIGRATED to enum
    // ════════════════════════════════════════════

    /// <summary>
    /// ✅ MIGRATED: Uses int for ItemLocation enum serialization
    /// Values: 0=Storage, 1=Pocket, 2=Bag
    /// </summary>
    [Serializable]
    public class ItemInstanceData
    {
        public string itemId;
        public int quantity;
        
        // ✅ NEW: Single location field (serialized as int)
        public int location;  // 0=Storage, 1=Pocket, 2=Bag
        
        // ❌ REMOVED: Old boolean flags
        // public bool isInBag;
        // public bool isInPocket;
    }

    // ════════════════════════════════════════════
    // EquipmentSaveData
    // ════════════════════════════════════════════

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

    // ════════════════════════════════════════════
    // SkillLoadoutSaveData
    // ════════════════════════════════════════════

    [Serializable]
    public class SkillLoadoutSaveData
    {
        public string normalSkill1Id;
        public string normalSkill2Id;
        public string ultimateSkillId;
    }
}