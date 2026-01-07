// ════════════════════════════════════════════
// UIManager.cs
// Handles base navigation, full-screen panels,
// and generic room panels (no hard-coded rooms)
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

        #region Room Definition

        [System.Serializable]
        public class RoomUI
        {
            public string roomId;
            public Button openButton;
            public GameObject panel;
            public Button backButton;
        }

        #endregion

        #region Serialized Fields

        [Header("Base Panels")]
        [SerializeField] private GameObject mainBasePanel;
        [SerializeField] private GameObject worldMapPanel;

        [Header("Rooms")]
        [SerializeField] private RoomUI[] rooms;

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

            currentBasePanel?.SetActive(false);

            panel.SetActive(true);
            currentFullScreenPanel = panel;
        }

        public void CloseCurrentFullScreenPanel()
        {
            if (currentFullScreenPanel == null) return;

            currentFullScreenPanel.SetActive(false);
            currentFullScreenPanel = null;

            currentBasePanel?.SetActive(true);
        }

        public void OpenRoomPanel(GameObject roomPanel)
        {
            if (roomPanel == null) return;

            CloseAllRoomPanels();

            roomPanel.SetActive(true);
            currentRoomPanel = roomPanel;

            currentBasePanel?.SetActive(false);
        }

        public void CloseCurrentRoomPanel()
        {
            if (currentRoomPanel != null)
            {
                currentRoomPanel.SetActive(false);
                currentRoomPanel = null;
            }

            currentBasePanel?.SetActive(true);
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

            // Rooms (generic)
            foreach (var room in rooms)
            {
                if (room.openButton != null && room.panel != null)
                {
                    room.openButton.onClick.AddListener(
                        () => OpenRoomPanel(room.panel)
                    );
                }

                room.backButton?.onClick.AddListener(CloseCurrentRoomPanel);
            }

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
            if (rooms == null) return;

            foreach (var room in rooms)
                room.panel?.SetActive(false);

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
