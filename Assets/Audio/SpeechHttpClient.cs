using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// SpeechHttpClient
/// Component for sending recorded audio to a speech backend via HTTP.

public class SpeechHttpClient : MonoBehaviour
{
    [SerializeField]
    private string endpoint = "http://localhost:8000/conversation";

    public void SendAudio(AudioClip clip)
    {
        byte[] wavData = WavUtility.FromAudioClip(clip);
        StartCoroutine(PostAudio(wavData));
    }

    private IEnumerator PostAudio(byte[] wavData)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", wavData, "speech.wav", "audio/wav");

        using UnityWebRequest request = UnityWebRequest.Post(endpoint, form);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Speech backend error: " + request.error);
        }
        else
        {
            Debug.Log("Speech backend response: " + request.downloadHandler.text);
        }
    }
}
