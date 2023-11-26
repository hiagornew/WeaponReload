using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomLinearDrive : MonoBehaviour
{
    [SerializeField]
    protected Transform hostTransform;
    protected Transform startTransform;
    protected Transform endTransform;
    
    protected float initialMappingOffset;

    protected float LinearMapping;

    protected virtual void SetProperties(Transform start, Transform end, Transform host = null)
    {
        startTransform = start;
        endTransform = end;
        
        hostTransform = host;
    }
    
    protected virtual void UpdateLinearMapping(Transform updateTransform)
    {
        LinearMapping = Mathf.Clamp01(initialMappingOffset + CalculateLinearMapping(updateTransform));

        if (hostTransform)
        {
            hostTransform.transform.position =
                Vector3.Lerp(startTransform.position, endTransform.position, LinearMapping);
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
