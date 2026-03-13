using UnityEngine;
using UnityEngine.InputSystem;

/// PushToTalkInput
/// Component for handling push-to-talk input and coordinating audio recording and sending.
/// Also handles toggling subtitles on/off via the U key.
public class PushToTalkInput : MonoBehaviour
{
    public AudioRecorder recorder;
    public SpeechHttpClient speechClient;

    // Optional reference to SubtitleDisplay for toggling subtitles; assign in the Inspector
    public SubtitleDisplay subtitleDisplay;

    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        // Hold B to record, release to send audio to the backend
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
                speechClient.SendAudio(samples, sampleRate, channels);
            }
            else
            {
                Debug.LogWarning("No audio samples recorded or SpeechClient missing.");
            }
        }

        // Press U to toggle subtitles on/off
        if (Keyboard.current.uKey.wasPressedThisFrame && subtitleDisplay != null)
        {
            subtitleDisplay.SetSubtitlesEnabled(!subtitleDisplay.SubtitlesEnabled);
            Debug.Log("Subtitles " + (subtitleDisplay.SubtitlesEnabled ? "enabled" : "disabled"));
        }
    }
}
