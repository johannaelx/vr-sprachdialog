using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorToMenu : MonoBehaviour
{
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
