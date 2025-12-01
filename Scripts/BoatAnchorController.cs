using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoatAnchorController : MonoBehaviour
{
    [Header("Refs")]
    public BoatMovement boat;
    public RectTransform anchorUI;

    [Header("Wheel Controller (Fades in/out)")]
    public SteeringWheelController steeringWheelController;

    [Header("Zoom Controller")]
    public BoatCameraZoom zoomController;

    [Header("Cursor + Line")]
    public VirtualCursor virtualCursor;
    public MonoBehaviour lineScript;
    public RectTransform cursorImage;

    [Header("Anchor UI Movement")]
    public float upY = 0f;
    public float downY = -650f;
    public float uiSpeed = 500f;

    [Header("Audio Sources (3 separate)")]
    public AudioSource dropSFX;
    public AudioSource impactSFX;
    public AudioSource hoistSFX;

    [Header("Fishing System")]
    public FishingController fishingController;

    [Header("Fishing UI Prompt")]
    public GameObject castNetPromptUI;

    [Header("Maze Walls Fade")]
    public Material mazeWallsMaterial;
    [Range(0f, 1f)] public float wallsTargetAlpha = 0.8f;

    // ========================================================
    // NEW: Anchor UI Text Switching
    // ========================================================
    [Header("Anchor UI Text")]
    public TMP_Text anchorStateText;       // UI text field
    public string anchorUpText = "Anchor Up";     // text when anchor is raised
    public string anchorDownText = "Anchor Down"; // text when anchor is lowered
    // ========================================================

    private bool anchorDown = false;
    private bool isAnimatingAnchorUI = false;

    void Start()
    {
        if (cursorImage != null)
            cursorImage.gameObject.SetActive(false);

        if (fishingController != null)
            fishingController.enabled = false;

        if (castNetPromptUI != null)
            castNetPromptUI.SetActive(false);

        // NEW — set text to default state
        if (anchorStateText != null)
            anchorStateText.text = anchorUpText;
    }

    public void InitializeAnchorState()
    {
        anchorDown = false;
        isAnimatingAnchorUI = false;

        if (anchorUI != null)
        {
            Vector2 p = anchorUI.anchoredPosition;
            p.y = upY;
            anchorUI.anchoredPosition = p;
        }

        steeringWheelController.FadeInWheel();
        steeringWheelController.EnableWheel();

        cursorImage?.gameObject.SetActive(false);

        zoomController?.ApplyAnchorZoom(false);

        if (fishingController != null)
            fishingController.enabled = false;

        if (castNetPromptUI != null)
            castNetPromptUI.SetActive(false);

        virtualCursor.SetNormalSensitivity();

        // NEW
        if (anchorStateText != null)
            anchorStateText.text = anchorUpText;
    }

    void Update()
    {
        if (PauseManager.IsPaused)
            return;

        if (Input.GetKeyDown(KeyCode.A))
            ToggleAnchor();

        if (isAnimatingAnchorUI)
            AnimateAnchorUI();

        if (anchorDown && cursorImage != null)
            cursorImage.position = virtualCursor.Position;
    }

    void ToggleAnchor()
    {
        if (isAnimatingAnchorUI)
            return;

        anchorDown = !anchorDown;

        // ========================================================
        // NEW — Update UI Text
        // ========================================================
        if (anchorStateText != null)
            anchorStateText.text = anchorDown ? anchorDownText : anchorUpText;
        // ========================================================

        if (anchorDown) dropSFX?.Play();
        else hoistSFX?.Play();

        if (!anchorDown)
            virtualCursor.ResetPosition();

        ApplyAnchorState();
    }

    void ApplyAnchorState()
    {
        if (anchorDown)
            steeringWheelController.DisableWheel();
        else
            steeringWheelController.EnableWheel();

        // Force retract net if anchor raised
        if (!anchorDown && fishingController != null)
            fishingController.ForceRetractNet();

        lineScript.enabled = !anchorDown;

        cursorImage?.gameObject.SetActive(anchorDown);

        boat.controlsEnabled = !anchorDown;
        if (anchorDown)
            boat.StopBoatMovement();

        isAnimatingAnchorUI = true;

        if (!anchorDown)
            steeringWheelController.FadeInWheel();

        zoomController?.ApplyAnchorZoom(anchorDown);
    }

    void AnimateAnchorUI()
    {
        float targetY = anchorDown ? downY : upY;

        // Maze walls fade synced with zoom
        if (mazeWallsMaterial != null && zoomController != null)
        {
            float t = zoomController.ZoomT; // 0→1 zoom tween
            float target = anchorDown ? wallsTargetAlpha : 1f;

            Color c = mazeWallsMaterial.color;
            c.a = Mathf.Lerp(1f, target, t);
            mazeWallsMaterial.color = c;
        }

        Vector2 pos = anchorUI.anchoredPosition;
        pos.y = Mathf.MoveTowards(pos.y, targetY, uiSpeed * Time.deltaTime);
        anchorUI.anchoredPosition = pos;

        if (Mathf.Abs(pos.y - targetY) < 0.1f)
        {
            isAnimatingAnchorUI = false;

            dropSFX?.Stop();
            hoistSFX?.Stop();

            if (anchorDown)
            {
                impactSFX?.Play();
                steeringWheelController.FadeOutWheel();

                if (fishingController != null)
                    fishingController.enabled = true;

                if (castNetPromptUI != null)
                    castNetPromptUI.SetActive(true);

                virtualCursor.SetAnchorSensitivity();
            }
            else
            {
                if (fishingController != null)
                    fishingController.enabled = false;

                if (castNetPromptUI != null)
                    castNetPromptUI.SetActive(false);

                virtualCursor.SetNormalSensitivity();
            }
        }
    }
}
