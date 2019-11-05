using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public GameObject alleyObjects;
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(test());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   public void startDisableAlleyObjects()
    {
        StartCoroutine(disableAlleyObjects());
    }

    IEnumerator disableAlleyObjects()
    {
        foreach (Transform child in alleyObjects.transform)
        {
            yield return new WaitForSeconds(.3f);
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
    }

   
   

    IEnumerator test()
    {
        yield return new WaitForSeconds(5);
        StartCoroutine(disableAlleyObjects());

    }
}
