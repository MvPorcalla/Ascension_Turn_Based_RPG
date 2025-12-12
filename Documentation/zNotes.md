

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

Should I organize the `InventorySystem`, `CharacterSystem`, and `EquipmentSystem` folders inside a `Modules` folder for better project structure?

Should I create a separate `UI` folder for each module’s interface and have the UI reference its respective module, like having a central UI folder at the root, separate from the module folders, rather than putting a UI folder inside each module.


---

TODO: also other say my current GameSystemHUB is Inefficient and not robust

Currently, I’m using a Component-Based Architecture (CBA) with a Service Locator (SL) pattern. Would it make sense to switch to Dependency Injection (DI) or implement a hybrid approach combining SL and DI for better modularity and decoupling? Also, what naming convention do you recommend for system references and injected dependencies—for example, using _inventorySystem for private fields or InventorySystem for public properties?

## 5. Diagram


        ┌─────────────────────────────┐
        │  GameManager & SaveManager  │  ← Central Brain / Global Game State
        └──────────────┬──────────────┘
                       │
                       ▼
               ┌───────────────┐
               │ GameSystemHub │  ← Orchestrator / Coordinator (shouild i make mini hubs to split the work?)
               └───────┬───────┘
                       │
    ┌──────────────────┼───────────────────┐───────────────────┐ 
    │                  │                   │                   │
    ▼                  ▼                   ▼                   ▼
CharacterSystem  InventorySystem  CombatSystem               ETC...
   (.asmdef)        (.asmdef)      (.asmdef)

(currently using Service Locator (SL) which is not good they said, is Dependency Injection Hybrid with SL better?)

This diagram illustrates module orchestration and allowed dependencies.

---

**Cross-Module Communication Rule:**

All game modules (CharacterSystem, InventorySystem, CombatSystem, etc.) are **independent**. If a module needs functionality from another module:

1. It **requests the target system from `GameSystemHub`**.
2. It calls the required method on that system.
3. The result is returned to the requesting module.

**Example:**

* InventorySystem wants to use a potion to heal a player or apply a buff.
* InventorySystem **does not directly call** `PotionManager`.
* Instead, it asks `GameSystemHub` for `PotionManager` and calls `UsePotion(...)`.
* Any effect (healing, buff) is applied through `PotionManager` and the results flow back to InventorySystem.

This ensures **loose coupling**, **centralized orchestration**, and **easy swapping/testing** of modules without breaking dependencies.

---