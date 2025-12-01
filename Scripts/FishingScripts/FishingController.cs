using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;
using TMPro;

public class FishingController : MonoBehaviour
{
    [Header("Refs")]
    public VirtualCursor virtualCursor;
    public GameObject netObject;
    public Transform boat;

    [Header("Souls System")]
    public Transform soulsParent;

    [Header("RenderTexture UI")]
    public RectTransform rawImageRect;
    public Camera renderTextureCamera;

    [Header("UI Message")]
    public GameObject messagePrefab;
    public Canvas uiCanvas;
    public float messageDuration = 1f;

    [Header("Audio")]
   public SoulCaptureSFXPlayer soulCaptureSFX;

    public AudioSource netToggleSFX;   // <-- NEW: Plays on net deploy & retract

    [Header("Animation")]
    public Animator boatAnimator;
    public string deployNetTrigger = "DeployNet";
    public string retractNetTrigger = "RetractNet";

    // ======================================================
    // MULTI-BONE NET MOVEMENT SYSTEM
    // ======================================================
    [Header("Net Aiming (Multi-Bone Movement)")]
    public Transform netBoneBase;
    public Transform netBoneTip;

    public float baseBoneMoveSpeed = 3f;
    public float tipBoneMoveSpeed = 6f;

    [Range(0f, 1f)] public float baseBoneInfluence = 0.3f;
    [Range(0f, 1f)] public float tipBoneInfluence = 1.0f;

    public float maxNetReachDistance = 8f;
    private bool netBoneMovementEnabled = true;

    private Vector3 baseBoneDefaultPos;
    private Quaternion baseBoneDefaultRot;

    private Vector3 tipBoneDefaultPos;
    private Quaternion tipBoneDefaultRot;

    // ======================================================
    // UI NET SLIDE
    // ======================================================
    [Header("UI Net Slide")]
    public RectTransform uiNet;
    public float uiNetUpY = -63.77f;
    public float uiNetDownY = -400f;
    public float uiNetSlideSpeed = 5f;

    [Header("Net Text UI")]
    public TMP_Text netStatusText;
    public string textWhenNetActive = "Net Ready";
    public string textWhenNetInactive = "Net Retracted";

    // ======================================================

    private SoulShoalController currentShoal;
    private SoulShoalController[] allShoals;

    private bool netActive = false;


    void Start()
    {
        if (netBoneBase != null)
        {
            baseBoneDefaultPos = netBoneBase.localPosition;
            baseBoneDefaultRot = netBoneBase.localRotation;
        }

        if (netBoneTip != null)
        {
            tipBoneDefaultPos = netBoneTip.localPosition;
            tipBoneDefaultRot = netBoneTip.localRotation;
        }

        if (uiNet != null)
        {
            Vector2 p = uiNet.anchoredPosition;
            p.y = uiNetUpY;
            uiNet.anchoredPosition = p;
        }
    }

    void OnEnable()
    {
        netActive = false;

        if (netObject != null)
            netObject.SetActive(false);

        allShoals = FindObjectsOfType<SoulShoalController>();
    }

    void Update()
    {
        if (PauseManager.IsPaused)
            return;

        currentShoal = GetClosestActiveShoal();

        // -----------------------------------------
        // Toggle Net
        // -----------------------------------------
        if (Input.GetKeyDown(KeyCode.N))
        {
            netActive = !netActive;

            // Play toggle sound
            if (netToggleSFX != null)
                netToggleSFX.Play();

            // UI SLIDE + TEXT
            if (netStatusText != null)
                netStatusText.text = netActive ? textWhenNetActive : textWhenNetInactive;

            StopAllCoroutines();
            StartCoroutine(SlideUiNet(netActive));

            if (netActive)
            {
                boatAnimator.ResetTrigger(retractNetTrigger);
                boatAnimator.SetTrigger(deployNetTrigger);
                netObject.SetActive(true);
            }
            else
            {
                boatAnimator.ResetTrigger(deployNetTrigger);
                boatAnimator.SetTrigger(retractNetTrigger);
                ResetNetBones();
            }
        }

        if (!netActive)
            return;

        if (netBoneMovementEnabled)
            MoveNetBones();

        if (Input.GetMouseButtonDown(0))
            TryCatchFish();
    }

    // ======================================================
    // RESET BONES
    // ======================================================
    public void ResetNetBones()
    {
        if (netBoneBase != null)
        {
            netBoneBase.localPosition = baseBoneDefaultPos;
            netBoneBase.localRotation = baseBoneDefaultRot;
        }

        if (netBoneTip != null)
        {
            netBoneTip.localPosition = tipBoneDefaultPos;
            netBoneTip.localRotation = tipBoneDefaultRot;
        }
    }

    // ======================================================
    // FORCE RETRACT
    // ======================================================
    public void ForceRetractNet()
    {
        if (!netActive) return;

        netActive = false;

        boatAnimator.ResetTrigger(deployNetTrigger);
        boatAnimator.SetTrigger(retractNetTrigger);

        if (netToggleSFX != null)
            netToggleSFX.Play();

        ResetNetBones();
    }

    // ======================================================
    // MOVE BONES WITH MAX Y CLAMP
    // ======================================================
    void MoveNetBones()
    {
        Ray ray = GetRenderTextureRay();
        Vector3 target = Physics.Raycast(ray, out RaycastHit hit, 500f)
            ? hit.point
            : ray.origin + ray.direction * maxNetReachDistance;

        // BASE BONE
        Vector3 desiredBase = Vector3.Lerp(netBoneBase.position, target, baseBoneInfluence);
        Vector3 newBasePos = Vector3.Lerp(netBoneBase.position, desiredBase, Time.deltaTime * baseBoneMoveSpeed);

        newBasePos.y = Mathf.Min(newBasePos.y, baseBoneDefaultPos.y);
        netBoneBase.position = newBasePos;

        // TIP BONE
        Vector3 desiredTip = Vector3.Lerp(netBoneTip.position, target, tipBoneInfluence);
        Vector3 newTipPos = Vector3.Lerp(netBoneTip.position, desiredTip, Time.deltaTime * tipBoneMoveSpeed);

        newTipPos.y = Mathf.Min(newTipPos.y, tipBoneDefaultPos.y);
        netBoneTip.position = newTipPos;
    }

    public void EnableNetBoneMovement() => netBoneMovementEnabled = true;
    public void DisableNetBoneMovement() => netBoneMovementEnabled = false;

    // ======================================================
    // UI NET SLIDE
    // ======================================================
    IEnumerator SlideUiNet(bool down)
    {
        if (uiNet == null) yield break;

        float targetY = down ? uiNetDownY : uiNetUpY;

        while (Mathf.Abs(uiNet.anchoredPosition.y - targetY) > 0.1f)
        {
            Vector2 p = uiNet.anchoredPosition;
            p.y = Mathf.Lerp(p.y, targetY, Time.deltaTime * uiNetSlideSpeed);
            uiNet.anchoredPosition = p;
            yield return null;
        }

        Vector2 final = uiNet.anchoredPosition;
        final.y = targetY;
        uiNet.anchoredPosition = final;
    }

    // ======================================================
    // ORIGINAL REMAINING CODE
    // ======================================================
    SoulShoalController GetClosestActiveShoal()
    {
        SoulShoalController best = null;
        float bestDist = float.MaxValue;

        foreach (var s in allShoals)
        {
            if (s == null || !s.IsActive) continue;

            float d = Vector3.Distance(boat.position, s.transform.position);

            if (d < bestDist)
            {
                best = s;
                bestDist = d;
            }
        }

        return best;
    }

    void TryCatchFish()
    {
        if (PauseManager.IsPaused) return;
        if (currentShoal == null || !currentShoal.CanFish)
        {
            ShowMessage("Too far from the fish!");
            return;
        }

        Ray ray = GetRenderTextureRay();
        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            while (hit.collider.CompareTag("MazeWalls"))
            {
                Vector3 newOrigin = hit.point + ray.direction * 0.01f;

                if (!Physics.Raycast(newOrigin, ray.direction, out hit, 200f))
                {
                    ShowMessage("No fish here...");
                    return;
                }
            }

            if (hit.collider.CompareTag("Fish"))
            {
                soulCaptureSFX?.PlayRandomCapture();

                Destroy(hit.collider.gameObject);
                ActivateRandomSoul();
                SoulManager.Instance.AddSoul();
                return;
            }
        }

        ShowMessage("No fish here...");
    }

    void ActivateRandomSoul()
    {
        if (soulsParent == null) return;

        Transform[] disabled = GetDisabledSouls();
        if (disabled.Length == 0) return;

        disabled[Random.Range(0, disabled.Length)].gameObject.SetActive(true);
    }

    Transform[] GetDisabledSouls()
    {
        int total = soulsParent.childCount;
        int count = 0;

        for (int i = 0; i < total; i++)
            if (!soulsParent.GetChild(i).gameObject.activeSelf)
                count++;

        Transform[] arr = new Transform[count];
        int idx = 0;

        for (int i = 0; i < total; i++)
        {
            var c = soulsParent.GetChild(i);
            if (!c.gameObject.activeSelf)
                arr[idx++] = c;
        }

        return arr;
    }

    Ray GetRenderTextureRay()
    {
        if (rawImageRect == null || renderTextureCamera == null)
            return new Ray(Vector3.zero, Vector3.forward);

        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImageRect,
            virtualCursor.Position,
            null,
            out local))
        {
            return new Ray(Vector3.zero, Vector3.forward);
        }

        float w = rawImageRect.rect.width;
        float h = rawImageRect.rect.height;

        Vector2 uv = new Vector2(
            (local.x / w) + rawImageRect.pivot.x,
            (local.y / h) + rawImageRect.pivot.y
        );
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        var rt = renderTextureCamera.targetTexture;
        Vector3 px = new Vector3(uv.x * rt.width, uv.y * rt.height, 0);

        return renderTextureCamera.ScreenPointToRay(px);
    }

    void ShowMessage(string text)
    {
        GameObject msg = Instantiate(messagePrefab, uiCanvas.transform);
        TextMeshProUGUI tmp = msg.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = text;

        msg.transform.position = virtualCursor.Position;
        StartCoroutine(FadeAndDestroy(msg));
    }

    IEnumerator FadeAndDestroy(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        float t = messageDuration;

        while (t > 0)
        {
            t -= Time.deltaTime;
            if (cg != null)
                cg.alpha = Mathf.InverseLerp(0, messageDuration, t);
            yield return null;
        }

        Destroy(obj);
    }
}
