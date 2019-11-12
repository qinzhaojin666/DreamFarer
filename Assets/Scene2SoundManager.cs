using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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




    // Start is called before the first frame update
    void Start()
    {
        persons_stage = new int[3];
        
    }

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            time += Time.deltaTime;
            var current = events.Peek();
            if (current.Key >= time)
            {
                StartCoroutine(PerformAction(current.Value));
                events.Dequeue();
            }

        }
        
    }

    public void startConverstaions()
    {
        started = true;
        time = 0f;

    }

    public void startAmbience()
    {

    }

   private IEnumerator PerformAction(int e)
    {
        if (e < 0)
        {
            // this is the parameter case
        } else if (e < 3)
        {
            // this is the toggle light case
            GameObject person = people[e];
            
        } else if (e < 6)
        {
            // this is the change/leave stage
        }
        yield return null;

    }



}
