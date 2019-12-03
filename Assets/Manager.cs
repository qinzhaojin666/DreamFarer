using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour {

    // Game object sets for each scene
    public GameObject alleyObjects;
    public GameObject barObjects;
    public GameObject islandObjects;

    // Sound events managers for each scene
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
            if (!child.gameObject.CompareTag("dontEnable")) {
                child.gameObject.SetActive(true);
            }
            else {
                child.gameObject.SetActive(false);
            }
        }
        // Uncomment this line to start the test routine
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
            
            if (!child.gameObject.CompareTag("dontEnable")) {
                StartCoroutine(fadeInAndOut(child.gameObject, false, 2f, true));
            }
            else if (child.gameObject.activeInHierarchy) {
                OVRGrabbable g = child.GetComponent<OVRGrabbable>();
                if (g.isGrabbed) {
                    StartCoroutine(disableAfterGrabEnd(child, g));
                }
                else {
                    child.gameObject.SetActive(false);
                }
            }
        }
        startEnableBarObjects();
        scene1_sound_manager.EndAmbience();
        yield return new WaitForSeconds(0.5f);
        scene1_sound_manager.EndPartyTransitionEvent();
    }

    IEnumerator disableAfterGrabEnd(Transform current, OVRGrabbable currentMemoryG) {
        while (currentMemoryG.isGrabbed) {
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        current.gameObject.SetActive(false);
    }

    IEnumerator enableBarObjects() {

        for(int i = barObjects.transform.childCount-1; i >= 0; i--) {
            Transform child = barObjects.transform.GetChild(i);
            yield return new WaitForSeconds(.01f);

            if (!child.gameObject.CompareTag("dontEnable")) {
                child.gameObject.SetActive(true);
            }
        }
        
        scene2_sound_manager.StartAmbience();
        yield return new WaitForSeconds(3f);
        scene2_sound_manager.StartConverstaions();
    }

    IEnumerator disableBarObjects() {
        foreach (Transform child in barObjects.transform) {
            yield return new WaitForSeconds(.01f);
            StartCoroutine(fadeInAndOut(child.gameObject, false, 2f, true));
        }
        startEnableIslandObjects();
    }

    IEnumerator enableIslandObjects() {
        foreach (Transform child in islandObjects.transform) {
            yield return new WaitForSeconds(.1f);
            if (!child.gameObject.CompareTag("dontEnable")) {
                child.gameObject.SetActive(true);
            }
        }
        scene3_sound_manager.StartAmbience();
        yield return new WaitForSeconds(10f);
        scene3_sound_manager.StartRing();
    }


    IEnumerator test() {
        yield return new WaitForSeconds(5);
        StartCoroutine(disableAlleyObjects());
    }


    IEnumerator fadeInAndOut(GameObject objectToFade, bool fadeIn, float duration, bool isVanish) {
        float counter = 0f;

        //Set Values depending on if fadeIn or fadeOut
        float a, b;
        if (fadeIn) {
            a = 0;
            b = 1;
        }
        else {
            a = 1;
            b = 0;
        }

        int mode = 0;
        Color currentColor = Color.clear;

        SpriteRenderer tempSPRenderer = objectToFade.GetComponent<SpriteRenderer>();
        Image tempImage = objectToFade.GetComponent<Image>();
        RawImage tempRawImage = objectToFade.GetComponent<RawImage>();
        MeshRenderer tempRenderer = objectToFade.GetComponent<MeshRenderer>();
        Text tempText = objectToFade.GetComponent<Text>();

        //Check if this is a Sprite
        if (tempSPRenderer != null) {
            currentColor = tempSPRenderer.color;
            mode = 0;
        }
        //Check if Image
        else if (tempImage != null) {
            currentColor = tempImage.color;
            mode = 1;
        }
        //Check if RawImage
        else if (tempRawImage != null) {
            currentColor = tempRawImage.color;
            mode = 2;
        }
        //Check if Text 
        else if (tempText != null) {
            currentColor = tempText.color;
            mode = 3;
        }

        //Check if 3D Object
        else if (tempRenderer != null) {
            currentColor = tempRenderer.material.color;
            mode = 4;

            //ENABLE FADE Mode on the material if not done already
            tempRenderer.material.SetFloat("_Mode", 2);
            tempRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            tempRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            tempRenderer.material.SetInt("_ZWrite", 0);
            tempRenderer.material.DisableKeyword("_ALPHATEST_ON");
            tempRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            tempRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            tempRenderer.material.renderQueue = 3000;
        }
        else {
            if (isVanish) {
                objectToFade.transform.gameObject.SetActive(false);
            }
            yield break;
        }

        while (counter < duration) {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(a, b, counter / duration);

            switch (mode) {
                case 0:
                    tempSPRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 1:
                    tempImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 2:
                    tempRawImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 3:
                    tempText.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 4:
                    tempRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
            }
           
            yield return null;
        }
        if (isVanish) {
            objectToFade.transform.gameObject.SetActive(false);
        }
    }
}
