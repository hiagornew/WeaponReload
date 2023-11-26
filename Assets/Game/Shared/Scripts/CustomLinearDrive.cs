using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public abstract class CustomLinearDrive : MonoBehaviour
{
    [SerializeField]
    protected LinearMapping linearMapping;

    protected Transform hostTransform;
    protected Transform startTransform;
    protected Transform endTransform;
    
    protected float initialMappingOffset;

    protected virtual void OnValidate()
    {
        if (!linearMapping)
        {
            linearMapping = GetComponent<LinearMapping>();

            if (!linearMapping)
            {
                linearMapping = gameObject.AddComponent<LinearMapping>();
            }
        }
    }

    protected virtual void SetProperties(Transform start, Transform end, Transform host = null)
    {
        startTransform = start;
        endTransform = end;
        
        hostTransform = host;
    }
    
    protected virtual void UpdateLinearMapping(Transform updateTransform)
    {
        linearMapping.value = Mathf.Clamp01(initialMappingOffset + CalculateLinearMapping(updateTransform));

        if (hostTransform)
        {
            hostTransform.transform.position =
                Vector3.Lerp(startTransform.position, endTransform.position, linearMapping.value);
        }
    }

    protected virtual float CalculateLinearMapping(Transform updateTransform)
    {
        var direction = endTransform.position - startTransform.position;
        var length = direction.magnitude;
        direction.Normalize();

        var displacement = updateTransform.position - startTransform.position;

        return Vector3.Dot(displacement, direction) / length;
    }
}
