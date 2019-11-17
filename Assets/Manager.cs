using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {

    public GameObject alleyObjects;
    public GameObject barObjects;
    public GameObject islandObjects;

    private Scene1SoundManager scene1_sound_manager;
    private Scene2SoundManager scene2_sound_manager;
    private Scene3SoundManager scene3_sound_manager;
    float time;

    // Start is called before the first frame update
    void Start() {
        scene1_sound_manager = gameObject.GetComponent<Scene1SoundManager>();
        scene2_sound_manager = gameObject.GetComponent<Scene2SoundManager>();
        scene3_sound_manager = gameObject.GetComponent<Scene3SoundManager>();

        foreach (Transform child in barObjects.transform) {
            child.gameObject.SetActive(false);
        }
        foreach (Transform child in islandObjects.transform) {
            child.gameObject.SetActive(false);
        }
        foreach (Transform child in alleyObjects.transform) {
            child.gameObject.SetActive(true);
        }

        //StartCoroutine(test());
    }

    // Update is called once per frame
    void Update() {
        time += Time.deltaTime;
    }

    public void startDisableAlleyObjects() {
        StartCoroutine(disableAlleyObjects());
    }

    public void startEnableBarObjects() {
        StartCoroutine(enableBarObjects());
    }

    public void startDisableBarObjects() {
        StartCoroutine(disableBarObjects());
    }

    public void startEnableIslandObjects() {
        StartCoroutine(enableIslandObjects());
    }

    IEnumerator disableAlleyObjects() {
        scene1_sound_manager.EndAllMemorySounds();

        foreach (Transform child in alleyObjects.transform) {
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
            child.gameObject.SetActive(false);
        }
        startEnableBarObjects();
        scene1_sound_manager.EndAmbience();
        yield return new WaitForSeconds(0.5f);
        scene1_sound_manager.EndPartyTransitionEvent();
    }

    IEnumerator enableBarObjects() {

       for(int i = barObjects.transform.childCount-1; i >= 0; i--) {
            Transform child = barObjects.transform.GetChild(i);
            yield return new WaitForSeconds(.1f);

            child.gameObject.SetActive(true);
        }
        //foreach (Transform child in barObjects.transform)
        //{
        //    yield return new WaitForSeconds(.1f);

        //    child.gameObject.SetActive(true);
        //}
        scene2_sound_manager.StartAmbience();
        yield return new WaitForSeconds(1f);
        scene2_sound_manager.StartConverstaions();

    }

    IEnumerator disableBarObjects() {
        foreach (Transform child in barObjects.transform) {
            yield return new WaitForSeconds(.1f);
      
            child.gameObject.SetActive(false);
        }
        startEnableIslandObjects();
    }

    IEnumerator enableIslandObjects() {
        foreach (Transform child in islandObjects.transform) {
            yield return new WaitForSeconds(.1f);
            child.gameObject.SetActive(true);
        }
        scene3_sound_manager.StartAmbience();
        yield return new WaitForSeconds(10f);
        // TODO: Make phone booth appear here
        scene3_sound_manager.StartRing();
    }


    IEnumerator test() {
        yield return new WaitForSeconds(5);
        StartCoroutine(disableAlleyObjects());

    }
}
