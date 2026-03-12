using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

/// PushToTalkInput
/// Component for handling push-to-talk input and coordinating audio recording and sending.
public class PushToTalkInput : MonoBehaviour
{
    public AudioRecorder recorder;
    public SpeechHttpClient speechClient;
    public TextMeshProUGUI feedbackText;

    private bool isProcessing = false;
    private Coroutine feedbackCoroutine;

    private float idleTimer = 0f;
    private const float IDLE_THRESHOLD = 10f;

    void Start()
    {
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (speechClient != null)
            speechClient.OnPlaybackFinished += OnPlaybackFinished;
    }

    void OnDisable()
    {
        if (speechClient != null)
            speechClient.OnPlaybackFinished -= OnPlaybackFinished;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (!isProcessing)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= IDLE_THRESHOLD)
            {
                ShowFeedbackMessage("Press and hold B on the keyboard to speak.", 3f);
                idleTimer = 0f; 
            }
        }

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            ResetIdle();
            if (isProcessing)
            {
                ShowFeedbackMessage("Please wait for the baker to answer you first.", 3f);
            }
            else
            {
                recorder.StartRecording();
            }
        }

        if (Keyboard.current.bKey.wasReleasedThisFrame && !isProcessing)
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
                ShowFeedbackMessage("Press and hold B on the keyboard to speak.", 3f);
            }
        }
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