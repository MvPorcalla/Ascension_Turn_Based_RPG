// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Controllers/GlobalMenuController.cs
// ✅ RENAMED from GameMenuController
// ✅ FIXED: Handles persistent navigation (WorldMap is special case)
// Attach to: 02_UIPersistent → PersistentUICanvas → HUDLayer → GlobalMenu
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using Ascension.App;

namespace Ascension.Controllers
{
    /// <summary>
    /// Controls the 5 persistent navigation buttons visible in all gameplay scenes
    /// WorldMap button behavior changes based on current scene context
    /// </summary>
    public class GlobalMenuController : MonoBehaviour
    {
        [Header("Navigation Buttons")]
        [SerializeField] private Button worldMapButton;
        [SerializeField] private Button profileButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button questButton;
        [SerializeField] private Button codexButton;
        
        private void Start()
        {
            SetupButtons();
        }
        
        private void OnDestroy()
        {
            CleanupButtons();
        }
        
        private void SetupButtons()
        {
            worldMapButton?.onClick.AddListener(OnWorldMapClicked);
            profileButton?.onClick.AddListener(OnProfileClicked);
            inventoryButton?.onClick.AddListener(OnInventoryClicked);
            questButton?.onClick.AddListener(OnQuestClicked);
            codexButton?.onClick.AddListener(OnCodexClicked);
        }
        
        private void CleanupButtons()
        {
            worldMapButton?.onClick.RemoveAllListeners();
            profileButton?.onClick.RemoveAllListeners();
            inventoryButton?.onClick.RemoveAllListeners();
            questButton?.onClick.RemoveAllListeners();
            codexButton?.onClick.RemoveAllListeners();
        }
        
        #region Button Handlers
        private void OnWorldMapClicked()
        {
            SceneFlowManager.Instance?.OpenWorldMap();
        }
        
        private void OnProfileClicked()
        {
            SceneFlowManager.Instance?.OpenProfile();
        }
        
        private void OnInventoryClicked()
        {
            SceneFlowManager.Instance?.OpenInventory();
        }
        
        private void OnQuestClicked()
        {
            SceneFlowManager.Instance?.OpenQuest();
        }
        
        private void OnCodexClicked()
        {
            SceneFlowManager.Instance?.OpenCodex();
        }
        #endregion
    }
}