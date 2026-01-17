// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Data/Config/SceneMetadata.cs
// ✅ Rich metadata for each scene
// ════════════════════════════════════════════════════════════════════════

using System;
using UnityEngine;

namespace Ascension.Data.Config
{
    /// <summary>
    /// Complete configuration for a single scene
    /// All scene behavior is defined here - no more hardcoded arrays!
    /// </summary>
    [Serializable]
    public class SceneMetadata
    {
        [Header("Identity")]
        [Tooltip("Scene name (must match Unity scene file name exactly)")]
        public string sceneName;
        
        [Tooltip("Scene category (Persistent/Content/UI)")]
        public SceneCategory category;
        
        [Header("UI Behavior")]
        [Tooltip("Show PlayerHUD (health, exp, name, level)")]
        public bool showPlayerHUD = true;
        
        [Tooltip("Show GlobalMenu (WorldMap, Profile, Inventory, Quest, Codex buttons)")]
        public bool showGlobalMenu = true;
        
        [Header("Game Systems")]
        [Tooltip("Allow saving in this scene")]
        public bool allowSaving = true;
        
        [Tooltip("Pause game when app loses focus")]
        public bool pauseOnFocusLost = true;
        
        [Header("Optional Metadata")]
        [Tooltip("Display name for UI/debugging (defaults to sceneName if empty)")]
        public string displayName;
        
        [Tooltip("Background music track name (leave empty for silence)")]
        public string musicTrack;
        
        // ════════════════════════════════════════════════════════════════
        // HELPER PROPERTIES
        // ════════════════════════════════════════════════════════════════
        
        public string DisplayName => string.IsNullOrEmpty(displayName) ? sceneName : displayName;
        
        public bool IsValid => !string.IsNullOrWhiteSpace(sceneName);
    }
    
    /// <summary>
    /// Scene categories for organizational clarity
    /// </summary>
    public enum SceneCategory
    {
        Persistent,  // Bootstrap, UI_Persistent (never unload, DontDestroyOnLoad)
        Content,     // MainBase, Town, Dungeon, Combat (main gameplay scenes)
        UI           // UI_Profile, UI_Storage, etc. (additive UI panels)
    }
}