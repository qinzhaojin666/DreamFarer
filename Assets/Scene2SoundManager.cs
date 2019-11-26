using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using FMODUnity;
using UnityEngine.UI;

public class Scene2SoundManager : MonoBehaviour {

    // Private state variables
    private float time;
    private bool started;

    public Manager manager;

    // Timing for events
    private List<float> eventTimes = new List<float>() {
        0.216f,0.64f,1.226f,2.002f,2.003f,2.678f,2.976f,4.411f,4.448f,5.891f,5.891f,5.891f,6.332f,6.332f,
        6.84f, 8.342f,8.459f,8.459f, 10.213f,10.213f,10.673f,11.264f,11.724f,12.36f,12.752f,13.117f,15.586f,
        16f,16.256f,17.41f, 17.978f,18.844f,19.331f,21.049f,21.455f,21.455f,22.438f,22.438f,23.615f,25.915f,
        27.561f,27.561f,27.561f,29.175f,30.474f,30.74f, 31.75f, 33.116f,34.474f,34.744f,36.02f, 37.752f,
        38.338f,38.649f,39.231f,41.012f,41.45f, 41.892f,43.655f,44.421f,46.117f,47.005f,47.248f,48.701f,
        48.701f,50.739f,54.135f,54.689f,58.91f, 59.176f,60.231f,63.027f,65.611f,65.611f, 70f
    };
    // Event codes for events
    //   0,1,2 = toggle light of person 1,2,3 (respectively)
    //   3,4,5 = change clothing or leave for person 1,2,3 (respectively)
    //   anything negative = set phase parameter of ambience to absolute value
    private List<int> eventCodes = new List<int>() {
        0, 0, 1, 1, 2, 2, 0, 0, 1, 0, 1, 2, 0, 2, 1, 1, 0, 2, 0, 2, 2, 2, 1, 1, 3, 0, 0, -1, 1, 1, 2,
        2, 0, 0, 1, 2, 1, 2, 0, -2, 0, 3, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 2, 2, 1, 1, 5, 2, 2, 2, 2, 2, -3,
        2, 5, 1, 1, 1, 1, 4, 1, -4, 1, 4, 100
    };

    // Queue for actual event storage
    private Queue<KeyValuePair<float, int>> events;
    // State values for people (for which outfit to show)
    private int[] persons_stage;
    // Actual person game objects
    public GameObject[] people;
    // State for whether lights are toggled
    private bool[] light_bit = { false, false, false };
    // Player camera object
    public GameObject playerListener;
    // Objects for background people
    public GameObject[] peopleGroups;

    // FMOD sound event paths
    [EventRef] public string[] personSoundPaths;
    [EventRef] public string backingTrackPath;
    [EventRef] public string ambiencePath;

    // Max delta for phase parameter per second (used for smooth fades)
    public float phaseDelta = 1f;

    // Target for phase parameter (used for smooth fades)
    private float phaseTarget = 1f;

    // FMOD sound event instances
    private FMOD.Studio.EventInstance[] personSoundInstances;
    private FMOD.Studio.EventInstance backingTrackInstance;
    private FMOD.Studio.EventInstance ambienceInstance;

    // Parameter name for ambience
    private readonly string phaseParamName = "phase";
    // Number of people in scene
    private readonly int numPeople = 3;

    // Start is called before the first frame update
    void Start() {
        personSoundInstances = new FMOD.Studio.EventInstance[numPeople];
        events = new Queue<KeyValuePair<float, int>>();

        persons_stage = new int[numPeople];
        for (int i = 0; i < numPeople; i++) {
            persons_stage[i] = 0;

            personSoundInstances[i] = RuntimeManager.CreateInstance(personSoundPaths[i]);
            personSoundInstances[i].set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(people[i]));
        }

        ambienceInstance = RuntimeManager.CreateInstance(ambiencePath);
        ambienceInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(playerListener));

        backingTrackInstance = RuntimeManager.CreateInstance(backingTrackPath);
        backingTrackInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(playerListener));

        // Move all items from the timing list/code to the queue
        for (int i = 0; i < eventTimes.Count; i++) {
            events.Enqueue(new KeyValuePair<float, int>(eventTimes[i], eventCodes[i]));
        }
    }

    // Update is called once per frame
    void Update() {
        if (started && events.Count > 0) {
            time += Time.deltaTime;
            var current = events.Peek();

            if (current.Key <= time) {
                StartCoroutine(PerformAction(current.Value));
                events.Dequeue();
            }
            // Move at most phaseDelta towards the target per second
            ambienceInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(playerListener));
            ambienceInstance.getParameterByName(phaseParamName, out float currentPhase);
            ambienceInstance.setParameterByName(phaseParamName, Mathf.MoveTowards(currentPhase, phaseTarget, phaseDelta * Time.deltaTime));
        }
        
    }

    public void StartConverstaions() {
        started = true;
        time = 0f;

        backingTrackInstance.start();
        for (int i = 0; i < numPeople; i++) {
            personSoundInstances[i].start();
        }
        StartCoroutine(changeBackground());
    }


    public void StopConversations() {
        backingTrackInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        backingTrackInstance.release();
        for (int i = 0; i < numPeople; i++) {
            personSoundInstances[i].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            personSoundInstances[i].release();
        }
    }

    public void StartAmbience() {
        ambienceInstance.start();
    }

    public void StopAmbience() {
        ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambienceInstance.release();
    }

    private float getPhaseFromIntCode(int e) {
        if (e == -1) return 1.5f;
        else return (float)(-1 * e);
    }

    private void SetAmbienceParameter(float p) {
        phaseTarget = p;
    }

    private void movePersonsStage(int person_index)
    {
        if (persons_stage[person_index] == 0) {
            persons_stage[person_index]++;
            GameObject person = people[person_index];
            StartCoroutine(FadeOut3D(person.transform.GetChild(0).transform, 0, true, 2));
            person.transform.GetChild(1).gameObject.SetActive(true);

        }
        else if (persons_stage[person_index] == 1) {
            persons_stage[person_index]++;
            GameObject person = people[person_index];
            StartCoroutine(FadeOut3D(person.transform.GetChild(1).transform,0, true, 3));
        }

    }

    private IEnumerator changeBackground()
    {
        for (int i = 1; i < peopleGroups.Length; i++)
        {
            yield return new WaitForSeconds(5f);
            foreach (Transform child in peopleGroups[i-1].transform)
            {
                StartCoroutine(fadeInAndOut(child.gameObject, false, 2f));

            }
            peopleGroups[i].SetActive(true);
            foreach (Transform child in peopleGroups[i].transform)
            {
                StartCoroutine(fadeInAndOut(child.gameObject, true, 2f));
            }
        }
    }

    private IEnumerator PerformAction(int e) {
        if (e < 0) {
            // Set ambience parameter
            SetAmbienceParameter(getPhaseFromIntCode(e));
        }
        else if (e < numPeople) {
            // Toggle light of person e
            GameObject person = people[e];
            if (!light_bit[e]) {
                StartCoroutine(FadeLight(person.transform.GetChild(2).GetComponent<Light>(), 0f, 2f, 0.2f));
                light_bit[e] = !light_bit[e];
            }
            else {
                StartCoroutine(FadeLight(person.transform.GetChild(2).GetComponent<Light>(), 2f, 0f, 0.2f));
                light_bit[e] = !light_bit[e];
            }
        }
        else if (e < 2*numPeople) {
            // Advance state of person e % numPeople
            movePersonsStage(e % numPeople);
        }
        else {
            manager.startDisableBarObjects();
        }
        yield return null;
    }

    IEnumerator fadeInAndOut(GameObject objectToFade, bool fadeIn, float duration)
    {
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
    }
    IEnumerator FadeLight(Light l, float fadeStart, float fadeEnd, float fadeTime) {
        float t = 0.0f;

        while (t < fadeTime) {
            t += Time.deltaTime;

            l.intensity = Mathf.Lerp(fadeStart, fadeEnd, t / fadeTime);
            yield return null;
        }
        yield return null;
            
    }

    public IEnumerator FadeOut3D(Transform t, float targetAlpha, bool isVanish, float duration) {
        Renderer sr = t.GetComponent<Renderer>();
        float diffAlpha = (targetAlpha - sr.material.color.a);

        float counter = 0;
        while (counter < duration) {
            float alphaAmount = sr.material.color.a + (Time.deltaTime * diffAlpha) / duration;
            sr.material.color = new Color(sr.material.color.r, sr.material.color.g, sr.material.color.b, alphaAmount);

            counter += Time.deltaTime;
            yield return null;
        }
        sr.material.color = new Color(sr.material.color.r, sr.material.color.g, sr.material.color.b, targetAlpha);
        if (isVanish) {
            sr.transform.gameObject.SetActive(false);
        }
    }



}
