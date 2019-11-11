using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD;
public class MemorySoundManager : MonoBehaviour
{
    [FMODUnity.EventRef] public string memoryPath;
    public OVRGrabbable grabber;
    public GameObject playerCamera;
    public float maxDistance = 10;

    // Delta = max change in parameter per frame (allows for a smooth transition)
    public float delta = 0.075f;

    private FMOD.Studio.EventInstance memoryInstance;
    private readonly string grabbedDistName = "grabbedDistance";

    /*
     * Begins the memory sound.
     */
    void BeginMemorySound() {
        memoryInstance.start();
    }

    /*
     * Stops the memory sound and releases its resources.
     */
    void EndMemorySound() {
        memoryInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        memoryInstance.release();
    }

    // Start is called before the first frame update
    void Start()
    {
        memoryInstance = RuntimeManager.CreateInstance(memoryPath);
        memoryInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        BeginMemorySound();
    }

    /*
     * Given a distance between the object and listener, returns the
     *   corresponding parameter value in [0, 1]. For now, returns a simple scaling
     *   based on maxDistance, but may be swapped out for something smoother.
     */
    float getParameterFromDistance(float distance) {
        return Mathf.Min(1.0f, distance / maxDistance);
    }

    // Update is called once per frame
    void Update()
    {
        /*
         * GrabbedDistance behavior:
         *  - The higher the grabbedDistance is, the more dulled the sound is, and the initial loop
         *    can only be broken out of with a grabbedDistance of 0.
         *  - If the object is currently grabbed, the parameter decreases by delta each frame until it reaches 0.
         *  - Otherwise, the parameter is set to the distance between the object and the player listener.
         */

        float grabbedDistance;
        memoryInstance.getParameterByName(grabbedDistName, out grabbedDistance);
        if (grabber.isGrabbed) {
            grabbedDistance = Mathf.MoveTowards(grabbedDistance, 0.0f, delta);
        }
        else {
            float dist = Vector3.Distance(gameObject.transform.position, playerCamera.transform.position);
            grabbedDistance = Mathf.MoveTowards(grabbedDistance, getParameterFromDistance(dist), delta);
        }
        memoryInstance.setParameterByName(grabbedDistName, grabbedDistance);
    }
}
