**Script/**

// ════════════════════════════════════════════════════════════════════════
// Bootstrap.cs
// Entry point for game initialization and scene routing based on save state
// ════════════════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.Managers;

namespace Ascension.Core {}

// ════════════════════════════════════════════════════════════════════════
// GameSystemHub.cs
// Central coordinator for all game systems
// Place as parent GameObject with all managers as children
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using Ascension.Managers;
using Ascension.Systems;

namespace Ascension.Core
{}

// ════════════════════════════════════════════
// CharacterManager.cs
// Single source of truth for player character data
// ════════════════════════════════════════════

using UnityEngine;
using System;
using Ascension.Data.Models;
using Ascension.Data.SO;

namespace Ascension.Managers
{}

// ════════════════════════════════════════════
// DisclaimerController.cs
// Handles user disclaimer acceptance before proceeding to the next scene
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ════════════════════════════════════════════
// GameDatabaseSO.cs
// Central database for all game items, providing categorized access and fast lookup by ID
// ════════════════════════════════════════════

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ascension.Data.SO
{}

// ════════════════════════════════════════════
// GameManager.cs
// Central game controller - delegates player data to CharacterManager
// Handles: Game flow, saves, scene management
// ════════════════════════════════════════════

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.Data.Models;

namespace Ascension.Managers
{}

// ════════════════════════════════════════════
// PlayerHUD.cs
// Player HUD display - subscribes to CharacterManager events
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Managers;

namespace Ascension.UI
{}

// ════════════════════════════════════════════
// PlayerPreviewUI.cs
// Displays player derived stats in UI preview panel
// ════════════════════════════════════════════

using UnityEngine;
using TMPro;
using Ascension.Data.SO;
using Ascension.Systems;

namespace Ascension.UI
{}

// ════════════════════════════════════════════
// PotionManager.cs
// Handles potion usage, effects, and turn-based HoT
// Supports percentage and flat healing values
// ════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ascension.Managers;
using Ascension.Data.SO;

namespace Ascension.Systems
{}

// ════════════════════════════════════════════
// ProfilePanelManager.cs
// Manages player profile panel UI with attribute allocation
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Managers;

namespace Ascension.UI
{}

// ════════════════════════════════════════════
// SaveData.cs
// Serializable save data structure
// ════════════════════════════════════════════

using System;
using UnityEngine;

namespace Ascension.Data.Models
{}

// ════════════════════════════════════════════
// SaveManager.cs
// Manages game state persistence with backup system
// ════════════════════════════════════════════

using System;
using System.IO;
using UnityEngine;
using Ascension.Data.Models;

namespace Ascension.Managers
{}

// ════════════════════════════════════════════
// UIManager.cs
// Handles UI panel navigation and room interactions
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ascension.UI
{}

// ──────────────────────────────────────────────────
// UIManager.cs
// Manages UI panels and navigation between them
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{}