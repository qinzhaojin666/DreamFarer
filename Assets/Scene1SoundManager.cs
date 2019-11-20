using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using FMODUnity;

public class Scene1SoundManager : MonoBehaviour {

    public static readonly int numMemoryObjects = 3;
    private static readonly string grabbedDistName = "grabbedDistance";

    // Delta = max change in parameter per second (allows for a smooth transition)
    public float delta = 0.75f;

    [EventRef] public string[] memoryPaths;
    [EventRef] public string ambiencePath;
    [EventRef] public string partyTransitionPath;
    [EventRef] public string fadeNoisePath;

    public GameObject[] memoryObjects;
    public GameObject partyTransitionObject;
    public OVRGrabbable[] memoryGrabbers;
    public OVRGrabbable partyTransitionGrabber;

    public GameObject playerCamera;
    public float maxMemoryDistance = 1;

    private FMOD.Studio.EventInstance[] memoryInstances = new FMOD.Studio.EventInstance[numMemoryObjects];
    private FMOD.Studio.EventInstance ambienceInstance;
    public FMOD.Studio.EventInstance partyTransitionInstance;
    private float[] grabbedDistTargets = new float[numMemoryObjects];

    private bool partyTransitionInitiated;

    private int currentMemory = 0;

    public void BeginAmbience() {
        ambienceInstance.start();
    }

    public void EndAmbience() {
        ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambienceInstance.release();
    }

    public void BeginMemorySound(int i) {
        RuntimeManager.PlayOneShotAttached(fadeNoisePath, memoryObjects[i]);
        memoryInstances[i].start();
    }

    public void EnableNextObject()
    {
        if (currentMemory < numMemoryObjects) {
            EndMemorySound(currentMemory);
            memoryObjects[currentMemory].SetActive(false);

            if (currentMemory+1 == numMemoryObjects) {
                partyTransitionObject.SetActive(true);
                BeginPartyTransitionEvent();
            }
            else {
                memoryObjects[currentMemory+1].SetActive(true);
                BeginMemorySound(currentMemory+1);
            }
            currentMemory++;
        }
    }

    public void EndMemorySound(int i) {
        memoryInstances[i].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        memoryInstances[i].release();
    }

    public void EndAllMemorySounds() {
        for (int i = 0; i < numMemoryObjects; i++) {
            EndMemorySound(i);
        }
    }

 
    public void BeginPartyTransitionEvent() {
        RuntimeManager.PlayOneShotAttached(fadeNoisePath, partyTransitionObject);
        partyTransitionInstance.start();
    }

    public void EndPartyTransitionEvent() {
        partyTransitionInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        partyTransitionInstance.release();
    }

    // Start is called before the first frame update
    void Start() {
        partyTransitionInitiated = false;
        for (int i = 0; i < numMemoryObjects; i++) {
            memoryInstances[i] = RuntimeManager.CreateInstance(memoryPaths[i]);
            RuntimeManager.AttachInstanceToGameObject(memoryInstances[i], memoryObjects[i].transform, memoryObjects[i].GetComponent<Rigidbody>());
            grabbedDistTargets[i] = 0.5f;
        }
        ambienceInstance = RuntimeManager.CreateInstance(ambiencePath);
        ambienceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(playerCamera));
        BeginAmbience();
        partyTransitionInstance = RuntimeManager.CreateInstance(partyTransitionPath);
        partyTransitionInstance.set3DAttributes(RuntimeUtils.To3DAttributes(partyTransitionObject));

        BeginMemorySound(currentMemory);
    }

    private float getParameterFromDistance(float distance) {
        return Mathf.Min(1.0f, distance / maxMemoryDistance);
    }

    // Update is called once per frame
    void Update() {
        if (partyTransitionGrabber.isGrabbed) {
            partyTransitionInitiated = true;
        }

        bool beforePartyEvent = (currentMemory < numMemoryObjects);
        FMOD.Studio.EventInstance currentMemoryInstance = (beforePartyEvent ? memoryInstances[currentMemory] : partyTransitionInstance);
        OVRGrabbable currentMemoryGrabber = (beforePartyEvent ? memoryGrabbers[currentMemory] : partyTransitionGrabber);
        Vector3 currentMemoryPosition = (beforePartyEvent ? memoryObjects[currentMemory].transform.position : partyTransitionObject.transform.position);

        currentMemoryInstance.getParameterByName(grabbedDistName, out float grabbedDistance);
        ambienceInstance.getParameterByName(grabbedDistName, out float ambientGrabbedDistance);
        float grabbedDistTarget;
        if (currentMemoryGrabber.isGrabbed || partyTransitionInitiated) {
            grabbedDistTarget = 0;
        }
        else {
            grabbedDistTarget = getParameterFromDistance(Vector3.Distance(currentMemoryPosition, playerCamera.transform.position));
        }

        ambienceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(playerCamera));
        ambienceInstance.setParameterByName(grabbedDistName, Mathf.MoveTowards(ambientGrabbedDistance, grabbedDistTarget, delta * Time.deltaTime));
        currentMemoryInstance.setParameterByName(grabbedDistName, Mathf.MoveTowards(grabbedDistance, grabbedDistTarget, delta * Time.deltaTime));

        currentMemoryInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE state);
        if (state == FMOD.Studio.PLAYBACK_STATE.STOPPED) {
            EnableNextObject();
        }

    }
}
