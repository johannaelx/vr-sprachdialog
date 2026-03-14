using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


public class MainMenu : MonoBehaviour
{
    public void PlayBakeryScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }

    public void PlayFriendsScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+2);
    }

    public void QuitGame() {
        Debug.Log ("Quit!");
        Application.Quit();
    }
}
