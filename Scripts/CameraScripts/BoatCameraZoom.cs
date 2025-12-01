using UnityEngine;
using Unity.Cinemachine;

public class BoatCameraZoom : MonoBehaviour
{
    public CinemachineCamera cam;

    [Header("Scroll Zoom Settings")]
    public float zoomSpeed = 10f;
    public float minFOV = 2f;
    public float maxFOV = 60f;

    [Header("Anchor Zoom Settings")]
    public float anchorFOV = 40f;
    public float anchorTweenTime = 0.25f;

    [Header("Default Gameplay FOV")]
    public float defaultFOV = 50f;

    // ---- internal state ----
    private enum ZoomState { Idle, Tweening }
    private ZoomState state = ZoomState.Idle;

    private float tweenTimer;
    private float tweenFrom;
    private float tweenTo;

    private bool anchorMode = false;
    public bool enableZoom = true;

    public float ZoomT { get; private set; } = 0f;


    void Start()
    {
        if (cam != null)
            cam.Lens.FieldOfView = defaultFOV;
    }

    void Update()
    {
        if (PauseManager.IsPaused)
            return;

        UpdateTween();
        UpdateScrollZoom();
    }

    // ----------------------------------------------------------
    // SCROLL ZOOM
    // ----------------------------------------------------------
    void UpdateScrollZoom()
    {
        if (!enableZoom || anchorMode || cam == null)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.01f)
            return;

        float fov = cam.Lens.FieldOfView;
        fov -= scroll * zoomSpeed;
        cam.Lens.FieldOfView = Mathf.Clamp(fov, minFOV, maxFOV);
    }

    // ----------------------------------------------------------
    // ANCHOR ZOOM
    // ----------------------------------------------------------
    public void ApplyAnchorZoom(bool isDown)
    {
        if (cam == null)
            return;

        anchorMode = isDown;
        enableZoom = !isDown;

        StartTween(
            from: cam.Lens.FieldOfView,
            to: isDown ? anchorFOV : defaultFOV
        );
    }

    // ----------------------------------------------------------
    // TWEEN SYSTEM
    // ----------------------------------------------------------
    void StartTween(float from, float to)
    {
        state = ZoomState.Tweening;
        tweenTimer = 0f;
        tweenFrom = from;
        tweenTo = to;
    }

    void UpdateTween()
    {
        if (state != ZoomState.Tweening)
            return;

        tweenTimer += Time.deltaTime;
        float t = Mathf.Clamp01(tweenTimer / anchorTweenTime);

        ZoomT = t;

        if (cam != null)
            cam.Lens.FieldOfView = Mathf.Lerp(tweenFrom, tweenTo, t);

        if (t >= 1f)
        {
            state = ZoomState.Idle;

            // After releasing anchor, re-enable scroll zoom
            if (!anchorMode)
                enableZoom = true;
        }
    }

    // ----------------------------------------------------------
    public void ResetToDefaultFOV()
    {
        if (cam != null)
            cam.Lens.FieldOfView = defaultFOV;
    }
}
