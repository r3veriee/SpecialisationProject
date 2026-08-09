using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        // Always ensure the cursor is visible and unlocked on the main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerPrefs.SetInt("SessionDeaths", 0);
        PlayerPrefs.SetFloat("Level1Time", 0f);
        PlayerPrefs.SetInt("Level1Deaths", 0);
        PlayerPrefs.SetInt("Level2Deaths", 0);
        PlayerPrefs.SetInt("Level2Time", 0);
    }

    public void PlayGame()
    {
        // Make sure your first level is named exactly this in your files
        SceneManager.LoadScene("Level1");
    }

    public void QuitGame()
    {
        Debug.Log("Game Exited!");
        Application.Quit();
    }
}