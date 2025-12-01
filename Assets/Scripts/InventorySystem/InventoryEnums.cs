// ──────────────────────────────────────────────────
// InventoryEnums.cs
// Enums for inventory system locations and haptic feedback
// ──────────────────────────────────────────────────

/// <summary>
/// Item location within the inventory system
/// </summary>
public enum ItemLocation
{
    Bag,        // Player's 12-slot bag (expandable with equipment)
    Pocket,     // Player's 6-slot quick access (consumables/materials/misc only)
    Storage     // Unlimited storage room
}


// Future use enums for DragNDrop and mobile haptics
/// <summary>
/// Haptic feedback types for mobile interactions
/// </summary>
public enum HapticType
{
    Selection,      // Light tap (button press, UI interaction)
    LightImpact,    // Light bump (drag start, pick up)
    Success,        // Success pattern (successful drop, completion)
    Warning         // Warning pattern (invalid action, error)
}