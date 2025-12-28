using System;
using System.Collections.Generic;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [Serializable]
    public class DayGroup
    {
        public string dayName;
        public List<PathSession> paths = new List<PathSession>();
    }

    [Header("Days / Paths")]
    public List<DayGroup> days = new List<DayGroup>();

    [Header("Start State")]
    public int startDayIndex = 0;
    public int startNpcIndex = 0;

    [Header("Debug")]
    public bool logDebug = true;

    private int dayIndex;
    private int npcIndex;

    public static DayManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    public PathSession CurrentSession
    {
        get
        {
            if (days == null || days.Count == 0) return null;
            if (dayIndex < 0 || dayIndex >= days.Count) return null;
            var list = days[dayIndex].paths;
            if (list == null || list.Count == 0) return null;
            if (npcIndex < 0 || npcIndex >= list.Count) return null;
            return list[npcIndex];
        }
    }

    private void Start()
    {
        dayIndex = Mathf.Clamp(startDayIndex, 0, days.Count - 1);
        npcIndex = 0;

        // 先全关，再开目标
        DisableAllPaths();
        npcIndex = Mathf.Clamp(startNpcIndex, 0, days[dayIndex].paths.Count - 1);
        EnableOnly(dayIndex, npcIndex);

        if (logDebug) Debug.Log($"[DayManager] Start at Day={dayIndex}, NPC={npcIndex}");
    }

    // --------- 你要的三个按钮入口 ---------

    // 1) 当前 NPC 离开（不切换、不销毁）
    public void LeaveCurrentNpc()
    {
        var s = CurrentSession;
        if (s == null)
        {
            if (logDebug) Debug.LogWarning("[DayManager] No current session to leave.");
            return;
        }

        if (logDebug) Debug.Log($"[DayManager] LeaveCurrentNpc: Day={dayIndex} NPC={npcIndex} Path={s.name}");
        s.ForceLeave();
    }

    // 2) 切换下一个 NPC（关当前，开下一个）
    public void NextNpc()
    {
        var day = days[dayIndex];
        if (day.paths == null || day.paths.Count == 0) return;

        int next = npcIndex + 1;
        if (next >= day.paths.Count)
        {
            if (logDebug) Debug.Log("[DayManager] Already last NPC of today.");
            return; // 你也可以改成自动 NextDay()
        }

        EnableOnly(dayIndex, next);

        if (logDebug) Debug.Log($"[DayManager] NextNpc: Day={dayIndex} NPC={next}");
    }

    // 3) 下一天（切换天，开该天第一个 NPC）
    public void NextDay()
    {
        int nextDay = dayIndex + 1;
        if (nextDay >= days.Count)
        {
            if (logDebug) Debug.Log("[DayManager] Already last day.");
            return;
        }

        dayIndex = nextDay;
        npcIndex = 0;

        DisableAllPaths();
        EnableOnly(dayIndex, npcIndex);

        if (logDebug) Debug.Log($"[DayManager] NextDay -> Day={dayIndex} NPC={npcIndex}");
    }

    // --------- 内部工具 ---------

    private void EnableOnly(int d, int n)
    {
        // 先关当前天全部
        for (int i = 0; i < days[d].paths.Count; i++)
        {
            if (days[d].paths[i] != null) days[d].paths[i].SetActive(false);
        }

        // 再开目标
        var target = days[d].paths[n];
        if (target != null) target.SetActive(true);

        npcIndex = n;
    }

    private void DisableAllPaths()
    {
        foreach (var day in days)
        {
            if (day.paths == null) continue;
            foreach (var p in day.paths)
            {
                if (p != null) p.SetActive(false);
            }
        }
    }
}
