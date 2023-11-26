using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

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
    [ConditionalField(nameof(sliding))]
    public LinearDrive linearDrive;

    [Foldout("Information", true)]
    [ReadOnly]
    public GrabBehaviour currentGrabBehaviour;

#if UNITY_EDITOR
    [ButtonMethod]
    protected override string RefreshComponents()
    {
        rigidbody = null;
        throwable = null;
        inputManager = null;
        physicsColliders = new Collider[0];
        triggerColliders = new Collider[0];
        parentGrabbableObject = null;
        
        EditorUtility.SetDirty(this);
        
        //OnValidate();
        
        return "References updated";
    }
#endif

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (!rigidbody)
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        if (!throwable)
        {
            throwable = GetComponent<CustomThrowable>();

            if (throwable)
            {
                throwable.firstHandAttachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand |
                                            Hand.AttachmentFlags.DetachOthers;
                
                throwable.interactable.hideHandOnAttach = false;
            }
        }

        if (!inputManager)
        {
            inputManager = FindObjectOfType<InputManager>();
        }

        if (physicsColliders == null || physicsColliders.Length == 0 ||
            triggerColliders == null || triggerColliders.Length == 0)
        {
            var colliders = GetComponentsInChildren<Collider>(true);

            var pColliders = new List<Collider>();
            var tColliders = new List<Collider>();

            for (var i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];

                if (collider.isTrigger)
                {
                    tColliders.Add(collider);
                    continue;
                }

                pColliders.Add(collider);
            }

            physicsColliders = pColliders.ToArray();
            triggerColliders = tColliders.ToArray();
        }

        if (!parentGrabbableObject)
        {
            var grabbableObjects = GetComponentsInParent<GrabbableObject>();

            for (var i = 0; i < grabbableObjects.Length; i++)
            {
                if (grabbableObjects[i] == this)
                {
                    continue;
                }

                parentGrabbableObject = grabbableObjects[i];
            }
        }

        if (sliding)
        {
            if (!linearDrive)
            {
                linearDrive = gameObject.AddComponent<LinearDrive>();
            }
        }
        else
        {
            if (linearDrive)
            {
                StartCoroutine(LateAction(RemoveLinearDrive));
            }
        }
    }

    private IEnumerator LateAction(Action action)
    {
        yield return null;
        action?.Invoke();
    }

    private void RemoveLinearDrive()
    {
        DestroyImmediate(linearDrive.linearMapping);
        DestroyImmediate(linearDrive);
        linearDrive = null;
    }
#endif
    
    protected override void HandHoverUpdate(Hand hand)
    {
        if (hand.hoveringInteractable != throwable.interactable)
        {
            return;
        }

        if (hand.currentAttachedObject)
        {
            return;
        }

        if (!inputManager.HandCanInteract(hand))
        {
            return;
        }

        var startingGrabType = hand.GetBestGrabbingType(inputManager.grabGrabType, true);

        if (startingGrabType == GrabTypes.None)
        {
            return;
        }

        if (!parentGrabbableObject.firstGrabBehaviour)
        {
            return;
        }
        
        Grab(hand);
    }
    
    protected override void HandAttachedUpdate(Hand hand)
    {
        DetachByDistance();
        DetachByPressing();
    }

    private void DetachByPressing()
    {
        if (holdingHands.Count == 0)
        {
            return;
        }
        
        if (!inputManager.grabPressing.GetStateDown(holdingHands[0].handType))
        {
            return;
        }
        
        inputManager.PauseHand(holdingHands[0],inputManager.grabGrabType);
        
        Detach(holdingHands[0]);
    }

    private void DetachByDistance()
    {
        if (holdingHands.Count == 0)
        {
            return;
        }
        
        if (sliding)
        {
            return;
        }
        
        var distance = Vector3.Distance(holdingHands[0].transform.position, parentGrabbableObject.transform.position);

        if (distance < distanceToExit)
        {
            return;
        }
        
        Detach(holdingHands[0]);
    }

    public void Grab(Hand hand)
    {
        throwable.Attach(hand);
        
        currentGrabBehaviour = inputManager.FindGrabBehaviourByHand(hand);
        currentGrabBehaviour.currentGrabbedObject = this;
        currentGrabBehaviour.grabingStep = GrabBehaviour.GrabingStep.Grabbed;
    }

    public override void Detach(Hand hand)
    {
        hand.DetachObject(gameObject);

        currentGrabBehaviour.grabingStep = GrabBehaviour.GrabingStep.Waiting;
        currentGrabBehaviour.currentGrabbedObject = null;
        currentGrabBehaviour = null;
    }
    
    private void OnAttachedToHand(Hand hand)
    {
        AddHand(hand);
    }

    private void OnDetachedFromHand(Hand hand)
    {
        RemoveHand(hand);
    }
}