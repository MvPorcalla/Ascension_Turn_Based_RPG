

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


===========================================================================================================

"Group related systems together: Character/Combat-related (Character, Equipment, Inventory), Social/Interaction (NPC, Intimacy), Mini-games (Blacksmith, Crafting, Brewing). Let systems within a group communicate directly. Use a scoped event bus only for communication between groups, keeping dependencies clean and modular."

----

CRITICAL: ========================================================================================

---

### prompt (only separate abilities)

> Refactor my save logic so that abilities are stored in their own `abilitiesData` section instead of being mixed into inventory items.
>
> Keep `inventoryData.items` as a single flat array (do NOT separate by category).
>
> Keep `equipmentData` and `skillLoadoutData` unchanged.
>
> Include sample item IDs, quantities, and locations.
>
> Output the full JSON ready to use.

---

POPUP LOGIC ============================================================

## **Future Shop Context (When You Build It)**

```csharp
// In PopupManager.cs (already in your code):
public static PopupContext FromShop()
{
    return new PopupContext
    {
        SourceLocation = ItemLocation.None,
        Source = PopupSource.Shop,
        CanEquip = false,  // Can't equip from shop
        CanMove = false,   // Can't move to bag directly
        CanUse = false,    // Can't use from shop
        CanSell = false    // Shop items show "Buy" button
    };
}
```
---

## **Visual Summary**

| **Location** | **Equip Button** | **Move Button** | **Use Button** (Potions) |
|-------------|------------------|-----------------|--------------------------|
| Storage     | ✅ "Equip"       | ✅ "Add to Bag" | ❌ Hidden                |
| Bag         | ✅ "Equip"       | ✅ "Store"      | ✅ "Use"                 |
| Equipped    | ✅ "Unequip"     | ❌ Hidden       | ❌ Hidden                |
| Shop (future) | ❌ Hidden      | ❌ Hidden       | ❌ Hidden                |

---


TODO: Move EquippedGearSlotUI.cs from storage UI to Inventory UI

---

TODO: Inevmtory Grid having the Max slots x - is this fine to keep or should not be editable and only can be change through the config of the Inventory system

Filter have this bug where the filter active color only show up after i press something in my storage ui


Open `SceneManifest` in Inspector and adjust:
```
03_AvatarCreation:
  ✅ Category: Content
  ✅ Show Player HUD: false
  ✅ Show Global Menu: false
  ✅ Allow Saving: false

12_Combat:
  ✅ Category: Content
  ✅ Show Player HUD: true
  ✅ Show Global Menu: false  ← Hide menu during combat
  ✅ Allow Saving: true



======

TODO: Later SceneManifest.cs and SceneMetadata.cs

public enum SceneCategory
{
    Core,           // Bootstrap, disclaimer, character creation
    Hub,            // MainBase (central navigation)
    GameplayUI,     // Storage, Inventory, Cooking, Brewing, Crafting
    MenuUI,         // Profile, Quest, Codex, WorldMap
    Combat,         // Combat scenes
    Exploration     // Future: Town, Dungeon, etc.
}
```

**Mapping:**
```
Core:
├── 00_Disclaimer
├── 01_Bootstrap
└── 02_AvatarCreation

Hub:
└── 03_MainBase

GameplayUI: (Room/Activity screens - accessed from MainBase)
├── UI_Storage
├── UI_Inventory
├── UI_Cooking
├── UI_Brewing
└── UI_Crafting

MenuUI: (Global navigation - accessible anywhere)
├── UI_Profile
├── UI_Quest
├── UI_Codex
└── UI_WorldMap

Combat:
└── 12_Combat

Exploration: (Future)
├── 05_Town
└── 06_Dungeon


=============================

📝 Quick Note: Main Menu Implementation (For Later)Scene Order:
00_Disclaimer → 01_Bootstrap → 02_MainMenu (press start game/newgame and Delete save button) → 03_AvatarCreation / 04_MainBaseWhy this structure:

02_MainMenu
Start/New Game Button - show new game if there  is no save and start gaem if there is
Delete Save - this will nuke the save (i think i already have this logic in my savemanager.cs) with lods of modal popup asking if are they sure for safety net.

=============================

**Important RULE:**
* Before refactoring, reviewing, or suggesting changes to any code, always ask me if you need the full script or relevant context.
* Do **not** assume or invent code. Refactor only based on code I explicitly provide.
* Generate code only. If you include a summary, keep it to 1 short paragraph max.

* The **Modules/System** folder should contain **only pure data logic** (no UI, scenes, or presentation code).
* All UI presentation logic should always go in the **UI** folder.
