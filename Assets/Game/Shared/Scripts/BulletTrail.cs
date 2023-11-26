using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    public static BulletTrail instance;

    [SerializeField] private LineRenderer trail;

    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnValidate()
    {
        if(!trail)
        {
            trail = GetComponent<LineRenderer>();
        }
    }

    public void SetTrailInitialPosition(Vector3 initialPos)
    {
        trail.enabled = false;
        trail.SetPosition(0, initialPos);
    }

    public void SetTrailFinalPosition(Vector3 finalPos)
    {
        trail.enabled = true;
        trail.SetPosition(1, finalPos);
    }
}
