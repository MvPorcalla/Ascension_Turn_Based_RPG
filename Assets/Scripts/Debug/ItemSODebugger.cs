// ════════════════════════════════════════════
// Assets\Scripts\Editor\ItemSODebugger.cs
// Editor-only debug utilities for ItemBaseSO testing
// ✅ UPDATED: Uses new AddToBag/AddToStorage API
// ════════════════════════════════════════════

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Ascension.Data.SO.Item;
using Ascension.Core;
using Ascension.Character.Manager;
using Ascension.Inventory.Manager;
using Ascension.GameSystem;

/// <summary>
/// Adds context menu items to ItemBaseSO assets in the Project window
/// This keeps ScriptableObjects pure while providing debug functionality
/// </summary>
public static class ItemSODebugger
{
    #region Context Menu - Add to Inventory
    
    [MenuItem("Assets/Debug Item/Add 5x to Bag", true)]
    private static bool ValidateAddToBag()
    {
        return Application.isPlaying && Selection.activeObject is ItemBaseSO;
    }
    
    [MenuItem("Assets/Debug Item/Add 5x to Bag")]
    private static void AddToBag()
    {
        ItemBaseSO item = Selection.activeObject as ItemBaseSO;
        if (item == null) return;
        
        InventoryManager invMgr = GetInventoryManager();
        if (invMgr == null) return;
        
        int quantity = item.IsStackable ? 5 : 1;
        
        // ✅ FIXED: Use new AddToBag() method
        var result = invMgr.AddToBag(item.ItemID, quantity);
        
        if (result.Success)
        {
            Debug.Log($"✅ Added {quantity}x {item.ItemName} to BAG");
        }
        else if (result.IsBagFull())
        {
            Debug.LogWarning($"⚠️ Bag is full! Could not add {item.ItemName}");
        }
        else
        {
            Debug.LogError($"❌ Failed to add item: {result.Message}");
        }
    }
    
    [MenuItem("Assets/Debug Item/Add 10x to Storage", true)]
    private static bool ValidateAddToStorage()
    {
        return Application.isPlaying && Selection.activeObject is ItemBaseSO;
    }
    
    [MenuItem("Assets/Debug Item/Add 10x to Storage")]
    private static void AddToStorage()
    {
        ItemBaseSO item = Selection.activeObject as ItemBaseSO;
        if (item == null) return;
        
        InventoryManager invMgr = GetInventoryManager();
        if (invMgr == null) return;
        
        int quantity = item.IsStackable ? 10 : 1;
        
        // ✅ FIXED: Use new AddToStorage() method
        var result = invMgr.AddToStorage(item.ItemID, quantity);
        
        if (result.Success)
        {
            Debug.Log($"✅ Added {quantity}x {item.ItemName} to STORAGE");
        }
        else if (result.IsStorageFull())
        {
            Debug.LogWarning($"⚠️ Storage is full! Could not add {item.ItemName}");
        }
        else
        {
            Debug.LogError($"❌ Failed to add item: {result.Message}");
        }
    }
    
    #endregion
    
    #region Context Menu - Potion Specific
    
    [MenuItem("Assets/Debug Item/Use Potion", true)]
    private static bool ValidateUsePotion()
    {
        return Application.isPlaying && Selection.activeObject is PotionSO;
    }
    
    [MenuItem("Assets/Debug Item/Use Potion")]
    private static void UsePotion()
    {
        PotionSO potion = Selection.activeObject as PotionSO;
        if (potion == null) return;
        
        PotionManager potionMgr = GetPotionManager();
        CharacterManager charMgr = GetCharacterManager();
        
        if (potionMgr == null || charMgr == null) return;
        
        if (!charMgr.HasActivePlayer)
        {
            Debug.LogWarning("⚠️ No active player to use potion on!");
            return;
        }
        
        bool success = potionMgr.UsePotion(potion, charMgr.CurrentPlayer, charMgr.BaseStats);
        
        if (success)
            Debug.Log($"✅ Used {potion.ItemName}");
        else
            Debug.LogWarning($"❌ Failed to use {potion.ItemName}");
    }
    
    #endregion
    
    #region Context Menu - Weapon Specific
    
    [MenuItem("Assets/Debug Item/Equip Weapon", true)]
    private static bool ValidateEquipWeapon()
    {
        return Application.isPlaying && Selection.activeObject is WeaponSO;
    }
    
    [MenuItem("Assets/Debug Item/Equip Weapon")]
    private static void EquipWeapon()
    {
        WeaponSO weapon = Selection.activeObject as WeaponSO;
        if (weapon == null) return;
        
        CharacterManager charMgr = GetCharacterManager();
        if (charMgr == null) return;
        
        if (!charMgr.HasActivePlayer)
        {
            Debug.LogWarning("⚠️ No active player to equip weapon on!");
            return;
        }
        
        charMgr.EquipWeapon(weapon);
        Debug.Log($"✅ Equipped {weapon.WeaponName}");
    }
    
    [MenuItem("Assets/Debug Item/Roll Weapon Stats", true)]
    private static bool ValidateRollStats()
    {
        return Selection.activeObject is WeaponSO;
    }
    
    [MenuItem("Assets/Debug Item/Roll Weapon Stats")]
    private static void RollWeaponStats()
    {
        WeaponSO weapon = Selection.activeObject as WeaponSO;
        if (weapon == null) return;
        
        weapon.RollBonusStats();
        EditorUtility.SetDirty(weapon);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"✅ Rolled bonus stats for {weapon.WeaponName}");
        Debug.Log(weapon.GetInfoText());
    }
    
    #endregion
    
    #region Context Menu - Gear Specific
    
    [MenuItem("Assets/Debug Item/Roll Gear Stats", true)]
    private static bool ValidateRollGearStats()
    {
        return Selection.activeObject is GearSO;
    }
    
    [MenuItem("Assets/Debug Item/Roll Gear Stats")]
    private static void RollGearStats()
    {
        GearSO gear = Selection.activeObject as GearSO;
        if (gear == null) return;
        
        gear.RollBonusStats();
        EditorUtility.SetDirty(gear);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"✅ Rolled bonus stats for {gear.GearName}");
        Debug.Log(gear.GetInfoText());
    }
    
    #endregion
    
    #region Context Menu - General Info
    
    [MenuItem("Assets/Debug Item/Print Item Info", true)]
    private static bool ValidatePrintInfo()
    {
        return Selection.activeObject is ItemBaseSO;
    }
    
    [MenuItem("Assets/Debug Item/Print Item Info")]
    private static void PrintInfo()
    {
        ItemBaseSO item = Selection.activeObject as ItemBaseSO;
        if (item == null) return;
        
        Debug.Log($"=== {item.ItemName} ===");
        Debug.Log($"ID: {item.ItemID}");
        Debug.Log($"Type: {item.ItemType}");
        Debug.Log($"Rarity: {item.Rarity}");
        Debug.Log($"Stackable: {item.IsStackable} (Max: {item.MaxStackSize})");
        Debug.Log($"\n{item.GetInfoText()}");
    }
    
    #endregion
    
    #region Helper Methods
    
    private static InventoryManager GetInventoryManager()
    {
        if (ServiceContainer.Instance == null)
        {
            Debug.LogError("❌ ServiceContainer not found! Make sure you're in Play Mode.");
            return null;
        }
        
        InventoryManager invMgr = ServiceContainer.Instance.Get<InventoryManager>();
        
        if (invMgr == null)
            Debug.LogError("❌ InventoryManager not found in ServiceContainer!");
        
        return invMgr;
    }
    
    private static CharacterManager GetCharacterManager()
    {
        if (ServiceContainer.Instance == null)
        {
            Debug.LogError("❌ ServiceContainer not found! Make sure you're in Play Mode.");
            return null;
        }
        
        CharacterManager charMgr = ServiceContainer.Instance.Get<CharacterManager>();
        
        if (charMgr == null)
            Debug.LogError("❌ CharacterManager not found in ServiceContainer!");
        
        return charMgr;
    }
    
    private static PotionManager GetPotionManager()
    {
        if (ServiceContainer.Instance == null)
        {
            Debug.LogError("❌ ServiceContainer not found! Make sure you're in Play Mode.");
            return null;
        }
        
        PotionManager potionMgr = ServiceContainer.Instance.Get<PotionManager>();
        
        if (potionMgr == null)
            Debug.LogError("❌ PotionManager not found in ServiceContainer!");
        
        return potionMgr;
    }
    
    #endregion
}
#endif