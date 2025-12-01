using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject aboutPanel;
    public GameObject controlsPanel;

    void Start()
    {
        ShowMain();
    }

    public void ShowMain()
    {
        mainPanel.SetActive(true);
        aboutPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    public void ShowAbout()
    {
        mainPanel.SetActive(false);
        aboutPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void ShowControls()
    {
        mainPanel.SetActive(false);
        aboutPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }
}
