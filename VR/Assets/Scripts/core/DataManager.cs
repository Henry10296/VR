using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 数据管理器 - 管理所有游戏数据
/// </summary>
public class DataManager : Singleton<DataManager>
{
    // 数据容器
    private Dictionary<Type, object> _dataContainers = new Dictionary<Type, object>();
    
    // 运行时数据缓存
    private Dictionary<string, object> _runtimeCache = new Dictionary<string, object>();
    
    protected override void OnInitialize()
    {
        Debug.Log("[DataManager] Initialized");
    }
    
    /// <summary>
    /// 注册数据
    /// </summary>
    public void RegisterData<T>(T data)
    {
        Type type = typeof(T);
        _dataContainers[type] = data;
    }
    
    /// <summary>
    /// 获取数据
    /// </summary>
    public T GetData<T>() where T : class
    {
        Type type = typeof(T);
        if (_dataContainers.ContainsKey(type))
        {
            return _dataContainers[type] as T;
        }
        return null;
    }
    
    /// <summary>
    /// 设置运行时缓存
    /// </summary>
    public void SetCache(string key, object value)
    {
        _runtimeCache[key] = value;
    }
    
    /// <summary>
    /// 获取运行时缓存
    /// </summary>
    public T GetCache<T>(string key, T defaultValue = default)
    {
        if (_runtimeCache.ContainsKey(key))
        {
            return (T)_runtimeCache[key];
        }
        return defaultValue;
    }
    
    /// <summary>
    /// 清空缓存
    /// </summary>
    public void ClearCache()
    {
        _runtimeCache.Clear();
    }
}
