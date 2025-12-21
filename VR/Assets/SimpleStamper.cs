using UnityEngine;

public class SimpleStamper : MonoBehaviour
{
    [Tooltip("调节印记的上下位置：如果是负数就是往回拉，如果是正数就是往下推")]
    public float raiseHeight = 0.002f;// 默认设为负数，试试往回拉
    [Header("设置部分")]
    [Tooltip("这里拖入你刚才做好的 Stamp_Mark_Prefab")]
    public GameObject inkMarkPrefab;

    [Tooltip("这里选择 Stampable 层，确保只在护照上盖章")]
    public LayerMask targetLayers;

    [Tooltip("防止一瞬间盖重叠，设置0.5秒冷却")]
    public float cooldown = 0.5f;

    // 内部计时器
    private float _timer;

    void Update()
    {
        // 每一帧都在计算冷却时间
        if (_timer > 0) _timer -= Time.deltaTime;
    }

    // 当感应器（Trigger）穿过别的物体时触发
    void OnTriggerEnter(Collider other)
    {
        // 1. 如果还在冷却中，直接跳过
        if (_timer > 0) return;

        // 2. 检查碰到的物体是不是在 "Stampable" 层
        // (这段位运算代码是Unity检查Layer的标准写法)
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            DoStamp(other);
        }
    }

    void DoStamp(Collider target)
    {
        // 重置冷却时间
        _timer = cooldown;

        // --- 第一步：确定位置 ---
        // 使用感应器(当前物体)的位置
        Vector3 spawnPos = transform.position;

        // --- 第二步：确定旋转 ---
        // 这一步最容易错。我们通常希望印记的方向和印章底面一致。
        // 如果你的Quad是Z轴朝前的，通常直接用 transform.rotation 即可。
        // 如果发现盖出来反了，需要在这里乘以一个欧拉角。
        // 这里的 Quaternion.Euler(90, 0, 0) 是让Quad绕着X轴转90度，从"墙"变成"地板"
        Quaternion spawnRot = transform.rotation * Quaternion.Euler(90, 0, 0);

        // --- 第三步：生成印记 ---
        GameObject newMark = Instantiate(inkMarkPrefab, spawnPos, spawnRot);

        // --- 第四步：微调位置 (防闪烁关键) ---
        // 让印记沿着印章的“前方/下方”稍微移动一点点，确保不和纸面重叠
        // 0.001f 是 1毫米
        // transform.forward 是朝下，用 "-=" 就是朝反方向（朝上）移动
        // 这样你只需要在面板里填 0.001，印记就会自动往上浮
        newMark.transform.position -= transform.forward * raiseHeight;

        // --- 第五步：父子绑定 (核心) ---
        // 把印记设为护照(target)的子物体。
        // 这样你拿起护照，章也会跟着走。
        newMark.transform.SetParent(target.transform, true);

        Debug.Log("盖章成功！");
    }
}