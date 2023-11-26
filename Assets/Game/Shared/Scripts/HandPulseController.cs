using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class HandPulseController : MonoBehaviour
{
    [Foldout("References", true)]
    // [SerializeField]
    // private Hand hand;
    
    private Coroutine pulseDuration;
    private Coroutine pulseLoop;

    // private IEnumerator PulseLoop()
    // {
    //     var wait = new WaitForSeconds(0.004f);
    //     while (true)
    //     {
    //         hand.TriggerHapticPulse(3999);
    //
    //         yield return wait;
    //     }
    // }

    public void EndPulseLoop()
    {
        if (pulseLoop == null)
        {
            return;
        }
        
        StopCoroutine(pulseLoop);
        pulseLoop = null;
    }

    public void BeginPulseLoop(float duration)
    {
        EndPulseLoop();

        if (duration > 0)
        {
            if (pulseDuration != null)
            {
                StopCoroutine(pulseDuration);
                pulseDuration = null;
            }

            pulseDuration = StartCoroutine(ByTime(duration));
        }

        // pulseLoop = StartCoroutine(PulseLoop());
    }

    private IEnumerator ByTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        EndPulseLoop();
    }
}
