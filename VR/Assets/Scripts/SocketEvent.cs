using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] XRSocketInteractor KeyItem;
    [SerializeField] bool isLocked;
    // Start is called before the first frame update
    void Start()
    {
        if (KeyItem != null)
        {
            KeyItem.selectEntered.AddListener(onEnterCheck);
        }
        
    }

    private void onEnterCheck(SelectEnterEventArgs arg0)
    {
        bool isLocked = false;
        Debug.Log("UNLOCKKKKKEDDDDD");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
