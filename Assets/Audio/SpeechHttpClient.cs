using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// SpeechHttpClient
/// Component for sending recorded audio to a speech backend via HTTP.
[RequireComponent(typeof(AudioSource))]
public class SpeechHttpClient : MonoBehaviour
{
    [SerializeField]
    private string endpoint = "http://localhost:8000/conversation";

    private AudioSource audioSource;

    /// Fired when TTS playback has fully completed (or an error occurred).
    public event Action OnPlaybackFinished;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // 2D Audio
    }

    public void SendAudio(float[] samples, int sampleRate, int channels)
    {
        byte[] wavData = WavUtility.FromSamples(samples, sampleRate, channels);
        StartCoroutine(PostAudio(wavData));
    }

    private IEnumerator PostAudio(byte[] wavData)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", wavData, "speech.wav", "audio/wav");

        using UnityWebRequest request = UnityWebRequest.Post(endpoint, form);

        request.downloadHandler = new DownloadHandlerAudioClip(endpoint, AudioType.WAV);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Speech backend error: " + request.error);
            OnPlaybackFinished?.Invoke();
            yield break;
        }
        
        AudioClip ttsClip = DownloadHandlerAudioClip.GetContent(request);

        if (ttsClip == null)
        {
            Debug.LogError("Received null AudioClip from backend");
            OnPlaybackFinished?.Invoke();
            yield break;
        }
        
        audioSource.Stop();
        audioSource.PlayOneShot(ttsClip);

        Debug.Log("TTS playback started");

        // Wait until playback is done
        yield return new WaitForSeconds(ttsClip.length);

        Debug.Log("TTS playback finished");
        OnPlaybackFinished?.Invoke();
    }
}