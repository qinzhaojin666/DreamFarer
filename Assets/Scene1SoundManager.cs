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

    // FMOD sound event paths
    [EventRef] public string[] memoryPaths;
    [EventRef] public string ambiencePath;
    [EventRef] public string partyTransitionPath;
    [EventRef] public string fadeNoisePath;
    [EventRef] public string startingCallPath;

    // Game objects and grabbers for sound events
    public GameObject[] memoryObjects;
    public GameObject partyTransitionObject;
    public OVRGrabbable[] memoryGrabbers;
    public OVRGrabbable partyTransitionGrabber;

    // Player camera game object
    public GameObject playerCamera;

    // Max radius for memory events' area of sound
    public float maxMemoryDistance = 1;

    // FMOD sound event instances
    private FMOD.Studio.EventInstance[] memoryInstances = new FMOD.Studio.EventInstance[numMemoryObjects];
    private FMOD.Studio.EventInstance ambienceInstance;
    private FMOD.Studio.EventInstance startingCallInstance;
    public FMOD.Studio.EventInstance partyTransitionInstance;

    // Other private state variables
    private bool partyTransitionInitiated;
    private bool temp = false;
    private int currentMemory = 0;

    public void BeginAmbience() {
        ambienceInstance.start();
    }

    public void EndAmbience() {
        ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambienceInstance.release();
    }

    // Disables object only AFTER the user is done grabbing it (since disabling an
    //   object while grabbing it breaks grabbing for that hand)
    IEnumerator DisableAfterGrabEnd(int current, OVRGrabbable currentMemoryG)
    {
        while (currentMemoryG.isGrabbed) {
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        while (currentMemoryG.isGrabbed) {
            yield return null;
        }
        memoryObjects[current].SetActive(false);
    }

    public void BeginMemorySound(int i) {
        RuntimeManager.PlayOneShotAttached(fadeNoisePath, memoryObjects[i]);
        RuntimeManager.AttachInstanceToGameObject(memoryInstances[i],
                                                  memoryObjects[i].transform,
                                                  memoryObjects[i].GetComponent<Rigidbody>());
        memoryInstances[i].start();
        print("playing memory: " + i);
    }

    public void EnableNextObject()
    {
        if (currentMemory < numMemoryObjects) {
            EndMemorySound(currentMemory);
            bool beforePartyEvent = (currentMemory < numMemoryObjects);
            OVRGrabbable currentMemoryGrabber = (beforePartyEvent ? memoryGrabbers[currentMemory] : partyTransitionGrabber);
            StartCoroutine(DisableAfterGrabEnd(currentMemory, currentMemoryGrabber));

            if (currentMemory + 1 == numMemoryObjects) {
                partyTransitionObject.SetActive(true);
                BeginPartyTransitionEvent();
            }
            else {
                memoryObjects[currentMemory + 1].SetActive(true);
                BeginMemorySound(currentMemory + 1);
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

        // Initialize sound events from paths 
        for (int i = 0; i < numMemoryObjects; i++) {
            memoryInstances[i] = RuntimeManager.CreateInstance(memoryPaths[i]);
        }
        ambienceInstance = RuntimeManager.CreateInstance(ambiencePath);
        ambienceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(playerCamera));
        BeginAmbience();

        startingCallInstance = RuntimeManager.CreateInstance(startingCallPath);
        startingCallInstance.set3DAttributes(RuntimeUtils.To3DAttributes(playerCamera));

        partyTransitionInstance = RuntimeManager.CreateInstance(partyTransitionPath);
        partyTransitionInstance.set3DAttributes(RuntimeUtils.To3DAttributes(partyTransitionObject));

        BeginMemorySound(currentMemory);
        StartCoroutine(waitfor());
    }

    // Extracts a grabbedDistance parameter value from a raw distance value
    private float getParameterFromDistance(float distance) {
        return Mathf.Min(1.0f, distance / maxMemoryDistance);
    }

    IEnumerator waitfor()
    {
        yield return new WaitForSeconds(10f);
        temp = true;
        yield return new WaitForSeconds(5f);    
        temp = false;
        yield return new WaitForSeconds(10f);
        temp = true;
        yield return new WaitForSeconds(5f);
        temp = false;
        yield return new WaitForSeconds(10f);
        temp = true;
        yield return new WaitForSeconds(5f);
        temp = false;
        yield return new WaitForSeconds(10f);

    }

    // Update is called once per frame
    void Update() {
        // If photo is being grabbed, start party transition
        if (partyTransitionGrabber.isGrabbed) {
            partyTransitionInitiated = true;
        }

        // Get current memory sound event instance
        bool beforePartyEvent = (currentMemory < numMemoryObjects);
        FMOD.Studio.EventInstance currentMemoryInstance = (beforePartyEvent ?
                                                           memoryInstances[currentMemory] :
                                                           partyTransitionInstance);
        OVRGrabbable currentMemoryGrabber = (beforePartyEvent ?
                                             memoryGrabbers[currentMemory] :
                                             partyTransitionGrabber);
        Vector3 currentMemoryPosition = (beforePartyEvent ? 
                                         memoryObjects[currentMemory].transform.position :
                                         partyTransitionObject.transform.position);

        // Figure out what the parameter target is based on distance & whether object is grabbed
        currentMemoryInstance.getParameterByName(grabbedDistName, out float grabbedDistance);
        ambienceInstance.getParameterByName(grabbedDistName, out float ambientGrabbedDistance);
        float grabbedDistTarget;
        if (currentMemoryGrabber.isGrabbed || partyTransitionInitiated) {
            grabbedDistTarget = 0;
        }
        else {
            grabbedDistTarget = getParameterFromDistance(Vector3.Distance(currentMemoryPosition,
                                                                          playerCamera.transform.position));
        }
        
        // Set the ambience & memory sound's parameters based on the target value
        ambienceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(playerCamera));
        ambienceInstance.setParameterByName(grabbedDistName,
                                            Mathf.MoveTowards(ambientGrabbedDistance,
                                                              grabbedDistTarget,
                                                              delta * Time.deltaTime));
        currentMemoryInstance.setParameterByName(grabbedDistName,
                                                 Mathf.MoveTowards(grabbedDistance,
                                                                   grabbedDistTarget,
                                                                   delta * Time.deltaTime));

        // If the current memory sound event is done, move on to the next one
        currentMemoryInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE state);
        if (state == FMOD.Studio.PLAYBACK_STATE.STOPPED) {
            EnableNextObject();
        }

    }
}
