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
    private string endpoint = "http://192.168.178.21:8000/conversation";
    public SubtitleDisplay subtitleDisplay;
    private AudioSource audioSource;

    /// Fired when TTS playback starts.
    public event Action OnPlaybackStarted;

    /// Fired when TTS playback has fully completed (or an error occurred).
    public event Action OnPlaybackFinished;

    // Matches the JSON schema returned by the /conversation endpoint
    [Serializable]
    private class ConversationResponse
    {
        public string reply;
        public string audio;
    }

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

        string sceneName = UnityEngine.SceneManagement.SceneManager
        .GetActiveScene().name;

        form.AddField("scene", sceneName);
        
        using UnityWebRequest request = UnityWebRequest.Post(endpoint, form);

        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Speech backend error: " + request.error);
            OnPlaybackFinished?.Invoke();
            yield break;
        }

        // Parse JSON response containing reply text and base64-encoded audio
        ConversationResponse response = JsonUtility.FromJson<ConversationResponse>(
            request.downloadHandler.text
        );

        if (response == null || string.IsNullOrEmpty(response.audio))
        {
            Debug.LogError("Invalid or empty response from speech backend");
            OnPlaybackFinished?.Invoke();
            yield break;
        }

        // Decode base64-encoded WAV audio from the backend
        byte[] audioBytes = Convert.FromBase64String(response.audio);
        AudioClip ttsClip = WavUtility.ToAudioClip(audioBytes);

        if (ttsClip == null)
        {
            Debug.LogError("Received null AudioClip from backend");
            OnPlaybackFinished?.Invoke();
            yield break;
        }

        if (subtitleDisplay != null && !string.IsNullOrEmpty(response.reply))
        {
            subtitleDisplay.ShowSubtitle(response.reply, ttsClip.length);
        }


        audioSource.Stop();
        audioSource.PlayOneShot(ttsClip);

        Debug.Log("TTS playback started");
        OnPlaybackStarted?.Invoke();

        // Wait until playback is done
        yield return new WaitForSeconds(ttsClip.length);

        Debug.Log("TTS playback finished");
        OnPlaybackFinished?.Invoke();
    }
}