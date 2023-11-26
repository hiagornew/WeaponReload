using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Valve.VR;

public class DevicesReader : MonoBehaviour
{
    [SerializeField, ReadOnly] private TrackerBehaviour trackerBehaviour;
    private SteamVR_Events.Action newPosesAction;
    
    [SerializeField]
    private List<BaseStation> baseStations = new List<BaseStation>();

    private TrackerMath currentTrackerMath;
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (!trackerBehaviour)
        {
            trackerBehaviour = FindObjectOfType<TrackerBehaviour>();
        }
    }
    #endif

    [Serializable]
    public class BaseStation
    {
        public int Index;
        public Vector3 Origin;
        public float[] RotationMatrix;

        public BaseStation(int Index, HmdMatrix34_t data)
        {
            this.Index = Index;
            Origin = new Vector3(data.m3, data.m7, data.m11);
            RotationMatrix = new[]
            {
                data.m0, data.m1, data.m2, data.m4, data.m5, data.m6, data.m8, data.m9, data.m10
            };
        }
    }

    private DevicesReader()
    {
        newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
    }

    private void Awake()
    {
        OnEnable();
    }

    private void OnEnable()
    {
        if (!SteamVR_Render.instance)
        {
            enabled = false;
            return;
        }

        newPosesAction.enabled = true;
    }

    private void OnDisable()
    {
        newPosesAction.enabled = false;
    }

    private void OnNewPoses(TrackedDevicePose_t[] poses)
    {
        if (currentTrackerMath != null)
        {
            return;
        }

        for (var i = 0; i < poses.Length; i++)
        {
            var pose = poses[i];

            if (!pose.bDeviceIsConnected)
                continue;

            if (!pose.bPoseIsValid)
                continue;

            var deviceClass = OpenVR.System.GetTrackedDeviceClass((uint) i);

            if (deviceClass != ETrackedDeviceClass.TrackingReference)
            {
                continue;
            }

            baseStations.Add(new BaseStation(i, pose.mDeviceToAbsoluteTracking));
        }

        if (baseStations.Count < 2)
        {
            return;
        }

        currentTrackerMath = new TrackerMath(
            baseStations[0].Origin, baseStations[1].Origin,
            baseStations[0].RotationMatrix, baseStations[1].RotationMatrix);
        
        trackerBehaviour.SetTrackerMath(currentTrackerMath);
    }

    public void ResetTrackerMath()
    {
        baseStations.Clear();
        currentTrackerMath = null;
    }
}