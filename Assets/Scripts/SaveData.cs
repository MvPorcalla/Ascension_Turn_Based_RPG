// ──────────────────────────────────────────────────
// SaveData.cs
// Serializable save data structure for player and inventory
// ──────────────────────────────────────────────────

using System;

[Serializable]
public class SaveData
{
    // Metadata
    public SaveMetaData metaData;
    
    // Player information
    public PlayerData playerData;
    
    // Inventory (renamed from PlayerInventoryData)
    public BagInventoryData inventoryData;
    
    public SaveData()
    {
        metaData = new SaveMetaData();
    }
    
    /// <summary>
    /// Remove this later if not needed
    /// Create a new save from current game state
    /// NOTE: This is only called by SaveManager now
    /// </summary>
    public static SaveData CreateSave(PlayerStats playerStats, BagInventoryData inventoryData)
    {
        SaveData save = new SaveData
        {
            metaData = SaveMetaData.CreateNew(),
            playerData = PlayerData.FromPlayerStats(playerStats),
            inventoryData = inventoryData
        };
        
        return save;
    }
    
    /// <summary>
    /// Update metadata when saving
    /// </summary>
    public void UpdateMetaData()
    {
        metaData.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        metaData.saveCount++;
    }
}

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
        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        return new SaveMetaData
        {
            saveVersion = UnityEngine.Application.version,
            createdTime = now,
            lastSaveTime = now,
            totalPlayTimeSeconds = 0f,
            saveCount = 1
        };
    }
}