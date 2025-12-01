using UnityEngine;

public class BoatMovement : MonoBehaviour
{
    public SteeringWheelController wheel;

    [Header("Boat Settings")]
    public float moveSpeed = 5f;
    public float maxTurnSpeed = 80f;

    [Header("Engine Audio")]
    public AudioSource boatEngineAudio;
    public float turnPitchAmount = 0.2f;   // ← HOW MUCH THE BOAT PITCHES LEFT/RIGHT
    private float basePitch = 1f;

    [HideInInspector] public bool controlsEnabled = false;

    private CharacterController controller;
    private float fixedY;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        fixedY = transform.position.y;

        if (boatEngineAudio != null)
        {
            basePitch = boatEngineAudio.pitch;
            boatEngineAudio.Stop();
        }
    }


    void FixedUpdate()
    {
        if (PauseManager.IsPaused)
            return;

        if (!controlsEnabled)
        {
            StopEngineAudio();
            return;
        }

        HandleSteering();
        HandleMovement();
    }


    public void StopBoatMovement()
    {
        controlsEnabled = false;
        StopEngineAudio();
    }


    void HandleSteering()
    {
        float t = wheel.currentWheelAngle / wheel.maxWheelAngle;
        t = Mathf.Pow(Mathf.Abs(t), 1.25f) * Mathf.Sign(t);

        float turn = t * maxTurnSpeed * Time.fixedDeltaTime;
        transform.Rotate(0f, turn, 0f);

        // 🔥 Pitch bend audio based on turning
        if (boatEngineAudio != null)
        {
            float pitch = basePitch + (t * turnPitchAmount);
            boatEngineAudio.pitch = pitch;
        }
    }


    void HandleMovement()
    {
        Vector3 move = transform.forward * moveSpeed;
        controller.Move(move * Time.fixedDeltaTime);

        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;

        // 🔥 Play engine audio when moving
        if (boatEngineAudio != null && !boatEngineAudio.isPlaying)
            boatEngineAudio.Play();
    }


    void StopEngineAudio()
    {
        if (boatEngineAudio != null)
        {
            boatEngineAudio.Stop();
            boatEngineAudio.pitch = basePitch;   // reset pitch
        }
    }
}
