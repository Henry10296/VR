using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICtrl : MonoBehaviour
{
    static UICtrl Instance = null;
    public Transform Book;
    public Transform Camera;

    public Transform Menu1;
    public Transform Menu2;
    private void Awake()
    {
        if (Instance != null) 
        {
            Destroy(Instance);
        }
        Instance = this;
    }

    public void SwitchToMenu2(float Time) 
    {
        Menu1.gameObject.SetActive(false);
        Book.gameObject.GetComponent<BookCtrl>().TurnForward(Time);
        Camera.gameObject.GetComponent<CameraCtrl>().SwitchToNextPoint(Time);
        Menu2.gameObject.SetActive(true);
    }
}
