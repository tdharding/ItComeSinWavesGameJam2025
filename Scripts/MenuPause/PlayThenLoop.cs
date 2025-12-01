using UnityEngine;

public class PlayThenLoop : MonoBehaviour
{
    [Header("Play first, then loop second")]
    public AudioClip firstClip;
    public AudioClip loopClip;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
    }

    private void Start()
    {
        StartCoroutine(PlaySequence());
    }

    private System.Collections.IEnumerator PlaySequence()
    {
        // Play first clip (no loop)
        if (firstClip != null)
        {
            audioSource.clip = firstClip;
            audioSource.loop = false;
            audioSource.Play();
            yield return new WaitForSeconds(firstClip.length);
        }

        // Play second clip (loop)
        if (loopClip != null)
        {
            audioSource.clip = loopClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
