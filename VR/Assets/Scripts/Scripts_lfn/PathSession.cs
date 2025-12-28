using UnityEngine;

public class PathSession : MonoBehaviour
{
    [Header("Optional refs (can be auto found)")]
    public PeopleWalkPath peopleWalkPath;          // 挂在同一根物体上
    public WindowResumeLeave windowResumeLeave;    // 你已有的“让NPC继续走完”的脚本（若不用也行）
    public bool logDebug = true;

    [Header("Runtime (read only)")]
    [SerializeField] private MovePath cachedMovePath; // 当前NPC clone身上的 MovePath

    private void Awake()
    {
        if (!peopleWalkPath) peopleWalkPath = GetComponent<PeopleWalkPath>();
        if (!windowResumeLeave) windowResumeLeave = GetComponentInChildren<WindowResumeLeave>(true);

        CacheCurrentNpcMovePath();
    }

    private void OnEnable()
    {
        // 每次启用这条路径时，重新抓一次，避免你切换天/切换NPC时引用旧的
        CacheCurrentNpcMovePath();
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void ForceLeave()
    {
        CacheCurrentNpcMovePath();

        if (cachedMovePath == null)
        {
            if (logDebug) Debug.LogWarning($"[PathSession] MovePath 为空，无法离开：{name}");
            return;
        }

        // ✅ 方案A：如果你已有 WindowResumeLeave（并且它内部已经能 Resume/ForceLeave）
        if (windowResumeLeave != null)
        {
            if (logDebug) Debug.Log($"[PathSession] Use WindowResumeLeave to leave. Path={name}");
            windowResumeLeave.OnClickResumeForceLeave(); // 你得保证 WindowResumeLeave 里有这个 public 方法
            return;
        }

        // ✅ 方案B：直接对 MovePath 做“继续走”（根据你 MovePath 的实现改函数名）
        // 比如你 MovePath 里可能叫 Resume() / Continue() / SetStopped(false) / EnableMove() 等
        if (logDebug) Debug.Log($"[PathSession] Direct leave via MovePath. NPC={cachedMovePath.name}");
        cachedMovePath.enabled = true; // 最保底：至少确保脚本启用
        // TODO：这里按你 MovePath 的真实API补一行，比如 cachedMovePath.Resume();
    }

    private void CacheCurrentNpcMovePath()
    {
        cachedMovePath = null;

        // 你的体系：Populate 之后，people 子物体下有 clone，clone 根上挂 MovePath
        // PeopleWalkPath 通常会有一个 people 容器（你层级里就叫 people）
        Transform peopleRoot = transform.Find("people");
        if (peopleRoot == null)
        {
            // 有些资产包可能叫 People/Persons/Characters，你按需加：Find("People")...
            peopleRoot = transform;
        }

        // 找第一个带 MovePath 的子物体（你说你保证每条路径只有一个NPC）
        cachedMovePath = peopleRoot.GetComponentInChildren<MovePath>(true);

        if (logDebug)
        {
            if (cachedMovePath != null) Debug.Log($"[PathSession] Found MovePath on: {cachedMovePath.name}");
            else Debug.LogWarning($"[PathSession] No MovePath found under {name}");
        }
    }
    public bool HasArrived { get; private set; }

    public void NotifyArrived()
    {
        HasArrived = true;
        if (logDebug) Debug.Log($"[PathSession] Arrived: {name}");
    }

}
