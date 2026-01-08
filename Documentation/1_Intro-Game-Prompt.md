This is a portrait-mode, mobile-first 2D turn-based RPG built in Unity (C#).
Combat is 1 vs multiple enemies per room (5–20). Turn order is determined by
Attack Speed. Skills have per-turn cooldowns and define their maximum number
of selectable targets.

The skill system is equipment-driven: equipped weapons determine which skills
can be used (e.g., sword weapons unlock sword-only skills). Skills unlock via
level and stat requirements rather than classes. Core stats (STR, AGI, END, WIS)
are fully respeccable. Gear has rarity tiers.

Theme is fantasy isekai. UI is tap-based (no drag-and-drop).

## Folder Structure

```
Scripts/
├── AppFlow/                                // High-level orchestration
│   ├── Ascension.Appflow.asmdef            // Future once all are solidified
│   ├── GameManager.cs
│   ├── PlayerStateController.cs
│   ├── SaveController.cs
│   └── SceneController.cs
│
├── Core/                                   // Core engine / bootstrap
│   ├── Ascension.Core.asmdef               // Future once all are solidified
│   ├── Bootstrap.cs
│   ├── SaveManager.cs
│   └── ServiceContainer.cs
│
├── Modules/
│   │
│   ├── CharacterSystem/
│   │   ├── Ascension.Character.asmdef      // Future once all are solidified
│   │   ├── Manager/
│   │   │   └── CharacterManager.cs
│   │   ├── Stats/
│   │   │   ├── CharacterStats.cs
│   │   │   ├── CharacterAttributes.cs
│   │   │   ├── CharacterItemStats.cs
│   │   │   └── CharacterDerivedStats.cs
│   │   ├── Runtime/
│   │   │   ├── CharacterCombatRuntime.cs
│   │   │   └── CharacterLevelSystem.cs
│   │   ├── Model/
│   │   │   └── CharacterData.cs
│   │   ├── Services/
│   │   │   ├── ItemQueryService.cs
│   │   │   ├── ItemStackingService.cs
│   │   │   └── ItemLocationService.cs
│   │   └── UI/
│   │       ├── PlayerHUD.cs
│   │       ├── PlayerPreviewUI.cs
│   │       ├── ProfilePanelManager.cs
│   │       ├── LevelUpManager.cs
│   │       └── CharacterCreationManager.cs
│   │
│   ├── InventorySystem/                    // ✅ PURE DATA MODULE (no UI, no scenes)
│   │   ├── Ascension.Inventory.asmdef      // Not implemented yet
│   │   │
│   │   ├── Config/
│   │   │   └── InventoryConfig.cs          // 
│   │   │
│   │   ├── Constant/
│   │   │   └── InventoryConstants.cs       // 
│   │   │
│   │   ├── Data/
│   │   │   ├── InventoryCore.cs            // The actual data container
│   │   │   ├── InventoryCoreData.cs        // For save/load
│   │   │   ├── InventoryResult.cs          // 
│   │   │   └── ItemInstance.cs             // Individual item
│   │   │
│   │   ├── Enums/
│   │   │   ├── InventoryEnums.cs           // ItemLocation enum
│   │   │   └── ItemLocationExtensions
│   │   │
│   │   ├── Manager/
│   │   │   ├── InventoryManager.cs         // Singleton, IGameService
│   │   │   └── SlotCapacityManager.cs      // Manages slot limits
│   │   │
│   │   ├── Services/
│   │   │   ├── ItemQueryService.cs         // Get items by location
│   │   │   ├── ItemStackingService.cs      // Stack merge/split
│   │   │   └── ItemLocationService.cs      // Move items between locations
│   │   │
│   │   └── UI/
│   │       └── ItemSlotUI.cs               // Reusable slot component
│   │
│   ├── StorageUI/                       // ✅ STORAGE ROOM UI MODULE
│   │   ├── Ascension.Storage.asmdef        // References: Inventory, SharedUI, Data
│   │   │
│   │   ├── Controller/
│   │   │   └── StorageRoomController.cs    // Main storage room scene controller
│   │   │
│   │   ├── Enums/
│   │   │   └── StorageEnums.cs             // StorageFilterType (if needed)
│   │   │
│   │   ├── Popup/
│   │   │   ├── InventoryItemPopup.cs       // For stackable items (materials, misc)
│   │   │   └── InventoryPotionPopup.cs     // For potions
│   │   │
│   │   └── UI/
│   │       ├── BagInventoryUI.cs           // Displays bag (12 slots)
│   │       ├── EquippedGearPreviewUI.cs       // 
│   │       ├── EquippedGearSlotUI.cs       // Reusable slot component
│   │       ├── StorageInventoryUI.cs       // Displays storage (60 slots)
│   │       └── StoragePopupContext.cs      // Context provider for GearPopup
│   │
│   ├── EquipmentSystem/                     // ✅ Complete Refactor to a module
│   │   ├── Ascension.Equipment.asmdef      // References: Inventory, Character, SharedUI
│   │   │
│   │   ├──Coordinators/
│   │   │   ├── GearEquipCoordinator.cs       // Equipment ↔ Inventory
│   │   │
│   │   ├── Data/
│   │   │   ├── EquipmentTransaction.cs
│   │   │   ├── EquippedGear.cs             // 7 equipped slots
│   │   │   └── SkillLoadout.cs             // 3 skill slots
│   │   │
│   │   ├── Enums/                          // im not sure if this is still usable
│   │   │   ├── EquipmentEnums.cs           // 
│   │   │   └── GearSlotTypeExtensions.cs          // 
│   │   │
│   │   ├── Manager/
│   │   │   ├── EquipmentManager.cs         // Manages equipped gear
│   │   │   └── SkillLoadoutManager.cs      // Manages skill hotbar
│   │   │
│   │   └── Services/
│   │       ├── GearSlotService.cs          // Slot validation
│   │       └── GearStatsService.cs         // Calculate stats
│   │
│   └── SharedUI/                            // ✅ SHARED UI COMPONENTS
│       ├── Ascension.SharedUI.asmdef
│       │
│       ├── Popups/
│       │   ├── GearPopup.cs                // Shared gear/weapon popup
│       │
│       └── Components/
│           └── BuffLineUI.cs               // Shared buff display component
│    
├── UI/                                     // UI module
│   ├── Ascension.UI.asmdef
│   ├── Core/
│   │   └── UIManager.cs
│   └── Panel/
│       └── DisclaimerController.cs
│
├── Data/                                   // Pure data / scriptable objects
    ├── Ascension.Data.asmdef
    ├── Enums/
    │   └── WeaponEnums.cs
    ├── Save/
    │   └── SaveData.cs
    └── ScriptableObject/
        ├── Item/
        │   ├── ItemBaseSO.cs
        │   ├── WeaponSO.cs
        │   ├── WeaponRaritySO.cs
        │   ├── GearSO.cs
        │   ├── PotionSO.cs
        │   ├── MaterialSO.cs
        │   └── AbilitySO.cs
        ├── Character/
        │   └── CharacterBaseStatsSO.cs
        └── Database/
            └── GameDatabaseSO.cs

---