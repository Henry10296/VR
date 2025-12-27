using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // 拖 Main Camera 的 Transform

    [Header("Move")]
    public float moveSpeed = 4.5f;
    public float rotationSmoothTime = 0.12f; // 转身平滑时间

    [Header("Gravity")]
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private CharacterController cc;
    private Vector3 velocity;
    private bool isGrounded;

    private float turnSmoothVelocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        // 你忘记拖 cameraTransform 时，自动找主相机
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A/D
        float z = Input.GetAxisRaw("Vertical");   // W/S

        Vector3 inputDir = new Vector3(x, 0f, z).normalized;

        // 地面检测
        if (groundCheck != null)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        else
            isGrounded = cc.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // ✅ 核心：把输入方向转到“相机朝向”下
        if (inputDir.sqrMagnitude > 0.001f && cameraTransform != null)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;

            // 平滑转向角色
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // 角色朝向就是移动方向
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        }

        // 重力
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
