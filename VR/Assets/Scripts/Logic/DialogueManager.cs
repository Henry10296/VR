using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : Singleton<DialogueManager>
{
    [Header("对话配置")]
    public float defaultDisplayDuration = 3f;
    public float typingSpeed = 0.05f; // 打字机效果速度
    
    [Header("当前状态")]
    private DialogueData _currentDialogue;
    private NPCProfile _currentNPC;
    private bool _isDialogueActive;
    public bool autoAdvance = false;
    private Coroutine _typingCoroutine;
    private Coroutine _autoCoroutine;

    public DialogueUI DialogueUI;
    // 对话模板库
    private Dictionary<string, DialogueTemplate> _templates = new Dictionary<string, DialogueTemplate>();
    
    // 事件
    public event Action<DialogueData> OnDialogueStart;
    /// <summary>
    /// 对话
    /// </summary>
    public event Action<DialogueData> OnDialogueUpdate;
    
    public event Action<string> OnDialogueTextUpdate; // 打字机效果
    
    public event Action OnDialogueEnd;
    
    public event Action<List<DialogueChoice>> OnShowChoices;
    
    protected override void OnInitialize()
    {
        InitializeTemplates();
        Debug.Log("[DialogueManager] Initialized");
    }
    
    /// <summary>
    /// 初始化对话模板
    /// </summary>
    private void InitializeTemplates()
    {
        // Day 1 - 简单问候
        var day1Template = new DialogueTemplate
        {
            templateName = "Day1_Simple",
            dialogues = new List<DialogueData>
            {
                new DialogueData
                {
                    id = 1,
                    speaker = "NPC",
                    text = "Good morning.",
                    displayDuration = 2f,
                    nextDialogueId = -1
                }
            }
        };
        _templates["Day1_Simple"] = day1Template;
        
        // Day 2 - 带入境目的
        var day2Template = new DialogueTemplate
        {
            templateName = "Day2_Purpose",
            dialogues = new List<DialogueData>
            {
                new DialogueData
                {
                    id = 1,
                    speaker = "NPC",
                    text = "Good morning.",
                    displayDuration = 2f,
                    nextDialogueId = 2
                },
                new DialogueData
                {
                    id = 2,
                    speaker = "Inspector",
                    text = "What is the purpose of your visit?",
                    displayDuration = 2f,
                    nextDialogueId = 3
                },
                new DialogueData
                {
                    id = 3,
                    speaker = "NPC",
                    text = "I'm here to work.",
                    displayDuration = 2f,
                    nextDialogueId = -1
                }
            }
        };
        _templates["Day2_Purpose"] = day2Template;
        
        // 求情对话
        var pleaTemplate = new DialogueTemplate
        {
            templateName = "Plea",
            dialogues = new List<DialogueData>
            {
                new DialogueData
                {
                    id = 1,
                    speaker = "NPC",
                    text = "Please... my family is waiting for me...",
                    displayDuration = 3f,
                    nextDialogueId = -1
                }
            }
        };
        _templates["Plea"] = pleaTemplate;
    }
    
    /// <summary>
    /// 开始NPC对话
    /// </summary>
    public void StartDialogue(NPCProfile npc)
    {
        _currentNPC = npc;
        
        // 根据当日规则选择对话模板
        string templateName = GetTemplateForDay(SessionManager.Instance.currentDay);
        
        if (_templates.TryGetValue(templateName, out var template))
        {
            _isDialogueActive = true;
            DialogueData firstDialogue = template.dialogues[0];
            
            // 替换NPC名字
            firstDialogue.speaker = npc.name;
            firstDialogue.text = GetDialogueTextForNPC(npc, firstDialogue.id);
            
            PlayDialogue(firstDialogue);
        }
        else
        {
            _isDialogueActive = false;
            Debug.LogWarning($"[DialogueManager] Template not found: {templateName}");
        }
    }
    
    /// <summary>
    /// 播放对话
    /// </summary>
    private void PlayDialogue(DialogueData dialogue)
    {
        _currentDialogue = dialogue;
        _isDialogueActive = true;
        OnDialogueStart?.Invoke(dialogue);
        if (_typingCoroutine != null) { StopCoroutine(_typingCoroutine); _typingCoroutine = null; }
        if (typingSpeed > 0)
        {
            _typingCoroutine = StartCoroutine(TypewriterEffect(dialogue.text));
        }
        else
        {
            OnDialogueTextUpdate?.Invoke(dialogue.text);
        }
        if (dialogue.voiceClip != null)
        {
            PlayVoice(dialogue.voiceClip);
        }
        if (dialogue.choices != null && dialogue.choices.Count > 0)
        {
            OnShowChoices?.Invoke(dialogue.choices);
        }
        else if (autoAdvance)
        {
            if (_autoCoroutine != null) { StopCoroutine(_autoCoroutine); _autoCoroutine = null; }
            if (dialogue.nextDialogueId > 0)
            {
                _autoCoroutine = StartCoroutine(AutoPlayNext(dialogue));
            }
            else
            {
                _autoCoroutine = StartCoroutine(AutoEndDialogue(dialogue));
            }
        }
    }
    
    /// <summary>
    /// 打字机效果
    /// </summary>
    private IEnumerator TypewriterEffect(string text)
    {
        string displayText = "";
        foreach (char c in text)
        {
            displayText += c;
            OnDialogueTextUpdate?.Invoke(displayText);
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    
    /// <summary>
    /// 自动播放下一句
    /// </summary>
    private IEnumerator AutoPlayNext(DialogueData dialogue)
    {
        yield return new WaitForSeconds(dialogue.displayDuration);
        
        // 查找下一句对话
        string templateName = GetTemplateForDay(SessionManager.Instance.currentDay);
        if (_templates.ContainsKey(templateName))
        {
            DialogueData nextDialogue = _templates[templateName].dialogues.Find(d => d.id == dialogue.nextDialogueId);
            if (nextDialogue != null)
            {
                PlayDialogue(nextDialogue);
            }
            else
            {
                EndDialogue();
            }
        }
    }
    
    /// <summary>
    /// 自动结束对话
    /// </summary>
    private IEnumerator AutoEndDialogue(DialogueData dialogue)
    {
        yield return new WaitForSeconds(dialogue.displayDuration);
        EndDialogue();
    }
    
    /// <summary>
    /// 选择对话选项
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        if (_currentDialogue.choices != null && choiceIndex < _currentDialogue.choices.Count)
        {
            int targetId = _currentDialogue.choices[choiceIndex].targetDialogueId;
            
            string templateName = GetTemplateForDay(SessionManager.Instance.currentDay);
            if (_templates.ContainsKey(templateName))
            {
                DialogueData nextDialogue = _templates[templateName].dialogues.Find(d => d.id == targetId);
                if (nextDialogue != null)
                {
                    PlayDialogue(nextDialogue);
                }
            }
        }
    }
    
    /// <summary>
    /// 结束对话
    /// </summary>
    public void EndDialogue()
    {
        _isDialogueActive = false;
        _currentDialogue = null;
        OnDialogueEnd?.Invoke();
        
        Debug.Log("[DialogueManager] Dialogue ended");
    }
    
    /// <summary>
    /// 播放语音
    /// </summary>
    private void PlayVoice(AudioClip clip)
    {
        // 调用音频管理器播放语音
        // AudioManager.Instance.PlayVoice(clip);
        Debug.Log($"[DialogueManager] Playing voice: {clip.name}");
    }
    
    /// <summary>
    /// 根据天数获取模板名
    /// </summary>
    private string GetTemplateForDay(int day)
    {
        var rules = SessionManager.Instance.currentDayRules;
        if (rules != null && !string.IsNullOrEmpty(rules.dialogueTemplate))
            return rules.dialogueTemplate;
        if (day == 1)
            return "Day1_Simple";
        else if (day >= 2)
            return "Day2_Purpose";
        return "Day1_Simple";
    }
    
    /// <summary>
    /// 根据NPC特性获取对话文本
    /// </summary>
    private string GetDialogueTextForNPC(NPCProfile npc, int dialogueId)
    {
        // 可以根据NPC的特性动态生成对话
        if (dialogueId == 1)
            return npc.greetingText;
        else if (dialogueId == 3)
            return npc.purposeText;
        
        return "...";
    }
    
    /// <summary>
    /// 播放特殊对话（求情、愤怒等）
    /// </summary>
    public void PlaySpecialDialogue(string type)
    {
        if (_currentNPC == null) return;
        
        DialogueData specialDialogue = new DialogueData
        {
            id = 999,
            speaker = _currentNPC.name,
            displayDuration = 3f,
            nextDialogueId = -1
        };
        
        specialDialogue.text = type;
        
        switch (type)
        {
            case "plea":
                specialDialogue.text = _currentNPC.pleaText ?? "求求你放我过去...";
                break;
            case "thanks":
                specialDialogue.text = "谢谢你！";
                break;
            case "angry":
                specialDialogue.text = "这不公平！";
                break;
        }
        
        PlayDialogue(specialDialogue);
    }
    
    public bool IsDialogueActive => _isDialogueActive;
    public DialogueData CurrentDialogue => _currentDialogue;
    public void RestartCurrentDialogue()
    {
        if (_currentDialogue != null)
        {
            PlayDialogue(_currentDialogue);
        }
    }
    public void Next()
    {
        if (_currentDialogue == null) return;
        if (_currentDialogue.choices != null && _currentDialogue.choices.Count > 0) return;
        string templateName = GetTemplateForDay(SessionManager.Instance.currentDay);
        if (_templates.ContainsKey(templateName) && _currentDialogue.nextDialogueId > 0)
        {
            DialogueData nextDialogue = _templates[templateName].dialogues.Find(d => d.id == _currentDialogue.nextDialogueId);
            if (nextDialogue != null)
            {
                PlayDialogue(nextDialogue);
                return;
            }
        }
        EndDialogue();
    }
    public void SkipTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        if (_currentDialogue != null)
        {
            OnDialogueTextUpdate?.Invoke(_currentDialogue.text);
        }
    }
    public void CloseDialogue()
    {
        if (_autoCoroutine != null)
        {
            StopCoroutine(_autoCoroutine);
            _autoCoroutine = null;
        }
        EndDialogue();
    }

    public void ShowText(string text)
    {
        ShowText("System", text, defaultDisplayDuration);
    }
    
    public void ShowText(string speaker, string text, float duration)
    {
        DialogueData d = new DialogueData
        {
            id = 1000,
            speaker = string.IsNullOrEmpty(speaker) ? "System" : speaker,
            text = text,
            displayDuration = duration > 0 ? duration : defaultDisplayDuration,
            nextDialogueId = -1
        };
        PlayDialogue(d);
    }

    public void RegisterTemplate(string name, DialogueTemplate template)
    {
        if (!string.IsNullOrEmpty(name) && template != null)
        {
            _templates[name] = template;
        }
    }
}
/// <summary>
/// 对话数据 - 单句对话内容
/// </summary>
[System.Serializable]
public class DialogueData
{
    public int id;
    public string speaker;          // 说话者名字
    public string text;             // 对话文本
    public AudioClip voiceClip;     // 语音文件
    public float displayDuration;   // 显示时长（秒）
    public int nextDialogueId;      // 下一句对话ID（-1表示结束）
    public List<DialogueChoice> choices; // 对话选项
}

/// <summary>
/// 对话选项
/// </summary>
[System.Serializable]
public class DialogueChoice
{
    public string choiceText;       // 选项文本
    public int targetDialogueId;    // 目标对话ID
}

/// <summary>
/// 对话模板 - 用于NPC
/// </summary>
[System.Serializable]
public class DialogueTemplate
{
    public string templateName;
    public List<DialogueData> dialogues;
}

/// <summary>
/// NPC对话配置
/// </summary>
[System.Serializable]
public class NPCDialogueProfile
{
    public string greetingText;     // 问候语 "早上好"
    public string purposeText;      // 入境目的 "我来工作"
    public string pleaText;         // 求情文本 "求求你放我过去"
    public string thanksText;       // 感谢 "谢谢你"
    public string angryText;        // 愤怒 "这不公平！"
    
    // 根据NPC特性选择
    public bool isNervous;          // 紧张的
    public bool isRude;             // 粗鲁的
    public bool isFriendly;         // 友好的
}
/// <summary>
/// 对话生成器 - 根据NPC特性生成对话
/// </summary>
public static class DialogueGenerator
{
    private static string[] greetings = {
        "Good morning.",
        "Hello.",
        "Good day.",
        "Hi."
    };
    
    private static string[] purposes = {
        "I'm here to work.",
        "I'm visiting family.",
        "I'm here for tourism.",
        "I'm here for business."
    };
    
    private static string[] pleas = {
        "Please, my family is waiting for me...",
        "Please, I really need to get through.",
        "I promise there will be no trouble.",
        "Please believe me."
    };
    
    private static string[] thanks = {
        "Thank you!",
        "Thank you for your help.",
        "Great, thanks!"
    };
    
    private static string[] angry = {
        "This is unfair!",
        "Why did you deny me?",
        "I will file a complaint!",
        "You'll regret this!"
    };
    
    /// <summary>
    /// 为NPC生成对话内容
    /// </summary>
    public static void GenerateDialogueForNPC(NPCProfile npc)
    {
        npc.greetingText = greetings[UnityEngine.Random.Range(0, greetings.Length)];
        npc.purposeText = purposes[UnityEngine.Random.Range(0, purposes.Length)];
        npc.pleaText = pleas[UnityEngine.Random.Range(0, pleas.Length)];
        
        // 根据违规者身份调整对话
        if (npc.isViolator)
        {
            // 违规者可能更紧张
            if (UnityEngine.Random.value > 0.5f)
            {
                npc.greetingText = "Uh... Good morning...";
            }
        }
    }
    
    /// <summary>
    /// 生成反应对话（批准/拒绝后）
    /// </summary>
    public static string GenerateReactionDialogue(DecisionType decision, bool isViolator)
    {
        if (decision == DecisionType.Approve)
        {
            return thanks[UnityEngine.Random.Range(0, thanks.Length)];
        }
        else if (decision == DecisionType.Deny || decision == DecisionType.Detain)
        {
            if (isViolator && UnityEngine.Random.value > 0.3f)
            {
                // 违规者可能求情
                return pleas[UnityEngine.Random.Range(0, pleas.Length)];
            }
            else
            {
                // 或者愤怒
                return angry[UnityEngine.Random.Range(0, angry.Length)];
            }
        }
        
        return "...";
    }
}
