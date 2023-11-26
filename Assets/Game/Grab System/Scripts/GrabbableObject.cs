using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    [Foldout("References", true)]
    public GrabbableHoldPoint[] grabbableHoldPoints;

    [Foldout("Component References", true)]
    [ReadOnly]
    public new Rigidbody rigidbody;
    [SerializeField, ReadOnly]
    protected Collider[] physicsColliders;
    [SerializeField, ReadOnly]
    protected Collider[] triggerColliders;

    [Foldout("Information", true)]
    [ReadOnly]
    public GrabBehaviour firstGrabBehaviour;
    [ReadOnly]
    public GrabBehaviour secondGrabBehaviour;
    // [ReadOnly]
    // public IgnoreHovering ignoreHovering;
    [ReadOnly]
    public GrabbableHoldPoint firstHandHoldPoint;
    [ReadOnly]
    public GrabbableHoldPoint secondHandHoldPoint;
    // [ReadOnly]
    // public List<Hand> holdingHands = new List<Hand>();

#if UNITY_EDITOR
    [ButtonMethod]
    protected virtual string RefreshComponents()
    {
        rigidbody = null;
        physicsColliders = new Collider[0];
        triggerColliders = new Collider[0];
        grabbableHoldPoints = new GrabbableHoldPoint[0];

        EditorUtility.SetDirty(this);

        return "References updated";
    }
#endif

    // public virtual void Grab(GrabBehaviour grabBehaviour)
    // {
    //     AddIgnoreHovering();
    //
    //     firstGrabBehaviour = grabBehaviour;
    //     
    //     throwable.Attach(hand);
    // }

    // public virtual void Grab(Hand hand, GrabbableHoldPoint grabbableHoldPoint, bool secondHand)
    // {
    //     if (!secondHand)
    //     {
    //         firstHandHoldPoint = grabbableHoldPoint;
    //
    //         firstGrabBehaviour = firstHandHoldPoint.grabBehaviour;
    //         SetSkeletonPoser(firstGrabBehaviour.hand, throwable.interactable.skeletonPoser, firstHandHoldPoint.handPoserName, true);
    //         
    //         throwable.Attach(hand);
    //         return;
    //     }
    //     
    //     secondHandHoldPoint = grabbableHoldPoint;
    //
    //     secondGrabBehaviour = secondHandHoldPoint.grabBehaviour;
    //     SetSkeletonPoser(secondGrabBehaviour.hand, throwable.secondHandInteractable.skeletonPoser, secondHandHoldPoint.handPoserName, true);
    //     
    //     throwable.Attach(hand,throwable.secondHandInteractable.gameObject);
    // }

    // public virtual void Detach(Hand hand)
    // {
    //     RemoveIgnoreHovering();
    //     
    //     RemoveHand(hand);
    //     throwable.Detach(hand,hand.currentAttachedObject);
    //     throwable.ApplyDetachVelocities(hand);
    //
    //     firstGrabBehaviour.grabingStep = GrabBehaviour.GrabingStep.Waiting;
    //     firstGrabBehaviour.currentGrabbedObject = null;
    //     firstGrabBehaviour = null;
    //
    //     SwitchHands();
    // }

    // public virtual void Detach(Hand hand, GrabbableHoldPoint grabbableHoldPoint)
    // {
    //     var swapOneHanded = holdingHands.Count > 1;
    //     GrabBehaviour grabBehaviour = null;
    //     GrabbableHoldPoint holdPoint = null;
    //
    //     if (swapOneHanded)
    //     {
    //         throwable.PhysicsDetach();
    //         
    //         grabBehaviour = grabbableHoldPoint == firstHandHoldPoint ? secondGrabBehaviour : firstGrabBehaviour;
    //         holdPoint = grabbableHoldPoint == firstHandHoldPoint ? secondHandHoldPoint : firstHandHoldPoint;
    //     }
    //     
    //     RemoveHand(hand);
    //     throwable.Detach(hand,hand.currentAttachedObject);
    //     
    //     if (firstGrabBehaviour)
    //     {
    //         SetSkeletonPoser(firstGrabBehaviour.hand,throwable.interactable.skeletonPoser, firstHandHoldPoint.handPoserName, false);
    //                                   
    //         firstGrabBehaviour.grabingStep = GrabBehaviour.GrabingStep.Waiting;
    //         firstGrabBehaviour.currentGrabbedObject = null;
    //         firstGrabBehaviour = null;
    //                                               
    //         firstHandHoldPoint.grabBehaviour = null;
    //         firstHandHoldPoint = null;
    //     }
    //     
    //     if (secondGrabBehaviour)
    //     {
    //         SetSkeletonPoser(secondGrabBehaviour.hand,throwable.secondHandInteractable.skeletonPoser, secondHandHoldPoint.handPoserName, false);
    //
    //         secondGrabBehaviour.grabingStep = GrabBehaviour.GrabingStep.Waiting;
    //         secondGrabBehaviour.currentGrabbedObject = null;
    //         secondGrabBehaviour = null;
    //         
    //         secondHandHoldPoint.grabBehaviour = null;
    //         secondHandHoldPoint = null;
    //     }
    //
    //     if (!swapOneHanded)
    //     {
    //         return;
    //     }
    //
    //     grabBehaviour.GrabObject(holdPoint);
    // }

    // private bool CanSwitchHands(SecondHandBehaviour secondHandBehaviour)
    // {
    //     if (!secondHandBehaviour)
    //     {
    //         return false;
    //     }
    //     
    //     if (secondHandBehaviour.holdingHands.Count == 0)
    //     {
    //         return false;
    //     }
    //
    //     return true;
    // }

    // private void SwitchHands()
    // {
    //     var secondHandBehaviour = GetComponentInChildren<SecondHandBehaviour>();
    //
    //     if (!CanSwitchHands(secondHandBehaviour))
    //     {
    //         return;
    //     }
    //
    //     var otherGrabBehaviour = secondHandBehaviour.currentGrabBehaviour;
    //
    //     secondHandBehaviour.Detach(secondHandBehaviour.holdingHands[0]);
    //
    //     otherGrabBehaviour.GrabObject(this);
    // }

    // protected virtual void HandHoverUpdate(Hand hand)
    // {
    //     if (holdingHands.Contains(hand))
    //     {
    //         return;
    //     }
    //
    //     if (!inputManager.HandCanInteract(hand))
    //     {
    //         return;
    //     }
    //
    //     var startingGrabType = hand.GetBestGrabbingType(inputManager.grabGrabType, true);
    //
    //     if (startingGrabType == GrabTypes.None)
    //     {
    //         return;
    //     }
    //
    //     if (throwable.holdType == HoldType.TwoHanded)
    //     {
    //         var grabBehaviour = inputManager.FindGrabBehaviourByHand(hand);
    //
    //         var holdPoint = FindNearestHoldPoint(grabBehaviour);
    //
    //         if (!holdPoint)
    //         {
    //             return;
    //         }
    //
    //         if (holdPoint.grabBehaviour)
    //         {
    //             return;
    //         }
    //
    //         inputManager.PauseHand(hand, inputManager.grabGrabType);
    //
    //         grabBehaviour.GrabObject(holdPoint);
    //     }
    //     else
    //     {
    //         if (firstGrabBehaviour)
    //         {
    //             return;
    //         }
    //
    //         firstGrabBehaviour = inputManager.FindGrabBehaviourByHand(hand);
    //
    //         firstGrabBehaviour.GrabObject(this);
    //     }
    // }

    private GrabbableHoldPoint FindNearestHoldPoint(GrabBehaviour grabBehaviour)
    {
        GrabbableHoldPoint closest = null;
        var closestDistance = float.MaxValue;

        for (var i = 0; i < grabbableHoldPoints.Length; i++)
        {
            var holdPoint = grabbableHoldPoints[i];
            var distance = Vector3.Distance(grabBehaviour.transform.position, holdPoint.transform.position);

            if (distance > closestDistance)
            {
                continue;
            }

            closestDistance = distance;
            closest = holdPoint;
        }

        return closest;
    }

    // protected virtual void HandAttachedUpdate(Hand hand)
    // {
    //     if (holdingHands.Count == 0)
    //     {
    //         return;
    //     }
    //
    //     var grabArray = GetActiveGrabBehaviours();
    //
    //     for (var i = 0; i < grabArray.Length; i++)
    //     {
    //         var grabBehaviour = grabArray[i];
    //
    //         if (throwable.holdType == HoldType.TwoHanded)
    //         {
    //             grabBehaviour.GrabEnding(firstGrabBehaviour == grabBehaviour
    //                 ? firstHandHoldPoint
    //                 : secondHandHoldPoint);
    //             continue;
    //         }
    //
    //         grabBehaviour.GrabEnding();
    //     }
    // }

    public GrabBehaviour[] GetActiveGrabBehaviours()
    {
        var list = new List<GrabBehaviour>();

        if (firstGrabBehaviour)
        {
            list.Add(firstGrabBehaviour);
        }

        if (secondGrabBehaviour)
        {
            list.Add(secondGrabBehaviour);
        }

        return list.ToArray();
    }

    // public void AddHand(Hand hand)
    // {
    //     if (holdingHands.Contains(hand))
    //     {
    //         return;
    //     }
    //
    //     holdingHands.Add(hand);
    // }
    //
    // public void RemoveHand(Hand hand)
    // {
    //     if (!holdingHands.Contains(hand))
    //     {
    //         return;
    //     }
    //
    //     holdingHands.Remove(hand);
    // }

    public virtual void SetColliders(bool value, bool trigger = false)
    {
        var colliders = trigger ? triggerColliders : physicsColliders;

        for (var i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = value;
        }
    }

    // public virtual void AddIgnoreHovering()
    // {
    //     if (ignoreHovering)
    //     {
    //         return;
    //     }
    //
    //     ignoreHovering = gameObject.AddComponent<IgnoreHovering>();
    // }
    //
    // public virtual void RemoveIgnoreHovering()
    // {
    //     if (!ignoreHovering)
    //     {
    //         return;
    //     }
    //
    //     Destroy(ignoreHovering);
    //     ignoreHovering = null;
    // }
    //
    // public Hand GetGripHand()
    // {
    //     switch (holdingHands.Count)
    //     {
    //         case 0:
    //             return null;
    //         case 1:
    //             return throwable.interactable.skeletonPoser.GetBlendingBehaviourEnabled("HoldingGrip")
    //                 ? holdingHands[0]
    //                 : null;
    //         case 2:
    //             for (var i = 0; i < holdingHands.Count; i++)
    //             {
    //                 var hand = holdingHands[i];
    //
    //                 if (!hand)
    //                 {
    //                     continue;
    //                 }
    //
    //                 var skeleton = i == 0
    //                     ? throwable.interactable.skeletonPoser
    //                     : throwable.secondHandInteractable.skeletonPoser;
    //
    //                 if (!skeleton.GetBlendingBehaviourEnabled("HoldingGrip"))
    //                 {
    //                     continue;
    //                 }
    //
    //                 return hand;
    //             }
    //
    //             break;
    //     }
    //
    //     return null;
    // }
}