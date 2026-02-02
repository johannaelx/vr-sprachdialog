using UnityEngine;
using UnityEngine.InputSystem;

/// PushToTalkInput
/// Component for handling push-to-talk input and coordinating audio recording and sending.
public class PushToTalkInput : MonoBehaviour
{
    public AudioRecorder recorder;
    public SpeechHttpClient speechClient;

    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            recorder.StartRecording();
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
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
    }
}
