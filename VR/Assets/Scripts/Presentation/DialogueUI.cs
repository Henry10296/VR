using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// 对话UI控制器 - 控制对话框显示
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("UI引用")]
    public GameObject dialoguePanel;
    /// <summary>
    /// 说话者
    /// </summary>
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject choicesPanel;
    public Button[] choiceButtons;
    public Button nextButton;
    public Button skipButton;
    public Button closeButton;
    
    [Header("动画")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;
    
    private CanvasGroup _canvasGroup;
    private bool _subscribed;
    private Coroutine _subscribeCoroutine;

    void EnsureRefs()
    {
        if (dialoguePanel == null)
        {
            var cg = GetComponentInChildren<CanvasGroup>(true);
            if (cg != null) dialoguePanel = cg.gameObject;
            else dialoguePanel = gameObject;
        }
        if (speakerNameText == null || dialogueText == null)
        {
            var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
            if (speakerNameText == null && tmps.Length > 0) speakerNameText = tmps[0];
            if (dialogueText == null && tmps.Length > 1) dialogueText = tmps[1];
        }
        if (choicesPanel == null)
        {
            var cp = transform.Find("ChoicesPanel");
            if (cp != null) choicesPanel = cp.gameObject;
        }
        if ((choiceButtons == null || choiceButtons.Length == 0) && choicesPanel != null)
        {
            choiceButtons = choicesPanel.GetComponentsInChildren<Button>(true);
        }
    }
    
    void Start()
    {
        EnsureRefs();
        _canvasGroup = dialoguePanel != null ? dialoguePanel.GetComponent<CanvasGroup>() : null;
        if (dialoguePanel != null && _canvasGroup == null)
        {
            _canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
        }
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (choicesPanel != null) choicesPanel.SetActive(false);
        SubscribeToManager();
    }
    
    void OnEnable()
    {
        EnsureRefs();
        SubscribeToManager();
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => DialogueManager.Instance.Next());
        }
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() => DialogueManager.Instance.SkipTyping());
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => DialogueManager.Instance.CloseDialogue());
        }
    }

    void OnDisable()
    {
        UnsubscribeFromManager();
    }

    private void SubscribeToManager()
    {
        if (_subscribed) return;
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart += OnDialogueStart;
            DialogueManager.Instance.OnDialogueTextUpdate += OnDialogueTextUpdate;
            DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
            DialogueManager.Instance.OnShowChoices += OnShowChoices;
            _subscribed = true;
            if (DialogueManager.Instance.IsDialogueActive)
            {
                DialogueManager.Instance.RestartCurrentDialogue();
            }
        }
        else
        {
            if (_subscribeCoroutine == null)
            {
                _subscribeCoroutine = StartCoroutine(WaitAndSubscribe());
            }
        }
    }

    private IEnumerator WaitAndSubscribe()
    {
        while (DialogueManager.Instance == null) { yield return null; }
        _subscribeCoroutine = null;
        SubscribeToManager();
    }

    private void UnsubscribeFromManager()
    {
        if (!_subscribed) return;
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart -= OnDialogueStart;
            DialogueManager.Instance.OnDialogueTextUpdate -= OnDialogueTextUpdate;
            DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
            DialogueManager.Instance.OnShowChoices -= OnShowChoices;
        }
        _subscribed = false;
        if (_subscribeCoroutine != null)
        {
            StopCoroutine(_subscribeCoroutine);
            _subscribeCoroutine = null;
        }
    }

    void OnDialogueStart(DialogueData dialogue)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (speakerNameText != null) speakerNameText.text = dialogue.speaker;
        if (dialogueText != null) dialogueText.text = "";
        RefreshControls();
        if (isActiveAndEnabled && _canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
    }
    
    void OnDialogueTextUpdate(string text)
    {
        if (dialogueText != null) dialogueText.text = text;
        if (dialoguePanel != null && !dialoguePanel.activeSelf) dialoguePanel.SetActive(true);
        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
    }
    
    void OnDialogueEnd()
    {
        if (isActiveAndEnabled && _canvasGroup != null)
        {
            StartCoroutine(FadeOutAndHide());
        }
        else if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        if (choicesPanel != null) choicesPanel.SetActive(false);
    }
    
    void OnShowChoices(List<DialogueChoice> choices)
    {
        choicesPanel.SetActive(true);
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < choices.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                var tmp = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = choices[i].choiceText;
                else
                {
                    var uiText = choiceButtons[i].GetComponentInChildren<UnityEngine.UI.Text>();
                    if (uiText != null) uiText.text = choices[i].choiceText;
                }
                int index = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
        RefreshControls();
    }
    
    void OnChoiceSelected(int index)
    {
        DialogueManager.Instance.SelectChoice(index);
        choicesPanel.SetActive(false);
        RefreshControls();
    }

    void RefreshControls()
    {
        bool auto = DialogueManager.Instance != null && DialogueManager.Instance.autoAdvance;
        bool hasChoices = false;
        var cd = DialogueManager.Instance != null ? DialogueManager.Instance.CurrentDialogue : null;
        if (cd != null && cd.choices != null) hasChoices = cd.choices.Count > 0;
        if (nextButton != null) nextButton.gameObject.SetActive(!auto && !hasChoices);
        if (skipButton != null) skipButton.gameObject.SetActive(true);
        if (closeButton != null) closeButton.gameObject.SetActive(true);
    }
    
    IEnumerator FadeIn()
    {
        float elapsed = 0;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeInDuration);
            yield return null;
        }
        _canvasGroup.alpha = 1;
    }
    
    IEnumerator FadeOutAndHide()
    {
        float elapsed = 0;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeOutDuration);
            yield return null;
        }
        _canvasGroup.alpha = 0;
        dialoguePanel.SetActive(false);
    }
    
    void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart -= OnDialogueStart;
            DialogueManager.Instance.OnDialogueTextUpdate -= OnDialogueTextUpdate;
            DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
            DialogueManager.Instance.OnShowChoices -= OnShowChoices;
        }
    }

}