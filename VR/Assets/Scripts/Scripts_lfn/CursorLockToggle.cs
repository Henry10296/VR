using UnityEngine;

public class CursorLockToggle : MonoBehaviour
{
    public bool lockOnStart = true;
    public KeyCode unlockKey = KeyCode.Escape;

    void Start()
    {
        if (lockOnStart) LockCursor();
        else UnlockCursor();
    }

    void Update()
    {
        // Esc 解锁
        if (Input.GetKeyDown(unlockKey))
            UnlockCursor();

        // 点击左键重新锁定（你也可以改成任意键）
        if (Input.GetMouseButtonDown(0))
            LockCursor();
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}