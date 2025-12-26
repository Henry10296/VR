using System;
using UnityEngine;

/// <summary>
/// NPCDialogueController
/// - Owns dialogue progression (lines + index)
/// - Does NOT handle UI.
/// - On each "advance", it can request NPCAnimationDriver to play talk once.
/// - Provides events so UI can update text without polling.
/// </summary>
public class NPCDialogueController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Optional. If null, will auto-find on same NPC root.")]
    [SerializeField] private NPCAnimationDriver animationDriver;

    [Header("Dialogue Data (Demo)")]
    [TextArea(2, 4)]
    [SerializeField] private string[] lines;

    [Header("Behavior")]
    [Tooltip("If true, calling StartDialogue() will immediately show the first line.")]
    [SerializeField] private bool showFirstLineImmediately = true;

    [Tooltip("If true, each line advance triggers a talk animation.")]
    [SerializeField] private bool playTalkEveryLine = true;

    [Tooltip("If true, StartDialogue() restarts from line 0 even if already in progress.")]
    [SerializeField] private bool restartIfAlreadyRunning = true;

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    // --------- Public State ----------
    public bool IsRunning { get; private set; }
    public bool IsFinished { get; private set; }
    public int CurrentIndex { get; private set; } = -1;

    /// <summary>Current line text. Empty if not running or index invalid.</summary>
    public string CurrentLine => (IsRunning && CurrentIndex >= 0 && CurrentIndex < (lines?.Length ?? 0))
        ? lines[CurrentIndex]
        : string.Empty;

    /// <summary>Total lines count (0 if null).</summary>
    public int TotalLines => lines?.Length ?? 0;

    // --------- Events (UI can subscribe) ----------
    /// <summary>Fires when a new line becomes active (index + text).</summary>
    public event Action<int, string> OnLineChanged;

    /// <summary>Fires when dialogue starts.</summary>
    public event Action OnDialogueStarted;

    /// <summary>Fires when dialogue ends.</summary>
    public event Action OnDialogueFinished;

    private void Awake()
    {
        if (animationDriver == null)
            animationDriver = GetComponentInChildren<NPCAnimationDriver>();
    }

    // =========================
    // Public API (UI will call these)
    // =========================

    /// <summary>
    /// Start the dialogue. By default will show line 0 immediately.
    /// </summary>
    public void StartDialogue()
    {
        if (TotalLines <= 0)
        {
            if (logDebug) Debug.LogWarning($"[NPCDialogueController] No lines assigned on {name}");
            // still consider as finished to avoid UI hanging
            IsRunning = false;
            IsFinished = true;
            CurrentIndex = -1;
            OnDialogueFinished?.Invoke();
            return;
        }

        if (IsRunning && !restartIfAlreadyRunning)
        {
            // already running, do nothing
            if (logDebug) Debug.Log($"[NPCDialogueController] StartDialogue ignored (already running) on {name}");
            return;
        }

        IsRunning = true;
        IsFinished = false;
        CurrentIndex = -1;

        OnDialogueStarted?.Invoke();
        if (logDebug) Debug.Log($"[NPCDialogueController] Dialogue START on {name}");

        if (showFirstLineImmediately)
        {
            // Go to first line
            NextLine();
        }
    }

    /// <summary>
    /// Advance to the next line.
    /// - If not running, it will auto-start (common for "Talk" button).
    /// - Each advance triggers talk animation (optional).
    /// - When out of lines, it finishes.
    /// </summary>
    public void NextLine()
    {
        if (!IsRunning)
        {
            // auto-start behaviour: first click opens dialogue
            StartDialogue();
            return;
        }

        int next = CurrentIndex + 1;

        if (next >= TotalLines)
        {
            FinishDialogue();
            return;
        }

        CurrentIndex = next;

        if (playTalkEveryLine)
            animationDriver?.PlayTalkOnce();

        OnLineChanged?.Invoke(CurrentIndex, CurrentLine);

        if (logDebug) Debug.Log($"[NPCDialogueController] Line {CurrentIndex}/{TotalLines - 1}: {CurrentLine}");
    }

    /// <summary>
    /// Force finish the dialogue immediately (e.g., player cancels).
    /// </summary>
    public void EndDialogue()
    {
        if (!IsRunning && IsFinished) return;
        FinishDialogue();
    }

    /// <summary>
    /// Reset dialogue state without starting.
    /// </summary>
    public void ResetDialogue()
    {
        IsRunning = false;
        IsFinished = false;
        CurrentIndex = -1;
        if (logDebug) Debug.Log($"[NPCDialogueController] ResetDialogue on {name}");
    }

    /// <summary>
    /// Replace lines at runtime (optional).
    /// Useful if later you load dialogue data from file/ScriptableObject.
    /// </summary>
    public void SetLines(string[] newLines, bool resetState = true)
    {
        lines = newLines;
        if (resetState) ResetDialogue();
    }

    // =========================
    // Internal
    // =========================

    private void FinishDialogue()
    {
        IsRunning = false;
        IsFinished = true;

        if (logDebug) Debug.Log($"[NPCDialogueController] Dialogue FINISH on {name}");

        OnDialogueFinished?.Invoke();
    }
}
