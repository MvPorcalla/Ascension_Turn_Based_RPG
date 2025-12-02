# MainbaseLayout (old Layout)

Canvas (Screen Space - Overlay, 1920x1080)
│
├── BackgroundLayer (optional full-screen background)
│   └── Image component (background color or sprite)
│
└── MainGameUI (fills screen)
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
│         ├── StatusSection
│         │    ├── Location
│         │    ├── GameTime
│         │    └── StatusEffectsContainer
│         │         ├── StatusEffectBox (Prefab)
│         │              └── StatusEffectIcon
│         │
│         ├── MainBasePanel (Default always open here when first enter game)
│         │    └── Gridpanel (GridLayout component)
│         │         ├── EquipmentRoom (button)
│         │         ├── StorageRoom
│         │         ├── ButtonBox
│         │         ├── ...
│         │
│         ├── WorldMapPanel
│         │    └── Gridpanel (GridLayout component)
│         │         ├── ButtonBox
│         │         ├── ButtonBox
│         │         ├── ButtonBox
│         │         ├── ...
│         │         
│         ├── GameMenu (prefab)
│              └── Gridpanel (GridLayout component)
│                   ├── WorldMapButton (Button)
│                   ├── ProfileButton (Button)
│                   ├── InventoryButton (Button)
│                   ├── QuestButton (Button)
│                   └── CodexButton (Button)
│
├── EquipmentRoomPanel    (fills screen)
│    ├── Roomheader
│    │    ├── backButton
│    │    └── Title
│    ├── PlayerStatsection
│    │    ├── Background
│    │    ├── Header
│    │    │    ├── backButton
│    │    │    └── Title
│    │    │
│    │    ├── OffensiveStats
│    │    │    ├── Base_AD
│    │    │    │    ├── Text_Label
│    │    │    │    └── AD_value
│    │    │    ├── Base_AP
│    │    │    │    ├── Text_Label
│    │    │    │    └── AP_value
│    │    │    ├── Base_CritDamage
│    │    │    │    ├── Text_Label
│    │    │    │    └── CritDamage_value
│    │    │    ├── Base_CritRate
│    │    │    │    ├── Text_Label
│    │    │    │    └── CritRate_value
│    │    │    ├── Base_Lethality
│    │    │    │    ├── Text_Label
│    │    │    │    └── Lethality_value
│    │    │    ├── Base_Penetration
│    │    │    │    ├── Text_Label
│    │    │    │    └── Penetration_value
│    │    │
│    │    └── DefensiveStats
│    │         ├── Base_HP
│    │         │    ├── Text_Label
│    │         │    └── HP_value
│    │         ├── Base_Lifesteal
│    │         │    ├── Text_Label
│    │         │    └── Lifesteal_value
│    │         ├── Base_Defense
│    │         │    ├── Text_Label
│    │         │    └── Defense_value
│    │         ├── Base_Evasion
│    │         │    ├── Text_Label
│    │         │    └── Evasion_value
│    │         └── Base_Tenacity
│    │              ├── Text_Label
│    │              └── Tenacity_value
│    │
│    ├── GearSection (Prefab)
│    │    ├── HelmetSlot
│    │    ├── ChestPlateSlot
│    │    ├── GloveSlot
│    │    ├── BootsSlot
│    │    ├── AccessorySlot1
│    │    ├── AccessorySlot2
│    │    └── slot
│    │
│    ├── LoadoutSection (Prefab)
│    │    ├── Weaponslot
│    │    ├── NormalSkillSlot1
│    │    ├── NormalSkillSlot2
│    │    ├── UltimateSkillSlot
│    │    ├── item1 (quick slot for consumables)
│    │    ├── item2 (quick slot for consumables)
│    │    └── item3 (quick slot for consumables)
│    │     
│    ├── StorageSection (only show Weapon, Gear, Skills, and Potions)
│         ├── Background
│         ├── InventoryHeader
│         │    ├── background
│         │    └── Title
│         ├── SortSection
│         │    └── SortButtons
│         │         ├── ...
│         └── InventoryPanel
│              └── InventoryViewport
│                   └── InventoryContent (GridLayoutGroup)
│                        ├── ItemSlot (Button)
│                        │    ├── rarity (image)
│                        │    ├── Button
│                        │    ├── ItemIcon (Image)
│                        │    ├── EquipedIndicator
│                        │    └── Quantity (TMP - max x999 after that new slot)
│                        ├── ...
│
├── StorageRoomPanel    (fills screen)
│    ├── Roomheader
│    │    ├── backButton
│    │    └── Title
│    │
│    ├── BagInventorySection (Players Bag 12 Max slots (can be increase by equipable bag))
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
│    ├── PocketSection (Players Bag 12 Max slots (can be increase by equipable bag))
│    │    ├── Pocektheader
│    │    │    ├── Title
│    │    │    └── StoreAllButton
│    │    │         └── text (TMP)
│    │    └── PocketPanel
│    │         └── PocektViewport
│    │              └── PocketContent (GridLayoutGroup)
│    │                   ├── EmptySlot (Prefab)
│    │                   │    ├── Button (button)
│    │                   │    ├── ItemIcon (Image)
│    │                   │    ├── EquipedIndicator
│    │                   │    └── Quantity (TMP - max x999 after that new slot)
│    │                   ├── ...
│    │
│    └── StorageSection (All Items Player have including weapon, misc, potion, gear, materials, etc)
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
├── PotionPopup (for items: Potion only stockable)
│    ├── PopupContainer
│         ├── Header
│         │    ├── PotionName
│         │    ├── backButton
│         │         ├── ButtonLabel (TMP)
│         ├── ContentContainer
│               ├── PotionContainer
│               │    └── PotionIcon
│               ├── PotionInfo
│               │    ├── PotionType
│               │         ├── TextLabel
│               │         ├── textType (dynamic base on the potion type)
│               ├── BuffEffectPanel
│               │    ├── BuffType (Prefab)
│               │         ├── TextLabel
│               │         ├── textValue
│               │         ├── textDuration
│               ├── PotionDescription
│               │    ├── Background
│               │    ├── text
│               │ 
│               ├── QuantityPanel (for quantity)
│               │    ├── QuantityText
│               │    ├── QuantityContainer
│               │    │    ├── QuantityText
│               │    │    ├── QuantityValue
│               │    ├── Plus5_button
│               │    │    ├── text
│               │    ├── Plus1_button
│               │    │    ├── text
│               │    ├── Minu1_button
│               │    │    ├── text
│               │    ├── Minus5_button
│               │         ├── text
│               ├── QuantitySlider (for quantity)
│               ├── UselButton
│               │    ├── ButtonLabel (TMP)
│               ├── AddtoPocketButton
│               │    ├── ButtonLabel (TMP)
│               ├── AddtoBagButton
│                    ├── ButtonLabel (TMP)
│ 
├── ItemPopup (for items: misc, materials, ingridient, etc that is stockable)
│    ├── PopupContainer
│         ├── Header
│         │    ├── itemName
│         │    ├── backButton
│         │         ├── ButtonLabel (TMP)
│         ├── ContentContainer
│               ├── ItemContainer
│               │    └── ItemIcon
│               ├── ItemDescription
│               │    ├── Background
│               │    ├── text
│               ├── QuantityContainer
│               │    ├── QuantityText
│               │    ├── QuantityValue
│               │ 
│               ├── QuantityPanel (for quantity)
│               │    ├── QuantityText
│               │    ├── Plus5_button
│               │    │    ├── text
│               │    ├── Plus1_button
│               │    │    ├── text
│               │    ├── Minu1_button
│               │    │    ├── text
│               │    ├── Minus5_button
│               │         ├── text
│               ├── QuantitySlider (for quantity)
│               ├── AddtoPocketButton
│               │    ├── ButtonLabel (TMP)
│               ├── AddtoBagButton
│                    ├── ButtonLabel (TMP)
│
├── GearPopUp  (for itemts: weapons, gears(hetmet, armorplate, gloves, boots, accessory, skills, etc) thats is not stockable)
│    ├── PopupContainer
│         ├── itemName
│         ├── ItemImageContainer
│         │    └── ItemImage
│         ├── StatPanel
│         │    └── Viewport
│         │         └── Content
│         │              ├── ItemBonusStats (Prefab)
│         │                   ├── Text_Label
│         │                   └── Text_value
│         ├── EffectPanel
│         │    └── Viewport
│         │         └── Content
│         │              ├── ItemEffect (Prefab)
│         │                   └── Text
│         ├── ItemDescriptionPanel
│         │    └── DescriptionText
│         ├── CloseButton
│         │    ├── ButtonLabel (TMP)
│         ├── EquipButton
│              ├── ButtonLabel (TMP)
│

│
├── ProfilePanel (fills screen)
│    ├── ProfileHeader
│    │    ├── backButton
│    │    └── Title
│    │
│    ├── PlayerInfoSection
│    │    ├── Background
│    │    ├── PlayerProfile
│    │    ├── PlayerName
│    │    ├── PlayerLevel
│    │    └── GuildRank
│    │          ├── Text
│    │          └── Rank
│    ├── PlayerPreview  (Prefab)
│    │    ├── Background
│    │    ├── Header
│    │    │    ├── backButton
│    │    │    └── Title
│    │    │
│    │    ├── OffensiveStats
│    │    │    ├── Base_AD
│    │    │    │    ├── Text_Label
│    │    │    │    └── AD_value
│    │    │    ├── Base_AP
│    │    │    │    ├── Text_Label
│    │    │    │    └── AP_value
│    │    │    ├── Base_CritDamage
│    │    │    │    ├── Text_Label
│    │    │    │    └── CritDamage_value
│    │    │    ├── Base_CritRate
│    │    │    │    ├── Text_Label
│    │    │    │    └── CritRate_value
│    │    │    ├── Base_Lethality
│    │    │    │    ├── Text_Label
│    │    │    │    └── Lethality_value
│    │    │    ├── Base_Penetration
│    │    │    │    ├── Text_Label
│    │    │    │    └── Penetration_value
│    │    │
│    │    └── DefensiveStats
│    │         ├── Base_HP
│    │         │    ├── Text_Label
│    │         │    └── HP_value
│    │         ├── Base_Lifesteal
│    │         │    ├── Text_Label
│    │         │    └── Lifesteal_value
│    │         ├── Base_AttackSpeed
│    │         │    ├── Text_Label
│    │         │    └── AttackSpeed_value
│    │         ├── Base_Defense
│    │         │    ├── Text_Label
│    │         │    └── Defense_value
│    │         ├── Base_Evasion
│    │         │    ├── Text_Label
│    │         │    └── Evasion_value
│    │         └── Base_Tenacity
│    │              ├── Text_Label
│    │              └── Tenacity_value
│    │
│    └── AttributeStatsSection
│         ├── Header
│         │    └── TitleText
│         ├── Attribute_STR
│         │    ├── Attribute_Text
│         │    ├── Attribute_Str_Value
│         │    └── Attribute_Buttons
│         │         └── Str_Buttons_minus
│         │         └── Str_Buttons_plus
│         ├── Attribute_INT
│         │    ├── Attribute_Text
│         │    ├── Attribute_Int_Value
│         │    └── Attribute_Buttons
│         │         └── Int_Buttons_minus
│         │         └── Int_Buttons_plus
│         ├── Attribute_AGI
│         │    ├── Attribute_Text
│         │    ├── Attribute_Agi_Value
│         │    └── Attribute_Buttons
│         │         └── Agi_Buttons_minus
│         │         └── Agi_Buttons_plus
│         ├── Attribute_END
│         │    ├── Attribute_Text
│         │    ├── Attribute_End_Value
│         │    └── Attribute_Buttons
│         │         └── End_Buttons_minus
│         │         └── End_Buttons_plus
│         ├── Attribute_WIS
│         │    ├── Attribute_Text
│         │    ├── Attribute_Wis_Value
│         │    └── Attribute_Buttons
│         │         └── Wis_Buttons_minus
│         │         └── Wis_Buttons_plus
│         ├── Spacer
│         └── Attribute_Menu
│              ├── PointsText
│              ├── PointsValue
│              └── ConfirmButton



│ 
├── InventoryPanel (Prefab) (fills screen)
│    ├── InventoryHeader
│         ├── backButton
│         └── Title
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
