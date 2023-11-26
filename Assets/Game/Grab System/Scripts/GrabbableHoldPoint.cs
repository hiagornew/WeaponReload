using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class GrabbableHoldPoint : MonoBehaviour
{
    [Foldout("Settings", true)]
    public string handPoserName;

    public bool detachByDistance;
    [ConditionalField(nameof(detachByDistance))]
    public float distanceToDetach = 0.3f;

    [Foldout("References", true)]
    [ReadOnly]
    public GrabbableObject grabbableObject;
    public Transform holdPosition;

    [Foldout("Information", true)]
    [ReadOnly]
    public GrabBehaviour grabBehaviour;

    private void OnValidate()
    {
        if (!grabbableObject)
        {
            grabbableObject = GetComponentInParent<GrabbableObject>();
        }
    }
}
