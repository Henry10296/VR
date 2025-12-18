using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VR_Button : MonoBehaviour
{
    public float deadTime = 1f;
    private bool DeadTimeActive = false;

    public UnityEvent onPressed, onRealesed;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Button") && !DeadTimeActive)
        {
            onPressed?.Invoke();
            Debug.Log("I've been pressed");
        }
    }

    // Update is called once per frame
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Button") && !DeadTimeActive)
        {
            onPressed?.Invoke();
            Debug.Log("I've been released");
            StartCoroutine(WaitForDeadTime());
        }


    }

    IEnumerator WaitForDeadTime()
    {
        DeadTimeActive = true;
        yield return new WaitForSeconds(deadTime);
        DeadTimeActive = false;
    }
}