using System;
using UnityEngine;

/// <summary>
/// NPCDialogueController (Textless / Step-based)
/// - No dialogue content stored.
/// - Only controls "how many lines" and "current step".
/// - Each step triggers NPC talk animation once.
/// - UI can subscribe to events (step changed / finished).
/// </summary>
public class NPCDialogueController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private NPCAnimationDriver animationDriver;

    [Header("Dialogue Steps (Demo)")]
    [Tooltip("How many 'sentences' in this dialogue. Each click advances 1 step.")]
    [Min(1)]
    [SerializeField] private int totalLines = 3;

    [Header("Behavior")]
    [Tooltip("If true, first click will auto-start and play the first talk.")]
    [SerializeField] private bool autoStartOnNext = true;

    [Tooltip("If true, every step triggers a talk animation.")]
    [SerializeField] private bool playTalkEveryLine = true;

    [Tooltip("If true, calling StartDialogue() resets to line 0 even if already running.")]
    [SerializeField] private bool restartIfAlreadyRunning = true;

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    public bool IsRunning { get; private set; }
    public bool IsFinished { get; private set; }

    /// <summary>0-based line index. -1 means not started.</summary>
    public int CurrentIndex { get; private set; } = -1;

    public int TotalLines => totalLines;

    // UI can subscribe (optional)
    public event Action<int, int> OnLineAdvanced; // (currentIndex, totalLines)
    public event Action OnDialogueStarted;
    public event Action OnDialogueFinished;

    private void Awake()
    {
        if (animationDriver == null)
            animationDriver = GetComponentInChildren<NPCAnimationDriver>();
    }

    public void StartDialogue()
    {
        if (totalLines < 1) totalLines = 1;

        if (IsRunning && !restartIfAlreadyRunning) return;

        IsRunning = true;
        IsFinished = false;
        CurrentIndex = -1;

        OnDialogueStarted?.Invoke();
        if (logDebug) Debug.Log($"[NPCDialogueController] START on {name}");
    }

    /// <summary>
    /// Advance one line (one "sentence").
    /// Each call can trigger talk animation once.
    /// </summary>
    public void NextLine()
    {
        if (!IsRunning)
        {
            if (!autoStartOnNext) return;
            StartDialogue();
        }

        int next = CurrentIndex + 1;

        if (next >= totalLines)
        {
            FinishDialogue();
            return;
        }

        CurrentIndex = next;

        if (playTalkEveryLine)
            animationDriver?.PlayTalkOnce();

        OnLineAdvanced?.Invoke(CurrentIndex, totalLines);

        if (logDebug) Debug.Log($"[NPCDialogueController] Line {CurrentIndex + 1}/{totalLines} on {name}");
    }

    public void EndDialogue()
    {
        if (!IsRunning && IsFinished) return;
        FinishDialogue();
    }

    public void ResetDialogue()
    {
        IsRunning = false;
        IsFinished = false;
        CurrentIndex = -1;
    }

    public void SetTotalLines(int count, bool reset = true)
    {
        totalLines = Mathf.Max(1, count);
        if (reset) ResetDialogue();
    }

    private void FinishDialogue()
    {
        IsRunning = false;
        IsFinished = true;

        if (logDebug) Debug.Log($"[NPCDialogueController] FINISH on {name}");

        OnDialogueFinished?.Invoke();
    }
}
