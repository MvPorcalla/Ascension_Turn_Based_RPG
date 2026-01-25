JSON File
    ↓
CharacterManager (Single Source of Truth)
    ↓
    ├─→ PlayerHUD (listens to events)
    ├─→ PotionPopupUI (reads stats)
    ├─→ PotionManager (modifies stats, triggers events)
    ├─→ InventoryManager (reads stats)
    └─→ Any other system...

---

#  New Unity Scene

📁 00_Disclamer.unity
├── Main Camera
├── Canvas

📁 01_Bootstrap.unity
├── [GameBootstrap] ← SINGLE initialization object
│   ├── SaveManager
│   ├── CharacterManager
│   ├── InventoryManager
│   ├── EquipmentManager
│   ├── SkillLoadoutManager
│   └── SceneFlowManager
│
└── [PersistentUI] ← Canvas with all UI (DontDestroyOnLoad)
    ├── EventSystem
    ├── HUDLayer
    │   ├── PlayerHUD
    │   └── GlobalMenu
    ├── PopupLayer
    │   ├── PotionPopup
    │   ├── ItemPopup
    │   └── GearPopup
    ├── ToastLayer
    │   └── ToastManager
    └── OverlayLayer
        └── FadeScreen

📁 01_Bootstrap.unity
├── [GameBootstrap] ← Main GameObject with GameBootstrap.cs
│   ├── SaveManager (child GameObject)
│   ├── CharacterManager (child GameObject)
│   ├── InventoryManager (child GameObject)
│   ├── EquipmentManager (child GameObject)
│   ├── SkillLoadoutManager (child GameObject) ← Need to see script
│   ├── PotionManager (child GameObject) ← Need to see script
│   └── SceneFlowManager (child GameObject)
│
└── [PersistentUICanvas] ← Assigned to GameBootstrap's persistentUICanvas field
    ├── EventSystem
    ├── HUDLayer (GameObject)
    │   ├── PlayerHUD (GameObject) ← Need to confirm script
    │   └── GlobalMenu (GameObject) ← Need to confirm script
    └── OverlayLayer (GameObject)
        └── FadeScreen (GameObject)

📁 02_AvatarCreation.unity ← ✅ Avatar Creation
├── Main Camera
├── Canvas
│   ├── AvatarCreationPanel
│   │   ├── NameInputField
│   │   ├── AppearanceCustomizer
│   │   ├── AttributePointsUI
│   │   └── ConfirmButton

📁 03_Mainbase.unity (Persistent Scene)
├── Main Camera
├── Controller <-  [MainPanelController.cs]
├── Canvas
│   ├── BackgroundLayer
│   │   └── MainBackground
│   ├── MainPanelsLayer ← ✅ CORE NAVIGATION ()
│   │   └── MainBasePanel (room selection grid)
│   │       └── Gridpanel
│   │            ├── ButtonBox
│   │            ├── ButtonBox
│   │            ├── ButtonBox
│   │            └── ...

📁 UI_Storage.unity (Load when entering Storage Room)
└── Canvas
    ├── Background
    ├── PopupLayer (GameObject)
    │   ├── PopupManager (with PopupManager.cs)
    │   │
    │   ├── PotionPopup (GameObject) (PotionPopup.cs)
    │   ├── ItemPopup (GameObject) (ItemPopup.cs)
    │   └── GearPopup (GameObject) (GearPopup.cs)
    ├── ToastLayer (GameObject)
    │   └── ToastManager (GameObject) ← Need to confirm script
    │
    ├── StorageRoomPanel    (fills screen)
    │    ├── Roomheader
    │    │    ├── backButton
    │    │    └── Title
    │    │
    │    ├── BagInventoryUI (Players Bag 12 Max slots (can be increase by equipable bag))
    │    │    ├── Bagheader
    │    │    │    ├── Title
    │    │    │    └── StoreAllButton
    │    │    │         └── text (TMP)
    │    │    └── BagPanel
    │    │         └── BagViewport
    │    │              └── BagContent (GridLayoutGroup)
    │    │                   ├── EmptySlot (Prefab)
    │    │                   │    ├── Button (button)
    │    │                   │    ├── ItemIcon (Image)
    │    │                   │    ├── EquipedIndicator
    │    │                   │    └── Quantity (TMP - max x999 after that new slot)
    │    │                   ├── ...
    │    │ 
    │    ├── EquippedGearPreview   ← (EquippedGearPreviewUI)
    │    │     ├── PreviewHeader
    │    │     └── PreviewContent
    │    │         ├── GPS_Weapon
    │    │         │    ├── Background   ← Image
    │    │         │    ├── Icon         ← Image
    │    │         │    ├── EmptyOverlay ← Image or GO
    │    │         │    └── Label        ← TMP_Text (optional)
    │    │         ├── GPS_Helmet
    │    │         ├── GPS_Chest
    │    │         ├── GPS_Gloves
    │    │         ├── GPS_Boots
    │    │         ├── GPS_Acc1
    │    │         └── GPS_Acc2
    │    │
    │    └── StorageInventoryUI (All Items Player have including weapon, misc, potion, gear, materials, etc)
    │         ├── Storageheader
    │         │    ├── background
    │         │    └── Title
    │         ├── SortSection
    │              ├── SortButtons
    │         │         ├── AllItemButton
    │         │         ├── WeaponButton
    │         │         ├── GearButton
    │         │         ├── PotionButton
    │         │         ├── MaterialsButton
    │         │         ├── MiscButton
    │         └── StoragePanel
    │              └── StorageViewport
    │                   └── StorageContent (GridLayoutGroup)
    │                        ├── SitemSlot (Prefab)
    │                        │    ├── Button (button)
    │                        │    ├── ItemIcon (Image)
    │                        │    ├── EquipedIndicator
    │                        │    └── Quantity (TMP - max x999 after that new slot)
    │                        ├── ...

---------------------------------------------------------------------------

# UI Scene

📁 UI_Worldmap.unity (Load when entering WorldMap)
└── Canvas
    └── WorldMapPanel

📁 UI_Storage.unity (Load when entering Storage Room)
└── Canvas
    └── StorageRoomPanel ← MOVE HERE
        ├── RoomHeader
        ├── BagInventoryUI (12 slots)
        ├── EquippedGearPreview (7 slots)
        └── StorageInventoryUI (60 slots)

📁 UI_Inventory.unity (Load when pressing InventoryButton)
└── Canvas
    └── InventoryPanel ← MOVE HERE
        ├── EquippedGearPreview (7 slots)
        └── BagInventoryUI (12 slots)

📁 UI_Profile.unity (Load when pressing ProfileButton)
└── Canvas
    └── MenuPanelsLayer
        └── ProfilePanel ← MOVE HERE
            ├── ProfileHeader
            ├── PlayerInfoSection
            ├── PlayerPreview
            └── AttributeStatsSection

📁 UI_Quest.unity (Load when pressing QuestButton)
└── Canvas
    └── QuestPanel ← MOVE HERE

📁 UI_Codex.unity (Load when pressing CodexButton)
└── Canvas
    └── CodexPanel ← MOVE HERE

📁 UI_Cooking.unity (Future - Load when entering Cooking Room)
└── Canvas
    └── CookingPanel

📁 UI_Brewing.unity (Future - Load when entering Brewing Room)
└── Canvas
    └── BrewingPanel

📁 UI_Crafting.unity (Future - Load when entering Crafting Room)
└── Canvas
    └── CraftingPanel


#  Final Scene Structure

📁 Scenes/
├── 00_Disclamer.unity
├── 01_Bootstrap.unity              ← Singleton Managers only (never unloaded)
├── 02_AvatarCreation.unity
├── 03_Mainbase.unity               ← Home/hub (main navigation)
│
├── Gameplay/
│   ├── 05_Dungeon_Forest.unity
│   ├── 12_Combat.unity (Probably should be UI_ since comabt is UI driven card turnbase)
│   └── ...
│
└── UI/                             ← Additive UI scenes
    ├── UI_WorldMap.unity           ← button navigations for any scene
    ├── UI_City.unity   
    ├── UI_Town.unity   
    ├── UI_Storage.unity            ← 79 slots (only when needed!)
    ├── UI_Inventory.unity          ← 19 slots
    ├── UI_Profile.unity            ← Character stats
    ├── UI_Quest.unity              ← Quest log
    ├── UI_Codex.unity              ← Monster/item database
    ├── UI_Cooking.unity            ← Cooking minigame
    ├── UI_Brewing.unity            ← Potion brewing
    └── UI_Crafting.unity           ← Weapon/gear crafting

---

# Build Settings → Scenes In Build:
┌─────────────────────────────────┐
│ ☑ 0. 00_Disclaimer              │ ← First scene (index 0)
│ ☑ 1. 01_Bootstrap               │ ← Second scene (index 1)
│ ☑ 2. 02_AvatarCreation          │
│ ☑ 3. 03_MainBase                │
│ ☑ 4. UI_WorldMap                │
│ ☑ 5. UI_Profile                 │
│ ... (rest of scenes)            │
└─────────────────────────────────┘

---

# GAME FLOW:
┌────────────────────────────┐
│ 00_Disclaimer              │ ← First launch only
└─────────────┬──────────────┘
              │ Accept Terms → MarkDisclaimerAccepted()
              ↓
┌────────────────────────────┐
│ 01_Bootstrap               │ ← ALWAYS LOADED (DontDestroyOnLoad)
│ ├─ ServiceController       │
│ └─ PersistentUICanvas      │ ← DontDestroyOnLoad, header/footer, popups/toasts
└─────────────┬──────────────┘
              │
     ┌────────┴────────┐
     │                 │
 No Save?          Save Exists?
     │                 │
     ▼                 ▼
02_AvatarCreation     03_MainBase (Additive)
(Additive Scene)      (Additive Scene)
     │                 │
     ▼                 ▼
03_MainBase            UI_* Scenes
(Additive Scene)       (Loaded into MainBase content panel)
     │
     ▼
UI_* Scenes (Additive)
(Messenger, Gallery, Settings, etc.)

