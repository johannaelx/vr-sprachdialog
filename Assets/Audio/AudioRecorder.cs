using UnityEngine;

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

    public float[] StopRecording()
    {
        if (!isRecording) return null;

        int sampleCount = Microphone.GetPosition(null);
        Microphone.End(null);

        float[] samples = new float[sampleCount];
        recordingClip.GetData(samples, 0);

        isRecording = false;
        Debug.Log("Recording stopped");

        return samples;
    }


}
