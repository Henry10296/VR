/*
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WindowStopZone : MonoBehaviour
{
    [Header("Optional: Animation")]
    [Tooltip("NPC到窗口后要切换的idle动画state名（不填则不切动画）")]
    public string idleStateName = "idle1";

    [Tooltip("NPC到窗口后要播放的talk动画state名（不填则不播）")]
    public string talkStateName = ""; // 例如 "talk1"

    [Tooltip("播完talk后回到idle（仅当填写talkStateName时有效）")]
    public bool returnToIdleAfterTalk = true;

    [Tooltip("talk播放前的延迟（秒）")]
    public float talkDelay = 0.15f;

    [Header("Events")]
    public bool triggerOnlyOncePerNpc = true;

    /// <summary>
    /// NPC到达窗口时触发：你可以在别的脚本里订阅它，用来Spawn证件等
    /// </summary>
    public event Action<GameObject> OnNpcStopped;

    private void Reset()
    {
        // 确保是Trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 只处理进入StopZone的NPC（必须带MovePath）
        var movePath = other.GetComponentInParent<MovePath>();
        if (movePath == null) return;

        // 防止同一个NPC反复触发
        if (triggerOnlyOncePerNpc)
        {
            var flag = movePath.GetComponent<NpcStopFlag>();
            if (flag != null && flag.hasStopped) return;

            if (flag == null) flag = movePath.gameObject.AddComponent<NpcStopFlag>();
            flag.hasStopped = true;
        }

        // 1) 停住：禁用MovePath（Update不再跑 -> 人就停了）
        movePath.enabled = false;

        // 2) 动画：切到idle（可选）
        var animator = movePath.GetComponentInChildren<Animator>();
        if (animator != null && !string.IsNullOrWhiteSpace(idleStateName))
        {
            animator.CrossFade(idleStateName, 0.1f);
        }

        // 3) 事件回调：让你的流程脚本去spawn证件
        OnNpcStopped?.Invoke(movePath.gameObject);

        // 4) 可选：播一句talk
        if (animator != null && !string.IsNullOrWhiteSpace(talkStateName))
        {
            StartCoroutine(PlayTalk(animator, talkStateName, idleStateName));
        }
    }

    private System.Collections.IEnumerator PlayTalk(Animator animator, string talk, string idle)
    {
        if (talkDelay > 0f) yield return new WaitForSeconds(talkDelay);

        animator.CrossFade(talk, 0.08f);

        if (returnToIdleAfterTalk && !string.IsNullOrWhiteSpace(idle))
        {
            // 等当前talk播完再回idle（用当前state时长粗略估计）
            // 注意：如果你的talk是循环的，请关掉 returnToIdleAfterTalk
            yield return null; // 等一帧让state切换生效
            var info = animator.GetCurrentAnimatorStateInfo(0);
            float wait = Mathf.Max(0.2f, info.length * 0.9f);
            yield return new WaitForSeconds(wait);

            animator.CrossFade(idle, 0.1f);
        }
    }

    /// <summary>
    /// 防止重复触发的小标记组件
    /// </summary>
    private class NpcStopFlag : MonoBehaviour
    {
        public bool hasStopped;
    }
}
*/
