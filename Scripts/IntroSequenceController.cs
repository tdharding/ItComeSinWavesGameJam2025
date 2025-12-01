using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class IntroSequenceController : MonoBehaviour
{
    // -------------------------------------------------------
    // Cameras
    // -------------------------------------------------------
    [Header("Cameras")]
    public CinemachineCamera IntroCam1;
    public CinemachineCamera IntroCam2;
    public CinemachineCamera IntroCam3;
    public CinemachineCamera IntroCam4;
    public CinemachineCamera IntroCam5;
    public CinemachineCamera BoatFollowCam;
    public BoatCameraZoom zoomController;

    // -------------------------------------------------------
    // Boats
    // -------------------------------------------------------
    [Header("Boats")]
    public GameObject CutsceneBoat;
    public GameObject GameplayBoat;
    public Transform IntroWaypointC;

    // -------------------------------------------------------
    // Waves
    // -------------------------------------------------------
    [Header("Wave Controller")]
    public WaveMaterialController waveController;

    [Header("Boat Visual Settings")]
    public BoatVisual boatVisual;
    public float IntroExtraYOffset = 0f;
    public float GameplayExtraYOffset = 0.05f;

    [Header("Wave Plane")]
    public GameObject wavePlaneObject;

    // -------------------------------------------------------
    // UI
    // -------------------------------------------------------
    [Header("Wheel UI")]
    public RectTransform WheelUIRect;
    public GameObject WheelUIGameObject;
    public float WheelUIStartY = -500f;

    [Header("Map UI")]
    public GameObject MapUI;

    [Header("Anchor UI + Script")]
    public GameObject AnchorUI;
    public BoatAnchorController AnchorScript;

    [Header("Additional UI")]
    public GameObject VerticalLineUI;

    [Header("Skip UI")]
    public GameObject SkipUI;

    // -------------------------------------------------------
    // Gameplay scripts
    // -------------------------------------------------------
    [Header("Gameplay Scripts")]
    public BoatMovement BoatMovementScript;
    public SteeringWheelController SteeringWheelControllerScript;

    // -------------------------------------------------------
    // Music
    // -------------------------------------------------------
    [Header("Music")]
    public AudioSource musicSource;
    public float MusicFadeDuration = 2f;
    public float MusicStartVolume = 0f;
    public float MusicTargetVolume = 1f;

    // -------------------------------------------------------
    // Cursor
    // -------------------------------------------------------
    [Header("Virtual Cursor")]
    public VirtualCursor virtualCursor;

    // -------------------------------------------------------
    // SisterNom + MAZE
    // -------------------------------------------------------
    [Header("SisterNom Animation")]
    public Animator sisterNomAnimator;
    public string sisterNomTrigger = "PlayNom";
    public BellSoundPlayer bellPlayer;

    [Header("MAZE Reveal")]
    public GameObject MazeObject;
    public Material MazeMaterial;
    public float MazeFadeDuration = 2f;
      public AudioSource mazeSound;

    // -------------------------------------------------------
    // Internal state
    // -------------------------------------------------------
    private bool introCompleted = false;


    // -------------------------------------------------------
    private void Start()
    {
        SetupInitialState();
    }


    private void Update()
    {
        if (!introCompleted && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("INTRO SKIPPED.");
            EnterGameplayState(true);
        }
    }


    // -------------------------------------------------------
    // INITIAL INTRO SETUP
    // -------------------------------------------------------
    private void SetupInitialState()
    {
        wavePlaneObject?.SetActive(true);

        BoatMovementScript.controlsEnabled = false;
        SteeringWheelControllerScript.DisableWheel();

        WheelUIGameObject.SetActive(false);
        VerticalLineUI.SetActive(false);
        MapUI?.SetActive(false);
        AnchorUI?.SetActive(false);

        if (AnchorScript != null)
            AnchorScript.enabled = false;

        Vector2 pos = WheelUIRect.anchoredPosition;
        pos.y = WheelUIStartY;
        WheelUIRect.anchoredPosition = pos;

        GameplayBoat.SetActive(false);
        if (CutsceneBoat != null) CutsceneBoat.SetActive(false);

        BoatFollowCam.gameObject.SetActive(false);

        boatVisual.extraYOffset = IntroExtraYOffset;

        waveController.ApplyStateInstant(waveController.IntroCam1State);

        IntroCam1.gameObject.SetActive(true);
        IntroCam2.gameObject.SetActive(false);
        IntroCam3.gameObject.SetActive(false);
        IntroCam4.gameObject.SetActive(false);
        IntroCam5.gameObject.SetActive(false);

        if (SkipUI != null)
            SkipUI.SetActive(true);
    }


    // -------------------------------------------------------
    // INTRO CUTSCENE EVENTS
    // -------------------------------------------------------
    public void OnIntroCam1Finish()
    {
        if (introCompleted) return;
        if (CutsceneBoat != null)
            CutsceneBoat.SetActive(true);

        SwitchToCamera(IntroCam1, IntroCam2, waveController.IntroCam2State);
    }

    public void OnReachedIntroWayPointB()
    {
        if (introCompleted) return;
        SwitchToCamera(IntroCam2, IntroCam3, waveController.IntroCam3State);
    }

    public void OnBoatIntroComplete()
    {
        if (introCompleted) return;
        SwitchToCamera(IntroCam3, IntroCam4, waveController.IntroCam4State);
    }

    public void IntroCam4_TriggerSisterNom()
    {
        if (introCompleted) return;
        sisterNomAnimator?.SetTrigger(sisterNomTrigger);
    }

    public void SisterNom_BellSoundPlay()
    {
        if (introCompleted) return;
        bellPlayer?.PlayBellSound();
    }

    public void SisterNom_AnimationFinished()
    {
        if (introCompleted) return;
mazeSound?.Play();
        SwitchToCamera(IntroCam4, IntroCam5, waveController.IntroCam5State);
        StartCoroutine(HandleCam5WaveTransition());
    }


    private IEnumerator HandleCam5WaveTransition()
    {
        yield return StartCoroutine(
            waveController.TransitionToState(
                waveController.IntroCam5State,
                waveController.TransitionDuration
            )
        );

        yield return StartCoroutine(FadeMaze());

        EnterGameplayState(false);
    }


    private IEnumerator FadeMaze()
    {
        if (MazeObject != null) MazeObject.SetActive(true);
        if (MazeMaterial == null) yield break;

        float t = 0f;
        Color c = MazeMaterial.color;
        c.a = 0;
        MazeMaterial.color = c;

        while (t < MazeFadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / MazeFadeDuration);
            MazeMaterial.color = c;
            yield return null;
            
        }

        c.a = 1;
        MazeMaterial.color = c;
    }


    // -------------------------------------------------------
    // FINAL UNIFIED GAMEPLAY STATE (FIXED)
    // -------------------------------------------------------
    private void EnterGameplayState(bool skipped)
    {
        if (introCompleted)
            return;

        introCompleted = true;

        Debug.Log(skipped ? "Gameplay started via SKIP." : "Gameplay started naturally.");

        if (SkipUI != null)
            SkipUI.SetActive(false);

        // ---------------------------------------------------
        // 🔥 APPLY GAMEPLAY WAVE STATE (THE FIX)
        // ---------------------------------------------------
        if (skipped)
        {
            waveController.ApplyStateInstant(waveController.GameplayState);
        }
        else
        {
            StartCoroutine(waveController.TransitionToState(
                waveController.GameplayState,
                waveController.TransitionDuration
            ));
        }

        // Destroy intro cameras
        if (IntroCam1 != null) Destroy(IntroCam1.gameObject);
        if (IntroCam2 != null) Destroy(IntroCam2.gameObject);
        if (IntroCam3 != null) Destroy(IntroCam3.gameObject);
        if (IntroCam4 != null) Destroy(IntroCam4.gameObject);
        if (IntroCam5 != null) Destroy(IntroCam5.gameObject);

        BoatFollowCam.gameObject.SetActive(true);

        StartCoroutine(FadeMusic());
        zoomController?.ResetToDefaultFOV();

        if (CutsceneBoat != null)
            CutsceneBoat.SetActive(false);

        boatVisual.extraYOffset = GameplayExtraYOffset;

        if (GameplayBoat != null)
        {
            GameplayBoat.SetActive(true);
            GameplayBoat.transform.SetPositionAndRotation(
                IntroWaypointC.position,
                IntroWaypointC.rotation
            );
        }

        WheelUIGameObject.SetActive(true);
        VerticalLineUI.SetActive(true);
        MapUI?.SetActive(true);
        AnchorUI?.SetActive(true);

        virtualCursor?.ResetPosition();
        BoatMovementScript.controlsEnabled = true;
        SteeringWheelControllerScript.StartWheelTransition();

        if (AnchorScript != null)
        {
            AnchorScript.enabled = true;
            AnchorScript.InitializeAnchorState();
        }

        if (skipped && MazeObject != null)
            MazeObject.SetActive(true);
    }


    // -------------------------------------------------------
    private void SwitchToCamera(
        CinemachineCamera fromCam,
        CinemachineCamera toCam,
        WaveMaterialController.WaveState state
    )
    {
        if (fromCam != null)
        {
            fromCam.gameObject.SetActive(false);
            Destroy(fromCam.gameObject);
        }

        toCam.gameObject.SetActive(true);
        waveController.ApplyStateInstant(state);
    }


    // -------------------------------------------------------
    private IEnumerator FadeMusic()
    {
        float t = 0f;

        musicSource.volume = MusicStartVolume;
        musicSource.Play();

        while (t < MusicFadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume =
                Mathf.Lerp(MusicStartVolume, MusicTargetVolume, t / MusicFadeDuration);

            yield return null;
        }

        musicSource.volume = MusicTargetVolume;
    }
}
