using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickAnywhereToStart : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Waves1";

    void Update()
    {
        // Detect any mouse click OR screen touch
        if (Input.GetMouseButtonDown(0))
        {
            Screen.fullScreen = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
