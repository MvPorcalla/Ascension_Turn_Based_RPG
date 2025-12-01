// ──────────────────────────────────────────────────
// UIManager.cs
// Manages UI panels and navigation between them
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Base Panels")]
    [SerializeField] private GameObject mainBasePanel;
    [SerializeField] private GameObject worldMapPanel;

    // ---------------------------------------------------------------

    [Header("Room Buttons (MainBase)")]
    [SerializeField] private Button equipmentRoomButton;
    // [SerializeField] private Button cookingHallButton;
    // [SerializeField] private Button potionSanctumButton;
    // [SerializeField] private Button forgeRoomButton;
    [SerializeField] private Button storageRoomButton;
    // [SerializeField] private Button companionHallButton;
    // [SerializeField] private Button trainingRoomButton;
    // [SerializeField] private Button achievementHallButton;
    // [SerializeField] private Button patchNotesButton;

    // Add more room buttons here later
    
    [Header("Room Panels")]
    [SerializeField] private GameObject equipmentRoomPanel;
    // [SerializeField] private GameObject cookingHallPanel;
    // [SerializeField] private GameObject potionSanctumPanel;
    // [SerializeField] private GameObject forgeRoomPanel;
    [SerializeField] private GameObject storageRoomPanel;
    // [SerializeField] private GameObject companionHallPanel;
    // [SerializeField] private GameObject trainingRoomPanel;
    // [SerializeField] private GameObject achievementHallPanel;
    // [SerializeField] private GameObject patchNotesPanel;

    // Add more room panels here later
    // etc...

    [Header("Room Back Buttons")]
    [SerializeField] private Button equipmentRoomBackButton;
    // [SerializeField] private Button cookingHallBackButton;
    // [SerializeField] private Button potionSanctumBackButton;
    // [SerializeField] private Button forgeRoomBackButton;
    [SerializeField] private Button storageRoomBackButton;
    // [SerializeField] private Button companionHallBackButton;
    // [SerializeField] private Button trainingRoomBackButton;
    // [SerializeField] private Button achievementHallBackButton;
    // [SerializeField] private Button patchNotesBackButton;

    // Add more room back buttons here later

    // ---------------------------------------------------------------

    [Header("GameMenu Buttons")]
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

    [Header("Back Buttons")]
    [SerializeField] private Button profileBackButton;
    [SerializeField] private Button inventoryBackButton;
    [SerializeField] private Button questBackButton;
    [SerializeField] private Button codexBackButton;

    // ---------------------------------------------------------------

    [Header("World Map Buttons")]
    [SerializeField] private Button homeBaseButton;
    // [SerializeField] private Button TowerButton;         // open new scene
    // [SerializeField] private Button DungeonButton;       // open new scene
    // [SerializeField] private Button CaptialCityButton;   // open new scene
    // [SerializeField] private Button TownCity1Button;     // open new scene
    // [SerializeField] private Button TownCity2Button;     // same scene as TownCity1


    [Header("HUD")]
    [SerializeField] private TMP_Text locationText;

    private GameObject currentBasePanel;
    private GameObject currentFullScreenPanel;
    private GameObject currentRoomPanel;

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

    private void InitializeButtons()
    {
        // GameMenu buttons
        worldMapButton.onClick.AddListener(OpenWorldMap);
        profileButton.onClick.AddListener(() => OpenPanel(profilePanel));
        inventoryButton.onClick.AddListener(() => OpenPanel(inventoryPanel));
        questButton.onClick.AddListener(() => OpenPanel(questPanel));
        codexButton.onClick.AddListener(() => OpenPanel(codexPanel));

        // Room buttons
        equipmentRoomButton.onClick.AddListener(() => OpenRoomPanel(equipmentRoomPanel));
        storageRoomButton.onClick.AddListener(() => OpenRoomPanel(storageRoomPanel));
        // Add more room button listeners here

        // Back buttons
        homeBaseButton.onClick.AddListener(ReturnToHomeBase);
        profileBackButton.onClick.AddListener(CloseCurrentFullScreenPanel);
        inventoryBackButton.onClick.AddListener(CloseCurrentFullScreenPanel);
        questBackButton.onClick.AddListener(CloseCurrentFullScreenPanel);
        codexBackButton.onClick.AddListener(CloseCurrentFullScreenPanel);

        equipmentRoomBackButton.onClick.AddListener(CloseCurrentRoomPanel);
        storageRoomBackButton.onClick.AddListener(CloseCurrentRoomPanel);
    }

    #region Base Panel

    public void SetBasePanel(GameObject panel)
    {
        mainBasePanel.SetActive(panel == mainBasePanel);
        worldMapPanel.SetActive(panel == worldMapPanel);
        currentBasePanel = panel;
        UpdateLocationText();
    }

    public void OpenWorldMap() => SetBasePanel(worldMapPanel);
    public void ReturnToHomeBase() => SetBasePanel(mainBasePanel);

    #endregion

    #region Full-Screen Panels

public void OpenPanel(GameObject panel)
{
    CloseAllFullScreenPanels();

    // Hide base panel
    if (currentBasePanel != null)
        currentBasePanel.SetActive(false);

    panel.SetActive(true);
    currentFullScreenPanel = panel;
}

public void CloseCurrentFullScreenPanel()
{
    if (currentFullScreenPanel != null)
    {
        currentFullScreenPanel.SetActive(false);
        currentFullScreenPanel = null;

        // Show base panel again
        if (currentBasePanel != null)
            currentBasePanel.SetActive(true);
    }
}


    private void CloseAllFullScreenPanels()
    {
        profilePanel.SetActive(false);
        inventoryPanel.SetActive(false);
        questPanel.SetActive(false);
        codexPanel.SetActive(false);
        currentFullScreenPanel = null;
    }

    #endregion

    #region Room Panels

    public void OpenRoomPanel(GameObject roomPanel)
    {
        CloseAllRoomPanels();
        roomPanel.SetActive(true);
        currentRoomPanel = roomPanel;
        
        // Hide base panel when room is open
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
        
        // Show base panel again
        if (currentBasePanel != null)
            currentBasePanel.SetActive(true);
    }

    private void CloseAllRoomPanels()
    {
        equipmentRoomPanel.SetActive(false);
        storageRoomPanel.SetActive(false);
        // Add more room panels here
        currentRoomPanel = null;
    }

    #endregion

    #region Location Text

    private void UpdateLocationText()
    {
        if (currentBasePanel == mainBasePanel)
            locationText.text = "Home Base";
        else if (currentBasePanel == worldMapPanel)
            locationText.text = "World Map";
    }

    #endregion
}