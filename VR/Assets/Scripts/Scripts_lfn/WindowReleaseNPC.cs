using UnityEngine;

public class WindowReleaseNPC : MonoBehaviour
{
    [Header("Target NPC")]
    public Animator npcAnimator;

    [Tooltip("IsWalking 参数名，必须和 Animator 一致")]
    public string isWalkingBool = "IsWalking";

    [Tooltip("是否在放行后禁用自身，防止重复触发")]
    public bool disableAfterRelease = true;

    /// <summary>
    /// 给 UI 按钮调用：放行 NPC
    /// </summary>
    public void ReleaseNPC()
    {
        if (npcAnimator == null)
        {
            Debug.LogWarning("[WindowReleaseNPC] NPC Animator is null.");
            return;
        }

        npcAnimator.SetBool(isWalkingBool, true);
        Debug.Log("[WindowReleaseNPC] NPC released, resume walking.");

        if (disableAfterRelease)
            gameObject.SetActive(false);
    }
}