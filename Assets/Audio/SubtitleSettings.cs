using UnityEngine;
using UnityEngine.UI;

/// SubtitleSettings
/// Attach to a UI Toggle in your pause/settings menu.
/// Wire up the SubtitleDisplay reference and call OnToggleChanged from the
/// Toggle's OnValueChanged event in the Inspector.
public class SubtitleSettings : MonoBehaviour
{
    [SerializeField]
    private SubtitleDisplay subtitleDisplay;

    [SerializeField]
    private Toggle subtitleToggle;

    void Start()
    {
        if (subtitleToggle != null && subtitleDisplay != null)
            subtitleToggle.isOn = subtitleDisplay.SubtitlesEnabled;
    }

    /// Bind this to the Toggle's OnValueChanged event in the Inspector.
    public void OnToggleChanged(bool value)
    {
        if (subtitleDisplay != null)
            subtitleDisplay.SetSubtitlesEnabled(value);
    }
}
