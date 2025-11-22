# MainbaseLayout

Canvas (Screen Space - Overlay, 1920x1080)
│
├── BackgroundLayer (optional full-screen background)
│   └── Image component (background color or sprite)
│
└── MainGameUI (fills screen)
│    └── PlayerHUD
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
│         │         ├── ButtonBox
│         │         ├── ButtonBox
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
│         ├── GameMenu
│              └── Gridpanel (GridLayout component)
│                   ├── WorldMapButton (Button)
│                   ├── ProfileButton (Button)
│                   ├── InventoryButton (Button)
│                   ├── QuestButton (Button)
│                   └── CodexButton (Button)
│
├── ProfilePanel    (fills screen)
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
│    ├── PlayerStatsSection
│    │    ├── Background
│    │    ├── CombatStats
│    │    │    ├── Header
│    │    │    │    └── TitleText
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
│    │    │    ├── Base_PhysicalPen
│    │    │    │    ├── Text_Label
│    │    │    │    └── PhysicalPen_value
│    │    │    ├── Base_MagicPen
│    │    │    │    ├── Text_Label
│    │    │    │    └── MagicPen_value
│    │    │    ├── Base_HP
│    │    │    │    ├── Text_Label
│    │    │    │    └── HP_value
│    │    │    ├── Base_Armor
│    │    │    │    ├── Text_Label
│    │    │    │    └── Armor_value
│    │    │    ├── Base_MagicResist
│    │    │    │    ├── Text_Label
│    │    │    │    └── MagicResist_value
│    │    │    ├── Base_Evasion
│    │    │    │    ├── Text_Label
│    │    │    │    └── Evasion_value
│    │    │    └── Base_Tenacity
│    │    │         ├── Text_Label
│    │    │         └── Tenacity_value
│    │    │
│    │    └── AttributeStats
│    │         ├── Header
│    │         │    └── TitleText
│    │         ├── Attribute_STR
│    │         │    ├── Attribute_Text
│    │         │    ├── Attribute_Str_Value
│    │         │    └── Attribute_Buttons
│    │         │         └── Str_Buttons_minus
│    │         │         └── Str_Buttons_plus
│    │         ├── Attribute_INT
│    │         │    ├── Attribute_Text
│    │         │    ├── Attribute_Int_Value
│    │         │    └── Attribute_Buttons
│    │         │         └── Int_Buttons_minus
│    │         │         └── Int_Buttons_plus
│    │         ├── Attribute_AGI
│    │         │    ├── Attribute_Text
│    │         │    ├── Attribute_Agi_Value
│    │         │    └── Attribute_Buttons
│    │         │         └── Agi_Buttons_minus
│    │         │         └── Agi_Buttons_plus
│    │         ├── Attribute_END
│    │         │    ├── Attribute_Text
│    │         │    ├── Attribute_End_Value
│    │         │    └── Attribute_Buttons
│    │         │         └── End_Buttons_minus
│    │         │         └── End_Buttons_plus
│    │         ├── Attribute_WIS
│    │         │    ├── Attribute_Text
│    │         │    ├── Attribute_Wis_Value
│    │         │    └── Attribute_Buttons
│    │         │         └── Wis_Buttons_minus
│    │         │         └── Wis_Buttons_plus
│    │         ├── Spacer
│    │         └── Attribute_Menu
│    │              ├── PointsText
│    │              ├── PointsValue
│    │              └── ConfirmButton
│ 
├── InventoryPanel  (fills screen)
│    ├── InventoryHeader
│         ├── backButton
│         └── Title
│
├── QuestPanel      (fills screen)
│    ├── QuestHeader
│         ├── backButton
│         └── Title
│
├── CodexPanel      (fills screen)
│    ├── CodexHeader
│         ├── backButton
│         └── Title
