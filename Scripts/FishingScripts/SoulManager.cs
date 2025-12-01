using UnityEngine;

public class SoulManager : MonoBehaviour
{
    public static SoulManager Instance;

    public int SoulsSaved = 0;

    private void Awake()
    {
        // Singleton pattern to ensure only one SoulManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Add souls to the persistent counter.
    /// </summary>
    public void AddSoul(int amount = 1)
    {
        SoulsSaved += amount;
        Debug.Log("Souls Saved = " + SoulsSaved);
    }
}
