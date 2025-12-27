using System;
using UnityEngine;

/// <summary>
/// NPCDecisionController
/// - Receives player's decision (1/2/3)
/// - Plays a reaction animation via NPCAnimationDriver
/// - Then triggers fixed ending: NPC leaves along its existing path (speed adjustable)
/// - No UI code here.
/// </summary>
public class NPCDecisionController : MonoBehaviour
{
    public enum Decision
    {
        None = 0,
        Decision1 = 1,
        Decision2 = 2,
        Decision3 = 3
    }

    [Header("Dependencies")]
    [Tooltip("Optional. If null, will auto-find on same NPC root/children.")]
    [SerializeField] private NPCAnimationDriver animationDriver;

    [Header("Leaving")]
    [Tooltip("Leaving speed for MovePath (or your movement script). Adjustable for demo.")]
    [SerializeField] private float leaveSpeed = 2.0f;

    [Tooltip("If true, decision immediately triggers leaving after reaction is requested.")]
    [SerializeField] private bool leaveImmediately = true;

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    public Decision CurrentDecision { get; private set; } = Decision.None;
    public bool HasDecided => CurrentDecision != Decision.None;

    /// <summary>Fires when a decision is applied.</summary>
    public event Action<Decision> OnDecisionApplied;

    /// <summary>Fires when leaving is triggered.</summary>
    public event Action<Decision, float> OnLeavingTriggered;

    private void Awake()
    {
        if (animationDriver == null)
            animationDriver = GetComponentInChildren<NPCAnimationDriver>();
    }

    // -------------------------
    // Public API (UI calls these)
    // -------------------------

    public void Decision1() => ApplyDecision(1);
    public void Decision2() => ApplyDecision(2);
    public void Decision3() => ApplyDecision(3);

    /// <summary>
    /// Apply decision by id (1/2/3). Ignores if already decided (safe).
    /// </summary>
    public void ApplyDecision(int decisionId)
    {
        if (HasDecided)
        {
            if (logDebug) Debug.Log($"[NPCDecisionController] Already decided: {CurrentDecision} on {name}");
            return;
        }

        Decision d = decisionId switch
        {
            1 => Decision.Decision1,
            2 => Decision.Decision2,
            3 => Decision.Decision3,
            _ => Decision.None
        };

        if (d == Decision.None)
        {
            if (logDebug) Debug.LogWarning($"[NPCDecisionController] Invalid decisionId={decisionId} on {name}");
            return;
        }

        CurrentDecision = d;

        // 1) Reaction animation (can all be talk for demo)
        PlayReaction(d);

        // 2) Notify observers
        OnDecisionApplied?.Invoke(d);

        if (logDebug) Debug.Log($"[NPCDecisionController] Decision applied: {d} on {name}");

        // 3) Fixed ending: leave along current path
        if (leaveImmediately)
        {
            TriggerLeaving();
        }
    }

    /// <summary>
    /// If you want: reaction first, then leave later (e.g., after UI closes).
    /// </summary>
    public void TriggerLeaving()
    {
        if (!HasDecided)
        {
            if (logDebug) Debug.LogWarning($"[NPCDecisionController] TriggerLeaving called but no decision yet on {name}");
            return;
        }

        if (animationDriver != null)
        {
            animationDriver.SetLeaveSpeed(leaveSpeed);
            animationDriver.LeaveNow(); // walking + apply speed to MovePath if hook exists
        }

        OnLeavingTriggered?.Invoke(CurrentDecision, leaveSpeed);

        if (logDebug) Debug.Log($"[NPCDecisionController] Leaving triggered: {CurrentDecision}, speed={leaveSpeed} on {name}");
    }

    public void SetLeaveSpeed(float speed)
    {
        leaveSpeed = Mathf.Max(0f, speed);
        if (logDebug) Debug.Log($"[NPCDecisionController] SetLeaveSpeed({leaveSpeed}) on {name}");
    }

    public void ResetDecision()
    {
        CurrentDecision = Decision.None;
        if (logDebug) Debug.Log($"[NPCDecisionController] ResetDecision on {name}");
    }

    // -------------------------
    // Internal
    // -------------------------

    private void PlayReaction(Decision d)
    {
        if (animationDriver == null) return;

        switch (d)
        {
            case Decision.Decision1:
                animationDriver.PlayDecision1Reaction();
                break;
            case Decision.Decision2:
                animationDriver.PlayDecision2Reaction();
                break;
            case Decision.Decision3:
                animationDriver.PlayDecision3Reaction();
                break;
        }
    }
}
