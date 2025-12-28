using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    None,
    MainMenu,      // 主菜单
    DayBriefing,   // 每日简报
    Working,       // 工作中
    DaySettlement, // 每日结算
    GameOver       // 游戏结束
}
/// <summary>
/// 证件类型
/// </summary>
public enum DocumentType
{  
    Passport,      // 护照
    WorkPermit,    // 工作许可证
    IDCard,        // 身份证
    EntryPermit    // 入境许可
}

public class GameManager : Singleton<GameManager>
{
    [Header("游戏状态")]
    public GameState currentState = GameState.None;
    public int currentDay = 0;
    
    [Header("游戏配置")]
    public bool isVRMode = true;
    
    private Dictionary<GameState, Action> _stateEnterActions;
    private Dictionary<GameState, Action> _stateExitActions;
    
    protected override void OnInitialize()
    {
        InitializeStateActions();
        Debug.Log("[GameManager] Initialized");
    }
    
    private void InitializeStateActions()
    {
        _stateEnterActions = new Dictionary<GameState, Action>
        {
            { GameState.MainMenu, OnEnterMainMenu },
            { GameState.DayBriefing, OnEnterDayBriefing },
            { GameState.Working, OnEnterWorking },
            { GameState.DaySettlement, OnEnterDaySettlement },
            { GameState.GameOver, OnEnterGameOver }
        };
        
        _stateExitActions = new Dictionary<GameState, Action>
        {
            { GameState.Working, OnExitWorking }
        };
    }
    
    private void EnsureStateActions()
    {
        if (_stateEnterActions == null || _stateExitActions == null)
        {
            InitializeStateActions();
        }
    }
    
    /// <summary>
    /// 切换游戏状态
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;
        
        EnsureStateActions();
        
        Debug.Log($"[GameManager] State: {currentState} -> {newState}");
        
        if (_stateExitActions.ContainsKey(currentState))
        {
            _stateExitActions[currentState]?.Invoke();
        }
        
        GameState oldState = currentState;
        currentState = newState;
        
        if (_stateEnterActions.ContainsKey(newState))
        {
            _stateEnterActions[newState]?.Invoke();
        }
    }
    
    /// <summary>
    /// 开始新游戏
    /// </summary>
    public void StartNewGame()
    {
        currentDay = 0;
        EventBus.Publish(EventNames.GAME_STARTED, true);
        SceneManager.sceneLoaded += OnSceneLoadedStartGame;
        SceneManager.LoadScene("SampleScene");
    }
    
    private void OnSceneLoadedStartGame(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedStartGame;
        StartNextDay();
    }
    
    /// <summary>
    /// 开始下一天
    /// </summary>
    public void StartNextDay()
    {
        currentDay++;
        ChangeState(GameState.DayBriefing);
    }

    public void StartWork()
    {
        if (isActiveAndEnabled)
        {
            StartCoroutine(AutoStartWorking());
        }
        else if (SessionManager.Instance != null && SessionManager.Instance.isActiveAndEnabled)
        {
            SessionManager.Instance.StartCoroutine(AutoStartWorking());
        }
        else
        {
            ChangeState(GameState.Working);
        }
                                                                                                                                                                   
    }
    // 状态进入/退出回调
    private void OnEnterMainMenu() { }
    
    private void OnEnterDayBriefing()
    {
        SessionManager.Instance.StartNewDay(currentDay);
        EventBus.Publish(EventNames.DAY_STARTED, currentDay);
       
    }
    
    private IEnumerator AutoStartWorking()
    {
        yield return new WaitForSeconds(ConfigManager.Instance.npcArriveDelay);
        ChangeState(GameState.Working);
    }
    
    private void OnEnterWorking() 
    {
        Debug.Log("[GameManager] 进入工作状态");
        var ui = UnityEngine.Object.FindObjectOfType<DialogueUI>(true);
        if (ui != null && !ui.gameObject.activeInHierarchy)
        {
            Transform t = ui.transform;
            Transform toEnable = null;
            while (t != null)
            {
                if (!t.gameObject.activeSelf) toEnable = t;
                t = t.parent;
            }
            if (toEnable != null) toEnable.gameObject.SetActive(true);
            else ui.gameObject.SetActive(true);
        }
    
        // 1. 调用 NPC 管理器生成今天的队列
        NPCManager.Instance.GenerateDailyNPCs();
    
        // 2. 触发第一个 NPC 进场
        // 这里的逻辑可以写在 InspectionManager 里，也可以直接调用
        InspectionManager.Instance.CallNextNPC(); 
    }
    private void OnEnterDaySettlement()
    {
        EventBus.Publish(EventNames.DAY_ENDED, currentDay);
    }
    private void OnEnterGameOver()
    {
        EventBus.Publish(EventNames.GAME_OVER, true);
    }
    
    private void OnExitWorking() { }
    
    
    
}
