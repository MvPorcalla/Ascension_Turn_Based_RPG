// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Data/Config/SceneManifest.cs
// ✅ Central scene registry with editor auto-population
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ascension.Data.Config
{
    [CreateAssetMenu(fileName = "SceneManifest", menuName = "UI/Configuration/Scene Manifest")]
    public class SceneManifest : ScriptableObject
    {
        [SerializeField] private List<SceneMetadata> scenes = new List<SceneMetadata>();
        
        // ════════════════════════════════════════════════════════════════
        // PUBLIC API - Query by Scene Name
        // ════════════════════════════════════════════════════════════════
        
        /// <summary>
        /// Get complete metadata for a scene (returns null if not found)
        /// </summary>
        public SceneMetadata GetSceneData(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return null;
            return scenes.Find(s => s.sceneName == sceneName);
        }
        
        /// <summary>
        /// Check if scene exists in manifest
        /// </summary>
        public bool HasScene(string sceneName)
        {
            return GetSceneData(sceneName) != null;
        }
        
        /// <summary>
        /// Check if scene is a content scene
        /// </summary>
        public bool IsContentScene(string sceneName)
        {
            var data = GetSceneData(sceneName);
            return data != null && data.category == SceneCategory.Content;
        }
        
        /// <summary>
        /// Check if scene is a UI scene
        /// </summary>
        public bool IsUIScene(string sceneName)
        {
            var data = GetSceneData(sceneName);
            return data != null && data.category == SceneCategory.UI;
        }
        
        /// <summary>
        /// Check if scene is persistent (should never be unloaded)
        /// </summary>
        public bool IsPersistentScene(string sceneName)
        {
            var data = GetSceneData(sceneName);
            return data != null && data.category == SceneCategory.Persistent;
        }
        
        // ════════════════════════════════════════════════════════════════
        // PUBLIC API - Query by Category
        // ════════════════════════════════════════════════════════════════
        
        /// <summary>
        /// Get all scenes of a specific category
        /// </summary>
        public List<SceneMetadata> GetScenesByCategory(SceneCategory category)
        {
            return scenes.FindAll(s => s.category == category);
        }
        
        /// <summary>
        /// Get scene names for a category (useful for validation/debugging)
        /// </summary>
        public List<string> GetSceneNames(SceneCategory category)
        {
            return scenes
                .Where(s => s.category == category)
                .Select(s => s.sceneName)
                .ToList();
        }
        
        /// <summary>
        /// Get formatted list of scenes for error messages
        /// </summary>
        public string GetSceneNamesFormatted(SceneCategory category)
        {
            var names = GetSceneNames(category);
            return names.Count > 0 ? string.Join(", ", names) : "NONE";
        }
        
        // ════════════════════════════════════════════════════════════════
        // EDITOR VALIDATION
        // ════════════════════════════════════════════════════════════════
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            RemoveInvalidEntries();
            ValidateSceneNamesExist();
            CheckForDuplicates();
        }
        
        private void RemoveInvalidEntries()
        {
            scenes.RemoveAll(s => !s.IsValid);
        }
        
        /// <summary>
        /// ✅ CRITICAL: Verify all scenes exist in Build Settings
        /// This catches typos and missing scenes at edit-time!
        /// </summary>
        private void ValidateSceneNamesExist()
        {
            var buildScenes = EditorBuildSettings.scenes
                .Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path))
                .ToHashSet();
            
            foreach (var sceneData in scenes)
            {
                if (!buildScenes.Contains(sceneData.sceneName))
                {
                    Debug.LogError(
                        $"[SceneManifest] Scene '{sceneData.sceneName}' not found in Build Settings!\n" +
                        $"Either add it to Build Settings or remove from this manifest.",
                        this
                    );
                }
            }
        }
        
        private void CheckForDuplicates()
        {
            var duplicates = scenes
                .GroupBy(s => s.sceneName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
            
            foreach (var duplicate in duplicates)
            {
                Debug.LogWarning($"[SceneManifest] Duplicate scene entry: '{duplicate}'", this);
            }
        }
        
        // ════════════════════════════════════════════════════════════════
        // EDITOR TOOLS
        // ════════════════════════════════════════════════════════════════
        
        [ContextMenu("Auto-Populate from Build Settings")]
        private void AutoPopulateFromBuildSettings()
        {
            if (!EditorUtility.DisplayDialog(
                "Auto-Populate Scene Manifest",
                "This will CLEAR all existing entries and rebuild from Build Settings.\n\nContinue?",
                "Yes, Rebuild",
                "Cancel"))
            {
                return;
            }
            
            scenes.Clear();
            
            foreach (var buildScene in EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled) continue;
                
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(buildScene.path);
                
                scenes.Add(new SceneMetadata
                {
                    sceneName = sceneName,
                    category = InferCategory(sceneName),
                    showPlayerHUD = ShouldShowHUD(sceneName),
                    showGlobalMenu = ShouldShowMenu(sceneName),
                    allowSaving = ShouldAllowSaving(sceneName)
                });
            }
            
            EditorUtility.SetDirty(this);
            Debug.Log($"[SceneManifest] ✓ Auto-populated {scenes.Count} scenes from Build Settings", this);
        }
        
        /// <summary>
        /// Smart category inference based on scene naming conventions
        /// </summary>
        private SceneCategory InferCategory(string sceneName)
        {
            if (sceneName.StartsWith("UI_")) return SceneCategory.UI;
            if (sceneName.Contains("Bootstrap")) return SceneCategory.Persistent;
            if (sceneName.Contains("Persistent")) return SceneCategory.Persistent;
            return SceneCategory.Content;
        }
        
        /// <summary>
        /// Smart HUD visibility inference
        /// </summary>
        private bool ShouldShowHUD(string sceneName)
        {
            // Hide HUD on bootstrap/disclaimer/avatar creation
            if (sceneName.Contains("Bootstrap")) return false;
            if (sceneName.Contains("Disclaimer")) return false;
            if (sceneName.Contains("AvatarCreation")) return false;
            if (sceneName.Contains("Persistent")) return false; // UI_Persistent manages HUD itself
            
            return true;
        }
        
        /// <summary>
        /// Smart menu visibility inference
        /// </summary>
        private bool ShouldShowMenu(string sceneName)
        {
            // Hide menu during combat or avatar creation
            if (sceneName.Contains("Combat")) return false;
            if (sceneName.Contains("AvatarCreation")) return false;
            if (sceneName.Contains("Bootstrap")) return false;
            if (sceneName.Contains("Disclaimer")) return false;
            if (sceneName.Contains("Persistent")) return false;
            
            return true;
        }
        
        /// <summary>
        /// Smart saving permission inference
        /// </summary>
        private bool ShouldAllowSaving(string sceneName)
        {
            // Don't allow saving during avatar creation or bootstrap
            if (sceneName.Contains("AvatarCreation")) return false;
            if (sceneName.Contains("Bootstrap")) return false;
            if (sceneName.Contains("Disclaimer")) return false;
            
            return true;
        }
#endif
        
        // ════════════════════════════════════════════════════════════════
        // DEBUG HELPERS
        // ════════════════════════════════════════════════════════════════
        
        [ContextMenu("Print Scene Manifest")]
        private void PrintManifest()
        {
            Debug.Log("=== SCENE MANIFEST ===");
            
            foreach (SceneCategory category in Enum.GetValues(typeof(SceneCategory)))
            {
                var categoryScenes = GetScenesByCategory(category);
                Debug.Log($"\n{category} Scenes ({categoryScenes.Count}):");
                
                foreach (var scene in categoryScenes)
                {
                    Debug.Log(
                        $"  - {scene.sceneName}\n" +
                        $"      HUD: {scene.showPlayerHUD}, " +
                        $"Menu: {scene.showGlobalMenu}, " +
                        $"Save: {scene.allowSaving}"
                    );
                }
            }
        }
    }
}