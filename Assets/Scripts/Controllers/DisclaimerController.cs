// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Controllers/DisclaimerController.cs
// ✅ CORRECTED: This is the FIRST scene that loads (scene index 0)
// Attach to: 00_Disclaimer → Canvas
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ascension.Controllers
{
    /// <summary>
    /// ⚠️ CRITICAL: This scene loads FIRST (before Bootstrap)
    /// - No game systems are active
    /// - No managers initialized
    /// - Completely isolated
    /// </summary>
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
        [SerializeField] private bool skipDisclaimerForTesting = false;
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

            // ✅ Check if disclaimer was already accepted
            if (skipDisclaimerForTesting || HasAcceptedDisclaimer())
            {
                Log("Disclaimer already accepted, proceeding to Bootstrap");
                LoadBootstrapScene();
                return;
            }

            // First time - show disclaimer
            InitializeUI();
        }

        private void Start()
        {
            // Only register listeners if we're showing the disclaimer
            if (disclaimerPanel.activeSelf)
            {
                RegisterListeners();
                Log("Disclaimer scene ready - awaiting user input");
            }
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

            // Save acceptance
            MarkDisclaimerAccepted();

            Log("Disclaimer accepted! Loading Bootstrap scene...");
            LoadBootstrapScene();
        }

        private void OnExit()
        {
            Log("User declined disclaimer - exiting game");

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

        /// <summary>
        /// ✅ CORRECT: Load Bootstrap scene (replaces Disclaimer entirely)
        /// This starts the actual game initialization
        /// </summary>
        private void LoadBootstrapScene()
        {
            if (string.IsNullOrWhiteSpace(nextSceneName))
            {
                Debug.LogError("[DisclaimerController] Next scene name is not assigned!");
                return;
            }

            Log($"Loading {nextSceneName}...");
            
            // ✅ Use LoadScene (single mode) to REPLACE this scene
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
                LoadBootstrapScene();
            }
        }
#endif
        #endregion

        #region Disclaimer State Management
        /// <summary>
        /// Mark disclaimer as accepted (saved to PlayerPrefs)
        /// </summary>
        public static void MarkDisclaimerAccepted()
        {
            PlayerPrefs.SetInt(DISCLAIMER_KEY, 1);
            PlayerPrefs.Save();
            Debug.Log("[DisclaimerController] Disclaimer acceptance saved");
        }

        /// <summary>
        /// Check if user has previously accepted the disclaimer
        /// </summary>
        public static bool HasAcceptedDisclaimer()
        {
            return PlayerPrefs.GetInt(DISCLAIMER_KEY, 0) == 1;
        }
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
            MarkDisclaimerAccepted();
            Debug.Log("[DisclaimerController] Disclaimer force-accepted!");
        }
        #endregion
    }
}