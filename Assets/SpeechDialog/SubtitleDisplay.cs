using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SubtitleDisplay : MonoBehaviour
{
    public GameObject subtitleCanvas;
    public TMP_Text subtitleText;
    public InputActionReference toggleSubtitleAction;

    private bool subtitlesEnabled = false;
    private bool wasPressedLastFrame = false;
    private Coroutine hideCoroutine;

    private string currentSubtitle = "";
    private float currentSubtitleEndTime = -1f;

    void Start()
    {
        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(false);

        if (subtitleText != null)
            subtitleText.text = "";
    }

    void OnEnable()
    {
        if (toggleSubtitleAction != null && toggleSubtitleAction.action != null)
            toggleSubtitleAction.action.Enable();
    }

    void OnDisable()
    {
        if (toggleSubtitleAction != null && toggleSubtitleAction.action != null)
            toggleSubtitleAction.action.Disable();
    }

    void Update()
    {
        if (toggleSubtitleAction == null || toggleSubtitleAction.action == null)
            return;

        bool isPressed = toggleSubtitleAction.action.IsPressed();

        if (isPressed && !wasPressedLastFrame)
        {
            SetSubtitlesEnabled(!subtitlesEnabled);
        }

        wasPressedLastFrame = isPressed;
    }

    public void ShowSubtitle(string text, float duration)
    {
        if (subtitleText == null || subtitleCanvas == null)
            return;

        currentSubtitle = text;
        currentSubtitleEndTime = Time.time + duration;

        if (!subtitlesEnabled)
            return;

        subtitleText.text = currentSubtitle;
        subtitleCanvas.SetActive(true);

        RestartHideCoroutine(duration);
    }

    public void HideSubtitle()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(false);
    }

    public void SetSubtitlesEnabled(bool enabled)
    {
        subtitlesEnabled = enabled;

        if (!subtitlesEnabled)
        {
            HideSubtitle();
            return;
        }

        bool subtitleStillActive = !string.IsNullOrEmpty(currentSubtitle) && Time.time < currentSubtitleEndTime;

        if (subtitleStillActive)
        {
            float remainingTime = currentSubtitleEndTime - Time.time;

            subtitleText.text = currentSubtitle;
            subtitleCanvas.SetActive(true);
            RestartHideCoroutine(remainingTime);
        }
    }

    public bool SubtitlesEnabled => subtitlesEnabled;

    private void RestartHideCoroutine(float delay)
    {
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay(delay));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(false);

        currentSubtitle = "";
        currentSubtitleEndTime = -1f;
        hideCoroutine = null;
    }
}