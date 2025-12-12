using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public List<Transform> CameraPoints;
    int CameraIndex = 0;
    bool IsMoving = false;
    void Start()
    {
        transform.position = CameraPoints[CameraIndex].position;
        transform.rotation = CameraPoints[CameraIndex].rotation;
        CameraIndex++;
    }

    public void SwitchToNextPoint(float SwitchTime) 
    {
        StartCoroutine(SwitchToNextRoutine(SwitchTime));
    }


    IEnumerator SwitchToNextRoutine( float SwitchTime)
    {
        if(CameraIndex >= CameraPoints.Count) yield break;
        if (IsMoving) yield break;
        IsMoving = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 endPos = CameraPoints[CameraIndex].position;
        Quaternion endRot = CameraPoints[CameraIndex].rotation;

        float Timer = 0f;

        while (Timer < SwitchTime)
        {
            Timer += Time.deltaTime;
            float t = Mathf.Clamp01(Timer / SwitchTime);

            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;
       
        IsMoving = false;
    }

}
