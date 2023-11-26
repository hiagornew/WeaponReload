using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class InputManager : MonoBehaviour
{
    [Foldout("GrabBehaviours", true)]
    [SerializeField,ReadOnly]
    private GrabBehaviour[] grabBehaviours;
    
    [Foldout("Grab", true)]
    public GrabTypes grabGrabType = GrabTypes.Grip;
    public SteamVR_Action_Boolean grabPressing = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");

    [Foldout("Fire", true)]
    public GrabTypes fireGrabType = GrabTypes.Pinch;
    public SteamVR_Action_Boolean firePressing = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
    
    [Foldout("TouchPad", true)]
    public SteamVR_Action_Boolean dPadCenterPressing = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("DPadCenter");
    public SteamVR_Action_Vector2 trackPadPosition = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("TrackPadPosition");
    
    private List<Hand> toBeRemoved = new List<Hand>();
    
    private Dictionary<Hand,GrabTypes> pausedHands = new Dictionary<Hand,GrabTypes>();
    
    private readonly Vector2 fixedAngle = new Vector2(1, 0);

    private PressStateType trackerTriggerState;

    public enum DPadType
    {
        None,
        North,
        East,
        South,
        West,
    }

    public enum PressStateType
    {
        None,
        Hold,
        Down,
        Up,
    }

    private void OnValidate()
    {
        if (grabBehaviours == null || grabBehaviours.Length == 0)
        {
            grabBehaviours = FindObjectsOfType<GrabBehaviour>();
        }
    }

    public void SetTrackerTriggerState(int value)
    {
        switch (value)
        {
            case 0:
                trackerTriggerState = PressStateType.Up;
                break;
            case 1:
                trackerTriggerState = PressStateType.Down;
                break;
            case 2:
                trackerTriggerState = PressStateType.Hold;
                break;
            case 3:
                trackerTriggerState = PressStateType.None;
                break;
        }
    }

    public PressStateType GetTrackerTriggerState()
    {
        return trackerTriggerState;
    }

    public void PauseHand(Hand hand, GrabTypes grabType)
    {
        if (pausedHands.ContainsKey(hand))
        {
            return;
        }
        
        pausedHands.Add(hand, grabType);
    }
    
    public bool HandCanInteract(Hand hand)
    {
        return !pausedHands.ContainsKey(hand);
    }

    private void Update()
    {
        for (var i = 0; i < toBeRemoved.Count; i++)
        {
            var hand = toBeRemoved[i];

            if (!pausedHands.ContainsKey(hand))
            {
                continue;
            }

            pausedHands.Remove(hand);
        }
        
        toBeRemoved.Clear();
        
        for (var i = 0; i < pausedHands.Count; i++)
        {
            var hand = pausedHands.Keys.ElementAt(i);
            var grabType = pausedHands[hand];

            if (hand)
            {
                if (toBeRemoved.Contains(hand))
                {
                    continue;
                }

                switch (grabType)
                {
                    case GrabTypes.Pinch:
                        if (!firePressing.GetStateUp(hand.handType))
                        {
                            continue;
                        }
                        break;
                    case GrabTypes.Grip:
                        if (!grabPressing.GetStateUp(hand.handType))
                        {
                            continue;
                        }
                        break;
                }
            }
            
            toBeRemoved.Add(hand);
        }
    }

    public GrabBehaviour FindGrabBehaviourByHand(Hand hand)
    {
        for (var i = 0; i < grabBehaviours.Length; i++)
        {
            var grabBehaviour = grabBehaviours[i];

            if (!grabBehaviour.hand.Equals(hand))
            {
                continue;
            }
            
            return grabBehaviour;
        }

        return null;
    }

    public DPadType GetBestDPadPressingType(Hand hand, PressStateType pressStateType = PressStateType.Hold)
    {
        switch (pressStateType)
        {
            case PressStateType.Hold:
                if (!dPadCenterPressing.GetState(hand.handType))
                {
                    return DPadType.None;
                }
                break;
            case PressStateType.Down:
                if (!dPadCenterPressing.GetStateDown(hand.handType))
                {
                    return DPadType.None;
                }
                break;
            case PressStateType.Up:
                if (!dPadCenterPressing.GetStateUp(hand.handType))
                {
                    return DPadType.None;
                }
                break;
            default:
                return DPadType.None;
        }
        

        var pos = trackPadPosition.GetAxis(hand.handType);
        var angle = GetAngle(pos);

        if (angle < 135 && angle > 45)
        {
            return DPadType.North;
        }

        if (angle < 225 && angle > 135)
        {
            return DPadType.West;
        }

        if (angle < 315 && angle > 225)
        {
            return DPadType.South;
        }

        if (angle < 360 && angle > 315 ||
            angle < 45 && angle > 0)
        {
            return DPadType.East;
        }

        return DPadType.None;
    }
    
    private float GetAngle(Vector2 position)
    {
        var finalangle = Vector2.Angle(position, fixedAngle);

        if (position.y < 0)
        {
            finalangle = (-finalangle + 180) + 180;
        }

        return finalangle;
    }
}
