# 🎨 Unity Inspector Configuration Guide

## Quick Reference for Setting Up Components

---

## 1️⃣ InventoryGridUI Component

### **Use Case: Bag (12 slots)**

```
┌─────────────────────────────────────────┐
│ InventoryGridUI                         │
├─────────────────────────────────────────┤
│ Grid Configuration                      │
│  ├─ Location: Bag                       │
│  ├─ Max Slots: 12                       │
│  └─ Show Empty Slots: ✓                 │
│                                         │
│ UI References                           │
│  ├─ Grid Content: [BagContent]          │
│  └─ Item Slot Prefab: [ItemSlotUI]      │
│                                         │
│ Popup Context                           │
│  └─ Popup Source: BagUI                 │
│                                         │
│ Optional Filter                         │
│  └─ Filter Bar: [none]                  │
└─────────────────────────────────────────┘
```

**When to use:** 
- Storage scene bag panel
- Shows "Equip" and "Move to Storage" buttons

---

### **Use Case: Bag in Inventory Panel (Persistent)**

```
┌─────────────────────────────────────────┐
│ InventoryGridUI                         │
├─────────────────────────────────────────┤
│ Grid Configuration                      │
│  ├─ Location: Bag                       │
│  ├─ Max Slots: 12                       │
│  └─ Show Empty Slots: ✓                 │
│                                         │
│ UI References                           │
│  ├─ Grid Content: [BagContent]          │
│  └─ Item Slot Prefab: [ItemSlotUI]      │
│                                         │
│ Popup Context                           │
│  └─ Popup Source: InventoryPanel  ← KEY │
│                                         │
│ Optional Filter                         │
│  └─ Filter Bar: [none]                  │
└─────────────────────────────────────────┘
```

**When to use:**
- Persistent inventory panel (accessed via 'I' key)
- Shows "Equip" and "Use" buttons (NO "Move to Storage")

---

### **Use Case: Storage (60 slots, scrollable)**

```
┌─────────────────────────────────────────┐
│ InventoryGridUI                         │
├─────────────────────────────────────────┤
│ Grid Configuration                      │
│  ├─ Location: Storage                   │
│  ├─ Max Slots: 60                       │
│  └─ Show Empty Slots: ✓                 │
│                                         │
│ UI References                           │
│  ├─ Grid Content: [StorageContent]      │
│  │   (inside ScrollRect > Viewport)     │
│  └─ Item Slot Prefab: [ItemSlotUI]      │
│                                         │
│ Popup Context                           │
│  └─ Popup Source: StorageUI             │
│                                         │
│ Optional Filter                         │
│  └─ Filter Bar: [FilterBar] ← CONNECT  │
└─────────────────────────────────────────┘
```

**When to use:**
- Storage scene storage panel
- Shows "Equip" and "Move to Bag" buttons
- Connects to filter bar for item type filtering

---

## 2️⃣ EquipmentSlotUI Component

### **Individual Slot Configuration**

```
┌─────────────────────────────────────────┐
│ EquipmentSlotUI                         │
├─────────────────────────────────────────┤
│ Slot Configuration                      │
│  └─ Slot Type: [Weapon] ← SET PER SLOT │
│                                         │
│ UI References                           │
│  ├─ Slot Button: [Button]               │
│  ├─ Icon Image: [Icon]                  │
│  ├─ Background Image: [Background]      │
│  ├─ Empty Overlay: [EmptyOverlay]       │
│  └─ Label Text: [Label]                 │
│                                         │
│ Visual Feedback                         │
│  ├─ Empty Slot Color: (0.2, 0.2, 0.2)  │
│  └─ Filled Slot Color: (1, 1, 1)       │
│                                         │
│ Popup Context                           │
│  └─ Popup Source: EquippedGear          │
└─────────────────────────────────────────┘
```

**7 Slots to Configure:**
1. GPS_Weapon → `Slot Type: Weapon`
2. GPS_Helmet → `Slot Type: Helmet`
3. GPS_Chest → `Slot Type: Chest`
4. GPS_Gloves → `Slot Type: Gloves`
5. GPS_Boots → `Slot Type: Boots`
6. GPS_Acc1 → `Slot Type: Accessory1`
7. GPS_Acc2 → `Slot Type: Accessory2`

---

## 3️⃣ InventoryFilterBarUI Component

```
┌─────────────────────────────────────────┐
│ InventoryFilterBarUI                    │
├─────────────────────────────────────────┤
│ Filter Buttons                          │
│  ├─ All Items Button: [AllBtn]          │
│  ├─ Weapon Button: [WeaponBtn]          │
│  ├─ Gear Button: [GearBtn]              │
│  ├─ Potion Button: [PotionBtn]          │
│  ├─ Materials Button: [MaterialsBtn]    │
│  └─ Misc Button: [MiscBtn]              │
│                                         │
│ Visual Feedback (Optional)              │
│  ├─ Normal Color: (1, 1, 1, 1)          │
│  ├─ Active Color: (1, 1, 0, 1)          │
│  └─ Use Color Feedback: ✓               │
└─────────────────────────────────────────┘
```

**Note:** Only used in Storage scene, NOT in Bag panels

---

## 4️⃣ PlayerInventoryPanelController

```
┌─────────────────────────────────────────┐
│ PlayerInventoryPanelController          │
├─────────────────────────────────────────┤
│ Panel References                        │
│  ├─ Panel Root: [PanelContainer]        │
│  └─ Background Overlay: [Background]    │
│                                         │
│ UI Components                           │
│  ├─ Bag Grid UI: [BagGrid]              │
│  └─ Equipped Gear Preview: [Preview]    │
│                                         │
│ Buttons                                 │
│  ├─ Close Button: [CloseBtn]            │
│  ├─ Bag Tab Button: [BagTab]            │
│  └─ Abilities Tab Button: [AbilitiesTab]│
│                                         │
│ Settings                                │
│  ├─ Toggle Key: I                       │
│  └─ Close On Background Click: ✓        │
└─────────────────────────────────────────┘
```

**Important:** This GameObject must be in scene `03_MainBase` and will persist via `DontDestroyOnLoad()`

---

## 5️⃣ StorageRoomController

```
┌─────────────────────────────────────────┐
│ StorageRoomController                   │
├─────────────────────────────────────────┤
│ UI Components                           │
│  ├─ Equipped Gear Grid: [none]          │
│  │   (using manual EquipmentSlotUI)     │
│  ├─ Bag Grid: [BagGrid]                 │
│  ├─ Storage Grid: [StorageGrid]         │
│  └─ Storage Filter Bar: [FilterBar]     │
│                                         │
│ Action Buttons                          │
│  ├─ Store All Button: [StoreAllBtn]     │
│  └─ Exit Button: [ExitBtn]              │
│                                         │
│ Optional UI                             │
│  ├─ Bag Count Text: [BagCountTxt]       │
│  └─ Storage Count Text: [StorageCountTxt]│
└─────────────────────────────────────────┘
```

---

## 🎯 Common Pitfalls & Solutions

### ❌ **Pitfall: Slots not spawning**

**Symptom:** Grid is empty, no slots appear

**Cause:** `Grid Content` not assigned or wrong transform

**Solution:**
```
InventoryGridUI:
└─ Grid Content: Must point to Transform with GridLayoutGroup
   Example: Canvas/BagPanel/BagGrid/BagContent
```

---

### ❌ **Pitfall: Wrong popup buttons**

**Symptom:** "Move to Storage" button appears in Inventory Panel

**Cause:** Wrong `Popup Source` setting

**Solution:**
```
Storage Scene Bag → Popup Source: BagUI
Inventory Panel Bag → Popup Source: InventoryPanel  ← Different!
```

---

### ❌ **Pitfall: Filter doesn't work**

**Symptom:** Clicking filter buttons does nothing

**Cause:** Filter bar not connected to grid

**Solution:**
```
StorageRoomController.Start():
└─ storageGrid.ConnectFilterBar(storageFilterBar);

OR in Inspector:
InventoryGridUI:
└─ Optional Filter > Filter Bar: [FilterBar reference]
```

---

### ❌ **Pitfall: Background click doesn't close panel**

**Symptom:** Clicking background does nothing

**Cause:** Background Image doesn't have raycast or Button component

**Solution:**
```
1. Select Background Image
2. Inspector: Image > Raycast Target ✓
3. Add Component → Button
4. PlayerInventoryPanelController will auto-wire it
```

---

### ❌ **Pitfall: Panel doesn't persist across scenes**

**Symptom:** Panel disappears when loading new scene

**Cause:** Not created in `03_MainBase` or missing DontDestroyOnLoad

**Solution:**
```
1. Create InventoryPanel in scene: 03_MainBase
2. Check PlayerInventoryPanelController.Awake():
   DontDestroyOnLoad(gameObject);
3. Ensure it's a ROOT GameObject (not child of Canvas)
```

---

## 🔧 Grid Layout Configuration

### **Bag Grid (3×4 layout)**

```
GridLayoutGroup Settings:
├─ Cell Size: (100, 100)
├─ Spacing: (10, 10)
├─ Start Corner: Upper Left
├─ Start Axis: Horizontal
├─ Child Alignment: Upper Left
├─ Constraint: Fixed Column Count = 3
```

**Result:** 12 slots in 3 columns, 4 rows

---

### **Storage Grid (6×10 scrollable)**

```
ScrollRect Settings:
├─ Content: StorageContent (has GridLayoutGroup)
├─ Horizontal: false
├─ Vertical: true
├─ Movement Type: Elastic
└─ Scroll Sensitivity: 10

GridLayoutGroup Settings (on Content):
├─ Cell Size: (80, 80)
├─ Spacing: (5, 5)
├─ Start Corner: Upper Left
├─ Start Axis: Horizontal
├─ Child Alignment: Upper Left
├─ Constraint: Fixed Column Count = 6
```

**Result:** 60 slots in 6 columns, scrollable

---

### **Equipment Preview (Vertical List)**

```
VerticalLayoutGroup Settings:
├─ Child Force Expand: Width ✓, Height ✗
├─ Child Control Size: Width ✓, Height ✓
├─ Spacing: 10
└─ Padding: 10 (all sides)
```

**Manual Slots (not spawned):**
- GPS_Weapon
- GPS_Helmet
- GPS_Chest
- GPS_Gloves
- GPS_Boots
- GPS_Acc1
- GPS_Acc2

---

## 📊 Quick Reference Table

| Component | Location | Max Slots | Filter | Popup Source | Use Case |
|-----------|----------|-----------|--------|--------------|----------|
| BagGrid (Storage) | Bag | 12 | No | BagUI | Storage scene |
| BagGrid (Panel) | Bag | 12 | No | InventoryPanel | Persistent panel |
| StorageGrid | Storage | 60 | Yes | StorageUI | Storage scene |
| EquippedGear | Equipped | 7 | No | EquippedGear | Both scenes |

---

## 🎬 Video Walkthrough (Pseudocode)

```
1. Create InventoryPanel prefab
   ├─ Add PlayerInventoryPanelController
   ├─ Add Background (Image, Button, raycast enabled)
   └─ Add BagGrid (InventoryGridUI, Popup Source = InventoryPanel)

2. Create Storage scene layout
   ├─ Add BagGrid (InventoryGridUI, Popup Source = BagUI)
   ├─ Add StorageGrid (InventoryGridUI, Popup Source = StorageUI)
   ├─ Add FilterBar (InventoryFilterBarUI)
   └─ Connect FilterBar to StorageGrid

3. Add 7 EquipmentSlotUI components
   ├─ Set Slot Type individually (Weapon, Helmet, etc.)
   └─ Connect UI references (Button, Icon, Background, etc.)

4. Test!
   ├─ Open Inventory Panel (press I)
   ├─ Click item → popup shows Equip/Use (no Move)
   ├─ Go to Storage scene
   └─ Click item → popup shows Equip/Move/Use
```

---

**That's it!** Follow these configurations and your UI will work perfectly with the new component-based architecture. 🎯