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

    private const string DisclaimerKey = "DisclaimerAccepted";

    private void Awake()
    {
        // If already accepted, skip
        if (PlayerPrefs.GetInt(DisclaimerKey, 0) == 1)
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        // Safety check
        if (!disclaimerPanel || !tosPanel || !acknowledgeToggle || !agreeButton || !exitButton || !tosButton || !tosBackButton)
        {
            Debug.LogError("DisclaimerController: One or more UI references are not assigned in the Inspector.");
            return;
        }

        // Set initial UI state BEFORE the first frame
        agreeButton.interactable = false;
        tosPanel.SetActive(false);
        disclaimerPanel.SetActive(true);
    }

    private void Start()
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
    }

    private void OnAgree()
    {
        PlayerPrefs.SetInt(DisclaimerKey, 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnExit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OpenTOS()
    {
        tosPanel.SetActive(true);
        disclaimerPanel.SetActive(false);
    }

    private void BackToDisclaimer()
    {
        tosPanel.SetActive(false);
        disclaimerPanel.SetActive(true);
    }
}
