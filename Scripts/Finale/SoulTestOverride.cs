using UnityEngine;

public class SoulTestOverride : MonoBehaviour
{
    [Header("Override SoulsSaved for Testing")]
    public bool applyOverride = true;
    public int overrideValue = 0;

    private void Start()
    {
        if (applyOverride && SoulManager.Instance != null)
        {
            SoulManager.Instance.SoulsSaved = overrideValue;
            Debug.Log($"[SoulTestOverride] SoulsSaved overridden to {overrideValue}");
        }
    }
}
