I’m making a simple, UI-driven game with turn-based combat, crafting minigames, an inventory system and bag system and NPC dialogue features and many more.

This is a portrait-mode, mobile-first 2D turn-based RPG built in Unity (C#).
Combat is 1 vs multiple enemies per room (5–20). Turn order is determined by
Attack Speed. Skills have per-turn cooldowns and define their maximum number
of selectable targets.

The skill system is equipment-driven: equipped weapons determine which skills
can be used (e.g., sword weapons unlock sword-only skills). Skills unlock via
level and stat requirements rather than classes. Core stats (STR, AGI, END, WIS)
are fully respeccable. Gear has rarity tiers.

Theme is fantasy isekai. UI is tap-based (no drag-and-drop).

- I’m using Unity’s additive scene loading to layer multiple scenes—like persistent UI and the game world—so they coexist without unloading each other.

---

## Folder Structure

Scripts/
│
├── Core/                                   ← Bootstrap + Core Systems
│   ├── GameBootstrap.cs                    ← Single initialization point
│   ├── GameEvents.cs                       ← Static event hub (KEEP!)
│   ├── SaveManager.cs                      ← Save/load logic
│   └── SceneFlowManager.cs                 ← Scene orchestration
│       
├── CharacterCreation/
│   ├── Data/
│   │   └── CharacterCreationData.cs           ← Pure data
│   ├── Manager/
│   │   └── CharacterCreationManager.cs        ← Business logic
│   └── UI/
│       └── CharacterCreationUI.cs             ← Presentation layer
│
├── Data/                                   // Pure data / scriptable objects
│   ├── Config/
│   │   ├── SceneConfig.cs
│   │   ├── ToastConfig.cs
│   │   └── PopupConfig.cs
│   ├── Enums/
│   │   └── WeaponEnums.cs
│   ├── Save/
│   │   ├── SaveData.cs                     ← ✅ Updated (DTOs only)
│   │   └── SaveDataExtensions.cs           ← ✅ NEW (conversion logic)
│   └── ScriptableObject/
│       ├── Character/
│       │   └── CharacterBaseStatsSO.cs
│       ├── Database/
│       │   └── GameDatabaseSO.cs
│       └── Items/
│           ├── ItemBaseSO.cs
│           ├── WeaponSO.cs
│           ├── WeaponRaritySO.cs
│           ├── GearSO.cs
│           ├── PotionSO.cs
│           ├── MaterialSO.cs
│           └── AbilitySO.cs
│
├── Modules
    ├── CharacterSystem/
    │   ├── Core/                              ← Core character data (ALL SERIALIZABLE)
    │   │   ├── CharacterStats.cs              ← ✅ Main container (runtime + save)
    │   │   ├── CharacterAttributes.cs         ← STR, INT, AGI, END, WIS
    │   │   ├── CharacterItemStats.cs          ← Equipment bonuses
    │   │   └── CharacterDerivedStats.cs       ← Calculated stats (AD, AP, HP)
    │   │
    │   ├── Runtime/                           ← Volatile state (SERIALIZABLE)
    │   │   ├── CharacterLevelSystem.cs        ← Leveling & EXP
    │   │   └── CharacterCombatRuntime.cs      ← Current HP, buffs, cooldowns
    │   │
    │   ├── Manager/                           ← Singleton manager
    │       └── CharacterManager.cs            ← Manages player instance
    │
    ├── EquipmentSystem/
    │   ├── Data/
    │   │   └── EquippedGear.cs              ← ✅ Pure data
    │   ├── Enums/
    │   │   └── EquipmentEnums.cs            ← ✅ Core enums
    │   ├── Extensions/
    │   │   └── GearSlotTypeExtensions.cs    ← ✅ UI helpers
    │   ├── Manager/
    │   │   └── EquipmentManager.cs          ← ✅ Simplified (no coordinator)
    │   └── Services/
    │       ├── GearSlotService.cs           ← ✅ Slot validation
    │       └── GearStatsService.cs          ← ✅ Stats calculation
    │
    ├── InventorySystem/
        │
        ├── Config/                                  ← Configuration
        │   └── InventoryConfig.cs                   ← Constants (slot capacities)
        │
        ├── Constants/                               ← Static Constants
        │   └── InventoryConstants.cs                ← Item ID prefixes, log tags
        │
        ├── Core/                                    ← Business Logic Layer
        │   └── InventoryCore.cs                     ← Core logic (triggers GameEvents)
        │
        ├── Data/                                    ← Data Models
        │   ├── ItemInstance.cs                      ← Item instance data
        │   ├── InventoryResult.cs                   ← Result pattern with error handling
        │   └── InventoryCoreData.cs                 ← Save/load serialization
        │
        ├── Enums/                                   ← Enumerations
        │   └── InventoryEnums.cs                    ← ItemLocation, InventoryErrorCode
        │
        ├── Manager/                                 ← Singleton manager
        │   └── InventoryManager.cs                  ← Public API (delegates to InventoryCore)
        │
        └── Services/                                ← Service Layer (Business Logic)
            ├── ItemQueryService.cs                  ← Read operations (queries)
            ├── ItemStackingService.cs               ← Stacking logic (merge/split)
            ├── ItemLocationService.cs               ← Movement operations
            └── SlotCapacityManager.cs               ← Capacity tracking

    ├── PotionSystem/          ← NEW FOLDER (pure logic, no UI)
        ├── Manager/
        │   └── PotionManager.cs
        ├── Data/
        │   ├── ActiveBuff.cs
        │   └── ActiveHealOverTurn.cs
        └── Services/           ← OPTIONAL (for future complexity)

    ├── SkillSystem/                        // Not Implemented yet will do in the future
        ├── Core/
        │   └── SkillCollection.cs          ← Like InventoryCore (owns all unlocked skills)
        │
        ├── Data/
        │   ├── SkillLoadout.cs             ← Pure data (current equipped skills)
        │   └── SkillInstance.cs            ← ✅ NEW (unlocked skill data)
        │
        ├── Manager/
        │   ├── SkillCollectionManager.cs   ← Like InventoryManager (unlock/query skills)
        │   └── SkillLoadoutManager.cs      ← Like EquipmentManager (equip/unequip only)
        │
        └── Services/
            ├── SkillQueryService.cs        ← Query unlocked skills
            └── SkillLoadoutService.cs      ← Validate loadout rules

├── UI/                                     ← All UI scripts
│   ├── Components/                         ← Reusable UI building blocks
│   │   ├── Inventory/
│   │   │   ├── PlayerHUDUI.cs
│   │   │   ├── PlayerStatsPreviewUI.cs
│   │   │   └── LevelUpPanelUI.cs
│   │   │   ├── HealthBarUI.cs                  // Future
│   │   │   └── ExpBarUI.cs                     // Future
│   │   ├── Inventory/
│   │   │   ├── EquipmentSlotUI.cs
│   │   │   ├── 
│   │   │   ├── InventoryFilterBarUI.cs
│   │   │   ├── InventoryGridUI.cs
│   │   │   └── ItemSlotUI.cs
│   │   └── Shared/
│   │       ├── BuffLineUI.cs
│   │       └── StatDisplayUI.cs
│   │       
│   ├── Controller/                             ← Note: Will be move to UI folder
│   │   ├── DisclamerController.cs              ←
│   │   ├── GlobalMenuController.cs             ← 
│   │   ├── MainBasePanelController.cs          ← 
│   │   ├── PersistentUIController.cs           ← Already been Refactored (I think)
│   │   ├── PlayerInventoryController.cs        ← 
│   │   └── StorageRoomController.cs            ←
│   │
│   ├── Popups/                             ← Modal dialogs
│   │   ├── PopupManager.cs
│   │   ├── PopupActionHandler.cs
│   │   ├── PopupContext.cs
│   │   ├── GearPopup.cs
│   │   ├── ItemPopup.cs
│   │   └── PotionPopup.cs
│   │
│   ├── ScreensPanels/                    ← Full-screen UI for GlobalMenu (scenes)
│   │   ├── ProfilePanelUI.cs
│   │   ├── PlayerBagUI.cs
│   │   └── WorldMapUI.cs
│   │
│   └── Toast/                      ← Notifications
│       ├── ToastManager.cs
│       └── ToastNotification.cs

# Note: This folder structure is from an older version of the project. Some scripts listed here no longer exist or have been refactored.
│
├── UI/                                     ← All UI scripts
│   ├── Components/                         ← Reusable UI building blocks
│   │   ├── Inventory/
│   │   │   ├── PlayerHUD.cs
│   │   │   ├── PlayerStatsPreviewUI.cs
│   │   │   └── LevelUpManager.cs
│   │   │   ├── HealthBarUI.cs                  // Future
│   │   │   └── ExpBarUI.cs                     // Future
│   │   ├── Inventory/
│   │   │   ├── InventoryGridUI.cs
│   │   │   ├── ItemSlotUI.cs
│   │   │   └── EquipmentSlotUI.cs
│   │   └── Shared/
│   │       ├── BuffLineUI.cs
│   │       └── StatDisplayUI.cs
│   │       
│   ├── Controller/                             ← Note: Will be move to UI folder
│   │   ├── DisclamerController.cs              ←
│   │   ├── GlobalMenuController.cs             ← 
│   │   ├── MainBasePanelController.cs          ← 
│   │   ├── PersistentUIController.cs           ← Already been Refactored (I think)
│   │   ├── PlayerInventoryController.cs        ← 
│   │   └── StorageRoomController.cs            ←
│   │
│   ├── Popups/                             ← Modal dialogs
│   │   ├── PopupManager.cs
│   │   ├── GearPopup.cs
│   │   ├── ItemPopup.cs
│   │   └── PotionPopup.cs
│   │
│   ├── ScreensPanels/                    ← Full-screen UI for GlobalMenu (scenes)
│   │   ├── ProfilePanelUI.cs
│   │   ├── PlayerBagUI.cs
│   │   └── WorldMapUI.cs
│   │
│   └── Toast/                      ← Notifications
│       ├── ToastManager.cs
│       └── ToastNotification.cs
│
├── Utilities/                      ← Helper scripts
│   ├── UIHelpers.cs
│   └── ColorUtility.cs
│
└── Enums/                          ← Shared enums (optional folder)
    ├── GearSlotType.cs
    ├── ItemLocation.cs
    └── AttributeType.cs


---

**Important RULE:**
* Before refactoring, reviewing, or suggesting changes to any code, always ask me if you need the full script or relevant context.
* Do **not** assume or invent code. Refactor only based on code I explicitly provide.
* The **Modules/System** folder should contain **only pure data logic** (no UI, scenes, or presentation code).
* All UI presentation logic should always go in the **UI** folder.