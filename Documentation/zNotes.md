

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



------------------ Disclaimer and Boostrap --------------------

TODO: add a loader on the 00_Disclaimer and 01_Boostrap scene 

------------------ Disclaimer and Boostrap --------------------

TODO: later

Rename the [CreateAssetMenu] entries as follows:

Change the ones currently under Game to ItemSO/... (e.g., ItemSO/Weapon, ItemSO/Consumable, etc.).
Change the ones currently under RPG to BaseStats/CharacterStats.

separate the raritySO in its own menu

This will make the asset creation menu more organized and consistent.


========================================== TODO ==========================================

TODO: 

change the Gearslots and hotbarslots from image to buttons

im confuse cuz why is it say storage room panel in the fiesld and asking me to put my storageroompanel when in my equipmentroompanel i have build in storage that is a prefab so i can reuse it to switch between skills storage and gear storage

EquipmentRoomPanel.cs



issue on debug for roll bonus stats it dont display anything when used

--------------

add rarity to the potionSO

should i reuse the weaponraritySO.cs? to apply same multiplier here for potions?

--------------

TODO: do the equipmentRoom bext dont be a lazy ass bitch

--------------

Also, can I add a new category for growth-type weapons? For example, normal weapons follow the current logic, while new weapon type is growth-type weapons:

Always start as Common
Growth type Mechanics:
- Scale based on kills: The weapon’s rarity increases after a certain number of kills while the weapon is in equip so its the weapons counter. When the weapon is unequipped, it has no owner. The first time a player equips it, the weapon registers the player’s ID. From that point on, its rarity grows based on that player’s monster kills using that weapon.

- Scale based on player level: Similarly, the weapon’s rarity can increase when the player reaches certain level thresholds. It ties to the player upon first equip, and then tracks level-based growth for that player.

- Other growth-related effects can also be added do you have suggestion?.

Is this feasible to implement?


on weapon creation should i make a cap of 100 stats additional overall? for all common items? so all are baalnce?

what is ideal for base stats cap? for

- common = 
- rare = 
- epic = 
- legendary = 
- Mythic = 


---

TODO: Move the following UI scripts to their own folder for better separation:

- Assets\Scripts\Modules\CharacterSystem\UI\PlayerStatsPreviewUI.cs
- Assets\Scripts\Modules\CharacterSystem\UI\PlayerHUD.cs

New folder: Assets\Scripts\Modules\UI\

---

TODO: 

i removed IGameService at potionManager for now 


App Launch
 └─ 00_Consent
     └─ (if accepted)
         ↓
 └─ 01_Bootstrap
     ├─ Create ServiceContainer
     ├─ Register all core services
     ├─ Initialize all services (in order)
     ├─ Mark container READY
         ↓
 └─ 02_CharacterCreation
     ├─ GameManager already alive
     ├─ PlayerStateController ready
     └─ Create character
         ↓
 └─ 03_MainBase
     └─ Normal gameplay

---

Final Folder Structure

Scripts/Modules/EquipmentSystem/
├── Manager/
│   └── EquipmentManager.cs ✅
│
├── Data/
│   └── EquippedGear.cs ✅
│
├── Services/
│   ├── GearSlotService.cs ✅
│   ├── GearEquipService.cs ✅
│   └── GearStatsService.cs ✅
│
├── UI/
│   ├── EquipmentRoomUI.cs ✅
│   ├── GearSlotUI.cs ✅
│   └── EquipmentStorageUI.cs ✅
│
└── Enums/
    └── EquipmentEnums.cs ✅


## ✅ **Current Status: Almost Complete!**

Based on the refactoring we just did, here's your **actual** status:

### **Phase 1: Core Equipment (Gear Only)** ✅ COMPLETE

| File | Status | Notes |
|------|--------|-------|
| `EquipmentEnums.cs` | ✅ Done | Updated - removed consumable slots |
| `EquippedGear.cs` | ✅ Done | No changes needed |
| `GearSlotService.cs` | ✅ Done | Fixed - removed consumable filter |
| `GearEquipService.cs` | ✅ Done | No changes needed |
| `GearStatsService.cs` | ✅ Done | No changes needed |
| `EquipmentManager.cs` | ✅ Done | No changes needed |
| `EquipmentRoomUI.cs` | ✅ Done | Updated - removed consumable slots |
| `GearSlotUI.cs` | ✅ Done | No changes needed |
| `EquipmentStorageUI.cs` | ✅ Done | Fixed - removed potion popup |

---

### **Phase 1.2: Popup System** ✅ COMPLETE

| File | Status | Notes |
|------|--------|-------|
| `EquipmentGearPopup.cs` | ✅ Done | For weapons/gear |
| `EquipmentPotionPopup.cs` | ❌ Deleted | Removed (use inventory pocket instead) |

---

### **Phase 2: Skill Loadout System** ✅ COMPLETE (Renamed from Hotbar)

| File | Status | Notes |
|------|--------|-------|
| `SkillLoadout.cs` | ✅ Done | Renamed from `HotbarLoadout.cs` |
| `SkillLoadoutManager.cs` | ✅ Done | Renamed from `HotbarManager.cs` |
| `SkillSlotUI.cs` | ✅ Done | Renamed from `HotbarSlotUI.cs` |
| `SkillLoadoutSaveData.cs` | ✅ Done | Renamed from `HotbarSaveData.cs` |

**Save System Integration:** ✅ Done
- `SaveData.cs` - Updated
- `SaveManager.cs` - Updated
- `SaveController.cs` - Updated
- `ServiceContainer.cs` - Updated

---

### **Phase 3: Skills System (Future/Separate)** ⏳ TODO

| Component | Status | Notes |
|-----------|--------|-------|
| `SkillManager.cs` | ⏳ TODO | Not started yet |
| `SkillSaveData.cs` | ⏳ TODO | Not started yet |
| Weapon-type validation | ⏳ TODO | Not implemented yet |
| Skill assignment popup | ⏳ TODO | Placeholder in `EquipmentStorageUI` |

---

## 🎯 **What You Have NOW:**

✅ **Equipment System** - Players can equip weapons and gear
✅ **Skill Loadout System** - Players can assign 3 skills (2 normal + 1 ultimate)
✅ **Save/Load System** - Everything persists correctly
✅ **UI System** - Clean, mobile-friendly interface
✅ **No Redundancy** - Consumables managed via Inventory Pocket (not hotbar)

---

## 🚧 **What's LEFT (Phase 3):**

### **1. Skill Assignment Popup**
Currently when you click an ability in storage, it just logs a warning:
```csharp
Debug.LogWarning("[EquipmentStorageUI] Skill assignment popup not yet implemented");
```

**You need:** A popup similar to `EquipmentGearPopup` but for skills.

### **2. Weapon-Type Skill Validation**
Currently in `SkillLoadoutManager.cs`, there's a TODO:
```csharp
// TODO Phase 3: Validate weapon compatibility
// For now, allow any skill
```

**You need:** Logic to check if a skill matches the equipped weapon type.

Example:
- Sword equipped → Can only assign sword skills
- Staff equipped → Can only assign staff skills

### **3. Skill Data Management (Optional)**
Right now skills are just stored as IDs. You might want:
- Default skills for each weapon
- Skill unlock system
- Skill progression/leveling

---

## 📋 **My Recommendation:**

Your **core system is complete**! You can now:
1. ✅ Equip weapons and gear
2. ✅ Assign skills to loadout slots
3. ✅ Save and load everything
4. ✅ View stats and equipment

**For Phase 3, you should implement:**
1. **SkillAssignmentPopup** (similar to `EquipmentGearPopup`)
2. **Weapon-Type Validation** (add to `SkillLoadoutManager`)

---


===========================================================================================================

"Group related systems together: Character/Combat-related (Character, Equipment, Inventory), Social/Interaction (NPC, Intimacy), Mini-games (Blacksmith, Crafting, Brewing). Let systems within a group communicate directly. Use a scoped event bus only for communication between groups, keeping dependencies clean and modular."

----

CRITICAL: ========================================================================================


Also, in the Equipment Room, the accessory gear slots should allow equipping two accessories. They can be the same item or different items—it doesn’t matter, since there are two accessory slots.

---

Refactor my save JSON so that abilities are stored in their own abilities section, instead of inside items.

Example:

```json
"inventoryData": {
  "items": []
},
"abilities": {
}
```

Also, restructure inventoryData so that items are separated by category: weapons, armor (helmet, chestplate, gloves, boots), accessories, potions, and materials, instead of storing everything in a single items array.
Keep equipmentData and skillLoadoutData the same, and include sample item IDs and quantities in each category. Output the full JSON ready to use.

---
