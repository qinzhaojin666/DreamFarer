using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public GameObject alleyObjects;
    public GameObject barObjects;
    public GameObject islandObjects;

    public Scene2SoundManager scene2_sound_manager;
    float time;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in barObjects.transform)
        {
            child.gameObject.SetActive(false);
        }
        foreach (Transform child in islandObjects.transform)
        {
            child.gameObject.SetActive(false);
        }
        foreach (Transform child in alleyObjects.transform)
        {
            child.gameObject.SetActive(true);
        }

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
    public void startDisableBarObjects()
    {
        StartCoroutine(disableBarObjects());
    }

    public void startEnableIslandObjects()
    {
        StartCoroutine(enableIslandObjects());

    }

    IEnumerator disableAlleyObjects()
    {
        foreach (Transform child in alleyObjects.transform)
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
            if (child.gameObject.CompareTag("grabbable"))
            {
                MemorySoundManager m = child.gameObject.GetComponent<MemorySoundManager>();
                m.EndMemorySound();
            }
            child.gameObject.SetActive(false);
        }
        startEnableBarObjects();
    }

    IEnumerator enableBarObjects()
    {

       for(int i = barObjects.transform.childCount-1; i >= 0; i--)
{
            Transform child = barObjects.transform.GetChild(i);
            yield return new WaitForSeconds(.1f);

            child.gameObject.SetActive(true);
        }
        //foreach (Transform child in barObjects.transform)
        //{
        //    yield return new WaitForSeconds(.1f);
        
        //    child.gameObject.SetActive(true);
        //}
        yield return new WaitForSeconds(.25f);
        scene2_sound_manager.StartAmbience();
        yield return new WaitForSeconds(.75f);
        scene2_sound_manager.StartConverstaions();


    }

    IEnumerator disableBarObjects()
    {
        foreach (Transform child in barObjects.transform)
        {
            yield return new WaitForSeconds(.1f);
      
            child.gameObject.SetActive(false);
        }
        startEnableIslandObjects();
    }

    IEnumerator enableIslandObjects()
    {
        foreach (Transform child in islandObjects.transform)
        {
            yield return new WaitForSeconds(.1f);
            child.gameObject.SetActive(true);
        }


    }


    IEnumerator test()
    {
        yield return new WaitForSeconds(5);
        StartCoroutine(disableAlleyObjects());

    }
}
