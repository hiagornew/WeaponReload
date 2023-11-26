using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MyBox;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using Debug = UnityEngine.Debug;

public class TrackerSceneController : MonoBehaviour
{
    [Foldout("References", true)]
    [SerializeField]
    private GameObject[] enableThings;
    
    [SerializeField]
    private GameObject[] enableNormalThings;

    public void Initialize(bool trackerTest)
    {
        Debug.Log(trackerTest);
        
        if (!trackerTest)
        {
            for (int i = 0; i < enableNormalThings.Length; i++)
            {
                enableNormalThings[i].SetActive(true);
                
            }
            return;
        }

        for (int i = 0; i < enableThings.Length; i++)
        {
            enableThings[i].SetActive(true);
        }
    }
}