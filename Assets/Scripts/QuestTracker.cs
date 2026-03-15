using UnityEngine;
using UnityEngine.UI;

public class QuestTracker : MonoBehaviour
{
    public Toggle[] questToggles;
    public AudioSource audioSource;
    public AudioClip successClip;

    public GameObject questPanel;
    public GameObject successMessage;

    public BakerAnimations bakerAnimations;    

    private bool successPlayed = false;

    public void CheckAllTasks()
    {
        foreach (Toggle toggle in questToggles)
        {
            if (!toggle.isOn)
                return;
        }

        if (!successPlayed)
        {
            successPlayed = true;

            if (audioSource != null && successClip != null)
                audioSource.PlayOneShot(successClip);

            if (questPanel != null)
                questPanel.SetActive(false);

            if (successMessage != null)
                successMessage.SetActive(true);

            
            if (bakerAnimations != null)
                bakerAnimations.DanceTimes(5);
        }
    }
}