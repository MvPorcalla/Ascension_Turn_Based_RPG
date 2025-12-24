// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\StorageSystem\Controller\StorageRoomController.cs
// High-level coordinator for Storage Room scene
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using Ascension.Inventory.Manager;

namespace Ascension.Storage.UI
{
    /// <summary>
    /// Main controller for the Storage Room scene.
    /// Coordinates sub-panels and handles room-level actions.
    /// </summary>
    public class StorageRoomController : MonoBehaviour
    {
        [Header("Sub-Panels")]
        [SerializeField] private BagInventoryUI bagInventoryUI;
        [SerializeField] private PocketInventoryUI pocketInventoryUI;
        [SerializeField] private StorageInventoryUI storageInventoryUI;

        [Header("Quick Actions")]
        [SerializeField] private Button storeAllButton;
        [SerializeField] private Button backButton;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (storeAllButton != null)
                storeAllButton.onClick.AddListener(OnStoreAllBagClicked);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        #region Quick Actions

        private void OnStoreAllBagClicked()
        {
            InventoryManager.Instance.Inventory.StoreAllItems();
            Debug.Log("[StorageRoomController] All bag items stored!");
        }

        private void OnBackClicked()
        {
            Debug.Log("[StorageRoomController] Back button clicked");
            gameObject.SetActive(false);
        }

        #endregion
    }
}