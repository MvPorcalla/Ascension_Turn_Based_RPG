## 🎯 **Component Assignment Guide**

### **1. StorageRoomPanel** - Add `StorageRoomController.cs`

**GameObject:** `StorageRoomPanel`

**Script:** `StorageRoomController.cs`

**Inspector Assignments:**
```
StorageRoomController Component:
├── Sub-Panels:
│   ├── Bag Inventory UI: → Drag "BagInventorySection" here
│   ├── Pocket Inventory UI: → Drag "PocketSection" here
│   └── Storage Inventory UI: → Drag "StorageSection" here (you'll create this)
│
└── Quick Actions:
    ├── Store All Button: → Drag "StoreAllButton" (from BagHeader)
    └── Back Button: → Drag "backButton" (from RoomHeader)
```

---

### **2. BagInventorySection** - Add `BagInventoryUI.cs`

**GameObject:** `BagInventorySection`

**Script:** `BagInventoryUI.cs`

**Inspector Assignments:**
```
BagInventoryUI Component:
├── UI References:
│   ├── Inventory Content: → Drag "BagContent" (the GridLayoutGroup)
│   ├── Item Slot Prefab: → Your item slot prefab
│   └── Empty Slot Prefab: → Your EmptySlot prefab
│
└── Popups:
    ├── Item Popup: → Drag your InventoryItemPopup GameObject
    └── Potion Popup: → Drag your InventoryPotionPopup GameObject
```

---

### **3. PocketSection** - Add `PocketInventoryUI.cs`

**GameObject:** `PocketSection`

**Script:** `PocketInventoryUI.cs`

**Inspector Assignments:**
```
PocketInventoryUI Component:
├── UI References:
│   ├── Pocket Content: → Drag "PocketContent" (the GridLayoutGroup)
│   ├── Item Slot Prefab: → Your item slot prefab (same as bag)
│   └── Empty Slot Prefab: → Your EmptySlot prefab (same as bag)
│
└── Popups:
    ├── Item Popup: → Drag your InventoryItemPopup GameObject
    └── Potion Popup: → Drag your InventoryPotionPopup GameObject
```

---

### **4. Create StorageSection** - Add `StorageInventoryUI.cs`

You need to create a new section for storage! Here's the hierarchy:

```markdown
├── StorageRoomPanel
│    ├── ... (existing sections)
│    │
│    └── StorageSection (NEW - add this!)
│         ├── StorageHeader
│         │    └── Title (TMP - "Storage")
│         │
│         ├── FilterButtons (NEW - horizontal layout)
│         │    ├── AllItemsButton
│         │    ├── WeaponButton
│         │    ├── GearButton
│         │    ├── PotionButton
│         │    ├── MaterialsButton
│         │    └── MiscButton
│         │
│         └── StoragePanel
│              └── StorageViewport
│                   └── StorageContent (GridLayoutGroup)
│                        └── (items spawn here)
```

**GameObject:** `StorageSection`

**Script:** `StorageInventoryUI.cs`

**Inspector Assignments:**
```
StorageInventoryUI Component:
├── UI References:
│   ├── Storage Content: → Drag "StorageContent" (the GridLayoutGroup)
│   └── Item Slot Prefab: → Your item slot prefab (same as bag)
│
├── Filter Buttons:
│   ├── All Items Button: → Drag "AllItemsButton"
│   ├── Weapon Button: → Drag "WeaponButton"
│   ├── Gear Button: → Drag "GearButton"
│   ├── Potion Button: → Drag "PotionButton"
│   ├── Materials Button: → Drag "MaterialsButton"
│   └── Misc Button: → Drag "MiscButton"
│
└── Popups:
    ├── Item Popup: → Drag your InventoryItemPopup GameObject
    └── Potion Popup: → Drag your InventoryPotionPopup GameObject
```

---

## 📋 **Step-by-Step Setup Process**

### **Step 1: Add Scripts to Existing GameObjects**

1. **Select `StorageRoomPanel`**
   - Click "Add Component"
   - Search "StorageRoomController"
   - Add it
   - **Remove the old `StorageRoomUI` script** (if it exists)

2. **Select `BagInventorySection`**
   - Click "Add Component"
   - Search "BagInventoryUI"
   - Add it

3. **Select `PocketSection`**
   - Click "Add Component"
   - Search "PocketInventoryUI"
   - Add it

---

### **Step 2: Create StorageSection**

1. **Right-click `StorageRoomPanel`** → Create Empty
2. Rename to **"StorageSection"**
3. Create child structure:
   ```
   StorageSection
   ├── StorageHeader (Panel/Empty)
   │   └── Title (TextMeshPro)
   ├── FilterButtons (Empty with Horizontal Layout Group)
   │   ├── AllItemsButton (Button)
   │   ├── WeaponButton (Button)
   │   ├── GearButton (Button)
   │   ├── PotionButton (Button)
   │   ├── MaterialsButton (Button)
   │   └── MiscButton (Button)
   └── StoragePanel (Panel)
       └── StorageViewport (Scroll View)
           └── StorageContent (Empty with GridLayoutGroup)
   ```

4. **Add script to `StorageSection`:**
   - Select `StorageSection`
   - Add Component → `StorageInventoryUI`

---

### **Step 3: Assign All References**

Go through each component and drag the appropriate GameObjects into the Inspector fields (as detailed in sections 1-4 above).

---

## ⚠️ **Common Issues & Fixes**

### **Issue 1: "StoreAllButton is in BagHeader, not RoomHeader"**

You have two options:

**Option A:** Move button to RoomHeader
- Drag `StoreAllButton` from `BagHeader` to `RoomHeader`
- Assign it to `StorageRoomController`

**Option B:** Keep it in BagHeader (probably better UX)
- In `StorageRoomController`, the button reference is optional
- The button stores ALL bag items, so it makes sense to keep it with the bag section
- Just leave the `Store All Button` field empty in the controller

---

### **Issue 2: "Same popup references in 3 places?"**

Yes! You can assign the **same popup GameObjects** to all three UI scripts:
- `BagInventoryUI` → itemPopup, potionPopup
- `PocketInventoryUI` → itemPopup, potionPopup
- `StorageInventoryUI` → itemPopup, potionPopup

They all share the same popups since they show the same item types.

---

### **Issue 3: "What about the old StorageRoomUI references?"**

After you set everything up:
1. Select `StorageRoomPanel`
2. Find the old `StorageRoomUI` component
3. Click the three dots → Remove Component
4. Unity will automatically clean up the references

---

## 🎨 **Layout Tip**

Your `StorageRoomPanel` should probably use a **Vertical Layout Group** to stack sections:

```
StorageRoomPanel (Vertical Layout Group)
├── RoomHeader (fixed height)
├── BagInventorySection (flexible height)
├── PocketSection (flexible height)
└── StorageSection (flexible height - takes remaining space)
```

---

## ✅ **Final Hierarchy**

```markdown
StorageRoomPanel (has StorageRoomController.cs)
├── RoomHeader
│   ├── backButton ← Referenced in controller
│   └── Title
│
├── BagInventorySection (has BagInventoryUI.cs)
│   ├── BagHeader
│   │   ├── Title
│   │   └── StoreAllButton (optional: reference in controller)
│   └── BagPanel
│       └── BagViewport
│           └── BagContent ← Referenced in BagInventoryUI
│
├── PocketSection (has PocketInventoryUI.cs)
│   ├── PocketHeader
│   │   └── Title
│   └── PocketPanel
│       └── PocketViewport
│           └── PocketContent ← Referenced in PocketInventoryUI
│
└── StorageSection (has StorageInventoryUI.cs) ← NEW!
    ├── StorageHeader
    │   └── Title
    ├── FilterButtons ← All referenced in StorageInventoryUI
    │   ├── AllItemsButton
    │   ├── WeaponButton
    │   ├── GearButton
    │   ├── PotionButton
    │   ├── MaterialsButton
    │   └── MiscButton
    └── StoragePanel
        └── StorageViewport
            └── StorageContent ← Referenced in StorageInventoryUI
```