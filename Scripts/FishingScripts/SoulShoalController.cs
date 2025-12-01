using System.Collections.Generic;
using UnityEngine;

public class SoulShoalController : MonoBehaviour
{
    [Header("Refs")]
    public Transform boat;
    public GameObject fishContainer;

    [Header("Distances")]
    public float activationDistance = 30f;
    public float fishingDistance = 8f;

    [Header("Audio")]
    public AudioSource proximitySound;
    public float maxVolume = 0.9f;
    public float fadeSpeed = 2f;

    public bool IsActive { get; private set; }
    public bool CanFish { get; private set; }

    private bool soundPlaying = false;

    // Tracked fish
    private List<Transform> fishList = new List<Transform>();
    private int remainingFish = 0;

    // ---------------------------------------------------------
    void Start()
    {
        RegisterAllFish();
    }

    // ---------------------------------------------------------
    void RegisterAllFish()
    {
        fishList.Clear();
        AddFishRecursive(fishContainer.transform);

        remainingFish = fishList.Count;

        if (remainingFish == 0)
            Debug.LogWarning($"{name}: Shoal contains NO fish objects!");
    }

    void AddFishRecursive(Transform root)
    {
        foreach (Transform child in root)
        {
            if (child.CompareTag("Fish"))
                fishList.Add(child);

            AddFishRecursive(child);
        }
    }

    // ---------------------------------------------------------
    void Update()
    {
        // Count remaining fish by null reference (destroyed = caught)
        int alive = 0;
        for (int i = 0; i < fishList.Count; i++)
        {
            if (fishList[i] != null)
                alive++;
        }

        remainingFish = alive;
        bool shoalEmpty = (remainingFish == 0);

        float d = Vector3.Distance(boat.position, transform.position);

        // ---------------------------------------------
        // Shoal Activation
        // ---------------------------------------------
        IsActive = !shoalEmpty && d <= activationDistance;

        // Toggle ONLY renderer visibility (safe for LOD)
        SetFishVisuals(IsActive);

        // ---------------------------------------------
        // Fishing Distance
        // ---------------------------------------------
        CanFish = !shoalEmpty && d <= fishingDistance;

        // ---------------------------------------------
        // Proximity Audio
        // ---------------------------------------------
        HandleAudio(shoalEmpty, d);
    }

    // ---------------------------------------------------------
    void SetFishVisuals(bool visible)
    {
        if (fishContainer == null) 
            return;

        Renderer[] rends = fishContainer.GetComponentsInChildren<Renderer>(true);

        foreach (var r in rends)
            r.enabled = visible;
    }

    // ---------------------------------------------------------
    void HandleAudio(bool shoalEmpty, float distance)
    {
        if (proximitySound == null)
            return;

        // All fish caught → fade out and stop
        if (shoalEmpty)
        {
            proximitySound.volume = Mathf.MoveTowards(
                proximitySound.volume,
                0f,
                Time.deltaTime * fadeSpeed
            );

            if (proximitySound.volume <= 0.01f && soundPlaying)
            {
                proximitySound.Stop();
                soundPlaying = false;
            }

            return;
        }

        // Has fish → adjust volume depending on range
        float targetVolume = IsActive
            ? Mathf.Lerp(0f, maxVolume, Mathf.InverseLerp(activationDistance, 0f, distance))
            : 0f;

        if (IsActive && !soundPlaying)
        {
            proximitySound.Play();
            soundPlaying = true;
        }

        proximitySound.volume = Mathf.MoveTowards(
            proximitySound.volume,
            targetVolume,
            Time.deltaTime * fadeSpeed
        );

        if (!IsActive && proximitySound.volume <= 0.01f && soundPlaying)
        {
            proximitySound.Stop();
            soundPlaying = false;
        }
    }

    // ---------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fishingDistance);
    }
}
