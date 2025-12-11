using UnityEngine;

/// <summary>
/// 泛型单例基类 - 所有Manager都继承这个
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject($"[{typeof(T).Name}]");
                        _instance = singleton.AddComponent<T>();
                        DontDestroyOnLoad(singleton);
                    }
                }
            }
            return _instance;
        }
    }
    
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            OnInitialize();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化方法 - 子类重写这个方法来做初始化
    /// </summary>
    protected virtual void OnInitialize() { }
}