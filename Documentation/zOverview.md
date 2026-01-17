Bootstrap.cs - Entry point ✅
GameManager.cs - Core game state ✅
Scene structure documentation ✅

To properly analyze the flow, I need to see these critical scripts:

ServiceContainer.cs - Since Bootstrap waits for it to initialize
SceneFlowManager.cs - Referenced in GameManager for scene tracking
PersistentUIController.cs - Marked as DontDestroyOnLoad in Bootstrap
SaveManager.cs - Used by GameManager.LoadGame()
CharacterManager.cs - Used by GameManager for player state
DisclaimerController.cs - Used for checking acceptance
SceneManifest.cs (or SceneConfigSO) - Referenced in GameManager

===========================================================================================================

TODO: Possible issues

Critical Issue Spotted (SceneFlowManager):
⚠️ MAJOR PROBLEM: SceneFlowManager tracks _currentMainScene, but Bootstrap loads scenes directly using SceneManager.LoadSceneAsync() instead of going through SceneFlowManager!
Look at this flow:
Bootstrap.cs line ~120:
yield return StartCoroutine(LoadSceneAdditiveAsync(SCENE_AVATAR_CREATION));
// ❌ Direct scene load - SceneFlowManager doesn't know about it!

SceneFlowManager.cs:
private string _currentMainScene = null;  // ❌ Still null!
This will cause:

SceneFlowManager thinks no main scene is loaded
GameManager's CurrentMainScene property returns "NONE"
Future scene transitions will fail
Your debug menu will show incorrect state

Quick Fix Preview (I'll give full solution after seeing all scripts):
csharp// Bootstrap needs to either:
// 1. Use SceneFlowManager.LoadMainScene() instead of direct loading, OR
// 2. Manually set SceneFlowManager._currentMainScene after loading

---

New Critical Issue Found (SaveManager):
✅ SaveManager looks solid - good dependency injection, proper backup system, ordered loading.
⚠️ But I found a potential race condition:
csharp// SaveManager.cs - Line ~106
private void InjectDependencies()
{
    _characterManager = container.GetRequired<CharacterManager>();
    _inventoryManager = container.GetRequired<InventoryManager>();
    _equipmentManager = container.GetRequired<EquipmentManager>();
    _skillLoadoutManager = container.GetRequired<SkillLoadoutManager>();
}
Problem: SaveManager's Initialize() injects dependencies on managers that initialize AFTER it in the ServiceContainer's init order:
csharp// ServiceContainer.cs - Init order
typeof(SaveManager),           // 1st - but needs CharacterManager!
typeof(CharacterManager),      // 2nd
typeof(InventoryManager),      // 3rd
typeof(EquipmentManager),      // 4th
typeof(SkillLoadoutManager),   // 5th
This means: SaveManager.Initialize() runs, injects CharacterManager reference, but CharacterManager.Initialize() hasn't been called yet! The reference exists but the manager isn't ready.
Potential Fix: SaveManager shouldn't call GetRequired<>() in its Initialize() - it should lazy-load dependencies on first use, or the init order needs rethinking.

---

Issue Found (PersistentUIController):
⚠️ Potential Problem:
csharpprivate void Start()
{
    // ✅ Set initial visibility based on Bootstrap scene
    UpdateHUDVisibility("01_Bootstrap");
}
This might cause a flash of visible UI because:

Bootstrap loads (HUD visible by default in scene)
Start() runs (hides HUD)
But there's a 1-frame delay where HUD was visible

Better approach: Set the initial state in the Unity Inspector or in Awake() before any rendering happens.
Also, this line is redundant:
csharp// ✅ IMPORTANT: Ignore Bootstrap scene itself (we're part of it)
if (scene.name == "01_Bootstrap") return;
Since Bootstrap is never "loaded" (it's the starting scene), this check never triggers. The real issue is when Bootstrap loads OTHER scenes additively.

