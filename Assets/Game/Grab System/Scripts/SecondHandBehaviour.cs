using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;

public class SecondHandBehaviour : GrabbableObject
{
    [Foldout("Settings", true)]
    [SerializeField]
    private float distanceToExit = 0.1f;
    [SerializeField]
    private bool sliding;
    
    [Foldout("References", true)]
    [SerializeField]
    private GrabbableObject parentGrabbableObject;

    [Foldout("Information", true)]
    [ReadOnly]
    public GrabBehaviour currentGrabBehaviour;

    // protected override void HandHoverUpdate(Hand hand)
    // {
    //     if (hand.hoveringInteractable != throwable.interactable)
    //     {
    //         return;
    //     }
    //
    //     if (hand.currentAttachedObject)
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
    //     if (!parentGrabbableObject.firstGrabBehaviour)
    //     {
    //         return;
    //     }
    //     
    //     Grab(hand);
    // }
    //
    // protected override void HandAttachedUpdate(Hand hand)
    // {
    //     DetachByDistance();
    //     DetachByPressing();
    // }
    //
    // private void DetachByPressing()
    // {
    //     if (holdingHands.Count == 0)
    //     {
    //         return;
    //     }
    //     
    //     if (!inputManager.grabPressing.GetStateDown(holdingHands[0].handType))
    //     {
    //         return;
    //     }
    //     
    //     inputManager.PauseHand(holdingHands[0],inputManager.grabGrabType);
    //     
    //     Detach(holdingHands[0]);
    // }
    //
    // private void DetachByDistance()
    // {
    //     if (holdingHands.Count == 0)
    //     {
    //         return;
    //     }
    //     
    //     if (sliding)
    //     {
    //         return;
    //     }
    //     
    //     var distance = Vector3.Distance(holdingHands[0].transform.position, parentGrabbableObject.transform.position);
    //
    //     if (distance < distanceToExit)
    //     {
    //         return;
    //     }
    //     
    //     Detach(holdingHands[0]);
    // }
    //
    // public void Grab(Hand hand)
    // {
    //     throwable.Attach(hand);
    //     
    //     currentGrabBehaviour = inputManager.FindGrabBehaviourByHand(hand);
    //     currentGrabBehaviour.currentGrabbedObject = this;
    //     currentGrabBehaviour.grabingStep = GrabBehaviour.GrabingStep.Grabbed;
    // }
    //
    // public override void Detach(Hand hand)
    // {
    //     hand.DetachObject(gameObject);
    //
    //     currentGrabBehaviour.grabingStep = GrabBehaviour.GrabingStep.Waiting;
    //     currentGrabBehaviour.currentGrabbedObject = null;
    //     currentGrabBehaviour = null;
    // }
    //
    // private void OnAttachedToHand(Hand hand)
    // {
    //     AddHand(hand);
    // }
    //
    // private void OnDetachedFromHand(Hand hand)
    // {
    //     RemoveHand(hand);
    // }
}