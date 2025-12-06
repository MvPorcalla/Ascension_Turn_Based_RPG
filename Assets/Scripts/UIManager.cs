// ════════════════════════════════════════════
// UIManager.cs
// Handles UI panel navigation and room interactions
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ascension.UI
{
    public class UIManager : MonoBehaviour
    {
        #region Singleton
        public static UIManager Instance { get; private set; }
        #endregion

        #region Serialized Fields

        [Header("Base Panels")]
        [SerializeField] private GameObject mainBasePanel;
        [SerializeField] private GameObject worldMapPanel;

        [Header("Room Buttons (Main Base)")]
        [SerializeField] private Button equipmentRoomButton;
        [SerializeField] private Button storageRoomButton;

        [Header("Room Panels")]
        [SerializeField] private GameObject equipmentRoomPanel;
        [SerializeField] private GameObject storageRoomPanel;

        [Header("Room Back Buttons")]
        [SerializeField] private Button equipmentRoomBackButton;
        [SerializeField] private Button storageRoomBackButton;

        [Header("Game Menu Buttons")]
        [SerializeField] private Button worldMapButton;
        [SerializeField] private Button profileButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button questButton;
        [SerializeField] private Button codexButton;

        [Header("Full-Screen Panels")]
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject questPanel;
        [SerializeField] private GameObject codexPanel;

        [Header("Full-Screen Back Buttons")]
        [SerializeField] private Button profileBackButton;
        [SerializeField] private Button inventoryBackButton;
        [SerializeField] private Button questBackButton;
        [SerializeField] private Button codexBackButton;

        [Header("World Map Buttons")]
        [SerializeField] private Button homeBaseButton;

        [Header("HUD")]
        [SerializeField] private TMP_Text locationText;

        #endregion

        #region Private Fields

        private GameObject currentBasePanel;
        private GameObject currentFullScreenPanel;
        private GameObject currentRoomPanel;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeButtons();
            SetBasePanel(mainBasePanel);
            CloseAllFullScreenPanels();
            CloseAllRoomPanels();
        }

        #endregion

        #region Public Methods

        public void SetBasePanel(GameObject panel)
        {
            mainBasePanel.SetActive(panel == mainBasePanel);
            worldMapPanel.SetActive(panel == worldMapPanel);

            currentBasePanel = panel;
            UpdateLocationText();
        }

        public void OpenWorldMap() => SetBasePanel(worldMapPanel);
        public void ReturnToHomeBase() => SetBasePanel(mainBasePanel);

        public void OpenPanel(GameObject panel)
        {
            if (panel == null) return;

            CloseAllFullScreenPanels();

            if (currentBasePanel != null)
                currentBasePanel.SetActive(false);

            panel.SetActive(true);
            currentFullScreenPanel = panel;
        }

        public void CloseCurrentFullScreenPanel()
        {
            if (currentFullScreenPanel == null) return;

            currentFullScreenPanel.SetActive(false);
            currentFullScreenPanel = null;

            if (currentBasePanel != null)
                currentBasePanel.SetActive(true);
        }

        public void OpenRoomPanel(GameObject roomPanel)
        {
            if (roomPanel == null) return;

            CloseAllRoomPanels();
            roomPanel.SetActive(true);
            currentRoomPanel = roomPanel;

            if (currentBasePanel != null)
                currentBasePanel.SetActive(false);
        }

        public void CloseCurrentRoomPanel()
        {
            if (currentRoomPanel != null)
            {
                currentRoomPanel.SetActive(false);
                currentRoomPanel = null;
            }

            if (currentBasePanel != null)
                currentBasePanel.SetActive(true);
        }

        #endregion

        #region Private Methods

        private void InitializeButtons()
        {
            // Game menu
            worldMapButton?.onClick.AddListener(OpenWorldMap);
            profileButton?.onClick.AddListener(() => OpenPanel(profilePanel));
            inventoryButton?.onClick.AddListener(() => OpenPanel(inventoryPanel));
            questButton?.onClick.AddListener(() => OpenPanel(questPanel));
            codexButton?.onClick.AddListener(() => OpenPanel(codexPanel));

            profileBackButton?.onClick.AddListener(CloseCurrentFullScreenPanel);
            inventoryBackButton?.onClick.AddListener(CloseCurrentFullScreenPanel);
            questBackButton?.onClick.AddListener(CloseCurrentFullScreenPanel);
            codexBackButton?.onClick.AddListener(CloseCurrentFullScreenPanel);

            // Room buttons
            equipmentRoomButton?.onClick.AddListener(() => OpenRoomPanel(equipmentRoomPanel));
            storageRoomButton?.onClick.AddListener(() => OpenRoomPanel(storageRoomPanel));

            equipmentRoomBackButton?.onClick.AddListener(CloseCurrentRoomPanel);
            storageRoomBackButton?.onClick.AddListener(CloseCurrentRoomPanel);

            // World map
            homeBaseButton?.onClick.AddListener(ReturnToHomeBase);
        }

        private void CloseAllFullScreenPanels()
        {
            profilePanel?.SetActive(false);
            inventoryPanel?.SetActive(false);
            questPanel?.SetActive(false);
            codexPanel?.SetActive(false);

            currentFullScreenPanel = null;
        }

        private void CloseAllRoomPanels()
        {
            equipmentRoomPanel?.SetActive(false);
            storageRoomPanel?.SetActive(false);

            currentRoomPanel = null;
        }

        private void UpdateLocationText()
        {
            if (locationText == null) return;

            if (currentBasePanel == mainBasePanel)
                locationText.text = "Home Base";
            else if (currentBasePanel == worldMapPanel)
                locationText.text = "World Map";
        }

        #endregion
    }
}
