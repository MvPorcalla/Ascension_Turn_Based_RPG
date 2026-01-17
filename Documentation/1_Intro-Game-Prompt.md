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
├── App/                                    // High-level orchestration
│   ├── Commands/                              ✅ NEW FOLDER
│   │   ├── ICommand.cs                        ✅ Interface
│   │   ├── SaveGameCommand.cs                 ✅ Save validation
│   │   ├── LoadGameCommand.cs                 ✅ Load validation
│   │   ├── EquipItemCommand.cs                ✅ Equipment + undo support
│   │
│   ├── GameManager.cs                         ✅ REFACTORED
│   └── SceneFlowManager.cs                         ✅ REFACTORED
│
├── Core/                                   // Core engine / bootstrap
│   ├── Bootstrap.cs                           ✅ REFACTORED
│   ├── GameEvents.cs                          ✅ NEW
│   ├── IGameService.cs                        ✅ EXISTS
│   ├── SaveManager.cs                         ✅ REFACTORED
│   └── ServiceContainer.cs                    ✅ UPDATED (removed old controllers)
│
│
├── Data/                                   // Pure data / scriptable objects
│   ├── Config/
│   │   ├── SceneConfig.cs
│   │   ├── ToastConfig.cs
│   │   └── PopupConfig.cs
│   ├── Enums/
│   │   └── WeaponEnums.cs
│   ├── Save/
│   │   └── SaveData.cs
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
├── Modules/                                    // ✅ PURE DATA LOGIC - NO UI
│   │
│   ├── CharacterSystem/
│   │   ├── Manager/
│   │   │   └── CharacterManager.cs            // ✅ Events: OnStatsChanged, OnHealthChanged
│   │   ├── Model/
│   │   │   └── CharacterData.cs
│   │   ├── Runtime/
│   │   │   ├── CharacterCombatRuntime.cs
│   │   │   └── CharacterLevelSystem.cs
│   │   └── Stats/
│   │       ├── AttributeType.cs
│   │       ├── CharacterAttributes.cs
│   │       ├── CharacterAttributes.cs
│   │       ├── CharacterItemStats.cs
│   │       └── CharacterStats.cs
│   │
│   ├── EquipmentSystem/
│   │   ├── Coordinators/
│   │   │   └── GearEquipCoordinator.cs
│   │   ├── Data/
│   │   │   ├── EquipmentTransaction.cs
│   │   │   ├── EquippedGear.cs
│   │   │   └── SkillLoadout.cs
│   │   ├── Enums/
│   │   │   ├── EquipmentEnums.cs
│   │   │   └── GearSlotTypeExtensions.cs
│   │   ├── Manager/
│   │   │   ├── EquipmentManager.cs           // ✅ Events: OnEquipmentChanged
│   │   │   └── SkillLoadoutManager.cs
│   │   └── Services/
│   │       ├── GearSlotService.cs
│   │       └── GearStatsService.cs
│   │
│   ├── InventorySystem/
│   │   ├── Config/
│   │   │   └── InventoryConfig.cs
│   │   ├── Constants/
│   │   │   └── InventoryConstants.cs
│   │   ├── Data/
│   │   │   ├── InventoryCore.cs              // ✅ Events: OnInventoryChanged, OnItemMoved
│   │   │   ├── InventoryCoreData.cs
│   │   │   ├── InventoryResult.cs
│   │   │   └── ItemInstance.cs
│   │   ├── Enums/
│   │   │   ├── InventoryEnums.cs
│   │   │   └── ItemLocationExtensions.cs
│   │   ├── Manager/
│   │   │   ├── InventoryManager.cs           // ✅ Events: OnInventoryLoaded
│   │   │   └── SlotCapacityManager.cs
│   │   └── Services/
│   │       ├── ItemQueryService.cs
│   │       ├── ItemStackingService.cs
│   │       └── ItemLocationService.cs
│   │
│   └── PotionSystem/                          // (Future) rename GameSystem to PotionSystem
│       └── Manager/
│           └── PotionManager.cs
│
├── Controllers/                                // ✅ PURE PRESENTATION - Scene-specific orchestrators
│   ├── PlayerInventoryPanelController.cs       // ✅ Configures persistent bag panel
│   ├── StorageRoomController.cs                // ✅ Configures StorageScene grids
│   ├── CharacterSheetController.cs             // ✅ Character stats screen
│   └── ShopController.cs                       // ✅ Future: Shop UI
│
├── UI/                                         // ✅ PURE PRESENTATION - SUBSCRIBES TO MODULES
│   ├── Components/                             // ✅ Reusable UI building blocks
│   │   ├── Inventory/
│   │   │   ├── InventoryGridUI.cs              // ✅ NEW: Replaces Bag/Storage/Equipped UIs
│   │   │   ├── ItemSlotUI.cs                   // ✅ KEEP: Individual slot display
│   │   │   ├── EquipmentSlotUI.cs              // ✅ NEW: Special slot for equipped gear
│   │   │   └── InventoryFilterBarUI.cs         // ✅ NEW: Optional filter buttons
│   │   │
│   │   ├── Character/
│   │   │   ├── PlayerHudUI.cs                  // ✅ RENAMED: from PlayerHUD.cs
│   │   │   ├── HealthBarUI.cs                  // ✅ NEW: Extracted from PlayerHUD
│   │   │   ├── ExpBarUI.cs                     // ✅ NEW: Extracted from PlayerHUD
│   │   │   └── StatDisplayUI.cs                // ✅ NEW: Reusable stat display
│   │   │
│   │   └── Shared/
│   │       ├── BuffLineUI.cs                   // ✅ MOVED from SharedUI
│   │
│   ├── Popups/                                 // ✅ Modal dialogs
│   │   ├── Handlers/
│   │   │   └── PopupActionHandler.cs           // ✅ Business logic executor
│   │   ├── GearPopup.cs
│   │   ├── ItemPopup.cs
│   │   ├── PotionPopup.cs
│   │   ├── PopupManager.cs                     // ✅ Central popup controller
│   │   └── PopupContext.cs                     // ✅ NEW: Extract from PopupManager.cs
│   │
│   │
│   └── Toast/                                  // ✅ Non-blocking notifications
│       ├── ToastManager.cs
│       └── ToastNotification.cs
│
├── Utilities/                                  // ✅ NEW: Helper scripts
│   ├── UIHelpers.cs
│   └── ColorUtility.cs
│
├── DisclaimerController.cs                     // ✅ TODO: Move to UI/Controllers/
├── ProfilePanelManager.cs                      // ✅ TODO: Move to UI/Controllers/