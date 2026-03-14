using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class PushToTalkInput : MonoBehaviour
{
    public AudioRecorder recorder;
    public SpeechHttpClient speechClient;
    public TextMeshProUGUI feedbackText;

    public InputActionReference pushToTalkAction; // <- Controller-Button hier zuweisen

    private bool isProcessing = false;
    private Coroutine feedbackCoroutine;

    private float idleTimer = 0f;
    private const float IDLE_THRESHOLD = 10f;

    private bool wasPressedLastFrame = false;

    void Start()
    {
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (speechClient != null)
            speechClient.OnPlaybackFinished += OnPlaybackFinished;

        if (pushToTalkAction != null)
            pushToTalkAction.action.Enable();
    }

    void OnDisable()
    {
        if (speechClient != null)
            speechClient.OnPlaybackFinished -= OnPlaybackFinished;

        if (pushToTalkAction != null)
            pushToTalkAction.action.Disable();
    }

    void Update()
    {
        if (pushToTalkAction == null) return;

        bool isPressed = pushToTalkAction.action.IsPressed();

        if (!isProcessing)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= IDLE_THRESHOLD)
            {
                ShowFeedbackMessage("Drücke und halte den Controller-Button zum Sprechen.", 3f);
                idleTimer = 0f;
            }
        }

        // Button wurde gerade gedrückt
        if (isPressed && !wasPressedLastFrame)
        {
            ResetIdle();

            if (isProcessing)
            {
                ShowFeedbackMessage("Bitte warte, bis der Bäcker fertig geantwortet hat.", 3f);
            }
            else
            {
                recorder.StartRecording();
            }
        }

        // Button wurde gerade losgelassen
        if (!isPressed && wasPressedLastFrame && !isProcessing)
        {
            ResetIdle();

            int sampleRate;
            int channels;
            float[] samples = recorder.StopRecording(out sampleRate, out channels);

            if (samples != null && speechClient != null)
            {
                isProcessing = true;
                speechClient.SendAudio(samples, sampleRate, channels);
            }
            else
            {
                Debug.LogWarning("No audio samples recorded or SpeechClient missing.");
                ShowFeedbackMessage("Drücke und halte den Controller-Button zum Sprechen.", 3f);
            }
        }

        wasPressedLastFrame = isPressed;
    }

    private void ResetIdle()
    {
        idleTimer = 0f;
    }

    private void ShowFeedbackMessage(string message, float duration)
    {
        if (feedbackText == null) return;

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine = StartCoroutine(DisableFeedbackAfterDelay(message, duration));
    }

    private IEnumerator DisableFeedbackAfterDelay(string message, float duration)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        feedbackText.gameObject.SetActive(false);
        feedbackCoroutine = null;
    }

    private void OnPlaybackFinished()
    {
        isProcessing = false;
        ResetIdle();
    }
}