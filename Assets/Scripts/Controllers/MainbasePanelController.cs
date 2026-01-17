// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Controllers/MainbasePanelController.cs
// ✅ FIXED: Mainbase only handles room buttons (no panel switching)
// Attach to: 04_Mainbase → Canvas → MainBasePanel
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using Ascension.App;

namespace Ascension.Controllers
{
    /// <summary>
    /// Manages room buttons in the Mainbase scene
    /// Each button opens an additive UI scene via SceneFlowManager
    /// </summary>
    public class MainbasePanelController : MonoBehaviour
    {
        [Header("Room Buttons")]
        [SerializeField] private Button storageRoomButton;
        [SerializeField] private Button equipmentRoomButton;
        [SerializeField] private Button cookingRoomButton;
        [SerializeField] private Button brewingRoomButton;
        [SerializeField] private Button craftingRoomButton;
        
        #region Unity Callbacks
        private void Start()
        {
            SetupButtons();
        }
        
        private void OnDestroy()
        {
            CleanupButtons();
        }
        #endregion
        
        #region Button Setup
        private void SetupButtons()
        {
            storageRoomButton?.onClick.AddListener(OnStorageRoomClicked);
            equipmentRoomButton?.onClick.AddListener(OnEquipmentRoomClicked);
            cookingRoomButton?.onClick.AddListener(OnCookingRoomClicked);
            brewingRoomButton?.onClick.AddListener(OnBrewingRoomClicked);
            craftingRoomButton?.onClick.AddListener(OnCraftingRoomClicked);
        }
        
        private void CleanupButtons()
        {
            storageRoomButton?.onClick.RemoveAllListeners();
            equipmentRoomButton?.onClick.RemoveAllListeners();
            cookingRoomButton?.onClick.RemoveAllListeners();
            brewingRoomButton?.onClick.RemoveAllListeners();
            craftingRoomButton?.onClick.RemoveAllListeners();
        }
        #endregion
        
        #region Room Button Handlers
        private void OnStorageRoomClicked()
        {
            Debug.Log("[MainbaseController] Opening Storage Room");
            SceneFlowManager.Instance?.OpenStorage();
        }
        
        private void OnEquipmentRoomClicked()
        {
            Debug.Log("[MainbaseController] Equipment Room not yet implemented");
            // SceneFlowManager.Instance?.OpenEquipment();
        }
        
        private void OnCookingRoomClicked()
        {
            Debug.Log("[MainbaseController] Cooking Room not yet implemented");
            // SceneFlowManager.Instance?.OpenCooking();
        }
        
        private void OnBrewingRoomClicked()
        {
            Debug.Log("[MainbaseController] Brewing Room not yet implemented");
            // SceneFlowManager.Instance?.OpenBrewing();
        }
        
        private void OnCraftingRoomClicked()
        {
            Debug.Log("[MainbaseController] Crafting Room not yet implemented");
            // SceneFlowManager.Instance?.OpenCrafting();
        }
        #endregion
    }
}