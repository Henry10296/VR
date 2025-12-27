using UnityEngine;

public class WindowResumeLeave : MonoBehaviour
{
    [Header("Drag the StopByDistance on WindowStopZone here")]
    public StopByDistance stopByDistance;

    public void OnClickResumeForceLeave()
    {
        if (stopByDistance == null)
        {
            Debug.LogWarning("[WindowResumeLeave] stopByDistance is NULL.");
            return;
        }

        stopByDistance.ForceLeaveNow();
        Debug.Log("[WindowResumeLeave] Resume/ForceLeave clicked -> ForceLeaveNow called.");
    }
}