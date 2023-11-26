using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class DestroyAfterSecondsBehaviour : MonoBehaviour
{
    [Foldout("Settings", true)]
    [SerializeField]
    private float delayToDie = 0.5f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delayToDie);
        Destroy(gameObject);
    }
}
