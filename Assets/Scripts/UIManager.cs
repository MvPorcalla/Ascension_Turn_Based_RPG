// -------------------------------
// UIManager.cs
// -------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Base Panels")]
    [SerializeField] private GameObject mainBasePanel;
    [SerializeField] private GameObject worldMapPanel;

    [Header("Full-Screen Panels")]
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject questPanel;
    [SerializeField] private GameObject codexPanel;

    [Header("GameMenu Buttons")]
    [SerializeField] private Button worldMapButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button questButton;
    [SerializeField] private Button codexButton;

    [Header("Back Buttons")]
    [SerializeField] private Button worldMapBackButton;
    [SerializeField] private Button profileBackButton;
    [SerializeField] private Button inventoryBackButton;
    [SerializeField] private Button questBackButton;
    [SerializeField] private Button codexBackButton;

    [Header("HUD")]
    [SerializeField] private TMP_Text locationText;

    private GameObject currentBasePanel;
    private GameObject currentFullScreenPanel;

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
    }

    private void InitializeButtons()
    {
        // GameMenu buttons
        worldMapButton.onClick.AddListener(OpenWorldMap);
        profileButton.onClick.AddListener(() => OpenPanel(profilePanel));
        inventoryButton.onClick.AddListener(() => OpenPanel(inventoryPanel));
        questButton.onClick.AddListener(() => OpenPanel(questPanel));
        codexButton.onClick.AddListener(() => OpenPanel(codexPanel));

        // Back buttons
        worldMapBackButton.onClick.AddListener(ReturnToHomeBase);
        profileBackButton.onClick.AddListener(CloseCurrentFullScreenPanel);
        inventoryBackButton.onClick.AddListener(CloseCurrentFullScreenPanel);
        questBackButton.onClick.AddListener(CloseCurrentFullScreenPanel);
        codexBackButton.onClick.AddListener(CloseCurrentFullScreenPanel);
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
        panel.SetActive(true);
        currentFullScreenPanel = panel;
    }

    public void CloseCurrentFullScreenPanel()
    {
        if (currentFullScreenPanel != null)
        {
            currentFullScreenPanel.SetActive(false);
            currentFullScreenPanel = null;
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