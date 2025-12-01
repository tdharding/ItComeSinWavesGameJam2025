using UnityEngine;

public class IntroCam5AnimationRelay : MonoBehaviour
{
    public AudioSource mazeSound;   // Assign in Inspector

    // Called by animation event
    public void PlayMazeSound()
    {
        if (mazeSound != null)
            mazeSound.Play();
    }
}
