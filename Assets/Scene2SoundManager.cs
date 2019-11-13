using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using FMODUnity;


public class Scene2SoundManager : MonoBehaviour
{
    float time;
    bool started;
    //float[] timings;
    //float[] events;
    //Queue<float> timings_Q = new Queue<float>();
    //Queue<float> events_Q = new Queue<float>();
    //OrderedDictionary events;
    public Queue<KeyValuePair<float, int>> events;
    private int[] persons_stage;
    public GameObject[] people;
    public GameObject playerListener;

    [EventRef] public string[] personSoundPaths;
    [EventRef] public string backingTrackPath;
    [EventRef] public string ambiencePath;

    // Max delta for phase parameter per second (used for smooth fades)
    public float phaseDelta = 1f;

    // Target for phase parameter (used for smooth fades)
    private float phaseTarget = 1f;

    private FMOD.Studio.EventInstance[] personSoundInstances;
    private FMOD.Studio.EventInstance backingTrackInstance;
    private FMOD.Studio.EventInstance ambienceInstance;

    // Parameter name for ambience
    private readonly string phaseParamName = "phase";
    // Number of people in scene
    private readonly int numPeople = 3;

    // Start is called before the first frame update
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update() {
        if (started) {
            time += Time.deltaTime;
            var current = events.Peek();
            if (current.Key >= time) {
                StartCoroutine(PerformAction(current.Value));
                events.Dequeue();
            }
            // Move at most phaseDelta towards the target per second
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
    }

    public void StartAmbience() {
        ambienceInstance.start();
    }


    public void StopConversations() {
        backingTrackInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        backingTrackInstance.release();
        for (int i = 0; i < numPeople; i++) {
            personSoundInstances[i].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            personSoundInstances[i].release();
        }
    }

    public void StopAmbience() {
        ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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
        if (persons_stage[person_index] == 0)
        {
            persons_stage[person_index]++;
            GameObject person = people[person_index];
            person.transform.GetChild(0).gameObject.SetActive(false);
            person.transform.GetChild(1).gameObject.SetActive(true);

        }
        else if (persons_stage[person_index] == 1)
        {
            persons_stage[person_index]++;
            GameObject person = people[person_index];
            person.transform.GetChild(1).gameObject.SetActive(false);
        }

    }

    private IEnumerator PerformAction(int e) {
        if (e < 0) {
            // this is the parameter case
            SetAmbienceParameter(getPhaseFromIntCode(e));
        }
        else if (e < numPeople) {
            // this is the toggle light case
            GameObject person = people[e];
            //foreach (Transform child in person.transform)
            //{
            //    if (child.tag == "light")

            //}
            person.transform.GetChild(2).gameObject.SetActive(!person.transform.GetChild(2).gameObject.activeInHierarchy);
        }
        else if (e < 2*numPeople) {
            // this is the change/leave stage
            //GameObject person = people[e % numPeople];
            movePersonsStage(e % numPeople);

            // go to the next stage
        }
        yield return null;

    }



}
