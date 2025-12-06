// ════════════════════════════════════════════
// DisclaimerController.cs
// Handles user disclaimer acceptance before proceeding to the next scene
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DisclaimerController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Panels")]
    [SerializeField] private GameObject disclaimerPanel;
    [SerializeField] private GameObject tosPanel;

    [Header("UI Elements")]
    [SerializeField] private Toggle acknowledgeToggle;
    [SerializeField] private Button agreeButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button tosButton;
    [SerializeField] private Button tosBackButton;

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "01_Bootstrap";

    [Header("Debug Settings")]
    [SerializeField] private bool alwaysShowDisclaimer = false;
    [SerializeField] private bool enableDebugLogs = true;
    #endregion

    #region Private Fields
    private const string DISCLAIMER_KEY = "DisclaimerAccepted";
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (!ValidateReferences())
            return;

        if (!alwaysShowDisclaimer && PlayerPrefs.GetInt(DISCLAIMER_KEY, 0) == 1)
        {
            Log("Disclaimer already accepted, skipping to Bootstrap");
            LoadNextScene();
            return;
        }

        InitializeUI();
    }

    private void Start()
    {
        RegisterListeners();
        Log("Disclaimer scene ready");
    }

#if UNITY_EDITOR
    private void Update()
    {
        HandleDebugInputs();
    }
#endif
    #endregion

    #region Private Methods
    private bool ValidateReferences()
    {
        if (!disclaimerPanel || !tosPanel || !acknowledgeToggle || !agreeButton ||
            !exitButton || !tosButton || !tosBackButton)
        {
            Debug.LogError("[DisclaimerController] One or more UI references are not assigned in the Inspector!");
            return false;
        }
        return true;
    }

    private void InitializeUI()
    {
        agreeButton.interactable = false;
        acknowledgeToggle.isOn = false;
        tosPanel.SetActive(false);
        disclaimerPanel.SetActive(true);
    }

    private void RegisterListeners()
    {
        acknowledgeToggle.onValueChanged.AddListener(OnToggleChanged);
        agreeButton.onClick.AddListener(OnAgree);
        exitButton.onClick.AddListener(OnExit);
        tosButton.onClick.AddListener(OpenTOS);
        tosBackButton.onClick.AddListener(BackToDisclaimer);
    }

    private void OnToggleChanged(bool isOn)
    {
        agreeButton.interactable = isOn;
        Log($"Acknowledge toggle: {(isOn ? "checked" : "unchecked")}");
    }

    private void OnAgree()
    {
        if (!acknowledgeToggle.isOn)
        {
            Debug.LogWarning("[DisclaimerController] Cannot agree without acknowledging!");
            return;
        }

        PlayerPrefs.SetInt(DISCLAIMER_KEY, 1);
        PlayerPrefs.Save();

        Log("Disclaimer accepted, proceeding to Bootstrap");
        LoadNextScene();
    }

    private void OnExit()
    {
        Log("User exited from disclaimer");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OpenTOS()
    {
        tosPanel.SetActive(true);
        disclaimerPanel.SetActive(false);
        Log("Opened Terms of Service panel");
    }

    private void BackToDisclaimer()
    {
        tosPanel.SetActive(false);
        disclaimerPanel.SetActive(true);
        Log("Returned to Disclaimer panel");
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError("[DisclaimerController] Next scene name is not assigned!");
            return;
        }
        SceneManager.LoadScene(nextSceneName);
    }

    private void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[DisclaimerController] {message}");
    }

#if UNITY_EDITOR
    private void HandleDebugInputs()
    {
        if (Input.GetKeyDown(KeyCode.F9))
            ResetDisclaimerAcceptance();

        if (Input.GetKeyDown(KeyCode.F10))
        {
            ForceAcceptDisclaimer();
            LoadNextScene();
        }
    }
#endif
    #endregion

    #region Debug Helpers
    [ContextMenu("Reset Disclaimer Acceptance")]
    public void ResetDisclaimerAcceptance()
    {
        PlayerPrefs.DeleteKey(DISCLAIMER_KEY);
        PlayerPrefs.Save();
        Debug.Log("[DisclaimerController] Disclaimer acceptance reset!");
    }

    [ContextMenu("Force Accept Disclaimer")]
    public void ForceAcceptDisclaimer()
    {
        PlayerPrefs.SetInt(DISCLAIMER_KEY, 1);
        PlayerPrefs.Save();
        Debug.Log("[DisclaimerController] Disclaimer force-accepted!");
    }
    #endregion
}