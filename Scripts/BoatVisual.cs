using UnityEngine;

public class BoatVisual : MonoBehaviour
{
    [Header("Assign the water plane (wave mesh)")]
    public Transform waterTransform;

    [Header("Wave Settings")]
    public float extraYOffset = 0f;
    public float tiltSampleOffset = 0.1f;
    public float tiltMultiplier = 100f;
    public float rollMultiplier = 100f;
    public float maxTiltUp = 53f;
    public float maxTiltDown = -22f;
    public float maxRollAngle = 60f;

    [Header("Smooth Direction Flip Settings")]
    public bool flipTiltWhenMovingTowards = true;
    public float directionSmoothSpeed = 8f;
    public float deadzone = 0.15f;

    private float directionBlend = 0f;
    private Quaternion baseRotation;

    private Material mat;
    int freqID, speedID, depthID;

    Transform root;

    void Start()
    {
        root = transform.parent; // BoatRoot

        mat = waterTransform.GetComponent<MeshRenderer>().sharedMaterial;

        freqID  = Shader.PropertyToID("_Frequency");
        speedID = Shader.PropertyToID("_Speed");
        depthID = Shader.PropertyToID("_RippleDepth");

        baseRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        ApplyWaveHeightAndTilt();
    }

    void ApplyWaveHeightAndTilt()
    {
        Vector3 boatWorldPos = root.position;
        Vector3 waveCenter = waterTransform.position;

        float frequency = mat.GetFloat(freqID);
        float waveSpeed = mat.GetFloat(speedID);
        float ripple = mat.GetFloat(depthID);

        float phase = -(Time.time * waveSpeed);
        float meshScale = waterTransform.localScale.x;

        float Dist(Vector3 p)
        {
            return Vector3.Distance(p, waveCenter) / meshScale;
        }

        // WAVE HEIGHT
        float sine = Mathf.Sin(phase + Dist(boatWorldPos) * frequency);
        float amplitude = ripple * meshScale;
        float height = sine * amplitude + extraYOffset;

        // Apply only to visual (local Y)
        Vector3 localPos = transform.localPosition;
        localPos.y = height;
        transform.localPosition = localPos;

        // SMOOTH DIRECTION DETECTOR
        Vector3 radialDir = (boatWorldPos - waveCenter).normalized;
        Vector3 moveDir   = root.forward;

        float rawDot = Vector3.Dot(moveDir, radialDir);
        float targetDir;

        if (Mathf.Abs(rawDot) < deadzone)
            targetDir = 0f;
        else
        {
            float sign = Mathf.Sign(rawDot);
            float t = (Mathf.Abs(rawDot) - deadzone) / (1f - deadzone);
            targetDir = Mathf.Clamp01(t) * sign;
        }

        directionBlend = Mathf.Lerp(directionBlend, targetDir, Time.deltaTime * directionSmoothSpeed);
        if (Mathf.Abs(directionBlend) < 0.0001f) directionBlend = 0f;

        // PITCH SAMPLING
        Vector3 front = boatWorldPos + radialDir * tiltSampleOffset;
        Vector3 back  = boatWorldPos - radialDir * tiltSampleOffset;
        float frontH = Mathf.Sin(phase + Dist(front) * frequency) * amplitude;
        float backH  = Mathf.Sin(phase + Dist(back)  * frequency) * amplitude;

        float pitchAmount = (backH - frontH);

        if (flipTiltWhenMovingTowards)
        {
            float flipAmount = Mathf.InverseLerp(0f, -1f, directionBlend);
            float flip = Mathf.Lerp(1f, -1f, flipAmount);
            pitchAmount *= flip;
        }

        float pitchAngle = Mathf.Clamp(pitchAmount * tiltMultiplier, maxTiltDown, maxTiltUp);

        // ROLL SAMPLING
        Vector3 tangentDir = Vector3.Cross(Vector3.up, radialDir).normalized;

        Vector3 right = boatWorldPos + tangentDir * tiltSampleOffset;
        Vector3 left  = boatWorldPos - tangentDir * tiltSampleOffset;
        float rightH = Mathf.Sin(phase + Dist(right) * frequency) * amplitude;
        float leftH  = Mathf.Sin(phase + Dist(left)  * frequency) * amplitude;

        float rollAmount = (leftH - rightH);
        float rollAngle = Mathf.Clamp(rollAmount * rollMultiplier, -maxRollAngle, maxRollAngle);

        // APPLY FINAL LOCAL ROTATION
        Quaternion tilt = Quaternion.Euler(pitchAngle, 0, rollAngle);
        transform.localRotation = baseRotation * tilt;
    }
}
