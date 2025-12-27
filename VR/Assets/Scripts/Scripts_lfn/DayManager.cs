using System.Collections.Generic;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Day Roots (Day_1, Day_2, Day_3...)")]
    public List<GameObject> dayRoots = new List<GameObject>();

    [Header("Config")]
    public int npcsPerDay = 3;
    public bool autoAdvanceDay = true;

    [Header("Runtime (Read Only)")]
    [SerializeField] private int currentDayIndex = 0;
    [SerializeField] private int finishedCount = 0;

    private PathSession currentSession;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartDay(0);
    }

    public void StartDay(int dayIndex)
    {
        if (dayRoots == null || dayRoots.Count == 0)
        {
            Debug.LogError("[DayManager] dayRoots is empty.");
            return;
        }

        currentDayIndex = Mathf.Clamp(dayIndex, 0, dayRoots.Count - 1);
        finishedCount = 0;
        currentSession = null;

        // Only enable current day root
        for (int i = 0; i < dayRoots.Count; i++)
        {
            if (dayRoots[i] != null)
                dayRoots[i].SetActive(i == currentDayIndex);
        }

        // Reset PathSessions in current day
        var sessions = dayRoots[currentDayIndex].GetComponentsInChildren<PathSession>(true);
        foreach (var s in sessions)
            s.ResetSession();

        Debug.Log($"[DayManager] StartDay: {currentDayIndex + 1} (sessions={sessions.Length})");
    }

    public void SetCurrentSession(PathSession session)
    {
        if (session == null) return;

        // 防止已完成的又抢 current
        if (session.IsFinished)
            return;

        currentSession = session;
        Debug.Log($"[DayManager] CurrentSession = {session.name}");
    }

    /// <summary>
    /// UI按钮绑定这个：放行/强制离开
    /// </summary>
    public void ForceLeaveCurrent()
    {
        if (currentSession == null)
        {
            Debug.LogWarning("[DayManager] ForceLeaveCurrent: currentSession is NULL (no NPC arrived yet).");
            return;
        }

        Debug.Log($"[DayManager] ForceLeaveCurrent -> {currentSession.name}");
        currentSession.ForceLeave();
    }

    public void ReportFinished(PathSession session)
    {
        if (session == null) return;

        finishedCount++;
        Debug.Log($"[DayManager] FinishedCount = {finishedCount}/{npcsPerDay}");

        // 清空 current（避免重复点按钮还打到旧session）
        if (currentSession == session)
            currentSession = null;

        if (finishedCount >= npcsPerDay)
        {
            Debug.Log($"[DayManager] Day {currentDayIndex + 1} Completed.");

            if (autoAdvanceDay)
            {
                int next = currentDayIndex + 1;
                if (next < dayRoots.Count)
                    StartDay(next);
                else
                    Debug.Log("[DayManager] All days completed (Demo End).");
            }
        }
    }
}
