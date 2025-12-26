using UnityEngine;

/// <summary>
/// NPCAnimationDriver
/// - Only controls NPC Animator (and optional movement speed hook).
/// - No UI / Dialogue / Decision logic here.
/// - Designed for demo: each "Next dialogue line" can call PlayTalkOnce().
/// </summary>
public class NPCAnimationDriver : MonoBehaviour
{
    [Header("Animator Reference")]
    [Tooltip("If null, will auto-find Animator in children.")]
    [SerializeField] private Animator animator;

    [Header("Animator Parameters (must match your controller)")]
    [SerializeField] private string isWalkingBool = "IsWalking";
    [SerializeField] private string doTalkTrigger = "DoTalk";

    [Header("Optional: Decision Reaction Triggers (can all be DoTalk for demo)")]
    [Tooltip("If empty, will fallback to doTalkTrigger.")]
    [SerializeField] private string decision1Trigger = "";
    [Tooltip("If empty, will fallback to doTalkTrigger.")]
    [SerializeField] private string decision2Trigger = "";
    [Tooltip("If empty, will fallback to doTalkTrigger.")]
    [SerializeField] private string decision3Trigger = "";

    [Header("Leaving / Movement (optional hook)")]
    [Tooltip("If you have a MovePath script on the same NPC root, assign it here for speed control.")]
    [SerializeField] private MonoBehaviour movePathBehaviour; // keep generic to avoid hard dependency
    [Tooltip("If using MovePath, this is the leaving speed you want to apply (units/second or your script's speed meaning).")]
    [SerializeField] private float leaveSpeed = 2.0f;

    [Header("Safety / Debug")]
    [SerializeField] private bool forceAlwaysAnimate = true;
    [SerializeField] private bool logDebug = false;

    private bool _isWalkingCached;

    private void Awake()
    {
        EnsureAnimator();
        CacheWalkingState();
    }

    private void EnsureAnimator()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null && forceAlwaysAnimate)
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    private void CacheWalkingState()
    {
        if (animator == null) return;

        // If parameter doesn't exist, GetBool will just return false.
        _isWalkingCached = animator.GetBool(isWalkingBool);
    }

    // -------------------------
    // Core Public API (call these from Dialogue/Decision/UI)
    // -------------------------

    /// <summary>Start/stop walk state via Animator bool.</summary>
    public void SetWalking(bool isWalking)
    {
        EnsureAnimator();
        if (animator == null) return;

        if (_isWalkingCached == isWalking) return; // avoid redundant SetBool spam

        animator.SetBool(isWalkingBool, isWalking);
        _isWalkingCached = isWalking;

        if (logDebug) Debug.Log($"[NPCAnimationDriver] SetWalking({isWalking}) on {name}");
    }

    /// <summary>Convenience: start walking.</summary>
    public void StartWalking() => SetWalking(true);

    /// <summary>Convenience: stop walking.</summary>
    public void StopWalking() => SetWalking(false);

    /// <summary>
    /// Trigger one talk animation. Good to call on every "Next dialogue line" click.
    /// </summary>
    public void PlayTalkOnce()
    {
        EnsureAnimator();
        if (animator == null) return;

        animator.ResetTrigger(doTalkTrigger);
        animator.SetTrigger(doTalkTrigger);

        if (logDebug) Debug.Log($"[NPCAnimationDriver] PlayTalkOnce() on {name}");
    }

    /// <summary>
    /// Play reaction for decision 1. If not assigned, fallback to talk.
    /// </summary>
    public void PlayDecision1Reaction()
    {
        TriggerOrFallback(decision1Trigger);
        if (logDebug) Debug.Log($"[NPCAnimationDriver] Decision1 reaction on {name}");
    }

    /// <summary>
    /// Play reaction for decision 2. If not assigned, fallback to talk.
    /// </summary>
    public void PlayDecision2Reaction()
    {
        TriggerOrFallback(decision2Trigger);
        if (logDebug) Debug.Log($"[NPCAnimationDriver] Decision2 reaction on {name}");
    }

    /// <summary>
    /// Play reaction for decision 3. If not assigned, fallback to talk.
    /// </summary>
    public void PlayDecision3Reaction()
    {
        TriggerOrFallback(decision3Trigger);
        if (logDebug) Debug.Log($"[NPCAnimationDriver] Decision3 reaction on {name}");
    }

    /// <summary>
    /// Set leaving speed variable (used by LeaveNow if you hook up a movement script).
    /// </summary>
    public void SetLeaveSpeed(float speed)
    {
        leaveSpeed = Mathf.Max(0f, speed);
        if (logDebug) Debug.Log($"[NPCAnimationDriver] SetLeaveSpeed({leaveSpeed}) on {name}");
    }

    /// <summary>
    /// Fixed ending: NPC leaves (walk + optional speed change).
    /// This function does NOT decide when to leave; other systems call it.
    /// </summary>
    public void LeaveNow()
    {
        // 1) Ensure walking animation on
        StartWalking();

        // 2) Optional: try to apply movement speed to your movement script without hard-coding its type.
        // If your MovePath has a public field/property "walkSpeed" or "WalkSpeed", you can support it here.
        TryApplySpeedToMovePath(leaveSpeed);

        if (logDebug) Debug.Log($"[NPCAnimationDriver] LeaveNow() speed={leaveSpeed} on {name}");
    }

    // -------------------------
    // Internal Helpers
    // -------------------------

    private void TriggerOrFallback(string triggerName)
    {
        EnsureAnimator();
        if (animator == null) return;

        string t = string.IsNullOrWhiteSpace(triggerName) ? doTalkTrigger : triggerName;
        animator.ResetTrigger(t);
        animator.SetTrigger(t);
    }

    /// <summary>
    /// Best-effort speed hook without a compile-time dependency.
    /// Supports:
    /// - public float walkSpeed;
    /// - public float WalkSpeed {get;set;}
    /// - public float runSpeed;
    /// You can extend this later once you confirm your movement script API.
    /// </summary>
    private void TryApplySpeedToMovePath(float speed)
    {
        if (movePathBehaviour == null) return;

        var type = movePathBehaviour.GetType();

        // Try field: walkSpeed
        var walkSpeedField = type.GetField("walkSpeed");
        if (walkSpeedField != null && walkSpeedField.FieldType == typeof(float))
        {
            walkSpeedField.SetValue(movePathBehaviour, speed);
            return;
        }

        // Try property: WalkSpeed
        var walkSpeedProp = type.GetProperty("WalkSpeed");
        if (walkSpeedProp != null && walkSpeedProp.PropertyType == typeof(float) && walkSpeedProp.CanWrite)
        {
            walkSpeedProp.SetValue(movePathBehaviour, speed);
            return;
        }

        // Try field: runSpeed (some scripts use this naming)
        var runSpeedField = type.GetField("runSpeed");
        if (runSpeedField != null && runSpeedField.FieldType == typeof(float))
        {
            runSpeedField.SetValue(movePathBehaviour, speed);
            return;
        }

        if (logDebug)
            Debug.LogWarning($"[NPCAnimationDriver] Could not apply speed. Movement script '{type.Name}' has no walkSpeed/WalkSpeed/runSpeed.");
    }
}
