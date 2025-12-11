// ════════════════════════════════════════════
// Assets\Scripts\Managers\Model\SaveData.cs
// Serializable save data structure
// ONLY references Data types - no Character/Inventory modules
// ════════════════════════════════════════════

using System;
using UnityEngine;

namespace Ascension.Manager.Model
{
    [Serializable]
    public class SaveData
    {
        public SaveMetaData metaData;
        public CharacterSaveData characterData;
        public InventorySaveData inventoryData;
        public EquipmentSaveData equipmentData;
        
        public SaveData()
        {
            metaData = new SaveMetaData();
        }
        
        public void UpdateMetaData()
        {
            metaData.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            metaData.saveCount++;
        }
    }

    // ════════════════════════════════════════════
    // SaveMetaData
    // ════════════════════════════════════════════

    [Serializable]
    public class SaveMetaData
    {
        public string saveVersion;
        public string createdTime;
        public string lastSaveTime;
        public float totalPlayTimeSeconds;
        public int saveCount;
        
        public static SaveMetaData CreateNew()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            return new SaveMetaData
            {
                saveVersion = Application.version,
                createdTime = timestamp,
                lastSaveTime = timestamp,
                totalPlayTimeSeconds = 0f,
                saveCount = 1
            };
        }
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
        
        // Base Attributes
        public int strength;
        public int dexterity;
        public int intelligence;
        public int vitality;
        public int luck;
    }

    // ════════════════════════════════════════════
    // InventorySaveData - Pure serializable data
    // ════════════════════════════════════════════

    [Serializable]
    public class InventorySaveData
    {
        public ItemInstanceData[] items;
        public int maxBagSlots;
    }

    [Serializable]
    public class ItemInstanceData
    {
        public string itemId;
        public int quantity;
    }

    // ════════════════════════════════════════════
    // EquipmentSaveData - Pure serializable data
    // ════════════════════════════════════════════

    [Serializable]
    public class EquipmentSaveData
    {
        public string weaponId;
        public string helmetId;
        public string chestId;
        public string glovesId;
        public string bootsId;
    }
}