using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局事件系统 - 用于模块间解耦通信
/// </summary>
public static class EventBus
{
    private static Dictionary<string, Delegate> _eventTable = new Dictionary<string, Delegate>();
    
    /// <summary>
    /// 订阅事件
    /// </summary>
    public static void Subscribe<T>(string eventName, Action<T> callback)
    {
        if (!_eventTable.ContainsKey(eventName))
        {
            _eventTable[eventName] = callback;
        }
        else
        {
            _eventTable[eventName] = Delegate.Combine(_eventTable[eventName], callback);
        }
    }
    
    /// <summary>
    /// 取消订阅
    /// </summary>
    public static void Unsubscribe<T>(string eventName, Action<T> callback)
    {
        if (_eventTable.ContainsKey(eventName))
        {
            _eventTable[eventName] = Delegate.Remove(_eventTable[eventName], callback);
            if (_eventTable[eventName] == null)
            {
                _eventTable.Remove(eventName);
            }
        }
    }
    
    /// <summary>
    /// 发布事件
    /// </summary>
    public static void Publish<T>(string eventName, T data)
    {
        if (_eventTable.ContainsKey(eventName))
        {
            Action<T> callback = _eventTable[eventName] as Action<T>;
            callback?.Invoke(data);
        }
    }
    
    /// <summary>
    /// 清空所有事件
    /// </summary>
    public static void Clear()
    {
        _eventTable.Clear();
    }
}

/// <summary>
/// 事件名称常量 - 统一管理所有事件名
/// </summary>
public static class EventNames
{
    // 游戏流程事件
    public const string GAME_STARTED = "Game.Started";
    public const string DAY_STARTED = "Day.Started";
    public const string DAY_ENDED = "Day.Ended";
    public const string GAME_OVER = "Game.Over";
    
    // NPC事件
    public const string NPC_ARRIVED = "NPC.Arrived";
    public const string NPC_PROCESSED = "NPC.Processed";
    public const string NPC_DEPARTED = "NPC.Departed";
    
    // 交互事件
    public const string DOCUMENT_GRABBED = "Document.Grabbed";
    public const string DOCUMENT_PLACED = "Document.Placed";
    public const string STAMP_APPLIED = "Stamp.Applied";
    public const string BUTTON_PRESSED = "Button.Pressed";
    
    // UI事件
    public const string UI_SHOW_RESULT = "UI.ShowResult";
    public const string UI_SHOW_SETTLEMENT = "UI.ShowSettlement";
    
    // 系统事件
    public const string FAMILY_STATUS_UPDATED = "Family.StatusUpdated";
    public const string MONEY_CHANGED = "Money.Changed";
}
