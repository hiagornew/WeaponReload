using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class ScanHologramController : MonoBehaviour
{
    [Foldout("References", true)]
    [SerializeField]
    private new Renderer renderer;

    private static readonly int Direction = Shader.PropertyToID("_Direction");

    private void Update()
    {
        var desiredRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        var directional = new Vector4(desiredRotation.x,desiredRotation.y,desiredRotation.z,desiredRotation.w);
        renderer.material.SetVector(Direction,directional);
    }
}
