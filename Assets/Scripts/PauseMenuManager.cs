using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The main background panel that dims the screen")]
    public GameObject pauseMenuPanel;
    public GameObject mainButtonsUI;
    public GameObject settingsUI;

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsUI != null) settingsUI.SetActive(false);
        if (mainButtonsUI != null) mainButtonsUI.SetActive(true);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;

        if (settingsUI != null) settingsUI.SetActive(false);
        if (mainButtonsUI != null) mainButtonsUI.SetActive(true);
    }

    void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
    }

    public void OpenSettings()
    {
        if (mainButtonsUI != null) mainButtonsUI.SetActive(false);
        if (settingsUI != null) settingsUI.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsUI != null) settingsUI.SetActive(false);
        if (mainButtonsUI != null) mainButtonsUI.SetActive(true);
    }
    // -------------------------------------

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}