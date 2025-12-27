using UnityEngine;
using System.Collections;

public class StopByDistance : MonoBehaviour
{
    [Header("Stop Point")]
    public Transform stopPoint;                 // 不填就用本物体 Transform

    [Header("NPC Targeting")]
    public GameObject npc;
    public bool autoFindNpc = true;

    [Header("Distance")]
    public float stopDistance = 0.8f;

    [Header("Animator Params (use YOUR variables)")]
    public string isWalkingBool = "IsWalking";
    public string doTalkTrigger = "DoTalk";

    [Header("On Stop")]
    public bool playTalkOnStop = true;
    public float talkDelay = 0.15f;
    public bool returnToIdleAfterTalk = false;  // 先不强行回 idle，交给状态机/你的 transition

    [Header("Debug")]
    public bool logDebug = false;

    private MovePath _movePath;
    private Animator _anim;

    private bool _walkSet;      // ✅ 防止重复 set walking
    private bool _stopped;      // ✅ 防止重复 stop
    private bool _talkTriggered;

    void Awake()
    {
        if (stopPoint == null) stopPoint = transform;
    }

    void Update()
    {
        // 1) 找 NPC（适配 Populate 动态生成）
        if ((npc == null || _movePath == null) && autoFindNpc)
            TryFindClosestNpc();

        if (npc == null || _movePath == null) return;

        // 2) Grab Animator once
        if (_anim == null)
        {
            _anim = npc.GetComponentInChildren<Animator>();
            if (_anim != null)
                _anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        // 3) ✅ 开始走：只触发一次（用你给的变量）
        if (!_stopped && !_walkSet && _anim != null)
        {
            _anim.SetBool(isWalkingBool, true);
            _walkSet = true;

            if (logDebug) Debug.Log($"[StopByDistance] Set {isWalkingBool}=true once for {npc.name}");
        }

        // 4) 到点判断（XZ 平面距离）
        Vector3 a = npc.transform.position; a.y = 0f;
        Vector3 b = stopPoint.position;     b.y = 0f;
        float d = Vector3.Distance(a, b);

        if (!_stopped && d <= stopDistance)
        {
            StopNpc();
        }
    }

    private void StopNpc()
    {
        _stopped = true;

        // 真停：关 MovePath
        if (_movePath != null) _movePath.enabled = false;

        // ✅ 停下：用变量切回 idle（你的状态机应当：IsWalking=false -> idle1）
        if (_anim != null)
        {
            _anim.SetBool(isWalkingBool, false);
        }

        if (playTalkOnStop && !_talkTriggered)
        {
            _talkTriggered = true;

            if (talkDelay <= 0f) TriggerTalk();
            else StartCoroutine(TalkDelayed(talkDelay));
        }

        if (logDebug) Debug.Log($"[StopByDistance] STOP: {npc.name}");
    }

    private IEnumerator TalkDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerTalk();
    }

    private void TriggerTalk()
    {
        if (_anim == null) return;
        _anim.ResetTrigger(doTalkTrigger);
        _anim.SetTrigger(doTalkTrigger);

        if (logDebug) Debug.Log($"[StopByDistance] Trigger {doTalkTrigger} for {npc.name}");
    }

    // ✅ 放行继续走时调用
    public void ResumeNpc()
    {
        if (npc == null) return;

        if (_movePath == null) _movePath = npc.GetComponent<MovePath>();
        if (_anim == null) _anim = npc.GetComponentInChildren<Animator>();

        _stopped = false;
        _talkTriggered = false;

        if (_movePath != null) _movePath.enabled = true;

        // ✅ 继续走：再设 walking=true（仍然变量驱动）
        if (_anim != null) _anim.SetBool(isWalkingBool, true);

        // ✅ 允许下一次重新“只触发一次”
        _walkSet = true; // 已经在走了，不需要 Update 再 set

        if (logDebug) Debug.Log($"[StopByDistance] RESUME: {npc.name}");
    }

    private void TryFindClosestNpc()
    {
        var all = FindObjectsOfType<MovePath>();
        if (all == null || all.Length == 0) return;

        float best = float.MaxValue;
        MovePath bestMp = null;

        Vector3 sp = stopPoint.position; sp.y = 0f;

        foreach (var mp in all)
        {
            if (!mp.enabled) continue;
            Vector3 p = mp.transform.position; p.y = 0f;
            float d = Vector3.Distance(p, sp);
            if (d < best) { best = d; bestMp = mp; }
        }

        if (bestMp != null)
        {
            npc = bestMp.gameObject;
            _movePath = bestMp;

            _anim = null;
            _stopped = false;
            _talkTriggered = false;
            _walkSet = false; // ✅ 新 NPC 要重新 set walking=true 一次

            if (logDebug) Debug.Log($"[StopByDistance] Found NPC: {npc.name}");
        }
    }
    
    // 加在 StopByDistance 类里面（任意位置都行）
    public void ForceLeaveNow()
    {
        // 1) 恢复移动
        ResumeNpc();

        // 2) 关键：禁用自己，防止下一帧又把 NPC 停住
        enabled = false;

        if (logDebug) Debug.Log("[StopByDistance] ForceLeaveNow -> resumed MovePath and disabled StopByDistance.");
    }

}
