using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onGrabDrink : MonoBehaviour
{
    public Manager manger;
    private bool grabbedOnce = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool isGrabbed = GetComponent<OVRGrabbable>().isGrabbed;
        if ( !grabbedOnce && isGrabbed)
        {
            grabbedOnce = true;
            whenGrabbed();
        }
    }

    void whenGrabbed()
    {
        manger.startConversations();
    }
}
