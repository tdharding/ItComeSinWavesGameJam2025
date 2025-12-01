using UnityEngine;
using Unity.Cinemachine;

public class GongWavesController : MonoBehaviour
{
    public enum GongWaveType
    {
        RockyWaves1,
        RockyWaves2
    }

    // ─────────────────────────────────────────────
    // CAMERAS + ANIMATOR
    // ─────────────────────────────────────────────
    [Header("Cinemachine Cameras")]
    public CinemachineCamera boatFollowCam;
    public CinemachineCamera gongCam;

    [Header("Gong Cinematic Animator Object")]
    public GameObject gongCamAnimatorGO;

    // ─────────────────────────────────────────────
    // WAVE MATERIAL CONTROL
    // ─────────────────────────────────────────────
    [Header("Wave Controller")]
    public WaveMaterialController waveController;

    [Header("Select Wave State for Gong Sequence (Dropdown")]
    public GongWaveType gongWaveType = GongWaveType.RockyWaves1;

    // ─────────────────────────────────────────────
    // GAMEPLAY SCRIPTS
    // ─────────────────────────────────────────────
    [Header("Gameplay Scripts")]
    public BoatMovement BoatMovementScript;
    public SteeringWheelController SteeringWheelControllerScript;
    public BoatAnchorController AnchorScript;

    // ─────────────────────────────────────────────
    // UI ELEMENTS
    // ─────────────────────────────────────────────
    [Header("UI Elements")]
    public GameObject WheelUIGameObject;
    public GameObject VerticalLineUI;
    public GameObject MapUI;
    public GameObject AnchorUI;

    [Header("Virtual Cursor")]
    public VirtualCursor virtualCursor;

    // ─────────────────────────────────────────────
    // CAMERA ZOOM
    // ─────────────────────────────────────────────
    [Header("Camera Zoom Controller")]
    public BoatCameraZoom zoomController;

    // ─────────────────────────────────────────────
    // AUDIO + MUSIC OBJECT
    // ─────────────────────────────────────────────
    [Header("Audio")]
    public AudioSource gongAudio;
    public AudioSource gameplayMusicSource;

    [Header("Return to Gameplay Music Object")]
    public GameObject returnToGameplayMusicObject;

    // ─────────────────────────────────────────────
    // BOAT VARIABLE OVERRIDES
    // ─────────────────────────────────────────────
    [Header("Boat Speed Overrides (Applied After Gong)")]
    public float newMoveSpeed = 8f;
    public float newMaxTurnSpeed = 120f;

    // ====================================================================
    // START OF THE GONG SEQUENCE
    // ====================================================================
    public void StartGongSequence()
    {
        gameplayMusicSource?.Stop();

        BoatMovementScript.controlsEnabled = false;
        AnchorScript.enabled = false;
        SteeringWheelControllerScript.DisableWheel();

        WheelUIGameObject.SetActive(false);
        VerticalLineUI.SetActive(false);
        MapUI.SetActive(false);
        AnchorUI.SetActive(false);

        WaveMaterialController.WaveState selectedState;

        switch (gongWaveType)
        {
            case GongWaveType.RockyWaves1:
                selectedState = waveController.RockyWaves1;
                break;

            case GongWaveType.RockyWaves2:
                selectedState = waveController.RockyWaves2;
                break;

            default:
                selectedState = waveController.RockyWaves1;
                break;
        }

        StartCoroutine(
            waveController.TransitionToState(
                selectedState,
                waveController.TransitionDuration
            )
        );

        boatFollowCam.gameObject.SetActive(false);
        gongCam.gameObject.SetActive(true);
        gongCamAnimatorGO.SetActive(true);
    }

    // ====================================================================
    // GONG STRIKE SOUND
    // ====================================================================
    public void PlayGongSound()
    {
        gongAudio?.Play();
    }

    // ====================================================================
    // END OF GONG SEQUENCE
    // ====================================================================
    public void OnSecondGongSequenceFinished()
    {
        gongCam.gameObject.SetActive(false);
        gongCamAnimatorGO.SetActive(false);
        boatFollowCam.gameObject.SetActive(true);

        WheelUIGameObject.SetActive(true);
        VerticalLineUI.SetActive(true);
        MapUI.SetActive(true);
        AnchorUI.SetActive(true);

        virtualCursor.ResetPosition();
        zoomController.ResetToDefaultFOV();

        BoatMovementScript.controlsEnabled = true;

        // APPLY NEW BOAT SPEED VALUES
        BoatMovementScript.moveSpeed = newMoveSpeed;
        BoatMovementScript.maxTurnSpeed = newMaxTurnSpeed;

        AnchorScript.enabled = true;
        AnchorScript.InitializeAnchorState();

        SteeringWheelControllerScript.StartWheelTransition();

        if (returnToGameplayMusicObject != null)
            returnToGameplayMusicObject.SetActive(true);
    }
}
