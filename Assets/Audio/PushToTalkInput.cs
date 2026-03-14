using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PushToTalkInput : MonoBehaviour
{
    public AudioRecorder recorder;
    public SpeechHttpClient speechClient;
    public TMP_Text feedbackText;
    public InputActionReference pushToTalkAction;

    private bool isProcessing = false;
    private bool wasPressedLastFrame = false;

    private const string DEFAULT_MESSAGE =
        "<color=#FFFF00><b>Halte B gedrückt, während du etwas sagst.</b></color>";

    private const string PROCESSING_MESSAGE =
        "<color=#FFFF00><b>Denkt nach...</b></color>";

    private const string WAIT_MESSAGE =
        "<color=#FFFF00><b>Warte erst die Antwort ab, bevor du etwas Neues sagst.</b></color>";

    void Start()
    {
        ShowFeedback(DEFAULT_MESSAGE);
    }

    void OnEnable()
    {
        if (speechClient != null)
        {
            speechClient.OnPlaybackStarted += OnPlaybackStarted;
            speechClient.OnPlaybackFinished += OnPlaybackFinished;
        }

        if (pushToTalkAction != null && pushToTalkAction.action != null)
            pushToTalkAction.action.Enable();
    }

    void OnDisable()
    {
        if (speechClient != null)
        {
            speechClient.OnPlaybackStarted -= OnPlaybackStarted;
            speechClient.OnPlaybackFinished -= OnPlaybackFinished;
        }

        if (pushToTalkAction != null && pushToTalkAction.action != null)
            pushToTalkAction.action.Disable();
    }

    void Update()
    {
        if (pushToTalkAction == null || pushToTalkAction.action == null)
            return;

        bool isPressed = pushToTalkAction.action.IsPressed();

        if (isPressed && !wasPressedLastFrame)
        {
            if (isProcessing)
            {
                ShowFeedback(WAIT_MESSAGE);
            }
            else
            {
                if (recorder != null)
                {
                    recorder.StartRecording();
                }
                else
                {
                    Debug.LogWarning("Recorder ist nicht zugewiesen.");
                }
            }
        }

        if (!isPressed && wasPressedLastFrame)
        {
            if (!isProcessing)
            {
                if (recorder != null)
                {
                    int sampleRate;
                    int channels;
                    float[] samples = recorder.StopRecording(out sampleRate, out channels);

                    if (samples != null && speechClient != null)
                    {
                        isProcessing = true;
                        ShowFeedback(PROCESSING_MESSAGE);
                        speechClient.SendAudio(samples, sampleRate, channels);
                    }
                    else
                    {
                        Debug.LogWarning("Keine Audio-Daten aufgenommen oder Speech Client fehlt.");
                        ShowFeedback(DEFAULT_MESSAGE);
                    }
                }
                else
                {
                    Debug.LogWarning("Recorder ist nicht zugewiesen.");
                    ShowFeedback(DEFAULT_MESSAGE);
                }
            }
        }

        wasPressedLastFrame = isPressed;
    }

    private void OnPlaybackStarted()
    {
        HideFeedback();
    }

    private void OnPlaybackFinished()
    {
        isProcessing = false;
        ShowFeedback(DEFAULT_MESSAGE);
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.richText = true;
            feedbackText.text = message;
            feedbackText.gameObject.SetActive(true);
        }
    }

    private void HideFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }
}