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
│     │     ├── PlayerPreview (prefab has a script already just ereuse it)
│     │     ├── GearSection
│     │     │    ├── GearHeader
│     │     │    │    ├── title
│     │     │    │
│     │     │    ├── GearContainer
│     │     │    │    ├── WeaponSlot
│     │     │    │    ├── HelmetSlot
│     │     │    │    ├── ChestPlateSlot
│     │     │    │    ├── GlovesSlot
│     │     │    │    ├── BootsSlot
│     │     │    │    ├── Accessory1Slot
│     │     │    │    ├── Accessory2Slot
│     │     │    │
│     │     │    ├── HotBarContainer
│     │     │         ├── NormalSkillSlot1
│     │     │         ├── NormalSkillSlot2
│     │     │         ├── UltimateSkillSlot
│     │     │         ├── Item1
│     │     │         ├── Item2
│     │     │         ├── Item3
│     │     │
│     │     ├── StorageSection (Gear: only show Weapon, Gear, and Potions) (Abilities: only show ability)
│     │           ├── StorageHeader
│     │           │    ├── background
│     │           │    ├── Title
│     │           │    ├── GearButton (switch the storage to Gear)
│     │           │    ├── AbilitiesButton (switch the storage to Abilities)
│     │           │    ├── SortSection
│     │           │         └── SortButtons
│     │           │              ├── GearSortButtons (sort for Gear)
│     │           │              │    ├── AllButton 
│     │           │              │    ├── WeaponsButton 
│     │           │              │    ├── HelmetsButton 
│     │           │              │    ├── ChestsButton 
│     │           │              │    ├── GlovesButton 
│     │           │              │    ├── BootsButton 
│     │           │              │    ├── AccessoriesButton 
│     │           │              │    └── PotionsButton
│     │           │              │
│     │           │              ├── AbilitiesSortButtons (sort for Abilities)
│     │           │
│     │           └── StoragePanel (Prefab) (sotage Gear / Abilities)
│     │                └── StorageViewport
│     │                     └── StorageContent (GridLayoutGroup)
│     │                          ├── ItemSlot (Button)
│     │                          │    ├── rarity (image)
│     │                          │    ├── Button
│     │                          │    ├── ItemIcon (Image)
│     │                          │    ├── EquipedIndicator
│     │                          │    └── Quantity (TMP - max x999 after that new slot)
│     │                          ├── ...
│     │
│     ├── StorageRoomPanel (Full screen)
│     │     ├── Roomheader
│     │     ├── BagInventorySection
│     │     ├── PocketSection
│     │     └── StorageSection
│     │

│ 
├── MenuPanelsLayer (empty GameObject)
│     ├── ProfilePanel (Full screen)
│     │     ├── ProfileHeader
│     │     ├── PlayerInfoSection
│     │     ├── PlayerPreview
│     │     └── AttributeStatsSection
│     


├── PopupLayer (empty GameObject)
│     ├── PotionPopup
│     ├── ItemPopup
│     ├── GearPopUp  (for itemts: weapons, gears(hetmet, armorplate, gloves, boots, accessory, skills, etc) thats is not stockable)
│     │    ├── PopupContainer
│     │         ├── itemName
│     │         ├── ItemImageContainer
│     │         │    └── ItemImage
│     │         ├── StatPanel
│     │         │    └── Viewport
│     │         │         └── Content
│     │         │              ├── ItemBonusStats (Prefab)
│     │         │                   ├── Text_Label
│     │         │                   └── Text_value
│     │         ├── EffectPanel
│     │         │    └── Viewport
│     │         │         └── Content
│     │         │              ├── ItemEffect (Prefab)
│     │         │                   └── Text
│     │         ├── ItemDescriptionPanel
│     │         │    └── DescriptionText
│     │         ├── CloseButton
│     │         │    ├── ButtonLabel (TMP)
│     │         ├── EquipButton
│     │              ├── ButtonLabel (TMP)
│
└── OverlayLayer (empty GameObject)
      ├── FadeScreen
      ├── Tooltip
      └── SystemMessages
