// -------------------------------
// SaveData.cs
// -------------------------------

using System;

/// <summary>
/// Root save data container.
/// Add more data sections here as your game grows
/// (inventory, quests, world state, etc.)
/// </summary>
[Serializable]
public class SaveData
{
    // Metadata
    public SaveMetaData metaData;
    
    // Player information
    public PlayerData playerData;
    
    // Future expansion:
    // public InventoryData inventoryData;
    // public QuestData questData;
    // public WorldStateData worldStateData;
    
    public SaveData()
    {
        metaData = new SaveMetaData();
    }
    
    /// <summary>
    /// Create a new save from current game state
    /// </summary>
    public static SaveData CreateSave(PlayerStats playerStats)
    {
        SaveData save = new SaveData
        {
            metaData = SaveMetaData.CreateNew(),
            playerData = PlayerData.FromPlayerStats(playerStats)
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

/// <summary>
/// Metadata about the save file itself
/// </summary>
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