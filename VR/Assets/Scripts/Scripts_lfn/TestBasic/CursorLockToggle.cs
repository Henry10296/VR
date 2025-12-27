using UnityEngine;

public class CursorLockToggle : MonoBehaviour
{
    public bool lockOnStart = false;
    public bool lockState = false;

    void Start()
    {
        if (lockOnStart) LockCursor();
        else UnlockCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            lockState = !lockState;

            if (lockState)
                LockCursor();
            else
                UnlockCursor();
        }
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