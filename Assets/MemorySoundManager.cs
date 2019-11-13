using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD;
public class MemorySoundManager : MonoBehaviour {

    [EventRef] public string memoryPath;
    public OVRGrabbable grabber;
    public GameObject playerCamera;
    public float maxDistance = 1;

    // Delta = max change in parameter per second (allows for a smooth transition)
    public float delta = 0.75f;

    private FMOD.Studio.EventInstance memoryInstance;
    private readonly string grabbedDistName = "grabbedDistance";

    /*
     * Begins the memory sound.
     */
    public void BeginMemorySound() {
        memoryInstance.start();
    }

    /*
     * Stops the memory sound and releases its resources.
     */
    public void EndMemorySound() {
        memoryInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        memoryInstance.release();
    }

    // Start is called before the first frame update
    void Start() {
        memoryInstance = RuntimeManager.CreateInstance(memoryPath);
        memoryInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        BeginMemorySound();
    }

    /*
     * Given a distance between the object and listener, returns the
     *   corresponding parameter value in [0, 1]. For now, returns a simple scaling
     *   based on maxDistance, but may be swapped out for something more complex if needed.
     */
    private float getParameterFromDistance(float distance) {
        return Mathf.Min(1.0f, distance / maxDistance);
    }

    // Update is called once per frame
    void Update() {
        /*
         * GrabbedDistance behavior:
         *  - The higher the grabbedDistance is, the more dulled the sound is, and the initial loop
         *    can only be broken out of with a grabbedDistance of 0.
         *  - If the object is being grabbed, the target value is 0. Otherwise, it's based on the distance
         *    between the listener and the object.
         *  - Each frame, the grabbedDistance parameter gets closer to the target by at most delta per second.
         */

        memoryInstance.getParameterByName(grabbedDistName, out float grabbedDistance);
        float target;

        if (grabber.isGrabbed) {
            target = 0.0f;
        }
        else {
            float dist = Vector3.Distance(gameObject.transform.position, playerCamera.transform.position);
            target = getParameterFromDistance(dist);
        }
        // Move towards the target by at most delta per second
        memoryInstance.setParameterByName(grabbedDistName, Mathf.MoveTowards(grabbedDistance, target, delta * Time.deltaTime));
    }
}
