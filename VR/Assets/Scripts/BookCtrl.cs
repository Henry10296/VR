using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BookCtrl : MonoBehaviour
{
    public List<Transform> PagesFromTopToButtom;
    private List<float> PagesOriginalY;
    private List<float> PagesTurnedY;
    public Transform PagesTurnPoint;
    int PageTurned = 0;
    public float TurnTime = 0.2f;
    bool IsTurning = false;

    public void TurnForward(float TurnTime) 
    {
        StartCoroutine(TurnForwardRoutine(TurnTime));
    }

    IEnumerator TurnForwardRoutine(float TurnTime) 
    {
        if(IsTurning) yield break;
        IsTurning = true;
        if (PageTurned == PagesFromTopToButtom.Count) yield break;
        float Timer = 0f;
        float TotalAngle = 0f;
        while (Timer < TurnTime) 
        {
            Timer += Time.deltaTime;
            float angle = (Time.deltaTime / TurnTime) * 180f;
            PagesFromTopToButtom[PageTurned].RotateAround(PagesTurnPoint.position, PagesTurnPoint.forward, angle);
            TotalAngle += angle;
            yield return null;
        }
        float OffAngle = 180f - TotalAngle;
        PagesFromTopToButtom[PageTurned].RotateAround(PagesTurnPoint.position, PagesTurnPoint.forward, OffAngle);

        Vector3 Pos = PagesFromTopToButtom[PageTurned].position;
        PagesFromTopToButtom[PageTurned].position = new Vector3(Pos.x, PagesTurnedY[PageTurned], Pos.z);
        PageTurned++;
        IsTurning = false;
        Debug.Log("PageTurned = " + PageTurned.ToString());
    }

    void TurnBackward() 
    {
        if (PageTurned == 0) return;
        PageTurned--;
        PagesFromTopToButtom[PageTurned].RotateAround(PagesTurnPoint.position, PagesTurnPoint.forward, -180);
        Vector3 Pos = PagesFromTopToButtom[PageTurned].position;
        PagesFromTopToButtom[PageTurned].position = new Vector3(Pos.x, PagesOriginalY[PageTurned], Pos.z);
    }
    
    
    void Start()
    {
        PagesOriginalY = new List<float> ();
        PagesTurnedY = new List<float> ();
        for (int i = 0; i < PagesFromTopToButtom.Count; i++) 
        {
            PagesOriginalY.Add(PagesFromTopToButtom[i].position.y);
            PagesTurnedY.Add(PagesFromTopToButtom[PagesFromTopToButtom.Count-1-i].position.y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) 
        {
            TurnBackward();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) 
        {
            TurnForward(TurnTime);
        }
    }
}

