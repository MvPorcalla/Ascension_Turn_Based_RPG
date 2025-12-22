Perfect! Here's exactly how to set up your Equipment Room UI in Unity:

---

## **🎯 Component Assignment Guide**

### **1. Main EquipmentRoomPanel Setup**

**GameObject:** `EquipmentRoomPanel`  
**Add Component:** `EquipmentRoomUI.cs`

**Inspector Assignments:**

```
EquipmentRoomUI Component:
├── UI Panels
│   ├── Equipment Room Panel: EquipmentRoomPanel (itself)
│   └── Storage Room Panel: StorageRoomPanel (why storageroompanel? i have storagepanel as prefab inside my EquipmentRoomPanel)
│
├── Player Preview
│   └── Player Preview: PlayerPreview (drag the prefab instance)
│
├── Gear Slots (all use EquipmentSlotUI component)
│   ├── Weapon Slot: WeaponSlot
│   ├── Helmet Slot: HelmetSlot
│   ├── Chest Plate Slot: ChestPlateSlot
│   ├── Gloves Slot: GlovesSlot
│   ├── Boots Slot: BootsSlot
│   ├── Accessory1 Slot: Accessory1Slot
│   └── Accessory2 Slot: Accessory2Slot
│
├── Skill Slots (all use EquipmentSlotUI component)
│   ├── Normal Skill1 Slot: NormalSkillSlot1
│   ├── Normal Skill2 Slot: NormalSkillSlot2
│   └── Ultimate Skill Slot: UltimateSkillSlot
│
├── HotBar Slots (all use EquipmentSlotUI component)
│   ├── Hotbar Slot1: Item1
│   ├── Hotbar Slot2: Item2
│   └── Hotbar Slot3: Item3
│
├── Storage Section
│   ├── Storage Content: StorageContent (the GridLayoutGroup)
│   ├── Item Slot Prefab: ItemSlot prefab (see below)
│   ├── Gear Button: GearButton
│   ├── Abilities Button: AbilitiesButton
│   ├── Gear Sort Buttons: GearSortButtons (parent GameObject)
│   └── Abilities Sort Buttons: AbilitiesSortButtons (parent GameObject)
│
└── Popup
    └── Gear Info Popup: GearInfoPopup (drag the popup GameObject)
```

---

Canvas (Screen Space - Overlay)
├── EquipmentRoomPanel (Full Screen)
│   ├── RoomHeader
│   │   ├── Title (TMP)
│   │   └── BackButton
│   │
│   ├── PlayerPreview (Your existing prefab)
│   │
│   ├── GearSection
│   │   ├── GearHeader
│   │   │   └── Title (TMP: "Equipment")
│   │   │
│   │   └── GearContainer (Vertical Layout Group)
│   │       ├── WeaponSlot (GearSlotUI)
│   │       ├── HelmetSlot (GearSlotUI)
│   │       ├── ChestSlot (GearSlotUI)
│   │       ├── GlovesSlot (GearSlotUI)
│   │       ├── BootsSlot (GearSlotUI)
│   │       ├── Accessory1Slot (GearSlotUI)
│   │       └── Accessory2Slot (GearSlotUI)
│   │
│   └── StorageSection
│       ├── StorageHeader (Horizontal Layout)
│       │   ├── GearButton (Toggle filter to Gear)
│       │   ├── AbilitiesButton (Toggle filter to Abilities)
│       │   │
│       │   └── SortSection (Horizontal Layout)
│       │       ├── AllButton
│       │       ├── WeaponsButton
│       │       ├── HelmetsButton
│       │       ├── ChestsButton
│       │       ├── GlovesButton
│       │       ├── BootsButton
│       │       └── AccessoriesButton
│       │
│       └── StoragePanel (Scroll View)
│           └── StorageViewport
│               └── StorageContent (Grid Layout Group)
│                   └── ItemSlot (Prefab) - Generated at runtime
```

---

### **Step 2: Component Assignment**

#### **A. EquipmentRoomPanel GameObject**
```
- Add Component: EquipmentRoomUI.cs
- Assign References:
  ✓ Player Preview → PlayerPreview prefab
  ✓ Storage UI → EquipmentStorageUI component
  ✓ All 7 Gear Slots → Individual GearSlotUI components
  ✓ Back Button → Button component
```

#### **B. Each Gear Slot (WeaponSlot, HelmetSlot, etc.)**
```
- Add Component: GearSlotUI.cs
- Assign in Inspector:
  ✓ Slot Type → (Weapon, Helmet, Chest, etc.)
  ✓ Slot Button → Self Button component
  ✓ Slot Background → Background Image
  ✓ Item Icon → Child Image (for item sprite)
  ✓ Rarity Border → Border Image (colored by rarity)
  ✓ Slot Name Text → TMP Text (displays "Weapon", "Helmet")
  ✓ Empty Indicator → Small icon/text when slot is empty
```

#### **C. StorageSection GameObject**
```
- Add Component: EquipmentStorageUI.cs
- Assign References:
  ✓ Storage Content → Content Transform (Grid Layout)
  ✓ Item Slot Prefab → Your ItemSlotUI prefab
  ✓ Gear Button → Button for "Gear" tab
  ✓ Abilities Button → Button for "Abilities" tab
  ✓ All Sort Buttons → Individual filter buttons


  EquipmentRoomPanel
├── RoomHeader ✅
├── PlayerPreview ✅
├── GearSection ✅
│   ├── GearHeader
│   ├── GearContainer (7 gear slots) ✅
│   └── HotbarContainer 🆕 ADD THIS
│       ├── NormalSkillSlot1
│       ├── NormalSkillSlot2
│       ├── UltimateSkillSlot
│       ├── Item1Slot
│       ├── Item2Slot
│       └── Item3Slot
└── StorageSection ✅
```

---

### **Step 3: Create HotbarContainer**

1. **In EquipmentRoomPanel → GearSection:**
```
   Right-click GearSection → UI → Empty (name it "HotbarContainer")
```

2. **Add Layout:**
```
   Add Component → Horizontal Layout Group (or Grid if you prefer)
   ├─ Spacing: 10
   ├─ Child Force Expand: Width ✓
   └─ Padding: 10 all sides
```

3. **Add Visual Separator (Optional):**
```
   Above HotbarContainer, add a Panel called "HotbarHeader"
   └─ Add TMP Text: "HOTBAR"
```

---

### **Step 4: Create Hotbar Slot Prefab**

1. **Create GameObject:**
```
   Hierarchy → Right-click → UI → Button (name it "HotbarSlot")
```

2. **Structure:**
```
   HotbarSlot
   ├── SlotBackground (Image - colored based on type)
   ├── ItemIcon (Image - shows skill/potion icon)
   ├── SlotNameText (TMP - "Skill 1", "Item 1")
   ├── EmptyIndicator (Image/Icon - "+" or lock icon)
   └── QuantityText (TMP - "x5" for potions only)
```

3. **Add Component:**
```
   Add Component → HotbarSlotUI.cs
```

4. **Inspector Setup:**
```
   HotbarSlotUI Component:
   ├── Slot Type → (Set when instantiating)
   ├── Slot Button → Button component
   ├── Slot Background → SlotBackground Image
   ├── Item Icon → ItemIcon Image
   ├── Slot Name Text → SlotNameText TMP
   ├── Empty Indicator → EmptyIndicator GameObject
   ├── Quantity Text → QuantityText TMP
   └── Colors → Set your preferred colors
```

5. **Save as Prefab:**
```
   Drag HotbarSlot → Assets/Prefabs/UI/Equipment/
```

---

### **Step 5: Create 6 Hotbar Slots**

In `HotbarContainer`, create 6 instances:

1. **Duplicate the prefab 6 times**
2. **Rename and configure each:**
```
HotbarContainer/
├── NormalSkillSlot1 (HotbarSlotUI → Slot Type: NormalSkill1)
├── NormalSkillSlot2 (HotbarSlotUI → Slot Type: NormalSkill2)
├── UltimateSkillSlot (HotbarSlotUI → Slot Type: UltimateSkill)
├── Item1Slot (HotbarSlotUI → Slot Type: Item1)
├── Item2Slot (HotbarSlotUI → Slot Type: Item2)
└── Item3Slot (HotbarSlotUI → Slot Type: Item3)