using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using FMODUnity;

public class Scene3SoundManager : MonoBehaviour {

    [EventRef] public string ambiencePath;
    [EventRef] public string ringPath;
    [EventRef] public string callPath;

    public GameObject playerCamera;
    public GameObject phone;

    private FMOD.Studio.EventInstance ambienceInstance;
    private FMOD.Studio.EventInstance ringInstance;
    private FMOD.Studio.EventInstance callInstance;

    public OVRGrabbable phoneGrabber;
    private bool ringing = false;

    private void StartSoundEvent(FMOD.Studio.EventInstance instance) {
        instance.start();
    }

    private void StopSoundEvent(FMOD.Studio.EventInstance instance) {
        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();
    }

    public void StartAmbience() {
        StartSoundEvent(ambienceInstance);
    }

    public void StopAmbience() {
        StopSoundEvent(ambienceInstance);
    }

    public void StartRing() {
        ringing = true;
        StartSoundEvent(ringInstance);
    }

    public void StopRing() {
        ringing = false;
        StopSoundEvent(ringInstance);
    }

    public void StartCall() {
        StartSoundEvent(callInstance);
    }
    public void StopCall() {
        StopSoundEvent(callInstance);
    }

    // Start is called before the first frame update
    void Start() {
        ambienceInstance = RuntimeManager.CreateInstance(ambiencePath);
        ringInstance = RuntimeManager.CreateInstance(ringPath);
        callInstance = RuntimeManager.CreateInstance(callPath);

        RuntimeManager.AttachInstanceToGameObject(ringInstance, phone.transform, phone.GetComponent<Rigidbody>());
        RuntimeManager.AttachInstanceToGameObject(callInstance, phone.transform, phone.GetComponent<Rigidbody>());
        ambienceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(playerCamera.transform));
    }

    // Update is called once per frame
    void Update() {
        if (phoneGrabber.isGrabbed && ringing) {
            StopRing();
            StartCall();
        }

        ambienceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(playerCamera.transform));
    }
}
