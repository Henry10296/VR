using UnityEngine;

public class StopByDistance : MonoBehaviour
{
    [Header("Stop Point")]
    [Tooltip("If null, use this object's transform as stop point.")]
    public Transform stopPoint;

    [Header("NPC Targeting")]
    [Tooltip("Optional. If not assigned, script will auto-find the closest NPC with MovePath.")]
    public GameObject npc;

    [Tooltip("Auto-find NPC each frame until found (useful when NPC is spawned after Play starts).")]
    public bool autoFindNpc = true;

    [Header("Distance")]
    public float stopDistance = 0.9f;

    [Header("Animations (state names in Animator)")]
    public string walkState = "walk";
    public string idleState = "idle1";
    public string talkTrigger = "DoTalk"; // Trigger parameter name in Animator
    public bool playTalkOnStop = true;

    [Header("Session Hook (NEW)")]
    public PathSession session;   // ✅新增：告诉谁“到站了”

    [Header("Debug")]
    public bool logDebug = true;

    private MovePath _movePath;
    private Animator _anim;
    private bool _stopped;

    void Awake()
    {
        if (stopPoint == null) stopPoint = transform;
    }

    void Update()
    {
        // 1) Acquire NPC if needed
        if ((npc == null || _movePath == null) && autoFindNpc)
        {
            TryAutoFindClosestNpc();
        }

        if (npc == null || _movePath == null) return;

        // 2) While moving, ensure walk animation (avoid "滑行/idle平移")
        if (!_stopped)
        {
            EnsureAnimator();
            if (_anim != null && !string.IsNullOrWhiteSpace(walkState))
            {
                _anim.CrossFade(walkState, 0.08f);
            }
        }

        // 3) Distance check (XZ only, ignore height)
        Vector3 a = npc.transform.position; a.y = 0f;
        Vector3 b = stopPoint.position;     b.y = 0f;
        float d = Vector3.Distance(a, b);

        if (logDebug && Time.frameCount % 30 == 0)
        {
            Debug.Log($"[StopByDistance] NPC={npc.name}, dist={d:F2}, stopped={_stopped}");
        }

        // 4) Stop when within distance
        if (!_stopped && d <= stopDistance)
        {
            DoStop();
        }
    }

    private void DoStop()
    {
        _stopped = true;

        // 真正停住：禁用 MovePath（不是靠撞墙）
        _movePath.enabled = false;

        EnsureAnimator();

        if (_anim != null && !string.IsNullOrWhiteSpace(idleState))
        {
            _anim.Play(idleState, 0, 0f);
        }

        if (_anim != null && playTalkOnStop && !string.IsNullOrWhiteSpace(talkTrigger))
        {
            _anim.ResetTrigger(talkTrigger);
            _anim.SetTrigger(talkTrigger);
        }

        // ✅新增：到站通知（设置 currentSession）
        if (session != null)
            session.NotifyArrived();

        if (logDebug) Debug.Log($"[StopByDistance] STOPPED: {npc.name}");
    }

    private void EnsureAnimator()
    {
        if (_anim == null && npc != null)
        {
            _anim = npc.GetComponentInChildren<Animator>();
            if (_anim != null)
            {
                _anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
        }
    }

    private void TryAutoFindClosestNpc()
    {
        var all = FindObjectsOfType<MovePath>();
        if (all == null || all.Length == 0) return;

        float best = float.MaxValue;
        MovePath bestMp = null;

        Vector3 sp = stopPoint.position; sp.y = 0f;

        foreach (var mp in all)
        {
            if (!mp.enabled) continue;
            var go = mp.gameObject;
            Vector3 p = go.transform.position; p.y = 0f;

            float d = Vector3.Distance(p, sp);
            if (d < best)
            {
                best = d;
                bestMp = mp;
            }
        }

        if (bestMp != null)
        {
            _movePath = bestMp;
            npc = bestMp.gameObject;
            _anim = null;
            if (logDebug) Debug.Log($"[StopByDistance] Found NPC: {npc.name} (dist {best:F2})");
        }
    }

    // 你之后放行可以调用这个（你原本就有）
    public void ResumeNpc()
    {
        if (npc == null) return;
        if (_movePath == null) _movePath = npc.GetComponent<MovePath>();
        EnsureAnimator();

        _stopped = false;
        if (_movePath != null) _movePath.enabled = true;

        if (_anim != null && !string.IsNullOrWhiteSpace(walkState))
            _anim.CrossFade(walkState, 0.08f);

        if (logDebug) Debug.Log($"[StopByDistance] RESUME: {npc.name}");
    }

    /// <summary>
    /// ✅新增：UI点“放行/强制离开”就调用这个
    /// 关键点：恢复MovePath + 立刻禁用本StopByDistance，避免下一帧又把NPC停住
    /// </summary>
    public void ForceLeaveNow()
    {
        ResumeNpc();
        enabled = false;

        if (logDebug) Debug.Log("[StopByDistance] ForceLeaveNow -> resumed NPC and disabled StopByDistance.");
    }
}
