using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源管理器 - 管理资源加载和缓存
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    private Dictionary<string, UnityEngine.Object> _cache = new Dictionary<string, UnityEngine.Object>();
    
    protected override void OnInitialize()
    {
        Debug.Log("[ResourceManager] Initialized");
    }
    
    /// <summary>
    /// 加载资源
    /// </summary>
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        if (_cache.ContainsKey(path))
        {
            return _cache[path] as T;
        }
        
        T resource = Resources.Load<T>(path);
        if (resource != null)
        {
            _cache[path] = resource;
        }
        else
        {
            Debug.LogWarning($"[ResourceManager] Failed to load: {path}");
        }
        
        return resource;
    }
    
    /// <summary>
    /// 卸载资源
    /// </summary>
    public void Unload(string path)
    {
        if (_cache.ContainsKey(path))
        {
            Resources.UnloadAsset(_cache[path]);
            _cache.Remove(path);
        }
    }
    
    /// <summary>
    /// 清空所有缓存
    /// </summary>
    public void UnloadAll()
    {
        _cache.Clear();
        Resources.UnloadUnusedAssets();
    }
}
