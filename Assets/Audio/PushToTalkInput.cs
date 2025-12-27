using UnityEngine;
using UnityEngine.InputSystem;

public class PushToTalkInput : MonoBehaviour
{
    public AudioRecorder recorder;

    private bool isRecording = false;

    void Update()
    {
        Debug.Log("Update called in PushToTalkInput");

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.spaceKey.isPressed)
        {
            recorder.StartRecording();
            isRecording = true;
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame && isRecording)
        {
            float[] audioSamples = recorder.StopRecording();
            isRecording = false;

            if (audioSamples != null && audioSamples.Length > 0)
            {
                float duration = audioSamples.Length / (float)recorder.sampleRate;
                Debug.Log($"Audio duration: {duration:F2} seconds");

                //TODO: Send audioSamples to backend
            }
        }
    }
}
