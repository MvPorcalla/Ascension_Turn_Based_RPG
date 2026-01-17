---

### Camera Setup Table

| Scene Name        | Has Camera? | Camera Tag | Notes                      |
| ----------------- | ----------- | ---------- | -------------------------- |
| 01_Bootstrap      | ❌ No        | –          | System initialization only |
| 00_Disclaimer     | ✅ Yes       | MainCamera | Renders disclaimer UI      |
| 02_AvatarCreation | ✅ Yes       | MainCamera | Renders character creation |
| 03_MainBase       | ✅ Yes       | MainCamera | Renders game world         |
| UI_WorldMap       | ❌ No        | –          | Uses MainBase camera       |
| UI_Inventory      | ❌ No        | –          | Uses MainBase camera       |

---

### Camera Flow Diagram

```
01_Bootstrap
   └─ No Camera (logic only)

00_Disclaimer
   └─ MainCamera → renders Disclaimer UI

02_AvatarCreation
   └─ MainCamera → renders Character Creation UI

03_MainBase
   └─ MainCamera → renders Game World + UI (HUD from PersistentUICanvas)

UI_WorldMap
   └─ No Camera → relies on MainBase MainCamera

UI_Inventory
   └─ No Camera → relies on MainBase MainCamera
```
