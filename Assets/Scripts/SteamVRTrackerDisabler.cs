using UnityEngine;
using Valve.VR;

public class SteamVRTrackerDisabler : MonoBehaviour
{
    [SerializeField] private SteamVR_TrackedObject[] steamVRTrackedObjects;

    private void Start()
    {
        DisableTrackers();
    }

    void DisableTrackers()
    {
        foreach (var tracker in steamVRTrackedObjects)
        {
            tracker.enabled = false;
            var confLoader = tracker.GetComponent<TrackerConfigurationLoader>();
            if (confLoader != null)
            {
                confLoader.enabled = false;
            }
        }
    }
}
