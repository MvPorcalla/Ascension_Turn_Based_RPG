**Scripts/PlayerSystem** - will refactor later to add namespace

// ──────────────────────────────────────────────────
// PlayerStats.cs
// Core player statistics and systems
// Single source of truth for player data
// ──────────────────────────────────────────────────

using UnityEngine;
using Ascension.Data.SO;
using Ascension.Systems;

[System.Serializable]
public class PlayerStats
{}

// ──────────────────────────────────────────────────
// PlayerLevelSystem.cs
// Manages player leveling, EXP, and transcendence
// ──────────────────────────────────────────────────

using System;
using UnityEngine;

[Serializable]
public class PlayerLevelSystem
{}

// ──────────────────────────────────────────────────
// PlayerItemStats.cs (IMPROVED VERSION)
// ──────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerItemStats
{}

// ──────────────────────────────────────────────────
// PlayerDeriveStats.cs
// Calculates and holds the player's derived stats
// ──────────────────────────────────────────────────

using System;
using UnityEngine;
using Ascension.Data.SO;
using Ascension.Systems;

[Serializable]
public class PlayerDerivedStats
{}

// ──────────────────────────────────────────────────
// PlayerData.cs
// Serializable player data for saving/loading
// ──────────────────────────────────────────────────

using System;

[Serializable]
public class PlayerData
{}

// ──────────────────────────────────────────────────
// PlayerCombatRuntime.cs
// Manages runtime combat state for the player
// ──────────────────────────────────────────────────

using System;
using UnityEngine;

[Serializable]
public class PlayerCombatRuntime
{}

// ──────────────────────────────────────────────────
// PlayerAttribute.cs
// Holds base player attributes (STR, INT, etc.)
// ──────────────────────────────────────────────────

using System;

[Serializable]
public class PlayerAttributes
{}

// ──────────────────────────────────────────────────
// LevelUpManager.cs
// Manages the level-up UI and attribute allocation
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Managers;

public class LevelUpManager : MonoBehaviour
{}

// ──────────────────────────────────────────────────
// CharacterBaseStatsSO.cs
// ScriptableObject holding base character stats and scaling
// ──────────────────────────────────────────────────

using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "BaseStats/CharacterStats")]
public class CharacterBaseStatsSO : ScriptableObject
{}

// ──────────────────────────────────────────────────
// AvatarCreationManager.cs
// Manages the avatar creation process and UI
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Ascension.Managers;

public class AvatarCreationManager : MonoBehaviour
{}