using UnityEngine;
using UnityEngine.InputSystem;

/// PushToTalkInput
/// Component for handling push-to-talk input and coordinating audio recording and sending.
public class PushToTalkInput : MonoBehaviour
{
    public AudioRecorder recorder;
    public SpeechHttpClient speechClient;

    private bool isProcessing = false;

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
        if (Keyboard.current == null || isProcessing)
        {
            return;
        }

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            recorder.StartRecording();
        }

        if (Keyboard.current.bKey.wasReleasedThisFrame)
        {
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
            }
        }
    }

    private void OnPlaybackFinished()
    {
        isProcessing = false;
    }
}
