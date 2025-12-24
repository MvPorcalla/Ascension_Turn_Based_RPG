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
├── Modules/                                // All gameplay modules
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
│   ├── GameSystem/                                 // Game-wide systems, optional cross-module logic, Will be reworked into CombatSystem later
│   │   ├── Ascension.GameSystem.asmdef
│   │   └── PotionManager.cs                        // Will be reworked later
│   │
│   ├── InventorySystem/
│   │   ├── Ascension.Inventory.asmdef      // Future once all are solidified
│   │   ├── Manager/
│   │   │   └── InventoryManager.cs
│   │   ├── Data/
│   │   │   ├── InventoryCore.cs             // rename to InventoryCore.cs
│   │   │   ├── InventoryCoreData.cs         // rename to InventoryCoreData.cs
│   │   │   └── ItemInstance.cs
│   │   ├── Enum/
│   │   │   └── InventoryEnums.cs
│   │   ├── Popup/
│   │   │   ├── InventoryPotionPopup.cs
│   │   │   └── InventoryItemPopup.cs
│   │   └── UI/
│   │       ├── BagInventoryUI.cs
│   │       ├── PocketInevtoryUI.cs
│   │       ├── StorageInventoryUI.cs
│   │       ├── StorageRoomController.cs
│   │       ├── StoragePopupContext.cs
│   │       ├── ItemSlotUI.cs
│   │       └── BuffLineUI.cs
│   │
│   ├── EquipmentSystem/                     // In-progress module
│   │   ├── Manager/
│   │   │   ├── SkillLoadoutManager.cs
│   │   │   └── EquipmentManager.cs (IGameService)
│   │   │
│   │   ├── Data/
│   │   │   ├── EquippedGear.cs (Weapon, Helmet, Chest, etc.)
│   │   │   └── SkillLoadout.cs (Item1, Item2, Item3 references)
│   │   │
│   │   ├── Services/
│   │   │   ├── GearSlotService.cs (Slot validation, type checking)
│   │   │   ├── GearEquipService.cs (Equip/unequip logic)
│   │   │   └── GearStatsService.cs (Calculate total item stats)
│   │   │
│   │   ├── UI/
│   │   │   ├── EquipmentPopupContext.cs
│   │   │   ├── EquipmentRoomUI.cs (Main controller)
│   │   │   ├── GearSlotUI.cs (Individual gear slot display)
│   │   │   ├── SkillSlotUI.cs (Hotbar item slot)
│   │   │   └── EquipmentStorageUI.cs (Filtered storage view)
│   │   │
│   │   └── Enums/
│   │       └── EquipmentEnums.cs (GearSlotType, StorageFilter)
│   │
│   └── SharedUI/                          // NEW: Shared UI components
│       ├── Ascension.SharedUI.asmdef
│       ├── Popups/
│       │   ├── GearPopup.cs
│       │   ├── IGearPopupContext.cs
│       └── Components/
│           └── (future shared UI components)
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