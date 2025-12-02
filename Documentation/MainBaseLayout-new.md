# MainbaseLayout (new layout)

Canvas
├── BackgroundLayer (empty GameObject)
│     └── MainBackground (Image)
│
├── HUDLayer (empty GameObject)
│    └── PlayerHUD (prefab)
│         ├── PlayerInfo
│         │    ├── background (image)
│         │    ├── PlayerProfile
│         │    │    └── PlayerIMG
│         │    │
│         │    ├── PlayerName (TMP)
│         │    ├── PlayerLevel (TMP)
│         │    │
│         │    ├── HealthBar
│         │    │    ├── background
│         │    │    ├── Fill
│         │    │    └── Percentage (TMP)
│         │    │
│         │    ├── ExpBar
│         │         ├── background
│         │         ├── Fill
│         │         └── ExpCap (TMP)
│         │
│         ├── GameMenu (prefab)
│              └── Gridpanel (GridLayout component)
│                   ├── WorldMapButton (Button)
│                   ├── ProfileButton (Button)
│                   ├── InventoryButton (Button)
│                   ├── QuestButton (Button)
│                   └── CodexButton (Button)
│
├── MainPanelsLayer (empty GameObject)
│     ├── MainBasePanel
│     │     └── Gridpanel
│     │          ├── EquipmentRoom (Button)
│     │          ├── StorageRoom
│     │          ├── ButtonBox
│     │          └── ...
│     │
│     ├── WorldMapPanel
│     │     └── Gridpanel
│     │          ├── ButtonBox
│     │          ├── ButtonBox
│     │          ├── ButtonBox
│     │          └── ...
│     │
│     ├── EquipmentRoomPanel (Full screen)
│     │     ├── Roomheader
│     │     ├── PlayerStatsection
│     │     ├── GearSection
│     │     ├── LoadoutSection
│     │     └── StorageSection
│     │
│     ├── StorageRoomPanel (Full screen)
│     │     ├── Roomheader
│     │     ├── BagInventorySection
│     │     ├── PocketSection
│     │     └── StorageSection
│     │
│     ├── ProfilePanel (Full screen)
│           ├── ProfileHeader
│           ├── PlayerInfoSection
│           ├── PlayerPreview
│           └── AttributeStatsSection
│
├── PopupLayer (empty GameObject)
│     ├── PotionPopup
│     ├── ItemPopup
│     └── GearPopup
│
└── OverlayLayer (empty GameObject)
      ├── FadeScreen
      ├── Tooltip
      └── SystemMessages
