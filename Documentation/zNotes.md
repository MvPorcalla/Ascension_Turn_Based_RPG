

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



Heirarchy
├── GameSystems (GameSystemHub.cs) (Prefab)
│   ├── GameManager
│   ├── CharacterManager
│   ├── SaveManager
│   ├── InventoryManager
│   ├── EquipmentManager
│   └── PotionManager
│
├── Bootstrap



DontDestroyOnLoad only works for root GameObjects or components on root GameObjects.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Ascension.Systems.PotionManager:Awake () (at Assets/Scripts/PotionManager.cs:46)

DontDestroyOnLoad only works for root GameObjects or components on root GameObjects.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Ascension.Managers.SaveManager:InitializeSingleton () (at Assets/Scripts/SaveManager.cs:154)
Ascension.Managers.SaveManager:Awake () (at Assets/Scripts/SaveManager.cs:51)

DontDestroyOnLoad only works for root GameObjects or components on root GameObjects.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Ascension.Managers.CharacterManager:InitializeSingleton () (at Assets/Scripts/CharacterManager.cs:283)
Ascension.Managers.CharacterManager:Awake () (at Assets/Scripts/CharacterManager.cs:46)

DontDestroyOnLoad only works for root GameObjects or components on root GameObjects.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
InventoryManager:Awake () (at Assets/Scripts/InventorySystem/InventoryManager.cs:32)

DontDestroyOnLoad only works for root GameObjects or components on root GameObjects.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Ascension.Managers.GameManager:Awake () (at Assets/Scripts/GameManager.cs:50)

[GameSystemHub] EquipmentManager missing
UnityEngine.Debug:LogWarning (object)
Ascension.Core.GameSystemHub:ValidateCriticalSystems () (at Assets/Scripts/GameSystemHub.cs:94)
Ascension.Core.GameSystemHub:Start () (at Assets/Scripts/GameSystemHub.cs:44)