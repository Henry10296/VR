using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckpointInteractionUI : MonoBehaviour
{
    [Header("Target NPC (optional)")]
    [SerializeField] private GameObject targetNpcRoot;

    [Header("UI - Dialogue")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button btnNextLine;
    [SerializeField] private Button btnEndDialogue;

    [Header("UI - Decisions")]
    [SerializeField] private Button btnDecision1;
    [SerializeField] private Button btnDecision2;
    [SerializeField] private Button btnDecision3;

    [Header("UI - Animation Debug (optional)")]
    [SerializeField] private Button btnStartWalk;
    [SerializeField] private Button btnStopWalk;
    [SerializeField] private Button btnLeaveNow;

    [Header("UI - Reset (optional)")]
    [SerializeField] private Button btnResetAll;

    [Header("Behavior")]
    [SerializeField] private bool lockDecisionsUntilDialogueFinished = true;
    [SerializeField] private bool nextLineAutoStartsDialogue = true;
    [SerializeField] private bool disableDecisionsAfterPick = true;

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    private NPCAnimationDriver _anim;
    private NPCDialogueController _dialogue;
    private NPCDecisionController _decision;

    private bool _isBound;

    private void OnEnable()
    {
        BindButtons();
        TryBindToNpcIfNeeded();
        RefreshUI();
    }

    private void OnDisable()
    {
        UnsubscribeDialogueEvents();
        UnsubscribeDecisionEvents();
    }

    private void Update()
    {
        if (!_isBound)
        {
            TryBindToNpcIfNeeded();
            if (_isBound) RefreshUI();
        }
    }

    private void BindButtons()
    {
        if (btnNextLine) { btnNextLine.onClick.RemoveListener(OnClickNextLine); btnNextLine.onClick.AddListener(OnClickNextLine); }
        if (btnEndDialogue) { btnEndDialogue.onClick.RemoveListener(OnClickEndDialogue); btnEndDialogue.onClick.AddListener(OnClickEndDialogue); }

        if (btnDecision1) { btnDecision1.onClick.RemoveListener(OnClickDecision1); btnDecision1.onClick.AddListener(OnClickDecision1); }
        if (btnDecision2) { btnDecision2.onClick.RemoveListener(OnClickDecision2); btnDecision2.onClick.AddListener(OnClickDecision2); }
        if (btnDecision3) { btnDecision3.onClick.RemoveListener(OnClickDecision3); btnDecision3.onClick.AddListener(OnClickDecision3); }

        if (btnStartWalk) { btnStartWalk.onClick.RemoveListener(OnClickStartWalk); btnStartWalk.onClick.AddListener(OnClickStartWalk); }
        if (btnStopWalk) { btnStopWalk.onClick.RemoveListener(OnClickStopWalk); btnStopWalk.onClick.AddListener(OnClickStopWalk); }
        if (btnLeaveNow) { btnLeaveNow.onClick.RemoveListener(OnClickLeaveNow); btnLeaveNow.onClick.AddListener(OnClickLeaveNow); }

        if (btnResetAll) { btnResetAll.onClick.RemoveListener(OnClickResetAll); btnResetAll.onClick.AddListener(OnClickResetAll); }
    }

    private void TryBindToNpcIfNeeded()
    {
        if (_isBound) return;

        GameObject root = targetNpcRoot;

        // Auto-find if not assigned
        if (root == null)
        {
            var foundDialogue = FindObjectOfType<NPCDialogueController>(true);
            if (foundDialogue != null) root = foundDialogue.gameObject;
        }

        if (root == null) return;

        _anim = root.GetComponentInChildren<NPCAnimationDriver>(true);
        _dialogue = root.GetComponentInChildren<NPCDialogueController>(true);
        _decision = root.GetComponentInChildren<NPCDecisionController>(true);

        if (_anim == null && _dialogue == null && _decision == null)
        {
            if (logDebug) Debug.LogWarning("[CheckpointInteractionUI] Found targetNpcRoot but no controllers on it.");
            return;
        }

        SubscribeDialogueEvents();
        SubscribeDecisionEvents();
        _isBound = true;

        if (logDebug)
            Debug.Log($"[CheckpointInteractionUI] Bound to NPC root: {root.name} | Anim={(_anim ? "Y" : "N")} Dialogue={(_dialogue ? "Y" : "N")} Decision={(_decision ? "Y" : "N")}");
    }

    private void SubscribeDialogueEvents()
    {
        if (_dialogue == null) return;
        UnsubscribeDialogueEvents();

        _dialogue.OnDialogueStarted += HandleDialogueStarted;
        _dialogue.OnLineAdvanced += HandleLineAdvanced;     // ✅ 对齐你现在的事件名
        _dialogue.OnDialogueFinished += HandleDialogueFinished;
    }

    private void UnsubscribeDialogueEvents()
    {
        if (_dialogue == null) return;
        _dialogue.OnDialogueStarted -= HandleDialogueStarted;
        _dialogue.OnLineAdvanced -= HandleLineAdvanced;
        _dialogue.OnDialogueFinished -= HandleDialogueFinished;
    }

    private void SubscribeDecisionEvents()
    {
        if (_decision == null) return;
        UnsubscribeDecisionEvents();

        _decision.OnDecisionApplied += HandleDecisionApplied;
        _decision.OnLeavingTriggered += HandleLeavingTriggered;
    }

    private void UnsubscribeDecisionEvents()
    {
        if (_decision == null) return;
        _decision.OnDecisionApplied -= HandleDecisionApplied;
        _decision.OnLeavingTriggered -= HandleLeavingTriggered;
    }

    private void RefreshUI()
    {
        // Dialogue text display (textless => show progress)
        if (dialogueText)
        {
            if (_dialogue == null) dialogueText.text = "(No NPCDialogueController bound)";
            else
            {
                if (_dialogue.IsRunning)
                {
                    // CurrentIndex is 0-based, display as 1-based
                    int shown = Mathf.Max(0, _dialogue.CurrentIndex + 1);
                    dialogueText.text = $"Dialogue: Line {shown}/{_dialogue.TotalLines}";
                }
                else if (_dialogue.IsFinished) dialogueText.text = "(Dialogue finished)";
                else dialogueText.text = "(Press Talk / Next Line)";
            }
        }

        if (btnNextLine) btnNextLine.interactable = (_dialogue != null);
        if (btnEndDialogue) btnEndDialogue.interactable = (_dialogue != null && _dialogue.IsRunning);

        // Decision lock rules
        bool decisionsAllowed = (_decision != null);

        if (lockDecisionsUntilDialogueFinished && _dialogue != null)
            decisionsAllowed &= _dialogue.IsFinished;

        if (_decision != null && disableDecisionsAfterPick && _decision.HasDecided)
            decisionsAllowed = false;

        SetDecisionButtonsInteractable(decisionsAllowed);

        if (btnStartWalk) btnStartWalk.interactable = (_anim != null);
        if (btnStopWalk) btnStopWalk.interactable = (_anim != null);
        if (btnLeaveNow) btnLeaveNow.interactable = (_anim != null || _decision != null);

        if (btnResetAll) btnResetAll.interactable = true;
    }

    private void SetDecisionButtonsInteractable(bool enabled)
    {
        if (btnDecision1) btnDecision1.interactable = enabled;
        if (btnDecision2) btnDecision2.interactable = enabled;
        if (btnDecision3) btnDecision3.interactable = enabled;
    }

    // ---------------- Button callbacks ----------------

    private void OnClickNextLine()
    {
        Debug.Log("UI Button Clicked: NextLine");

        if (_dialogue == null) return;

        if (nextLineAutoStartsDialogue && !_dialogue.IsRunning && !_dialogue.IsFinished)
            _dialogue.StartDialogue();

        _dialogue.NextLine();
        RefreshUI();
    }

    private void OnClickEndDialogue()
    {
        _dialogue?.EndDialogue();
        RefreshUI();
    }

    private void OnClickDecision1() { _decision?.Decision1(); RefreshUI(); }
    private void OnClickDecision2() { _decision?.Decision2(); RefreshUI(); }
    private void OnClickDecision3() { _decision?.Decision3(); RefreshUI(); }

    private void OnClickStartWalk() { _anim?.StartWalking(); }
    private void OnClickStopWalk() { _anim?.StopWalking(); }

    private void OnClickLeaveNow()
    {
        if (_decision != null && _decision.HasDecided) _decision.TriggerLeaving();
        else _anim?.LeaveNow();
    }

    private void OnClickResetAll()
    {
        _dialogue?.ResetDialogue();
        _decision?.ResetDecision();
        RefreshUI();
    }

    // ---------------- Event handlers ----------------

    private void HandleDialogueStarted() => RefreshUI();

    private void HandleLineAdvanced(int currentIndex, int totalLines)
    {
        // update progress display
        if (dialogueText)
            dialogueText.text = $"Dialogue: Line {currentIndex + 1}/{totalLines}";
        RefreshUI();
    }

    private void HandleDialogueFinished() => RefreshUI();

    private void HandleDecisionApplied(NPCDecisionController.Decision d)
    {
        if (disableDecisionsAfterPick) SetDecisionButtonsInteractable(false);
        RefreshUI();
    }

    private void HandleLeavingTriggered(NPCDecisionController.Decision d, float speed) => RefreshUI();
}
