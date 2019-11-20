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

    public void BeginAmbience() {
        ambienceInstance.start();
    }

    public void EndAmbience() {
        ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambienceInstance.release();
    }

    public void BeginMemorySound(int i) {
        memoryInstances[i].start();



    }

    public void enableNextObject(int i)
    {
         i = i + 1;
        if (i == 3)
        {
            partyTransitionGrabber.gameObject.SetActive(true);
        } else
        {
            memoryObjects[i].SetActive(true);
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
            BeginMemorySound(i);
            grabbedDistTargets[i] = 0.5f;
        }
        ambienceInstance = RuntimeManager.CreateInstance(ambiencePath);
        ambienceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(playerCamera));
        BeginAmbience();
        partyTransitionInstance = RuntimeManager.CreateInstance(partyTransitionPath);
        partyTransitionInstance.set3DAttributes(RuntimeUtils.To3DAttributes(partyTransitionObject));
        BeginPartyTransitionEvent();
    }

    private float getParameterFromDistance(float distance) {
        return Mathf.Min(1.0f, distance / maxMemoryDistance);
    }

    // Update is called once per frame
    void Update() {
        //if (active) {
            if (partyTransitionGrabber.isGrabbed) {
                partyTransitionInitiated = true;
            }

            /*
             * GrabbedDistance behavior:
             *  - The higher the grabbedDistance is, the more dulled the sound is, and the initial loop
             *    can only be broken out of with a grabbedDistance of 0.
             *  - If the object is being grabbed, the target value is 0. Otherwise, it's based on the distance
             *    between the listener and the object.
             *  - Each frame, the grabbedDistance parameter gets closer to the target by at most delta per second.
             */
            float ambientTarget = 1.0f;
            float partyTarget = 1.0f;

            for (int i = 0; i < numMemoryObjects; i++) {
                memoryInstances[i].getParameterByName(grabbedDistName, out float grabbedDistance);

                if (partyTransitionInitiated) {
                    grabbedDistTargets[i] = 1;
                    ambientTarget = 0;
                    partyTarget = 0;
                }
                else {
                    if (memoryGrabbers[i].isGrabbed) {
                        grabbedDistTargets[i] = 0.0f;
                    }
                    else {
                        float dist = Vector3.Distance(memoryObjects[i].transform.position, playerCamera.transform.position);
                        grabbedDistTargets[i] = getParameterFromDistance(dist);
                    }
                    // Move towards the target by at most delta per second
                    ambientTarget = Mathf.Min(grabbedDistTargets[i], ambientTarget);

                    float partyDist = Vector3.Distance(partyTransitionObject.transform.position, playerCamera.transform.position);
                    partyTarget = getParameterFromDistance(partyDist);
                    ambientTarget = Mathf.Min(ambientTarget, partyTarget);
                }

                memoryInstances[i].setParameterByName(grabbedDistName, Mathf.MoveTowards(grabbedDistance, grabbedDistTargets[i], delta * Time.deltaTime));
            }

            // Set party transition target
            partyTransitionInstance.getParameterByName(grabbedDistName, out float partyGrabbedDist);
            partyTransitionInstance.setParameterByName(grabbedDistName, Mathf.MoveTowards(partyGrabbedDist, partyTarget, delta * Time.deltaTime));
            partyTransitionInstance.set3DAttributes(RuntimeUtils.To3DAttributes(partyTransitionObject));

            // Set ambient noise target to the min grabbedDist found
            ambienceInstance.getParameterByName(grabbedDistName, out float ambientGrabbedDist);
            ambienceInstance.setParameterByName(grabbedDistName, Mathf.MoveTowards(ambientGrabbedDist, ambientTarget, delta * Time.deltaTime));
            ambienceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(playerCamera));
        //}
    }
}
