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
        Debug.Log("Update called in PushToTalkInput");

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
            AudioClip clip = recorder.StopRecording();

            if (clip != null && speechClient != null)
            {
                speechClient.SendAudio(clip);
            }
            else
            {
                Debug.LogError("SpeechClient or AudioClip missing.");
            }
        }
    }
}
