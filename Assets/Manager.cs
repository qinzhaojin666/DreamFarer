﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public GameObject alleyObjects;
    public GameObject barObjects;
    float time;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(test());
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
    }

   public void startDisableAlleyObjects()
    {
        StartCoroutine(disableAlleyObjects());
    }

    public void startEnableBarObjects()
    {
        StartCoroutine(enableBarObjects());

    }

    IEnumerator disableAlleyObjects()
    {
        foreach (Transform child in alleyObjects.transform)
        {
            yield return new WaitForSeconds(.1f);
            if (child.gameObject.CompareTag("popup"))
            {
                print("popup found");
                float count = 0;
                while (count <= 45)
                {
                    child.Rotate(new Vector3(0, 0, -2f));
                    //child.RotateAround( , new Vector3(-1f, 0, 0));
                    count += 2;
                    yield return new WaitForSeconds(.0001f);
                }
            }
            child.gameObject.SetActive(false);
        }
        startEnableBarObjects();
    }

    IEnumerator enableBarObjects()
    {
        foreach (Transform child in barObjects.transform)
        {
            yield return new WaitForSeconds(.1f);
            //if (child.gameObject.CompareTag("popup"))
            //{
            //    print("popup found");
            //    float count = 0;
            //    while (count <= 45)
            //    {
            //        child.Rotate(new Vector3(0, 0, -2f));
            //        //child.RotateAround( , new Vector3(-1f, 0, 0));
            //        count += 2;
            //        yield return new WaitForSeconds(.0001f);
            //    }
            //}
            child.gameObject.SetActive(true);
        }
    }




    IEnumerator test()
    {
        yield return new WaitForSeconds(5);
        StartCoroutine(disableAlleyObjects());

    }
}
