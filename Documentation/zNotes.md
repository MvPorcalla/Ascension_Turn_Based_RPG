

------------------ STORAGE ROOOM --------------------

// TODO: Implement potion usage system

// TODO: Add Max cap on storage and can be increase by upgrading

// Example upgrade tiers
Tier 1: 50 slots (default)
Tier 2: 75 slots (+25) - Cost: 500 gold + 10 Iron Ore
Tier 3: 100 slots (+25) - Cost: 1500 gold + 20 Iron Ore + 5 Magic Crystals
Tier 4: 150 slots (+50) - Cost: 5000 gold + 50 Steel Ingots + 10 Magic Crystals
Tier 5: 200 slots (+50) - Cost: 15000 gold + 100 Mythril + 25 Rare Gems

// -------------------------------
// BagInventory.cs - Manages player's bag and storage
// -------------------------------
// TODO LIST - Future Features:
// [ ] Storage Upgrade System
//     - Add maxStorageSlots cap (default: 50, max: 200)
//     - Create StorageUpgradeTier ScriptableObject
//     - Implement UpgradeStorage() with material costs
//     - Add storage capacity UI display (X/Y slots)
//     - Create upgrade shop/menu UI
//     - Integrate with crafting material system
//     - Add visual feedback when storage is full
//     - Save/load storage capacity in BagInventoryData
// 
// [ ] Bag Upgrade System (equipment-based)
//     - Tie maxBagSlots to equipped backpack items
//     - Create backpack equipment ScriptableObjects
//
// [ ] Item Sorting/Filtering
//     - Sort by: Name, Rarity, Type, Quantity, Date Acquired
//     - Quick-stack all items of same type
//     - Auto-organize by category
// -------------------------------

After implementing this storage system, you might want to:

1. **Add Drag & Drop** - Drag items between bag/storage
2. **Add Search/Sort** - Search by name, sort by rarity
3. **Add Quick Actions** - "Take All Potions", etc.
4. **Add Item Comparison** - Compare equipped vs new item
5. **Add Equipment System** - Actually equip items from bag


----------------- Skill Pop Up -------------------

TODO: Make a Skill Pop Up UI for SkillsSO



------------------ Equipment ROOM --------------------

TODO: make a separate script called EWqupmentPopupUI.cs but reiusing the GearPopup Container



------------------ PlayerSystem --------------------

TODO: Fix

is the script of PlayerStatsPreviewUI.cs similar logic script with the AvatarCreationManager.cs?


Issue:

When I'm in avatar creation and I exit, it still saves. Why is that?
In the 02_AvatarCreation scene, it should only save once I confirm the avatar creation.

The current problem is that when I'm creating an avatar and I exit the game, it auto-saves. So when I return, it loads the save system with null data, and the 50 unallocated points that I haven't assigned yet are gone.

TODO: Next
create a full profile script along with my script PlayerStatsPreviewUI.cs