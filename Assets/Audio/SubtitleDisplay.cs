using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// SubtitleDisplay
/// Creates a Screen Space - Overlay Canvas fixed at the bottom of the screen,
/// similar to movie subtitles. Displays NPC reply text for the duration of the
/// TTS audio clip, then hides automatically.
///
/// Setup: Attach this script to any GameObject in the scene (e.g. SpeechManager).
/// Assign the SubtitleDisplay reference in SpeechHttpClient via the Inspector.
///
/// Toggle: Call SetSubtitlesEnabled(bool) from a settings/pause menu UI.
/// The preference is persisted via PlayerPrefs ("subtitles_enabled").
public class SubtitleDisplay : MonoBehaviour
{
    // Height of the subtitle bar in pixels
    private const float BarHeight = 120f;
    // Horizontal padding in pixels
    private const float HorizontalPadding = 80f;
    // Bottom margin from screen edge in pixels
    private const float BottomMargin = 30f;

    private bool subtitlesEnabled = true;
    private GameObject canvasRoot;
    private Text subtitleText;
    private Coroutine hideCoroutine;

    void Start()
    {
        subtitlesEnabled = PlayerPrefs.GetInt("subtitles_enabled", 1) == 1;
        BuildCanvas();
    }

    private void BuildCanvas()
    {
        // Root canvas: Screen Space Overlay so it always covers the full screen
        canvasRoot = new GameObject("SubtitleCanvas");
        Canvas canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        canvasRoot.AddComponent<CanvasScaler>();
        canvasRoot.AddComponent<GraphicRaycaster>();

        // Semi-transparent dark background bar at the bottom
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasRoot.transform, false);
        Image bg = bgGO.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.6f);

        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0f);
        bgRT.anchorMax = new Vector2(1f, 0f);
        bgRT.pivot = new Vector2(0.5f, 0f);
        bgRT.sizeDelta = new Vector2(0f, BarHeight);
        bgRT.anchoredPosition = new Vector2(0f, BottomMargin);

        // Subtitle text inside the bar
        GameObject textGO = new GameObject("SubtitleText");
        textGO.transform.SetParent(bgGO.transform, false);
        subtitleText = textGO.AddComponent<Text>();
        subtitleText.text = "";
        subtitleText.fontSize = 32;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.color = Color.white;
        subtitleText.fontStyle = FontStyle.Bold;

        // Use the default Unity font
        subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = new Vector2(-HorizontalPadding * 2f, 0f);
        textRT.anchoredPosition = Vector2.zero;

        // Add a shadow for readability
        Shadow shadow = textGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(2f, -2f);

        canvasRoot.SetActive(false);
    }

    /// Shows subtitle text for the given duration, then hides automatically.
    public void ShowSubtitle(string text, float duration)
    {
        if (!subtitlesEnabled || canvasRoot == null) return;

        subtitleText.text = text;
        canvasRoot.SetActive(true);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay(duration));
    }

    /// Immediately hides the subtitle canvas.
    public void HideSubtitle()
    {
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        if (canvasRoot != null) canvasRoot.SetActive(false);
    }

    /// Toggles subtitles on/off and persists the preference.
    public void SetSubtitlesEnabled(bool enabled)
    {
        subtitlesEnabled = enabled;
        PlayerPrefs.SetInt("subtitles_enabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
        if (!enabled) HideSubtitle();
    }

    public bool SubtitlesEnabled => subtitlesEnabled;

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (canvasRoot != null) canvasRoot.SetActive(false);
    }
}
