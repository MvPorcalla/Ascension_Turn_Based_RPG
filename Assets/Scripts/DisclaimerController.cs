// ──────────────────────────────────────────────────
// DisclaimerController.cs
// Handles: User disclaimer acceptance before proceeding
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DisclaimerController : MonoBehaviour
{
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
    [SerializeField] private bool alwaysShowDisclaimer = false; // Force show even if accepted
    [SerializeField] private bool enableDebugLogs = true;

    private const string DisclaimerKey = "DisclaimerAccepted";

    private void Awake()
    {
        // Safety check first
        if (!ValidateReferences())
            return;

        // Check if disclaimer should be skipped (unless debug flag is on)
        if (!alwaysShowDisclaimer && PlayerPrefs.GetInt(DisclaimerKey, 0) == 1)
        {
            Log("Disclaimer already accepted, skipping to Bootstrap");
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        // Set initial UI state BEFORE the first frame
        InitializeUI();
    }

    private void Start()
    {
        // Setup listeners
        acknowledgeToggle.onValueChanged.AddListener(OnToggleChanged);
        agreeButton.onClick.AddListener(OnAgree);
        exitButton.onClick.AddListener(OnExit);
        tosButton.onClick.AddListener(OpenTOS);
        tosBackButton.onClick.AddListener(BackToDisclaimer);

        Log("Disclaimer scene ready");
    }

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
        acknowledgeToggle.isOn = false; // Ensure toggle starts unchecked
        tosPanel.SetActive(false);
        disclaimerPanel.SetActive(true);
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

        PlayerPrefs.SetInt(DisclaimerKey, 1);
        PlayerPrefs.Save();
        
        Log("Disclaimer accepted, proceeding to Bootstrap");
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnExit()
    {
        Log("User exited from disclaimer");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
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

    private void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[DisclaimerController] {message}");
    }

    #region Debug Helpers

    /// <summary>
    /// Reset disclaimer acceptance (for testing)
    /// </summary>
    [ContextMenu("Reset Disclaimer Acceptance")]
    public void ResetDisclaimerAcceptance()
    {
        PlayerPrefs.DeleteKey(DisclaimerKey);
        PlayerPrefs.Save();
        Debug.Log("[DisclaimerController] Disclaimer acceptance reset!");
    }

    /// <summary>
    /// Quick test: Force accept disclaimer
    /// </summary>
    [ContextMenu("Force Accept Disclaimer")]
    public void ForceAcceptDisclaimer()
    {
        PlayerPrefs.SetInt(DisclaimerKey, 1);
        PlayerPrefs.Save();
        Debug.Log("[DisclaimerController] Disclaimer force-accepted!");
    }

#if UNITY_EDITOR
    private void Update()
    {
        // F9: Reset disclaimer (for testing)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            ResetDisclaimerAcceptance();
        }
        
        // F10: Force accept and proceed
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ForceAcceptDisclaimer();
            SceneManager.LoadScene(nextSceneName);
        }
    }
#endif

    #endregion
}