using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using MyBox;
using UnityEngine;

public class TrackerBehaviour : MonoBehaviour
{
    [Foldout("Settings", true)]
    [Header("Interpolation")]
    [SerializeField]
    private bool interpolation;
    public double interpolationDelay = 0.1;
    
    [Header("Offset")]
    [SerializeField]
    private Vector3 offSetPosition = new Vector3(-0.2f, 0, -0.1f);

    [SerializeField]
    private Vector3 offSetPositionInverted = new Vector3(0.1f, 0, 0.2f);

    [SerializeField]
    private Vector3 offsetRotation = new Vector3(0, 180, 0);

    [Header("Meshes")]
    [SerializeField]
    private bool showGun;

    [SerializeField]
    private bool showAxis;

    [SerializeField]
    private bool showBoard;

    [Foldout("References", true)]
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private GameObject axisMesh;

    [SerializeField]
    private GameObject gunMesh;

    [SerializeField]
    private GameObject boardMesh;

    [SerializeField]
    private Transform hostTransform;
    
    private TrackerMath trackerMath;
    private float[] angles = new float[4];
    
    private JiterringFilter jiterringFilter = new JiterringFilter();
    private Vector3 dataPosition;
    private Quaternion dataQuaternion;
    
    private Queue<int> triggerQueue = new Queue<int>();
    private int lastTriggerValue = -1;
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (!inputManager)
        {
            inputManager = FindObjectOfType<InputManager>();
        }
    }
    #endif

    public void SetTrackerMath(TrackerMath trackerMath)
    {
        this.trackerMath = trackerMath;
    }

    public void ReceivedData(string data)
    {
        if (trackerMath == null)
        {
            return;
        }
        
        var splitedData = data.Split(' ');
        for (var i = 0; i < splitedData.Length; i++)
        {
            var singleData = splitedData[i];

            var splitedSingleData = singleData.Split(':');

            var key = splitedSingleData[0];

            var sValue = splitedSingleData[1].Replace('.', ',');
            var fValue = float.Parse(sValue);

            switch (key)
            {
                case "b":
                    angles[0] = fValue;
                    break;
                case "B":
                    angles[1] = fValue;
                    break;
                case "c":
                    angles[2] = fValue;
                    break;
                case "C":
                    angles[3] = fValue;
                    break;
                case "x":
                    dataQuaternion.x = fValue;
                    break;
                case "y":
                    dataQuaternion.y = fValue;
                    break;
                case "z":
                    dataQuaternion.z = fValue;
                    break;
                case "w":
                    dataQuaternion.w = fValue;
                    break;
                case "t":
                    int.TryParse(sValue, out var iValue);

                    triggerQueue.Enqueue(iValue);
                    break;
            }
        }

        dataPosition = trackerMath.RealizarCalculos(angles[0], angles[1], angles[2], angles[3]);
        dataPosition.z = -dataPosition.z;
        dataPosition += trackerMath.IsFlipped ? offSetPositionInverted : offSetPosition;
    }

    private void Update()
    {
        UpdateHostTransform();
        UpdateTriggerValues();
        UpdateMeshes();
    }

    private void UpdateHostTransform()
    {
        //Position
        hostTransform.localPosition = interpolation ? jiterringFilter.SyncMovment(dataPosition, interpolationDelay) : dataPosition;

        //Rotation
        var euler = (Quaternion.Inverse(dataQuaternion) * Quaternion.Euler(offsetRotation)).eulerAngles;
        
        var aux = euler.x;
        euler.x = euler.z;
        euler.z = -aux;
        
        hostTransform.localRotation = Quaternion.Euler(euler);
    }

    private void UpdateMeshes()
    {
        gunMesh.SetActive(showGun);
        axisMesh.SetActive(showAxis);
        boardMesh.SetActive(showBoard);
    }

    private void UpdateTriggerValues()
    {
        if (triggerQueue.Count > 0)
        {
            var nextTriggerValue = triggerQueue.Dequeue();

            inputManager.SetTrackerTriggerState(nextTriggerValue);

            lastTriggerValue = nextTriggerValue;
            return;
        }

        if (lastTriggerValue == 1)
        {
            inputManager.SetTrackerTriggerState(2);
            return;
        }

        inputManager.SetTrackerTriggerState(3);
        lastTriggerValue = -1;
    }
}