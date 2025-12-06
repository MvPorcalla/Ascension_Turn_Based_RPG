// ════════════════════════════════════════════
// SaveData.cs
// Serializable save data structure
// ════════════════════════════════════════════

using System;
using UnityEngine;

namespace Ascension.Data.Models
{
    [Serializable]
    public class SaveData
    {
        public SaveMetaData metaData;
        public PlayerData playerData;
        public BagInventoryData inventoryData;
        
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
    // SaveMetaData.cs
    // Metadata for save file tracking
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
}