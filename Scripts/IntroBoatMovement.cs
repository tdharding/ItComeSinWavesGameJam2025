using UnityEngine;

public class IntroBoatMovement : MonoBehaviour
{
    [Header("References")]
    public Transform BoatRoot; // SAME object with CharacterController + BoatMovement
    public Transform WaypointA;
    public Transform WaypointB;
    public Transform WaypointC;

    [Header("Intro Movement Settings")]
    public float MoveSpeed = 3f;
    public float CheckpointRadius = 1.5f;
    public float EndRadius = 1.5f;
    public float MaxIntroDuration = 15f;

    [Header("Debug")]
    public bool ShowDebug = false;

    public bool ReachedB { get; private set; }
    public bool ReachedC { get; private set; }

    private bool moving;
    private float startTime;
    private float fixedY; // same as gameplay script uses

    void Start()
    {
        // Lock Y to match your BoatMovement system
        fixedY = BoatRoot.position.y;
    }

    public void BeginMovement()
    {
        // Reset state
        BoatRoot.position = new Vector3(
            WaypointA.position.x, 
            fixedY, 
            WaypointA.position.z
        );

        ReachedB = false;
        ReachedC = false;
        moving = true;
        startTime = Time.time;
    }

    void Update()
    {
        if (!moving) return;

        // --- SAFETY TIMEOUT --- //
        if (Time.time - startTime > MaxIntroDuration)
        {
            DebugLog("TIMEOUT: forcing C reached");
            ReachedB = true;
            ReachedC = true;
            moving = false;
            return;
        }

        // Flatten target to fixedY plane
        Vector3 target = new Vector3(
            WaypointC.position.x,
            fixedY,
            WaypointC.position.z
        );

        Vector3 diff = target - BoatRoot.position;

        // Handle extremely small float precision (WebGL issue)
        if (diff.sqrMagnitude < 0.0001f)
        {
            DebugLog("Zero diff detected — nudging");
            diff = (WaypointC.position - BoatRoot.position).normalized;
            if (diff == Vector3.zero)
                diff = Vector3.forward; 
        }

        Vector3 dir = diff.normalized;
        Vector3 move = dir * MoveSpeed * Time.deltaTime;

        // --- SAFE WEBGL MOVEMENT (NO CharacterController) --- //
        BoatRoot.position += move;

        // Hard-lock Y (same as BoatMovement)
        Vector3 p = BoatRoot.position;
        p.y = fixedY;
        BoatRoot.position = p;

        // --- CHECK REACHED WAYPOINTS --- //
        if (!ReachedB &&
            Vector3.Distance(BoatRoot.position, WaypointB.position) <= CheckpointRadius)
        {
            DebugLog("Reached B");
            ReachedB = true;
        }

        if (!ReachedC &&
            Vector3.Distance(BoatRoot.position, WaypointC.position) <= EndRadius)
        {
            DebugLog("Reached C");
            ReachedC = true;
            moving = false;
        }
    }

    private void DebugLog(string msg)
    {
        if (ShowDebug)
            Debug.Log("[IntroBoatMovement] " + msg);
    }
}
