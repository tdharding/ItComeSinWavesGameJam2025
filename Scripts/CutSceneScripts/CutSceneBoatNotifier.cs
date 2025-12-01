using UnityEngine;

public class CutSceneBoatNotifier : MonoBehaviour
{
    public IntroSequenceController intro;

    // Called by animation event at the final frame of "CutSceneBoat1"
    public void OnCutsceneBoatFinished()
    {
        intro.OnBoatIntroComplete();
    }

    // Called by animation event when reaching Intro Waypoint B
    public void ReachedIntroWayPointB()
    {
        intro.OnReachedIntroWayPointB();
    }
}
