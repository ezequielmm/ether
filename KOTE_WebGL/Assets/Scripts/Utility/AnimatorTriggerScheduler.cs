using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorTriggerScheduler : MonoBehaviour
{
    public float time = 5;
    public bool repeat = false;
    public string triggerName = "";
    public Animator[] animators;
    
    private float lastTime = 0;

    // Update is called once per frame
    void Update()
    {
        if (Time.time > lastTime + time)
        {
            lastTime = Time.time;
            foreach (var a in animators)
            {
                a.SetTrigger(triggerName);
            }
            enabled = repeat;
        }    
    }
}
