using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class DoorBehaviour : MonoBehaviour
{
    [Foldout("References", true)] 
    [SerializeField]
    private LinearDriveCustomInput linearDrive;

    [Foldout("Animation", true)] 
    [SerializeField]
    private Transform hostTransform;

    [SerializeField] private Transform startRotation;
    [SerializeField] private Transform endRotation;

    private void OnValidate()
    {
        if (!linearDrive)
        {
            linearDrive = GetComponent<LinearDriveCustomInput>();
        }
    }

    private void Update()
    {
        hostTransform.rotation =
            Quaternion.Lerp(startRotation.rotation, endRotation.rotation, linearDrive.linearMapping.value);
    }
}