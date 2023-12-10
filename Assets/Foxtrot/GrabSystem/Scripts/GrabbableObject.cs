using System;
using System.Collections.Generic;
using Foxtrot.Shared.Scripts;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Foxtrot.GrabSystem.Scripts
{
    [RequireComponent(typeof(CustomThrowable))]
    public class GrabbableObject : MonoBehaviour
    {
        [Foldout("References", true)]
        public GrabbableHoldPoint[] grabbableHoldPoints;
        [Foldout("Component References", true)]
        [ReadOnly]
        public new Rigidbody rigidbody;
        [ReadOnly]
        public CustomThrowable throwable;
        [SerializeField, ReadOnly]
        protected Collider[] physicsColliders;
        [SerializeField, ReadOnly]
        protected Collider[] triggerColliders;
        [Foldout("Information", true)]
        [ReadOnly]
        public GrabBehaviour firstGrabBehaviour;
        [ReadOnly]
        public GrabBehaviour secondGrabBehaviour;
        [ReadOnly]
        public IgnoreHovering ignoreHovering;
        [ReadOnly]
        public GrabbableHoldPoint firstHandHoldPoint;
        [ReadOnly]
        public GrabbableHoldPoint secondHandHoldPoint;
        [ReadOnly]
        public List<Hand> holdingHands = new List<Hand>();

        protected InputManager InputManager;
        
        #if UNITY_EDITOR
        [ButtonMethod]
        protected virtual string RefreshComponents()
        {
            throwable = null;
            rigidbody = null;
            physicsColliders = new Collider[0];
            triggerColliders = new Collider[0];
            grabbableHoldPoints = new GrabbableHoldPoint[0];

            EditorUtility.SetDirty(this);

            //OnValidate();

            return "References updated";
        }
        #endif
        
        #if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!rigidbody)
            {
                rigidbody = GetComponent<Rigidbody>();

                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }

            if (!throwable)
            {
                throwable = GetComponent<CustomThrowable>();

                throwable.releaseVelocityStyle = CustomThrowable.ReleaseStyle.ShortEstimation;
                throwable.interactable.hoverAttached = true;
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

                    var interactableParent = collider.GetComponentInParent<Interactable>();

                    if (interactableParent == null ||
                        throwable.interactable != interactableParent)
                    {
                        continue;
                    }

                    if (collider.isTrigger)
                    {
                        tColliders.Add(collider);
                    }
                    else
                    {
                        pColliders.Add(collider);
                    }
                }

                physicsColliders = pColliders.ToArray();
                triggerColliders = tColliders.ToArray();
            }

            if (throwable.holdType == HoldType.TwoHanded)
            {
                if (grabbableHoldPoints.Length == 0)
                {
                    grabbableHoldPoints = GetComponentsInChildren<GrabbableHoldPoint>();
                }
            }
        }
        #endif

        protected virtual void Start()
        {
            InputManager = InputManager.instance;
        }

        public virtual void Grab(Hand hand, GrabBehaviour grabBehaviour)
        {
            AddIgnoreHovering();

            firstGrabBehaviour = grabBehaviour;

            throwable.Attach(hand);
        }

        public virtual void Grab(Hand hand, GrabbableHoldPoint grabbableHoldPoint, bool secondHand)
        {
            if (!secondHand)
            {
                firstHandHoldPoint = grabbableHoldPoint;

                firstGrabBehaviour = firstHandHoldPoint.grabBehaviour;

                throwable.Attach(hand);
                return;
            }

            secondHandHoldPoint = grabbableHoldPoint;

            secondGrabBehaviour = secondHandHoldPoint.grabBehaviour;

            throwable.Attach(hand, throwable.secondHandInteractable.gameObject);
        }

        public virtual void Detach(Hand hand)
        {
            RemoveIgnoreHovering();

            RemoveHand(hand);
            throwable.Detach(hand, hand.currentAttachedObject);
            throwable.ApplyDetachVelocities(hand);

            firstGrabBehaviour.grabingStep = GrabBehaviour.GrabingStep.Waiting;
            firstGrabBehaviour.currentGrabbedObject = null;
            firstGrabBehaviour = null;

            SwitchHands();
        }

        public virtual void Detach(Hand hand, GrabbableHoldPoint grabbableHoldPoint)
        {
            var swapOneHanded = holdingHands.Count > 1;
            GrabBehaviour grabBehaviour = null;
            GrabbableHoldPoint holdPoint = null;

            if (swapOneHanded)
            {
                throwable.PhysicsDetach();

                grabBehaviour = grabbableHoldPoint == firstHandHoldPoint ? secondGrabBehaviour : firstGrabBehaviour;
                holdPoint = grabbableHoldPoint == firstHandHoldPoint ? secondHandHoldPoint : firstHandHoldPoint;
            }

            RemoveHand(hand);
            throwable.Detach(hand, hand.currentAttachedObject);

            if (firstGrabBehaviour)
            {
                firstGrabBehaviour.grabingStep = GrabBehaviour.GrabingStep.Waiting;
                firstGrabBehaviour.currentGrabbedObject = null;
                firstGrabBehaviour = null;

                firstHandHoldPoint.grabBehaviour = null;
                firstHandHoldPoint = null;
            }

            if (secondGrabBehaviour)
            {
                secondGrabBehaviour.grabingStep = GrabBehaviour.GrabingStep.Waiting;
                secondGrabBehaviour.currentGrabbedObject = null;
                secondGrabBehaviour = null;

                secondHandHoldPoint.grabBehaviour = null;
                secondHandHoldPoint = null;
            }

            if (!swapOneHanded)
            {
                return;
            }

            grabBehaviour.GrabObject(holdPoint);
        }

        private bool CanSwitchHands(SecondHandBehaviour secondHandBehaviour)
        {
            if (!secondHandBehaviour)
            {
                return false;
            }

            if (secondHandBehaviour.holdingHands.Count == 0)
            {
                return false;
            }

            return true;
        }

        private void SwitchHands()
        {
            var secondHandBehaviour = GetComponentInChildren<SecondHandBehaviour>();

            if (!CanSwitchHands(secondHandBehaviour))
            {
                return;
            }

            var otherGrabBehaviour = secondHandBehaviour.currentGrabBehaviour;

            secondHandBehaviour.Detach(secondHandBehaviour.holdingHands[0]);

            otherGrabBehaviour.GrabObject(this);
        }

        protected virtual void HandHoverUpdate(Hand hand)
        {
            if (holdingHands.Contains(hand))
            {
                return;
            }

            if (!InputManager.HandCanInteract(hand))
            {
                return;
            }

            var startingGrabType = hand.GetBestGrabbingType(Hand.GrabTypes.Grip);

            if (startingGrabType == Hand.GrabTypes.None)
            {
                return;
            }

            if (throwable.holdType == HoldType.TwoHanded)
            {
                var grabBehaviour = InputManager.FindGrabBehaviourByHand(hand);

                var holdPoint = FindNearestHoldPoint(grabBehaviour);

                if (!holdPoint)
                {
                    return;
                }

                if (holdPoint.grabBehaviour)
                {
                    return;
                }

                InputManager.PauseHand(hand, Hand.GrabTypes.Grip);

                grabBehaviour.GrabObject(holdPoint);
            }
            else
            {
                if (firstGrabBehaviour)
                {
                    return;
                }

                firstGrabBehaviour = InputManager.FindGrabBehaviourByHand(hand);

                firstGrabBehaviour.GrabObject(this);
            }
        }

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

        protected virtual void HandAttachedUpdate(Hand hand)
        {
            if (holdingHands.Count == 0)
            {
                return;
            }

            var grabArray = GetActiveGrabBehaviours();

            for (var i = 0; i < grabArray.Length; i++)
            {
                var grabBehaviour = grabArray[i];

                if (throwable.holdType == HoldType.TwoHanded)
                {
                    grabBehaviour.GrabEnding(firstGrabBehaviour == grabBehaviour
                        ? firstHandHoldPoint
                        : secondHandHoldPoint);
                    continue;
                }

                grabBehaviour.GrabEnding();
            }
        }

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

        public void AddHand(Hand hand)
        {
            if (holdingHands.Contains(hand))
            {
                return;
            }

            holdingHands.Add(hand);
        }

        public void RemoveHand(Hand hand)
        {
            if (!holdingHands.Contains(hand))
            {
                return;
            }

            holdingHands.Remove(hand);
        }

        public virtual void SetColliders(bool value, bool trigger = false)
        {
            var colliders = trigger ? triggerColliders : physicsColliders;

            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = value;
            }
        }

        public virtual void AddIgnoreHovering()
        {
            if (ignoreHovering)
            {
                return;
            }

            ignoreHovering = gameObject.AddComponent<IgnoreHovering>();
        }

        public virtual void RemoveIgnoreHovering()
        {
            if (!ignoreHovering)
            {
                return;
            }

            Destroy(ignoreHovering);
            ignoreHovering = null;
        }
    }
}