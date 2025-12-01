using UnityEngine;

public class BoatCollisionHandler : MonoBehaviour
{
    [Header("Crash Settings")]
    public float crashCooldown = 0.75f;
    public CrashSFXPlayer crashSFXPlayer;

    [Header("Soul Loss")]
    [Range(0f,1f)]
    public float loseSoulChance = 0.5f;
    public Transform soulsParent;
    public float launchForce = 6f;
    public float upwardForce = 2f;

    [Header("SFX")]
    public AudioSource soulLostSFX;   // <--- NEW

    private bool canCrash = true;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.collider.CompareTag("MazeWalls"))
            return;

        if (!canCrash)
            return;

        crashSFXPlayer?.PlayRandomCrash();
        StartCoroutine(CrashCooldown());

        if (Random.value <= loseSoulChance)
            LoseRandomSoul();
    }

    private System.Collections.IEnumerator CrashCooldown()
    {
        canCrash = false;
        yield return new WaitForSeconds(crashCooldown);
        canCrash = true;
    }

    void LoseRandomSoul()
    {
        Transform soul = GetRandomActiveSoul();
        if (soul == null)
            return;

        // --- play your 1 SFX here ---
        soulLostSFX?.Play();   // <--- NEW

        soul.SetParent(null);

        Rigidbody rb = soul.GetComponent<Rigidbody>();
        if (rb == null)
            rb = soul.gameObject.AddComponent<Rigidbody>();

        Vector3 randomDir =
            (transform.right * Random.Range(-1f, 1f) +
             transform.forward * Random.Range(-1f, 1f)).normalized;

        rb.AddForce(randomDir * launchForce + Vector3.up * upwardForce, ForceMode.Impulse);

        SoulManager.Instance.SoulsSaved = Mathf.Max(0, SoulManager.Instance.SoulsSaved - 1);

        Destroy(soul.gameObject, 5f);
    }

    Transform GetRandomActiveSoul()
    {
        var list = new System.Collections.Generic.List<Transform>();

        for (int i = 0; i < soulsParent.childCount; i++)
        {
            Transform s = soulsParent.GetChild(i);
            if (s.gameObject.activeSelf)
                list.Add(s);
        }

        if (list.Count == 0)
            return null;

        return list[Random.Range(0, list.Count)];
    }
}
