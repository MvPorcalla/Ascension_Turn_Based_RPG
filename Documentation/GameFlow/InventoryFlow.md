
┌─────────────────────────────────────────────────────┐
│         INVENTORY SYSTEM (Data Layer)               │
│                                                     │
│  InventoryManager (Singleton)                       │
│  ├─ InventoryCore                                   │
│  │  └─ List<ItemInstance> allItems                  │
│  │     ├─ ItemInstance("sword_iron", qty:1, Bag)    │
│  │     ├─ ItemInstance("potion_hp", qty:5, Pocket)  │
│  │     └─ ItemInstance("helmet_steel", qty:1, Storage) │
│  │                                                  │
│  └─ SlotCapacityManager                             │
│     ├─ maxBagSlots: 12                              │
│     ├─ maxPocketSlots: 6                            │
│     └─ maxStorageSlots: 60                          │
│                                                     │
│  API:                                               │
│  • AddItem(itemID, qty, addToBag)                   │
│  • RemoveItem(item, qty)                            │
│  • GetBagItems() → List<ItemInstance>               │
│  • GetPocketItems() → List<ItemInstance>            │
│  • GetStorageItems() → List<ItemInstance>           │
│  • MoveToBag(item, qty)                             │
│  • MoveToPocket(item, qty)                          │
│  • MoveToStorage(item, qty)                         │
│                                                     │
└──────────────────▲──────────────────▲───────────────┘
                   │                  │
                   │                  │
       ┌───────────┘                  └──────────────┐
       │                                             │
┌──────────────────────────┐        ┌─────────────────────────────────┐
│  STORAGE SYSTEM          │        │  EQUIPMENT SYSTEM               │
│  (Storage Room UI)       │        │  (Equipment Room UI)            │
│                          │        │                                 │
│  StorageRoomController   │        │  EquipmentRoomController        │
│  ├─ BagInventoryUI       │        │  ├─ GearSlotUI x7               │
│  │  └─ Queries:          │        │  │  └─ Queries:                 │
│  │     GetBagItems()     │        │  │     IsItemEquipped()         │
│  │                       │        │  │                              │
│  ├─ PocketInventoryUI    │        │  ├─ SkillSlotUI x3              │
│  │  └─ Queries:          │        │  │                              │
│  │     GetPocketItems()  │        │  └─ EquipmentStorageUI          │
│  │                       │        │     └─ Queries:                 │
│  └─ StorageInventoryUI   │        │        GetStorageItems()        │
│     └─ Queries:          │        │        Filter by gear/abilities │
│        GetStorageItems() │        │                                 │
│        Filter by ItemType│        │  EquipmentManager               │
│                          │        │  ├─ EquippedGear (data)         │
│  Popups:                 │        │  └─ Equip/Unequip logic         │
│  • InventoryItemPopup    │        │                                 │
│  • InventoryPotionPopup  │        │  Popups:                        │
│  • GearPopup (from SharedUI) │    │  • GearPopup (from SharedUI)    │
│                          │        │  • SkillAssignmentPopup         │
└──────────────────────────┘        └─────────────────────────────────┘