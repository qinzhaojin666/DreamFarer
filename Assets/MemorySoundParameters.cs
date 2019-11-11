using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD;
public class MemorySoundParameters : MonoBehaviour
{
    [FMODUnity.EventRef] public string memoryPath;
    public OVRGrabbable grabber;
    public GameObject playerCamera;
    public float maxDistance = 10;

    private FMOD.Studio.EventInstance memoryInstance;
    private readonly string grabbedDistName = "grabbedDistance";

    /*
     * Begins the memory sound.
     */
    void BeginSound() {
        memoryInstance.start();
    }

    /*
     * Stops the memory sound and releases its resources.
     */
    void EndSound() {
        memoryInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        memoryInstance.release();
    }

    // Start is called before the first frame update
    void Start()
    {
        memoryInstance = RuntimeManager.CreateInstance(memoryPath);
        memoryInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        BeginSound();
    }

    /*
     * Given a distance between the object and listener, returns the
     *   corresponding parameter value. For now, returns a simple scaling
     *   based on maxDistance, but will be swapped out for something smoother.
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
         *  - If the object is currently grabbed, the parameter is automatically set to 0.
         *  - Otherwise, the parameter is set to the distance between the object and the player listener.
         */

        float grabbedDistance;
        if (grabber.isGrabbed) {
            grabbedDistance = 0;
        }
        else {
            float dist = Vector3.Distance(gameObject.transform.position, playerCamera.transform.position);
            grabbedDistance = getParameterFromDistance(dist);
        }
        memoryInstance.setParameterByName(grabbedDistName, grabbedDistance);
    }
}
