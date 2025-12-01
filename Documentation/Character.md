JSON File
    ↓
CharacterManager (Single Source of Truth)
    ↓
    ├─→ PlayerHUD (listens to events)
    ├─→ PotionPopupUI (reads stats)
    ├─→ PotionManager (modifies stats, triggers events)
    ├─→ InventoryManager (reads stats)
    └─→ Any other system...