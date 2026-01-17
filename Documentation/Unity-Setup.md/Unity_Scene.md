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

📁 01_Bootstrap.unity (Partially persists)
│
├── ServiceController (DontDestroyOnLoad)
│   ├── GameManager
│   ├── SceneFlowManager
│   ├── SaveManager
│   └── ... (all managers)
│
├── PersistentUICanvas (DontDestroyOnLoad) [PersistentUIController]
│   ├── EventSystem
│   ├── PopupManager        [PopupManager.cs]
│   ├── PopupActionHandler  [PopupActionHandler.cs]
│   ├── ToastManager        [ToastManager.cs]
│   ├── HUDLayer
│   │   ├── PlayerHUD (prefab) [PlayerHud.cs] ← ALWAYS VISIBLE
│   │   │   ├── Background
│   │   │   ├── PlayerInfo
│   │   │   │   ├── PlayerProfile → PlayerIMG
│   │   │   │   ├── PlayerName (TMP)
│   │   │   │   └── PlayerLevel (TMP)
│   │   │   ├── HealthBar [HealthBarUI.cs]
│   │   │   │   ├── Background
│   │   │   │   ├── Fill
│   │   │   │   └── Percentage (TMP)
│   │   │   └── ExpBar [ExpBarUI.cs]
│   │   │       ├── Background
│   │   │       ├── Fill
│   │   │       └── ExpCap (TMP)
│   │   │
│   │   └── GlobalMenu (prefab) [GlobalMenuController.cs]
│   │       └── MenuGrid (GridLayoutGroup)
│   │           ├── WorldMapButton
│   │           ├── ProfileButton
│   │           ├── InventoryButton
│   │           ├── QuestButton
│   │           └── CodexButton
│   │
│   ├── PopupLayer                              ← ALWAYS AVAILABLE
│   │   ├── PotionPopup
│   │   ├── ItemPopup
│   │   └── GearPopup
│   │
│   ├── ToastContainer                          ← ALWAYS AVAILABLE
│   │   └── (Toast prefabs spawn here)
│   │
│   └── OverlayLayer
│       ├── FadeScreen
│       ├── Tooltip
│       └── SystemMessages
│
└── Bootstrap (GameObject with Bootstrap.cs - gets destroyed)

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
│       └── MainBasePanel (room selection grid)

---------------------------------------------------------------------------

# UI Scene

📁 UI_Storage.unity (Load when entering WorldMap)
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

