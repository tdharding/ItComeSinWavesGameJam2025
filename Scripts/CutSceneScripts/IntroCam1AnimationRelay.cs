using UnityEngine;

public class IntroCam1AnimationRelay : MonoBehaviour
{
    public IntroSequenceController controller;

    // Called by animation event on IntroCam1
    public void IntroCam1Finish()
    {
        if (controller != null)
            controller.OnIntroCam1Finish();
    }
}
