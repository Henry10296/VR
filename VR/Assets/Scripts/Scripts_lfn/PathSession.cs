using UnityEngine;

public class PathSession : MonoBehaviour
{
    [Header("References (drag in Inspector)")]
    public StopByDistance stopByDistance;

    [Header("Optional")]
    public bool logDebug = true;

    private bool arrived = false;
    private bool finished = false;

    public bool IsFinished => finished;
    public bool IsArrived => arrived;

    private void Awake()
    {
        // 让 StopByDistance 知道“到站要通知谁”
        if (stopByDistance != null)
            stopByDistance.session = this;
    }

    /// <summary>
    /// StopByDistance 停住NPC的那一刻会调用
    /// </summary>
    public void NotifyArrived()
    {
        if (finished) return;
        if (arrived) return;

        arrived = true;

        if (logDebug) Debug.Log($"[PathSession] Arrived: {name}");

        // 把自己注册为全局“当前NPC会话”
        if (DayManager.Instance != null)
            DayManager.Instance.SetCurrentSession(this);
        else
            Debug.LogError("[PathSession] DayManager.Instance is NULL.");
    }

    /// <summary>
    /// UI点击放行/强制离开后，DayManager会调用这个
    /// </summary>
    public void ForceLeave()
    {
        if (finished) return;

        if (stopByDistance == null)
        {
            Debug.LogError($"[PathSession] stopByDistance is NULL on {name}");
            return;
        }

        // 关键：强制放行（恢复MovePath + 禁用StopByDistance避免下一帧又停住）
        stopByDistance.ForceLeaveNow();

        finished = true;

        if (logDebug) Debug.Log($"[PathSession] Finished: {name}");

        if (DayManager.Instance != null)
            DayManager.Instance.ReportFinished(this);
    }

    public void ResetSession()
    {
        arrived = false;
        finished = false;

        // StopByDistance 默认应启用（当天开始可以重新拦停）
        if (stopByDistance != null)
        {
            stopByDistance.enabled = true;
            stopByDistance.session = this;
        }
    }
}