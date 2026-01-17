# MainbaseLayout (new layout)

Canvas
├── BackgroundLayer (empty GameObject)
│    └── MainBackground (Image)
│
├── HUDLayer (empty GameObject)
│    └── PlayerStatusHeader
│        ├── Background
│        │
│        ├── PlayerIdentity
│        │   ├── AvatarImage
│        │   ├── PlayerNameText
│        │   └── PlayerLevelText
│        │
│        ├── HealthBar
│        │   ├── Background
│        │   ├── Fill
│        │   └── PercentageText
│        │
│        ├── ExpBar
│        │   ├── Background
│        │   ├── Fill
│        │   └── ExpCapText
│        │
│        └── WorldStatusRow
│            ├── LocationText
│            ├── GameTimeText
│            └── StatusEffectContainer
│                └── StatusEffectIcon
│ 
├── GlobalMenuLayer       (should i name this menu or navigation)
│    └── MenuGrid
│          ├── WorldMapButton
│          ├── ProfileButton
│          ├── InventoryButton
│          ├── QuestButton
│          └── CodexButton
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
│ 
├── Cooking Panel (Future)
│ 
├── Potion brewing Panel (Future)
│ 
├── Crafting Panel (Future)
│
├── more Panel in the future
│ 
├── MenuPanelsLayer (empty GameObject)
│     ├── ProfilePanel (Full screen)
│     │     ├── ProfileHeader
│     │     ├── PlayerInfoSection
│     │     ├── PlayerPreview
│     │     └── AttributeStatsSection
│     
├── InventoryPanel (Prefab) (prssing her in this panel outside the EquippedGearPreview and BagInventoryUI should close the bag) (only Bag  and equiped Gear display)
│    ├── EquippedGearPreview
│        ├── PreviewContent
│        │    ├── GPS_Weapon
│        │    │    ├── Background   ← Image
│        │    │    ├── Icon         ← Image
│        │    │    ├── EmptyOverlay ← Image or GO
│        │    │    └── Label        ← TMP_Text (optional)
│        │    ├── GPS_Helmet
│        │    ├── GPS_Chest
│        │    ├── GPS_Gloves
│        │    ├── GPS_Boots
│        │    ├── GPS_Acc1
│        │    └── GPS_Acc2
│        └── BagInventoryUI
│             ├── Background
│             ├── BagHeader
│             │    └── Title
│             │    └── CloseButton
│             └── BagPanel
│                  └── BagViewport
│                       └── BagContent (GridLayoutGroup)
│                            ├── EmptySlot (Prefab)
│                            │    ├── Button (button)
│                            │    ├── ItemIcon (Image)
│                            │    ├── EquipedIndicator
│                            │    └── Quantity (TMP - max x999 after that new slot)
│                            ├── ...
│    
├── QuestPanel (Prefab) (fills screen)
│    ├── QuestHeader
│         ├── backButton
│         └── Title
│
├── CodexPanel (Prefab) (fills screen)
│    ├── CodexHeader
│         ├── backButton
│         └── Title


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
├── ToastManager (GameObject) [ToastManager.cs (Component)]
│   └── ToastContainer (Empty GameObject)
│        ├── ToastPrefab (ToastNotification.cs + CanvasGroup)
│             └── Panel (Image - Background)
│                 ├── Icon (Image - Optional)
│                 └── MessageText (TMP_Text)
│
└── OverlayLayer (empty GameObject)
      ├── FadeScreen
      ├── Tooltip
      └── SystemMessages