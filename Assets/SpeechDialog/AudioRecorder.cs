using UnityEngine;

/// AudioRecorder
/// Component for recording audio from the microphone.  
public class AudioRecorder : MonoBehaviour
{
    public int sampleRate = 16000;
    public int maxRecordSeconds = 20;

    private AudioClip recordingClip;
    private bool isRecording = false;

    public void StartRecording()
    {
        if (isRecording) return;

        recordingClip = Microphone.Start(
            deviceName: null,
            loop: false,
            lengthSec: maxRecordSeconds,
            frequency: sampleRate
        );

        isRecording = true;
        Debug.Log("Recording started.");
    }

    public float[] StopRecording(out int sampleRate, out int channels)
{
    if (!isRecording)
    {
        sampleRate = 0;
        channels = 0;
        return null;
    }

    int position = Microphone.GetPosition(null);

        Microphone.End(null);
        isRecording = false;

        if (position <= 0)
        {
            Debug.LogWarning("No microphone data recorded");
            sampleRate = 0;
            channels = 0;
            return null;
        }

        sampleRate = recordingClip.frequency;
        channels = recordingClip.channels;

        float[] fullBuffer = new float[recordingClip.samples * channels];
        recordingClip.GetData(fullBuffer, 0);

        float[] trimmedSamples = new float[position * channels];
        System.Array.Copy(fullBuffer, trimmedSamples, trimmedSamples.Length);

        Debug.Log($"Recorded {position} samples");

        return trimmedSamples;
    }
}