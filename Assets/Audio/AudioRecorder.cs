using UnityEngine;

""" 
 AudioRecorder
 
 Component for recording audio from the microphone.
"""

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

    public AudioClip StopRecording()
    {
        if (!isRecording) return null;

        Microphone.End(null);
        isRecording = false;

        Debug.Log("Recording stopped");

        return recordingClip;
    }


}
