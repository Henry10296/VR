using UnityEngine;

public class ThirdPersonCameraOrbit : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // 玩家（或玩家的CameraTarget点）
    public Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Distance")]
    public float distance = 4.5f;
    public float minDistance = 2.0f;
    public float maxDistance = 7.0f;
    public float zoomSpeed = 3.0f;

    [Header("Mouse")]
    public float sensitivityX = 180f;        // 横向灵敏度
    public float sensitivityY = 120f;        // 纵向灵敏度
    public float minPitch = -35f;
    public float maxPitch = 70f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.08f;

    private float yaw;   // 左右
    private float pitch; // 上下
    private Vector3 posVelocity;

    void Start()
    {
        // 以当前相机角度作为初始角度，防止一开局瞬移
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 只有鼠标锁定时才转镜头（避免菜单时乱转）
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            yaw += mx * sensitivityX * Time.deltaTime;
            pitch -= my * sensitivityY * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            // 滚轮缩放（可选）
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                distance -= scroll * zoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 focusPoint = target.position + targetOffset;
        Vector3 desiredPos = focusPoint - rot * Vector3.forward * distance;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref posVelocity, positionSmoothTime);
        transform.rotation = rot;
    }

    // 给玩家移动脚本取相机Yaw用（可选）
    public float CurrentYaw => yaw;
}
