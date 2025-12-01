using UnityEngine;

public class GongWavesTrigger : MonoBehaviour
{
    [Header("Boat Reference")]
    public Transform boatTransform;

    [Header("Controller")]
    public GongWavesController controller;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        // Check if the entering collider belongs to the assigned boat Transform
        if (other.transform == boatTransform)
        {
            hasTriggered = true;
            controller.StartGongSequence();
        }
    }
}
