01_Bootstrap.unity
│
├── 📦 [GameBootstrap] ← Empty GameObject
│   │
│   │ ✅ COMPONENT: GameBootstrap.cs
│   │    ├─ Inspector Settings:
│   │    │  ├─ Persistent UI Canvas: [Drag PersistentUICanvas GameObject here]
│   │    │  ├─ Minimum Load Time: 1
│   │    │  └─ Show Debug Logs: ✓
│   │
│   ├── 📄 SaveManager ← Empty GameObject (child of GameBootstrap)
│   │   │
│   │   └─ ✅ COMPONENT: SaveManager.cs
│   │       ├─ Pretty Print Json: ✓
│   │       ├─ Enable Auto Backup: ✓
│   │       ├─ Enable Debug Logs: ✓
│   │       ├─ Max Backup Count: 3
│   │       └─ Allow Graceful Degradation: ✓
│   │
│   ├── 📄 CharacterManager ← Empty GameObject (child of GameBootstrap)
│   │   │
│   │   └─ ✅ COMPONENT: CharacterManager.cs
│   │       └─ Base Stats: [Drag CharacterBaseStatsSO here]
│   │
│   ├── 📄 InventoryManager ← Empty GameObject (child of GameBootstrap)
│   │   │
│   │   └─ ✅ COMPONENT: InventoryManager.cs
│   │       └─ Database: [Drag GameDatabaseSO here]
│   │
│   ├── 📄 EquipmentManager ← Empty GameObject (child of GameBootstrap)
│   │   │
│   │   └─ ✅ COMPONENT: EquipmentManager.cs
│   │       └─ Database: [Drag GameDatabaseSO here]
│   │
│   ├── 📄 SkillLoadoutManager ← Empty GameObject (child of GameBootstrap)
│   │   │
│   │   └─ ✅ COMPONENT: SkillLoadoutManager.cs
│   │       (No serialized fields currently)
│   │
│   ├── 📄 PotionManager ← Empty GameObject (child of GameBootstrap)
│   │   │
│   │   └─ ✅ COMPONENT: PotionManager.cs
│   │       ├─ Is In Combat: ☐ (unchecked by default)
│   │       └─ Enable Debug Logs: ✓
│   │
│   └── 📄 SceneFlowManager ← Empty GameObject (child of GameBootstrap)
│       │
│       └─ ✅ COMPONENT: SceneFlowManager.cs
│           ├─ Scene Manifest: [Drag SceneManifest ScriptableObject here]
│           ├─ Use Additive For Main Scenes: ✓
│           └─ Enable Debug Logs: ✓
│
└── 🖼️ [PersistentUICanvas] ← Canvas GameObject
    │
    │ ✅ COMPONENTS:
    │    ├─ Canvas (Render Mode: Screen Space - Overlay)
    │    ├─ Canvas Scaler (UI Scale Mode: Scale With Screen Size, Reference: 1080x1920)
    │    ├─ Graphic Raycaster
    │    └─ PersistentUIController.cs
    │        ├─ HUD Layer: [Drag HUDLayer GameObject here]
    │        ├─ Player HUD: [Drag PlayerHUD GameObject here]
    │        ├─ Global Menu: [Drag GlobalMenu GameObject here]
    │        ├─ Popup Layer: [Drag PopupLayer GameObject here]
    │        ├─ Toast Manager: [Drag ToastManager GameObject here]
    │        ├─ Overlay Layer: [Drag OverlayLayer GameObject here]
    │        ├─ Scene Manifest: [Drag SceneManifest ScriptableObject here]
    │        └─ Show Debug Logs: ✓
    │
    ├── 🎮 EventSystem ← Create via: GameObject > UI > Event System
    │   │
    │   └─ ✅ COMPONENTS:
    │       ├─ EventSystem
    │       └─ Standalone Input Module
    │
    ├── 📁 HUDLayer ← Empty GameObject (child of PersistentUICanvas)
    │   │
    │   ├── 👤 PlayerHUD ← UI Panel GameObject
    │   │   │
    │   │   └─ ✅ COMPONENT: PlayerHUD.cs (or your HUD script)
    │   │       └─ [Setup your HUD UI elements as children]
    │   │
    │   └── 🔘 GlobalMenu ← UI Panel GameObject
    │       │
    │       └─ ✅ COMPONENT: GlobalMenuController.cs (or your menu script)
    │           └─ [Setup your 5 navigation buttons as children]
    │
    ├── 📁 PopupLayer ← Empty GameObject (child of PersistentUICanvas)
    │   │
    │   ├── 🧪 PotionPopup ← UI Panel GameObject (hidden by default)
    │   │   │
    │   │   └─ ✅ COMPONENT: PotionPopup.cs (your popup script)
    │   │
    │   ├── 📦 ItemPopup ← UI Panel GameObject (hidden by default)
    │   │   │
    │   │   └─ ✅ COMPONENT: ItemPopup.cs
    │   │
    │   └── ⚔️ GearPopup ← UI Panel GameObject (hidden by default)
    │       │
    │       └─ ✅ COMPONENT: GearPopup.cs
    │
    ├── 📁 ToastLayer ← Empty GameObject (child of PersistentUICanvas)
    │   │
    │   └── 📢 ToastManager ← Empty GameObject
    │       │
    │       └─ ✅ COMPONENT: ToastManager.cs
    │
    └── 📁 OverlayLayer ← Empty GameObject (child of PersistentUICanvas)
        │
        └── 🌑 FadeScreen ← UI Image (full screen, black, hidden by default)
            └─ ✅ COMPONENT: Image (Color: Black, Alpha: 0)

---

📁 02_AvatarCreation.unity
│
├── 📷 Main Camera
│
├── 🎮 [CharacterCreationManager] ← NEW: Separate manager GameObject
│   │
│   └─ ✅ COMPONENT: CharacterCreationManager.cs
│       ├─ Base Stats: [Drag CharacterBaseStatsSO here]
│       ├─ Min Name Length: 3
│       ├─ Max Name Length: 20
│       └─ Enable Debug Logs: ✓
│
└── 🖼️ Canvas (Screen Space - Overlay, 1080x1920)
    │
    │ ✅ COMPONENTS:
    │    ├─ Canvas (Render Mode: Screen Space - Overlay)
    │    ├─ Canvas Scaler (Scale With Screen Size, 1080x1920)
    │    └─ Graphic Raycaster
    │
    ├── 🎨 Panel_BG (OPTIONAL - full-screen background)
    │   │
    │   └─ ✅ COMPONENT: Image (background sprite/color)
    │
    └── 📱 ScreenAvatarCreation ← Fills screen
        │
        └── 📦 LayoutAvatarCreation (800x1000, anchored middle-center)
            │
            └── 🎨 PanelAvatarCreation ← Main container
                │
                │ ✅ COMPONENT: CharacterCreationUI.cs
                │    ├─ Creation Manager: [Drag CharacterCreationManager GameObject]
                │    ├─ Name Input: [Drag InputField_Name]
                │    ├─ STR Minus Btn: [Drag Attribute_STR/Buttons/Button_Minus]
                │    ├─ STR Plus Btn: [Drag Attribute_STR/Buttons/Button_Plus]
                │    ├─ STR Value Text: [Drag Attribute_STR/Attribute_Value]
                │    ├─ ... (repeat for INT, AGI, END, WIS)
                │    ├─ Points Value Text: [Drag Points/PointsValue]
                │    ├─ AD Value Text: [Drag Base_AD/Text_Value]
                │    ├─ ... (all combat stat text fields)
                │    ├─ Confirm Button: [Drag Button_Confirm]
                │    ├─ Reset Button: [If you have one]
                │    ├─ Error Message Text: [Create if missing]
                │    ├─ Loading Indicator: [Create if missing]
                │    ├─ Points Remaining Color: Yellow
                │    └─ All Points Spent Color: Green
                │
                ├── 📝 HeaderGO
                │   └── Title_Attributes (TMP_Text)
                │
                ├── ✏️ InputField_Name
                │   ├── TMP_InputField component
                │   └── Placeholder ("Enter Name")
                │
                ├── 📊 Panel_Attributes (VerticalLayoutGroup)
                │   ├── Sub_HeaderGO
                │   │   └── Text_Title ("Attribute Allocation")
                │   │
                │   ├── Attribute_STR (HorizontalLayoutGroup)
                │   │   ├── Text_Attribut ("STR")
                │   │   ├── Attribute_Value (TMP_Text) ← Shows current value
                │   │   └── Attribute_Buttons (empty GameObject)
                │   │       ├── Button_Minus ("-") + TMP_Text
                │   │       └── Button_Plus ("+") + TMP_Text
                │   │
                │   ├── Attribute_INT (same structure)
                │   ├── Attribute_AGI (same structure)
                │   ├── Attribute_END (same structure)
                │   ├── Attribute_WIS (same structure)
                │   │
                │   ├── Spacer (10px height)
                │   │
                │   └── Points (HorizontalLayoutGroup)
                │       ├── PointsText ("Points Remaining:")
                │       └── PointsValue (TMP_Text) ← Updates with remaining points
                │
                ├── 📈 Panel_CombatStats (VerticalLayoutGroup)
                │   ├── Header_CombatStats
                │   │   └── Text_Title ("Combat Stats Preview")
                │   │
                │   ├── Base_AD (HorizontalLayoutGroup)
                │   │   ├── Text_Label ("Attack Damage:")
                │   │   └── Text_Value (TMP_Text) ← Updates dynamically
                │   │
                │   ├── Base_AP (same structure)
                │   ├── Base_CritDamage (same structure)
                │   ├── Base_CritRate (same structure)
                │   ├── Base_Lethality (same structure)
                │   ├── Base_PhysicalPen (same structure)
                │   ├── Base_MagicPen (same structure)
                │   ├── Base_HP (same structure)
                │   ├── Base_AR (same structure)
                │   ├── Base_MR (same structure)
                │   ├── Base_Evasion (same structure)
                │   └── Base_Tenacity (same structure)
                │
                └── 🔘 Button_Confirm ("Confirm & Start")
                    └─ Button component + TMP_Text

---

📁 03_Mainbase.unity
│
├── 📷 Main Camera
│   │
│   └─ ✅ COMPONENTS:
│       ├─ Camera
│       ├─ Audio Listener
│       └─ (Any post-processing if needed)
│
└── 🖼️ Canvas (Screen Space - Overlay, 1080x1920)
    │
    │ ✅ COMPONENTS:
    │    ├─ Canvas (Render Mode: Screen Space - Overlay)
    │    ├─ Canvas Scaler (Scale With Screen Size, 1080x1920)
    │    └─ Graphic Raycaster
    │
    ├── 🎨 BackgroundLayer ← Empty GameObject
    │   │
    │   └── MainBackground ← UI Image (full screen)
    │       │
    │       └─ ✅ COMPONENT: Image
    │           ├─ Source Image: [Your home/castle background sprite]
    │           ├─ RectTransform: Stretch-Stretch (fills screen)
    │           └─ Color: White (or tint if needed)
    │
    └── 📱 MainPanelsLayer ← Empty GameObject
        │
        └── 🏠 MainBasePanel ← Panel/Image (main container)
            │
            │ ✅ COMPONENT: MainbasePanelController.cs
            │    ├─ Storage Room Button: [Drag Button_StorageRoom]
            │    ├─ Equipment Room Button: [Drag Button_Equipment]
            │    ├─ Cooking Room Button: [Drag Button_Cooking]
            │    ├─ Brewing Room Button: [Drag Button_Brewing]
            │    ├─ Crafting Room Button: [Drag Button_Crafting]
            │    ├─ Title Text: [Drag TitleText] (optional)
            │    └─ Enable Debug Logs: ✓
            │
            ├── 📝 TitleText ← OPTIONAL (TMP_Text - "Main Base")
            │   │
            │   └─ ✅ COMPONENT: TextMeshProUGUI
            │       ├─ Text: "Main Base"
            │       ├─ Font Size: 48
            │       ├─ Alignment: Top Center
            │       └─ Color: White or Gold
            │
            └── 📋 GridPanel ← Room button container
                │
                │ ✅ COMPONENTS:
                │    ├─ RectTransform (centered, appropriate size)
                │    └─ GridLayoutGroup
                │        ├─ Cell Size: 300x300 (or your preferred size)
                │        ├─ Spacing: 20x20
                │        ├─ Start Corner: Upper Left
                │        ├─ Start Axis: Horizontal
                │        ├─ Child Alignment: Middle Center
                │        └─ Constraint: Fixed Column Count (2 or 3 columns)
                │
                ├── 🔘 Button_StorageRoom
                │   │
                │   │ ✅ COMPONENT: Button
                │   │    ├─ Interactable: ✓ (enabled)
                │   │    ├─ Navigation: None (or as needed)
                │   │    └─ Transition: Color Tint
                │   │        ├─ Normal Color: White
                │   │        ├─ Highlighted Color: Light Yellow
                │   │        ├─ Pressed Color: Gray
                │   │        └─ Disabled Color: Dark Gray (50% alpha)
                │   │
                │   ├── Icon ← UI Image (chest/storage icon)
                │   │   └─ Image (sprite: storage icon)
                │   │
                │   └── Label ← TMP_Text
                │       └─ TextMeshProUGUI
                │           ├─ Text: "Storage Room"
                │           ├─ Font Size: 24
                │           ├─ Alignment: Bottom Center
                │           └─ Color: White
                │
                ├── 🔘 Button_Cooking
                │   │ (Same structure)
                │   │ Interactable: ☐ (disabled)
                │   │
                │   ├── Icon (cooking pot icon)
                │   └── Label ("Cooking")
                │
                ├── 🔘 Button_Brewing
                │   │ (Same structure)
                │   │ Interactable: ☐ (disabled)
                │   │
                │   ├── Icon (potion bottle icon)
                │   └── Label ("Brewing")
                │
                └── 🔘 Button_Crafting
                    │ (Same structure)
                    │ Interactable: ☐ (disabled)
                    │
                    ├── Icon (hammer/anvil icon)
                    └── Label ("Crafting")

---

📁 UI_Storage.unity
│
├── 📷 Main Camera
│   │
│   └─ ✅ COMPONENTS:
│       ├─ Camera
│       ├─ Audio Listener
│       └─ (Any post-processing if needed)
│
└── 🖼️ Canvas (Screen Space - Overlay, 1080x1920)
    │
    │ ✅ COMPONENTS:
    │    ├─ Canvas (Render Mode: Screen Space - Overlay)
    │    ├─ Canvas Scaler (Scale With Screen Size, 1080x1920, Match: 0.5)
    │    └─ Graphic Raycaster
    │
    ├── 🎨 BackgroundLayer ← Empty GameObject
    │   │
    │   └── Background ← UI Image
    │       │
    │       └─ ✅ COMPONENT: Image
    │           ├─ Color: Black (0, 0, 0, 200)
    │           ├─ RectTransform: Stretch-Stretch
    │           └─ Raycast Target: ✓
    │
    └── 📱 StorageRoomPanel ← Empty GameObject (main container)
        │
        │ ✅ COMPONENT: StorageRoomController.cs ← ⭐ MAIN CONTROLLER
        │    ├─ Equipped Gear Grid: [Drag EquippedGearContent GameObject]
        │    ├─ Bag Grid: [Drag BagContent GameObject]
        │    ├─ Storage Grid: [Drag StorageContent GameObject]
        │    ├─ Storage Filter Bar: [Drag FilterBarSection GameObject]
        │    ├─ Store All Button: [Drag Button_StoreAll]
        │    ├─ Exit Button: [Drag Button_Back]
        │    ├─ Bag Count Text: [Drag Text_BagCount]
        │    ├─ Storage Count Text: [Drag Text_StorageCount]
        │    ├─ Title Text: [Drag Text_Title]
        │    └─ Enable Debug Logs: ✓
        │
        ├── 📋 RoomHeader ← Empty GameObject
        │   ├── Button_Back ← UI Button
        │   │   │
        │   │   └─ ✅ COMPONENT: Button
        │   │       ├─ Interactable: ✓
        │   │       └─ OnClick: (Auto-wired by StorageRoomController)
        │   │
        │   └── Text_Title ← TMP_Text
        │       │
        │       └─ ✅ COMPONENT: TextMeshProUGUI
        │           ├─ Text: "Storage Room"
        │           ├─ Font Size: 48
        │           └─ Alignment: Center
        │
        ├── 🎒 BagInventorySection ← Empty GameObject
        │   ├── BagHeader ← Empty GameObject
        │   │   ├── Text_BagTitle ← TMP_Text ("Bag")
        │   │   ├── Text_BagCount ← TMP_Text ("0/12")
        │   │   └── Button_StoreAll ← UI Button
        │   │       │
        │   │       └─ ✅ COMPONENT: Button
        │   │           ├─ Interactable: ✓
        │   │           └─ OnClick: (Auto-wired by StorageRoomController)
        │   │
        │   └── BagPanel ← UI Image
        │       └── BagScrollView ← ScrollRect viewport
        │           │
        │           │ ✅ COMPONENTS:
        │           │    ├─ ScrollRect
        │           │    │   ├─ Content: [Drag BagContent]
        │           │    │   ├─ Horizontal: ☐
        │           │    │   └─ Vertical: ✓
        │           │    └─ Mask
        │           │
        │           └── 🎯 BagContent ← Empty GameObject ⭐ CRITICAL
        │               │
        │               │ ✅ COMPONENTS:
        │               │    ├─ InventoryGridUI.cs ← ⭐ ADD THIS SCRIPT
        │               │    │   ├─ Grid Type: Bag
        │               │    │   ├─ Item Slot Prefab: [Your ItemSlot prefab]
        │               │    │   ├─ Max Slots: 12
        │               │    │   ├─ Popup Context Source: Bag
        │               │    │   └─ Enable Debug Logs: ✓
        │               │    │
        │               │    ├─ GridLayoutGroup
        │               │    │   ├─ Cell Size: (80, 80)
        │               │    │   ├─ Spacing: (10, 10)
        │               │    │   └─ Constraint: Fixed Column Count = 3
        │               │    │
        │               │    └─ ContentSizeFitter
        │               │        ├─ Horizontal: Unconstrained
        │               │        └─ Vertical: Preferred Size
        │               │
        │               └── [ItemSlot prefabs spawn here at runtime]
        │
        ├── 🛡️ EquippedGearSection ← Empty GameObject
        │   ├── EquippedHeader ← Empty GameObject
        │   │   └── Text_EquippedTitle ← TMP_Text ("Equipped Gear")
        │   │
        │   └── 🎯 EquippedGearContent ← Empty GameObject ⭐ CRITICAL
        │       │
        │       │ ✅ COMPONENTS:
        │       │    ├─ GridLayoutGroup (NO InventoryGridUI script!)
        │       │    │   ├─ Cell Size: (100, 100)
        │       │    │   ├─ Spacing: (15, 15)
        │       │    │   └─ Constraint: Fixed Column Count = 4
        │       │    │
        │       │    └─ ContentSizeFitter
        │       │        ├─ Horizontal: Preferred Size
        │       │        └─ Vertical: Preferred Size
        │       │
        │       ├── ⚔️ GPS_Weapon ← GameObject with Button
        │       │   │
        │       │   │ ✅ COMPONENTS:
        │       │   │    ├─ EquipmentSlotUI.cs ← ⭐ ADD THIS SCRIPT
        │       │   │    │   ├─ Slot Type: Weapon
        │       │   │    │   ├─ Background Image: [Drag child "Background"]
        │       │   │    │   ├─ Icon Image: [Drag child "Icon"]
        │       │   │    │   ├─ Empty Overlay: [Drag child "EmptyOverlay"]
        │       │   │    │   ├─ Label Text: [Drag child "Label"] (optional)
        │       │   │    │   └─ Enable Debug Logs: ✓
        │       │   │    │
        │       │   │    └─ Button
        │       │   │        ├─ Interactable: ✓
        │       │   │        ├─ Navigation: None
        │       │   │        └─ OnClick: (Handled by EquipmentSlotUI script)
        │       │   │
        │       │   ├── Background ← UI Image (child of GPS_Weapon)
        │       │   ├── Icon ← UI Image (child of GPS_Weapon)
        │       │   ├── EmptyOverlay ← UI Image (child of GPS_Weapon)
        │       │   └── Label ← TMP_Text (child of GPS_Weapon)
        │       │
        │       ├── 🪖 GPS_Helmet ← GameObject with Button
        │       │   │
        │       │   └─ ✅ COMPONENTS:
        │       │       ├─ EquipmentSlotUI.cs
        │       │       │   └─ Slot Type: Helmet
        │       │       └─ Button
        │       │
        │       ├── 👕 GPS_Chest ← (Same as Weapon)
        │       │   └─ EquipmentSlotUI.cs (Slot Type: Chest)
        │       │
        │       ├── 🧤 GPS_Gloves ← (Same as Weapon)
        │       │   └─ EquipmentSlotUI.cs (Slot Type: Gloves)
        │       │
        │       ├── 👢 GPS_Boots ← (Same as Weapon)
        │       │   └─ EquipmentSlotUI.cs (Slot Type: Boots)
        │       │
        │       ├── 💍 GPS_Acc1 ← (Same as Weapon)
        │       │   └─ EquipmentSlotUI.cs (Slot Type: Accessory1)
        │       │
        │       └── 💍 GPS_Acc2 ← (Same as Weapon)
        │           └─ EquipmentSlotUI.cs (Slot Type: Accessory2)
        │
        └── 📦 StorageInventorySection ← Empty GameObject
            ├── StorageHeader ← Empty GameObject
            │   ├── Text_StorageTitle ← TMP_Text ("Storage")
            │   └── Text_StorageCount ← TMP_Text ("0/60")
            │
            ├── 🔍 FilterBarSection ← Empty GameObject ⭐ CRITICAL
            │   │
            │   │ ✅ COMPONENT: InventoryFilterBarUI.cs ← ⭐ ADD THIS SCRIPT
            │   │    ├─ All Button: [Drag Button_All]
            │   │    ├─ Weapon Button: [Drag Button_Weapon]
            │   │    ├─ Gear Button: [Drag Button_Gear]
            │   │    ├─ Potion Button: [Drag Button_Potion]
            │   │    ├─ Material Button: [Drag Button_Material]
            │   │    ├─ Misc Button: [Drag Button_Misc]
            │   │    └─ Enable Debug Logs: ✓
            │   │
            │   └── FilterButtonsContainer ← Horizontal Layout Group
            │       ├── Button_All ← UI Button
            │       │   └─ ✅ COMPONENT: Button (OnClick handled by FilterBarUI)
            │       ├── Button_Weapon ← UI Button
            │       ├── Button_Gear ← UI Button
            │       ├── Button_Potion ← UI Button
            │       ├── Button_Material ← UI Button
            │       └── Button_Misc ← UI Button
            │
            └── StoragePanel ← UI Image
                └── StorageScrollView ← ScrollRect viewport
                    │
                    │ ✅ COMPONENTS:
                    │    ├─ ScrollRect
                    │    │   ├─ Content: [Drag StorageContent]
                    │    │   ├─ Horizontal: ☐
                    │    │   └─ Vertical: ✓
                    │    └─ Mask
                    │
                    └── 🎯 StorageContent ← Empty GameObject ⭐ CRITICAL
                        │
                        │ ✅ COMPONENTS:
                        │    ├─ InventoryGridUI.cs ← ⭐ ADD THIS SCRIPT
                        │    │   ├─ Grid Type: Storage
                        │    │   ├─ Item Slot Prefab: [Your ItemSlot prefab]
                        │    │   ├─ Max Slots: 60
                        │    │   ├─ Popup Context Source: Storage
                        │    │   └─ Enable Debug Logs: ✓
                        │    │
                        │    ├─ GridLayoutGroup
                        │    │   ├─ Cell Size: (80, 80)
                        │    │   ├─ Spacing: (10, 10)
                        │    │   └─ Constraint: Fixed Column Count = 5
                        │    │
                        │    └─ ContentSizeFitter
                        │        ├─ Horizontal: Unconstrained
                        │        └─ Vertical: Preferred Size
                        │
                        └── [ItemSlot prefabs spawn here at runtime]