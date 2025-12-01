using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteProximityCheck : MonoBehaviour
{
    [Header("Proximity Settings")]
    public Transform boat;         // Drag your boat here
    public float radius = 10f;     // Trigger distance
    public string sceneToLoad;     // Scene name to load

    [Header("Debug")]
    public bool drawGizmo = true;  // Toggle gizmo drawing

    private bool hasLoaded = false;

    void Update()
    {
        if (hasLoaded) return;
        if (boat == null || string.IsNullOrEmpty(sceneToLoad))
            return;

        float distance = Vector3.Distance(boat.position, transform.position);

        if (distance <= radius)
        {
            hasLoaded = true;
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmo) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
