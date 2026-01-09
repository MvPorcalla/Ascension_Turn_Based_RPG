Canvas (Screen Space - Overlay, 1920x1080)
│
├── Panel_BG (optional full-screen background)
│   └── Image component (background color or sprite)
│
└── ScreenAvatarCreation (fills screen)
     └── LayoutAvatarCreation (800x1000, anchored middle center)
          └── PanelAvatarCreation (image component)
               ├── HeaderGO
               │    └── Title_Attributes
               │
               ├── InputField_Name
               │    ├── TMP_InputField
               │    └── Placeholder ("Enter Name")
               │
               ├── Panel_Attributes (VerticalLayoutGroup)
               │    ├── Sub_HeaderGO
               │    │    └── Text_Title ( Attributes allocation )
               │    ├── Attribute_STR (HorizontalLayoutGroup)
               │    │    ├── Text_Attribut
               │    │    ├── Attribute_Value (TMP_Text)
               │    │    └── Attribute_Buttons (emptyGO)
               │    │         ├── Button_Minus ("-")
               │    │         │    └── Button component + TMP_Text
               │    │         └── Button_Plus ("+")
               │    │              └── Button component + TMP_Text
               │    │
               │    ├── Attribute_INT (same structure)
               │    ├── Attribute_AGI (same structure)
               │    ├── Attribute_END (same structure)
               │    ├── Attribute_WIS (same structure)
               │    ├── Spacer (10px)
               │    └── Points
               │        ├── PointsText (TMP_Text)
               │        └── PointsValue (TMP_Text)
               │    
               ├── Panel_CombatStats (VerticalLayoutGroup)
               │    ├── Header_ComabtStats
               │    │    └── Text_Title ( Combat stats )
               │    ├── Base_AD ("Attack Damage: 10")
               │    │    ├── Text_Label
               │    │    └── Text_Value (TMP_Text)
               │    ├── Base_AP ("Ability Power: 5")
               │    │    ├── Text_Label
               │    │    └── Text_Value (TMP_Text)
               │    ├── Base_CritDamage ("Crit Damage: 15%")
               │    │    ├── Text_Label
               │    │    └── Text_Value (TMP_Text)
               │    ├── Base_CritRate ("Crit Rate: 5%")
               │    │    ├── Text_Label
               │    │    └── Text_Value (TMP_Text)
               │    ├── Base_Lethality (" ")
               │    │    ├── Text_Label
               │    │    └── Text_Value (TMP_Text)
               │    ├── Base_PhysicalPen (" ")
               │    │    ├── Text_Label
               │    │    └── Text_Value (TMP_Text)
               │    ├── Base_MagicPen (" ")
               │    │    ├── Text_Label
               │    │    └── Text_Value (TMP_Text)
               │    ├── Base_HP ("HP: 100")
               │    │    ├── Text_Label
               │    │    └── Text_Value (TMP_Text)
               │    ├── Base_AR ("Armor: 5")
               │    │    ├── Text_Label
               │    │    └── Text_Value (TMP_Text)
               │    ├── Base_MR ("Magic Resist: 5")
               │    │    ├── Text_Label
               │    │    └── Text_Value (TMP_Text)
               │    ├── Base_Evasion ("Evasion: 2%")
               │    │    ├── Text_Label
               │    │    └── Text_Value (TMP_Text)
               │    └── Base_Tenacity ("Tenacity: 0%")
               │         ├── Text_Label
               │         └── Text_Value (TMP_Text)
               │
               └── Button_Confirm ("Confirm & Start")
                    └── Button component + TMP_Text
