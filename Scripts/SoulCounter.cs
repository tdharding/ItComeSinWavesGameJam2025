using UnityEngine;
using TMPro;

public class SoulCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI soulText;

    private void Start()
    {
        UpdateText();
    }

    private void Update()
    {
        // Update every frame, or call UpdateText() from an event if preferred
        UpdateText();
    }

    private void UpdateText()
    {
        if (SoulManager.Instance != null)
        {
            soulText.text = "Souls on board " + SoulManager.Instance.SoulsSaved;
        }
        else
        {
            soulText.text = "Souls on board 0";
        }
    }
}
