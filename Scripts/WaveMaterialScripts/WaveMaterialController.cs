using UnityEngine;
using System.Collections;

public class WaveMaterialController : MonoBehaviour
{
    [System.Serializable]
    public struct WaveState
    {
        public float Frequency;
        public float Speed;
        public float RippleDepth;
        public float Smoothness;
        public float Transparency;
        public float Strength;
    }

    [Header("Wave Material (Shared Instance)")]
    public Material waveMaterial;

    [Header("Transition Settings")]
    public float TransitionDuration = 2f;

    // ─────────────────────────────────────────
    // WAVE STATES
    // ─────────────────────────────────────────

    [Header("IntroCam1 Wave State")]
    public WaveState IntroCam1State;

    [Header("IntroCam2 Wave State")]
    public WaveState IntroCam2State;

    [Header("IntroCam3 Wave State")]
    public WaveState IntroCam3State;

    [Header("IntroCam4 Wave State")]
    public WaveState IntroCam4State;

    [Header("IntroCam5 Wave State")]
    public WaveState IntroCam5State;

    [Header("Gameplay Wave State")]
    public WaveState GameplayState;

    [Header("RockyWaves1 State")]
    public WaveState RockyWaves1;

    [Header("RockyWaves2 State")]
    public WaveState RockyWaves2;

    // ─────────────────────────────────────────
    // INSTANT APPLY
    // ─────────────────────────────────────────
    public void ApplyStateInstant(WaveState state)
    {
        waveMaterial.SetFloat("_Frequency", state.Frequency);
        waveMaterial.SetFloat("_Speed", state.Speed);
        waveMaterial.SetFloat("_RippleDepth", state.RippleDepth);

        waveMaterial.SetFloat("_Smoothness", state.Smoothness);
        waveMaterial.SetFloat("_Transparency", state.Transparency);
        waveMaterial.SetFloat("_Strength", state.Strength);
    }

    // ─────────────────────────────────────────
    // TRANSITION (LERP OVER TIME)
    // ─────────────────────────────────────────
    public IEnumerator TransitionToState(WaveState targetState, float duration)
    {
        float timer = 0f;

        // Current values
        float curFreq     = waveMaterial.GetFloat("_Frequency");
        float curSpeed    = waveMaterial.GetFloat("_Speed");
        float curRipple   = waveMaterial.GetFloat("_RippleDepth");
        float curSmooth   = waveMaterial.GetFloat("_Smoothness");
        float curTrans    = waveMaterial.GetFloat("_Transparency");
        float curStrength = waveMaterial.GetFloat("_Strength");

        // Rate = how much to change per second
        float freqRate     = (targetState.Frequency   - curFreq)     / duration;
        float speedRate    = (targetState.Speed       - curSpeed)    / duration;
        float rippleRate   = (targetState.RippleDepth - curRipple)   / duration;

        float smoothRate   = (targetState.Smoothness  - curSmooth)   / duration;
        float transRate    = (targetState.Transparency- curTrans)    / duration;
        float strengthRate = (targetState.Strength    - curStrength) / duration;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            curFreq     += freqRate     * Time.deltaTime;
            curSpeed    += speedRate    * Time.deltaTime;
            curRipple   += rippleRate   * Time.deltaTime;

            curSmooth   += smoothRate   * Time.deltaTime;
            curTrans    += transRate    * Time.deltaTime;
            curStrength += strengthRate * Time.deltaTime;

            waveMaterial.SetFloat("_Frequency",    curFreq);
            waveMaterial.SetFloat("_Speed",        curSpeed);
            waveMaterial.SetFloat("_RippleDepth",  curRipple);

            waveMaterial.SetFloat("_Smoothness",   curSmooth);
            waveMaterial.SetFloat("_Transparency", curTrans);
            waveMaterial.SetFloat("_Strength",     curStrength);

            yield return null;
        }

        // Ensure final target values are applied exactly
        waveMaterial.SetFloat("_Frequency",    targetState.Frequency);
        waveMaterial.SetFloat("_Speed",        targetState.Speed);
        waveMaterial.SetFloat("_RippleDepth",  targetState.RippleDepth);

        waveMaterial.SetFloat("_Smoothness",   targetState.Smoothness);
        waveMaterial.SetFloat("_Transparency", targetState.Transparency);
        waveMaterial.SetFloat("_Strength",     targetState.Strength);
    }
}
