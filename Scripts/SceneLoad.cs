using UnityEngine;

public class SceneLoad : MonoBehaviour
{
    [Header("Scene Startup Refs")]
    public GameObject mainCamera;
    public MonoBehaviour introSequenceController; // drag IntroSequenceController here

    void Start()
    {
        // 1. Ensure the main camera (with CinemachineBrain) is active
        if (mainCamera != null)
            mainCamera.SetActive(true);

        // 2. Enable the intro sequence controller
        if (introSequenceController != null)
            introSequenceController.enabled = true;

        // 3. Cursor setup
        if (!PauseManager.IsPaused)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
