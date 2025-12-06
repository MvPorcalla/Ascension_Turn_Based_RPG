**Scripts/InventorySystem**

// ──────────────────────────────────────────────────
// StorageRoomUI.cs
// Manages the Storage Room UI for item management
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Ascension.Managers;
using Ascension.Data.SO;

public class StorageRoomUI : MonoBehaviour
{}

// ──────────────────────────────────────────────────
// InventorySystem
// ItemSlotUI.cs
// UI component for displaying an item slot in inventory/storage
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Ascension.Data.SO;
using Ascension.Managers;

public class ItemSlotUI : MonoBehaviour
{}

// ──────────────────────────────────────────────────
// ItemInstance.cs
// Represents an instance of an item in inventory/storage
// ──────────────────────────────────────────────────

using System;

[Serializable]
public class ItemInstance
{}

// ──────────────────────────────────────────────────
// InventoryManager.cs
// Manages the player inventory and storage system
// ──────────────────────────────────────────────────

using UnityEngine;
using Ascension.Data.SO;
using Ascension.Systems;

public class InventoryManager : MonoBehaviour
{}

// ──────────────────────────────────────────────────
// InventoryEnums.cs
// Enums for inventory system locations and haptic feedback
// ──────────────────────────────────────────────────

/// <summary>
/// Item location within the inventory system
/// </summary>
public enum ItemLocation
{}

// ──────────────────────────────────────────────────
// BuffLineUI.cs
// Optional helper script to set up buff line UI elements
// ──────────────────────────────────────────────────

using UnityEngine;
using TMPro;

/// <summary>
/// Optional helper script to attach to BuffType prefab
/// Makes it easier to set up buff lines programmatically
/// </summary>
public class BuffLineUI : MonoBehaviour
{}

// ──────────────────────────────────────────────────
// BagInventoryData.cs
// Serializable data class for saving/loading BagInventory
// ──────────────────────────────────────────────────

using System;
using System.Collections.Generic;

[Serializable]
public class BagInventoryData
{}

// ──────────────────────────────────────────────────
// BagInventory.cs
// Manages the bag and storage inventory system
// ──────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ascension.Data.SO;
using Ascension.Systems;

[Serializable]
public class BagInventory
{}


**Scripts/InventorySystem/PopupScripts**
// ──────────────────────────────────────────────────
// InventoryPotionPopup.cs
// UI Popup for displaying potion details and actions
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Ascension.Managers;
using Ascension.Systems;
using Ascension.Data.SO;

public class InventoryPotionPopup : MonoBehaviour
{}

// ──────────────────────────────────────────────────
// InventoryItemPopup.cs
// UI popup for displaying item details and actions
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Data.SO;
using Ascension.Managers;

public class InventoryItemPopup : MonoBehaviour
{}

// ──────────────────────────────────────────────────
// InventoryGearPopup.cs
// UI popup for displaying gear item details and actions
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Data.SO;
using Ascension.Systems;

public class InventoryGearPopup : MonoBehaviour
{}
