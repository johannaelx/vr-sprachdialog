using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// SpeechHttpClient
/// Sends recorded audio to the speech backend and handles the JSON response
/// containing both the NPC reply text (for subtitles) and base64-encoded TTS audio.
[RequireComponent(typeof(AudioSource))]
public class SpeechHttpClient : MonoBehaviour
{
    [SerializeField]
    private string endpoint = "http://localhost:8000/conversation";

    // Optional subtitle display for showing NPC replies; assign in the Inspector
    [SerializeField]
    private SubtitleDisplay subtitleDisplay;

    private AudioSource audioSource;

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
        audioSource.spatialBlend = 0f; // 2D audio
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
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Speech backend error: " + request.error);
            yield break;
        }

        // Parse JSON response containing reply text and base64-encoded audio
        ConversationResponse response = JsonUtility.FromJson<ConversationResponse>(
            request.downloadHandler.text
        );

        if (response == null || string.IsNullOrEmpty(response.audio))
        {
            Debug.LogError("Invalid or empty response from speech backend");
            yield break;
        }

        // Decode base64-encoded WAV audio from the backend
        byte[] audioBytes = Convert.FromBase64String(response.audio);
        AudioClip ttsClip = WavUtility.ToAudioClip(audioBytes);

        if (ttsClip == null)
        {
            Debug.LogError("Failed to create AudioClip from backend response");
            yield break;
        }

        audioSource.Stop();
        audioSource.PlayOneShot(ttsClip);

        // Display NPC reply as subtitle for the duration of the clip
        if (subtitleDisplay != null && !string.IsNullOrEmpty(response.reply))
            subtitleDisplay.ShowSubtitle(response.reply, ttsClip.length);

        Debug.Log("TTS playback started");
    }
}
