using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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
    
    /// <summary>
    /// 切换游戏状态
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;
        
        Debug.Log($"[GameManager] State: {currentState} -> {newState}");
        
        // 退出旧状态
        if (_stateExitActions.ContainsKey(currentState))
        {
            _stateExitActions[currentState]?.Invoke();
        }
        
        GameState oldState = currentState;
        currentState = newState;
        
        // 进入新状态
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
    
    // 状态进入/退出回调
    private void OnEnterMainMenu() { }
    
    private void OnEnterDayBriefing()
    {
        EventBus.Publish(EventNames.DAY_STARTED, currentDay);
    }
    
    private void OnEnterWorking() { }
    
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
