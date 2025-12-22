// ════════════════════════════════════════════
// Assets/Scripts/Debug/QuickInventoryTest.cs
// ✅ FIXED: Removed duplicate Tooltip attributes
// Quick hotkey-based testing for InventoryManager
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Inventory.Manager;

/// <summary>
/// Quick hotkey-based testing for InventoryManager
/// Works from any scene after Bootstrap loads
/// 
/// HOTKEYS:
/// F1 - Test Storage Overflow
/// F2 - Check Capacity
/// F3 - Print Inventory
/// F4 - Add Test Items
/// F5 - Clear All
/// F6 - Upgrade Storage
/// </summary>
public class QuickInventoryTest : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool showGUI = true;
    [SerializeField] private bool persistAcrossScenes = true;

    private void Start()
    {
        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        Debug.Log("═══════════════════════════════════════");
        Debug.Log("✅ QuickInventoryTest loaded!");
        Debug.Log("Press F1-F6 to test inventory features");
        Debug.Log("═══════════════════════════════════════");
    }

    private void Update()
    {
        // Early exit if InventoryManager not loaded yet
        if (InventoryManager.Instance == null)
        {
            return;
        }

        // F1 - Test Storage Overflow
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("🔥 [F1] Testing Storage Overflow...");
            InventoryManager.Instance.DebugTestStorageOverflow();
        }

        // F2 - Check Capacity
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("📊 [F2] Checking Storage Capacity...");
            InventoryManager.Instance.DebugCheckStorageCapacity();
        }

        // F3 - Print Inventory
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("📋 [F3] Printing Full Inventory...");
            InventoryManager.Instance.DebugPrintInventory();
        }

        // F4 - Add Test Items
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("➕ [F4] Adding Test Items...");
            InventoryManager.Instance.DebugAddTestItems();
        }

        // F5 - Clear All
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("🗑️ [F5] Clearing All Items...");
            InventoryManager.Instance.DebugClearAll();
        }

        // F6 - Upgrade Storage
        if (Input.GetKeyDown(KeyCode.F6))
        {
            Debug.Log("⬆️ [F6] Upgrading Storage (+10 slots)...");
            InventoryManager.Instance.DebugUpgradeStorage();
        }
    }

    private void OnGUI()
    {
        if (!showGUI) return;

        if (InventoryManager.Instance == null)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "⚠️ Waiting for InventoryManager...");
            return;
        }

        // Show hotkey hints (top-left corner)
        string helpText = 
            "INVENTORY DEBUG:\n" +
            "F1 - Test Overflow\n" +
            "F2 - Check Capacity\n" +
            "F3 - Print Inventory\n" +
            "F4 - Add Test Items\n" +
            "F5 - Clear All\n" +
            "F6 - Upgrade Storage";

        GUI.Box(new Rect(10, 10, 180, 140), helpText);

        // Show current stats (top-left, below hotkeys)
        var inv = InventoryManager.Instance.Inventory;
        string stats = 
            $"Storage: {inv.GetStorageItemCount()}/{inv.maxStorageSlots}\n" +
            $"Bag: {inv.GetBagItemCount()}/{inv.maxBagSlots}\n" +
            $"Pocket: {inv.GetPocketItemCount()}/{inv.maxPocketSlots}";

        GUI.Box(new Rect(10, 160, 180, 70), stats);
    }
}