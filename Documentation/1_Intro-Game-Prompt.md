This is a portrait-mode, mobile-first 2D turn-based RPG built in Unity (C#).
Combat is 1 vs multiple enemies per room (5–20). Turn order is determined by
Attack Speed. Skills have per-turn cooldowns and define their maximum number
of selectable targets.

The skill system is equipment-driven: equipped weapons determine which skills
can be used (e.g., sword weapons unlock sword-only skills). Skills unlock via
level and stat requirements rather than classes. Core stats (STR, AGI, END, WIS)
are fully respeccable. Gear has rarity tiers.

Theme is fantasy isekai. UI is tap-based (no drag-and-drop).

---

Important: Before refactoring, reviewing, or giving suggestions about any code, always ask me first if you want to see the full script or relevant code context. Do not assume or guess the code—always request it before making changes.

---

## Folder Structure

```
Scripts/
├── AppFlow/                                // High-level orchestration
│   ├── GameManager.cs
│   ├── PlayerStateController.cs
│   ├── SaveController.cs
│   └── SceneController.cs
│
├── Core/                                   // Core engine / bootstrap
│   ├── Bootstrap.cs
│   ├── SaveManager.cs
│   └── ServiceContainer.cs
│
├── Data/                                   // Pure data / scriptable objects
│   ├── Enums/
│   │   └── WeaponEnums.cs
│   ├── Save/
│   │   └── SaveData.cs
│   └── ScriptableObject/
│       ├── Item/
│       │   ├── ItemBaseSO.cs
│       │   ├── WeaponSO.cs
│       │   ├── WeaponRaritySO.cs
│       │   ├── GearSO.cs
│       │   ├── PotionSO.cs
│       │   ├── MaterialSO.cs
│       │   └── AbilitySO.cs
│       ├── Character/
│       │   └── CharacterBaseStatsSO.cs
│       └── Database/
│           └── GameDatabaseSO.cs
│
├── Modules/
│   │
│   ├── CharacterSystem/
│   │   ├── Manager/
│   │   │   └── CharacterManager.cs
│   │   ├── Model/
│   │   │   └── CharacterData.cs
│   │   ├── Runtime/
│   │   │   ├── CharacterCombatRuntime.cs
│   │   │   └── CharacterLevelSystem.cs
│   │   ├── Stats/
│   │   │   ├── CharacterStats.cs
│   │   │   ├── CharacterAttributes.cs
│   │   │   ├── CharacterItemStats.cs
│   │   │   └── CharacterDerivedStats.cs
│   │   └── UI/
│   │       ├── CharacterCreationManager.cs
│   │       ├── LevelUpManager.cs
│   │       ├── PlayerHUD.cs
│   │       └── PlayerPreviewUI.cs 
│   │
│   ├── EquipmentSystem/                     // ✅ Complete Refactor to a module
│   │   ├──Coordinators/
│   │   │   └── GearEquipCoordinator.cs       // Equipment ↔ Inventory
│   │   ├── Data/
│   │   │   ├── EquipmentTransaction.cs
│   │   │   ├── EquippedGear.cs             // 7 equipped slots
│   │   │   └── SkillLoadout.cs             // 3 skill slots
│   │   ├── Enums/                          // im not sure if this is still usable
│   │   │   ├── EquipmentEnums.cs           // 
│   │   │   └── GearSlotTypeExtensions.cs   // 
│   │   ├── Manager/
│   │   │   ├── EquipmentManager.cs         // Manages equipped gear
│   │   │   └── SkillLoadoutManager.cs      // Manages skill hotbar
│   │   └── Services/
│   │       ├── GearSlotService.cs          // Slot validation
│   │       └── GearStatsService.cs         // Calculate stats
│   │
│   ├── InventorySystem/                    // ✅ PURE DATA MODULE (no UI, no scenes)
│   │   ├── Config/
│   │   │   └── InventoryConfig.cs          // 
│   │   ├── Constant/
│   │   │   └── InventoryConstants.cs       // 
│   │   ├── Data/
│   │   │   ├── InventoryCore.cs            // The actual data container
│   │   │   ├── InventoryCoreData.cs        // For save/load
│   │   │   ├── InventoryResult.cs          // 
│   │   │   └── ItemInstance.cs             // Individual item
│   │   ├── Enums/
│   │   │   ├── InventoryEnums.cs           // ItemLocation enum
│   │   │   └── ItemLocationExtensions
│   │   ├── Manager/
│   │   │   ├── InventoryManager.cs         // Singleton, IGameService
│   │   │   └── SlotCapacityManager.cs      // Manages slot limits
│   │   ├── Services/
│   │   │   ├── ItemQueryService.cs         // Get items by location
│   │   │   ├── ItemStackingService.cs      // Stack merge/split
│   │   │   └── ItemLocationService.cs      // Move items between locations
│   │   └── UI/
│   │       └── ItemSlotUI.cs               // Reusable slot component
│   │
│   ├── SharedUI/                           // ✅ SHARED UI COMPONENTS
│   │   ├── Popups/
│   │   │   ├── Config                      // 
│   │   │   │   └── PopupConfig.cs           // 
│   │   │   ├── GearPopup.cs                // Shared gear/weapon popup
│   │   │   ├── ItemPopup.cs                // For stackable items (materials, misc)
│   │   │   └── PotionPopup.cs              // For potions
│   │   └── Components/
│   │       └── BuffLineUI.cs               // Shared buff display component
│   │
│   └── StorageUI/                          // ✅ STORAGE ROOM UI MODULE (Purely UI)
│       ├── Controller/
│       │   └── StorageRoomController.cs    // Main storage room scene controller
│       ├── Enums/
│       │   └── StorageEnums.cs             // StorageFilterType (if needed)
│       └── UI/
│           ├── BagInventoryUI.cs           // Displays bag (12 slots)
│           ├── EquippedGearPreviewUI.cs    // 
│           ├── EquippedGearSlotUI.cs       // Reusable slot component
│           └── StorageInventoryUI.cs       // Displays storage (60 slots)
│
│ 
├── DisclaimerController.cs                 // undecided where to put
├── ProfilePanelManager.cs                  // undecided where to put
└── UIManager.cs                            // undecided where to put
---