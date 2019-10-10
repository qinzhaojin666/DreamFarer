using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD;

public class DistortionController : MonoBehaviour
{
    [FMODUnity.EventRef] public string trafficPath = "event:/Traffic";
    public FMOD.Studio.EventInstance trafficInstance;
    private bool usable = true;

    // Start is called before the first frame update
    void Start()
    {
        trafficInstance = RuntimeManager.CreateInstance(trafficPath);
        trafficInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        trafficInstance.start();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && usable) {
            trafficInstance.release();
            usable = false;
        }
        else if (Input.GetKeyDown(KeyCode.L)) {
            trafficInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        if (usable) {
            float currentTrafficVal;
            trafficInstance.getParameterByName("x", out currentTrafficVal);
            float newTrafficVal = currentTrafficVal + 0.05f;
            while (newTrafficVal >= 25.0f) {
                newTrafficVal -= 25.0f;
            }
            trafficInstance.setParameterByName("x", newTrafficVal);
        }
    }
}
