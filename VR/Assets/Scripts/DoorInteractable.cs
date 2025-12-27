using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DoorInteractable : MonoBehaviour
{
    [Header("拖入包含Animator的门或者门轴")]
    public Animator doorAnimator;

    private bool isOpen = false;

    // 绑定到 Select Entered 事件
    public void ToggleDoor(SelectEnterEventArgs args)
    {
        // 1. 切换状态
        isOpen = !isOpen;

        // 2. 播放动画
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("IsOpen", isOpen);
        }

        // 3. 震动反馈 (已使用新版写法)
        if (args.interactorObject is XRBaseInputInteractor inputInteractor)
        {
            // 门的震动可以稍微重一点、长一点，模拟沉重感
            inputInteractor.SendHapticImpulse(0.7f, 0.3f);
        }
    }
}