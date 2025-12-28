using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors; // 引用 VR 交互库

public class WindowInteractable : MonoBehaviour
{
    public Animator windowAnimator; // 拖入窗户的 Animator
    private bool isOpen = false;    // 记录当前是开还是关

    // 这个方法会被 XR 组件调用
    public void ToggleWindow(SelectEnterEventArgs args)
    {
        // 1. 切换开关状态
        isOpen = !isOpen;

        // 2. 播放动画
        if (windowAnimator != null)
        {
            windowAnimator.SetBool("IsOpen", isOpen);
        }

        // 3. 发送震动反馈 (修复弃用警告的关键在这里)
        // 旧版写法: if (args.interactorObject is XRBaseControllerInteractor controllerInteractor)
        // 新版写法: 使用 XRBaseInputInteractor
        if (args.interactorObject is XRBaseInputInteractor inputInteractor)
        {
            // 参数：震动强度 (0-1)，持续时间 (秒)
            inputInteractor.SendHapticImpulse(0.5f, 0.2f);
        }
    }
}