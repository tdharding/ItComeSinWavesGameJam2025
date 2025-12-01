using UnityEngine;

public class IntroCam4AnimationRelay : MonoBehaviour
{
    public IntroSequenceController controller;

    public void TriggerSisterNom()
    {
        if (controller != null)
            controller.IntroCam4_TriggerSisterNom();
    }
}
