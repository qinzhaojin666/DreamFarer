using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using FMODUnity;

public class Scene3SoundManager : MonoBehaviour {

    [EventRef] public string ambiencePath;
    [EventRef] public string ringPath;
    [EventRef] public string callPath;

    public Material endSky;

    public GameObject playerCamera;
    public GameObject phone;
    public GameObject phoneBooth;
    public Light sun;
    private bool callDone = false;

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
        phoneBooth.SetActive(true);
        phone.SetActive(true);
        
    }

    public void StopRing() {
        ringing = false;
        StopSoundEvent(ringInstance);
    }

    public void StartCall() {
        StartSoundEvent(callInstance);
        callDone = true;
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


    IEnumerator FadeLight(Light l, float fadeStart, float fadeEnd, float fadeTime)
    {
        float t = 0.0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;

            l.intensity = Mathf.Lerp(fadeStart, fadeEnd, t / fadeTime);
            print("yo");
            yield return null;
        }
        yield return null;

    }

    public void endScene()
    {
        StartCoroutine(FadeLight(sun, 0f, 1f, 5f));
        RenderSettings.skybox = endSky;

    }

    // Update is called once per frame
    void Update() {
        if (phoneGrabber.isGrabbed && ringing) {
            StopRing();
            StartCall();
        }

        ambienceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(playerCamera.transform));
        callInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE state);
        if (state == FMOD.Studio.PLAYBACK_STATE.STOPPED && callDone)
        {
            StopCall();
            endScene();
        }
    }
}
