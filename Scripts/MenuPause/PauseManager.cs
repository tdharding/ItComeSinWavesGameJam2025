using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("UI")]
    public GameObject pauseMenu;

    void Start()
    {
        ResumeGame(); // ensure clean starting state
    }

    void Update()
    {
        // ESC only PAUSES, never resumes
        if (Input.GetKeyDown(KeyCode.Escape) && !IsPaused)
            PauseGame();

        // Auto-pause if the game window loses focus
        if (!Application.isFocused && !IsPaused)
            PauseGame();
    }

    // ---------------------------------------------------------
    // PUBLIC BUTTONS / UI EVENTS
    // ---------------------------------------------------------

    public void OnResumeButtonPressed()
    {
        ResumeGame();

        // Force fullscreen back on
        if (Screen.fullScreenMode != FullScreenMode.ExclusiveFullScreen &&
            Screen.fullScreenMode != FullScreenMode.FullScreenWindow)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
        }
    }

    // NEW GAME BUTTON EVENT
    public void OnNewGameButtonPressed()
    {
        // Unpause first so the game runs normally
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Optional: wipe save data here
        // PlayerPrefs.DeleteAll();

        // Load your opening scene
        SceneManager.LoadScene("MainMenu");
    }

    // ---------------------------------------------------------
    // CORE PAUSE LOGIC
    // ---------------------------------------------------------

    public void PauseGame()
    {
        IsPaused = true;

        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        IsPaused = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
