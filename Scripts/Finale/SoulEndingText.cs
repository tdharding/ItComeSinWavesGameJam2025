using UnityEngine;
using TMPro;

public class SoulEndingText : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text endingText;

    [Header("Opening Line")]
    [TextArea] public string openingLine = "You have finished the game";

    [Header("Base Messages")]
    [TextArea] public string noSoulsMessage = "You rescued no souls...";
    [TextArea] public string baseMessageFormat = "You rescued {0} soul{1}.";

    [Header("Remark Messages")]
    [TextArea] public string remark_1_to_5 = "A small spark of hope in a bleak world.";
    [TextArea] public string remark_6_to_10 = "You showed courage in the face of darkness.";
    [TextArea] public string remark_11_to_19 = "Your compassion echoes far beyond the realm.";
    [TextArea] public string remark_20_plus = "An extraordinary feat — the world will remember your mercy.";

    [Header("Final Line")]
    [TextArea] public string finalLine = "Thank you for playing.";

    private void Start()
    {
        if (endingText == null)
        {
            Debug.LogError("Ending Text is not assigned.");
            return;
        }

        int souls = SoulManager.Instance.SoulsSaved;
        endingText.text = BuildEndingMessage(souls);
    }

    private string BuildEndingMessage(int souls)
    {
        // --- Opening line ---
        string message = openingLine + "\n\n";

        // --- Base line ---
        if (souls <= 0)
        {
            message += noSoulsMessage;
        }
        else
        {
            string plural = souls == 1 ? "" : "s";
            message += string.Format(baseMessageFormat, souls, plural);
        }

        // --- Remarks ---
        if (souls >= 20)
            message += "\n" + remark_20_plus;
        else if (souls > 10)
            message += "\n" + remark_11_to_19;
        else if (souls >= 6)
            message += "\n" + remark_6_to_10;
        else if (souls >= 1)
            message += "\n" + remark_1_to_5;

        // --- Final line ---
        message += "\n\n" + finalLine;

        return message;
    }
}
