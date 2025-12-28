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
        if (choicesPanel == null)
        {
            var cp = transform.Find("ChoicesPanel");
            if (cp != null) choicesPanel = cp.gameObject;
        }

        if (speakerNameText == null || dialogueText == null)
        {
            var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);

            TextMeshProUGUI speakerCandidate = null;
            TextMeshProUGUI dialogueCandidate = null;

            foreach (var t in tmps)
            {
                if (t == null) continue;
                if (choicesPanel != null && t.transform.IsChildOf(choicesPanel.transform)) continue;

                var n = t.gameObject.name;
                if (!string.IsNullOrEmpty(n))
                {
                    var ln = n.ToLowerInvariant();
                    if (speakerCandidate == null && (ln.Contains("speaker") || ln.Contains("name"))) speakerCandidate = t;
                    if (dialogueCandidate == null && (ln.Contains("dialogue") || ln.Contains("content") || ln.Contains("text"))) dialogueCandidate = t;
                }
            }

            if (dialogueText == null)
            {
                if (dialogueCandidate != null) dialogueText = dialogueCandidate;
                else
                {
                    TextMeshProUGUI best = null;
                    float bestArea = -1f;
                    foreach (var t in tmps)
                    {
                        if (t == null) continue;
                        if (choicesPanel != null && t.transform.IsChildOf(choicesPanel.transform)) continue;
                        var rt = t.rectTransform;
                        float area = Mathf.Abs(rt.rect.width * rt.rect.height);
                        if (area > bestArea) { bestArea = area; best = t; }
                    }
                    if (best != null) dialogueText = best;
                }
            }

            if (speakerNameText == null)
            {
                if (speakerCandidate != null) speakerNameText = speakerCandidate;
                else
                {
                    foreach (var t in tmps)
                    {
                        if (t == null) continue;
                        if (t == dialogueText) continue;
                        if (choicesPanel != null && t.transform.IsChildOf(choicesPanel.transform)) continue;
                        speakerNameText = t;
                        break;
                    }
                }
            }
        }
        if ((choiceButtons == null || choiceButtons.Length == 0) && choicesPanel != null)
        {
            choiceButtons = choicesPanel.GetComponentsInChildren<Button>(true);
        }
    }
    
    
    void Awake()
    {
        
        EnsureRefs();
        _canvasGroup = dialoguePanel != null ? dialoguePanel.GetComponent<CanvasGroup>() : null;
        if (dialoguePanel != null && _canvasGroup == null)
        {
            _canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
        }
        SubscribeToManager();
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (choicesPanel != null) choicesPanel.SetActive(false);
    }

    void OnEnable()
    {
        EnsureRefs();
        SubscribeToManager();
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

    }
    
    IEnumerator FadeIn()
    {
        float elapsed = 0;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
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
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeOutDuration);
            yield return null;
        }
        _canvasGroup.alpha = 0;
        dialoguePanel.SetActive(false);
    }


}