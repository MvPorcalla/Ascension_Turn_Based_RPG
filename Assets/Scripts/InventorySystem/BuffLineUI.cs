// -------------------------------
// BuffLineUI.cs - Helper script for buff display prefab (Optional)
// -------------------------------
using UnityEngine;
using TMPro;

/// <summary>
/// Optional helper script to attach to BuffType prefab
/// Makes it easier to set up buff lines programmatically
/// </summary>
public class BuffLineUI : MonoBehaviour
{
    [Header("Text Components")]
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text durationText;

    /// <summary>
    /// Setup buff line with all three values
    /// </summary>
    public void Setup(string label, string value, string duration)
    {
        if (labelText != null) labelText.text = label;
        if (valueText != null) valueText.text = value;
        if (durationText != null) durationText.text = duration;
    }

    /// <summary>
    /// Auto-find text components if not assigned in inspector
    /// </summary>
    private void Awake()
    {
        if (labelText == null)
            labelText = transform.Find("TextLabel")?.GetComponent<TMP_Text>();
        
        if (valueText == null)
            valueText = transform.Find("textValue")?.GetComponent<TMP_Text>();
        
        if (durationText == null)
            durationText = transform.Find("textDuration")?.GetComponent<TMP_Text>();
    }
}