using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        // Always ensure the cursor is visible and unlocked on the main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayGame()
    {
        // Make sure your first level is named exactly this in your files
        SceneManager.LoadScene("TestScene2");
    }

    public void QuitGame()
    {
        Debug.Log("Game Exited!");
        Application.Quit();
    }
}