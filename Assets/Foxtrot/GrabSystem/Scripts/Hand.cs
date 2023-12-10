using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Foxtrot.Shared.Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Foxtrot.GrabSystem.Scripts
{
    public class Hand : MonoBehaviour
    {
        public HandType handType;
        
        public const AttachmentFlags defaultAttachmentFlags = AttachmentFlags.ParentToHand |
                                                              AttachmentFlags.DetachOthers |
                                                              AttachmentFlags.DetachFromOtherHand |
                                                              AttachmentFlags.TurnOnKinematic |
                                                              AttachmentFlags.SnapOnAttach;

        public Hand otherHand;

        public bool useHoverSphere = true;
        public Transform hoverSphereTransform;
        public float hoverSphereRadius = 0.05f;
        public LayerMask hoverLayerMask = -1;
        public float hoverUpdateInterval = 0.1f;

        [Tooltip("A transform on the hand to center attached objects on")]
        public Transform objectAttachmentPoint;

        public struct AttachedObject
        {
            public GameObject attachedObject;
            public Interactable interactable;
            public Rigidbody attachedRigidbody;
            public CollisionDetectionMode collisionDetectionMode;
            public bool attachedRigidbodyWasKinematic;
            public bool attachedRigidbodyUsedGravity;
            public GameObject originalParent;
            public bool isParentedToHand;
            public GrabTypes grabbedWithType;
            public AttachmentFlags attachmentFlags;
            public Vector3 initialPositionalOffset;
            public Quaternion initialRotationalOffset;
            public Transform attachedOffsetTransform;
            public Transform handAttachmentPointTransform;
            public Vector3 easeSourcePosition;
            public Quaternion easeSourceRotation;
            public float attachTime;

            public bool HasAttachFlag(AttachmentFlags flag)
            {
                return (attachmentFlags & flag) == flag;
            }
        }

        private List<AttachedObject> attachedObjects = new List<AttachedObject>();

        public ReadOnlyCollection<AttachedObject> AttachedObjects
        {
            get { return attachedObjects.AsReadOnly(); }
        }

        public bool hoverLocked { get; private set; }

        private Interactable _hoveringInteractable;

        private TextMesh debugText;
        private int prevOverlappingColliders = 0;

        private const int ColliderArraySize = 32;
        private Collider[] overlappingColliders;
        
        // The flags used to determine how an object is attached to the hand.
        [Flags]
        public enum AttachmentFlags
        {
            SnapOnAttach = 1 << 0, // The object should snap to the position of the specified attachment point on the hand.
            DetachOthers = 1 << 1, // Other objects attached to this hand will be detached.
            DetachFromOtherHand = 1 << 2, // This object will be detached from the other hand.
            ParentToHand = 1 << 3, // The object will be parented to the hand.
            VelocityMovement = 1 << 4, // The object will attempt to move to match the position and rotation of the hand.
            TurnOnKinematic = 1 << 5, // The object will not respond to external physics.
            TurnOffGravity = 1 << 6, // The object will not respond to external physics.
        };
        
        public enum GrabTypes
        {
            None,
            Trigger,
            Grip,
            Primary,
            Secondary
        }

        public enum HandType
        {
            LeftHand,
            RightHand
        }

        //-------------------------------------------------
        // The Interactable object this Hand is currently hovering over
        //-------------------------------------------------
        public Interactable hoveringInteractable
        {
            get { return _hoveringInteractable; }
            set
            {
                if (_hoveringInteractable != value)
                {
                    if (_hoveringInteractable != null)
                    {
                        _hoveringInteractable.SendMessage("OnHandHoverEnd", this, SendMessageOptions.DontRequireReceiver);

                        //Note: The _hoveringInteractable can change after sending the OnHandHoverEnd message so we need to check it again before broadcasting this message
                        if (_hoveringInteractable != null)
                        {
                            this.BroadcastMessage("OnParentHandHoverEnd", _hoveringInteractable, SendMessageOptions.DontRequireReceiver); // let objects attached to the hand know that a hover has ended
                        }
                    }

                    _hoveringInteractable = value;

                    if (_hoveringInteractable != null)
                    {
                        _hoveringInteractable.SendMessage("OnHandHoverBegin", this, SendMessageOptions.DontRequireReceiver);

                        //Note: The _hoveringInteractable can change after sending the OnHandHoverBegin message so we need to check it again before broadcasting this message
                        if (_hoveringInteractable != null)
                        {
                            this.BroadcastMessage("OnParentHandHoverBegin", _hoveringInteractable, SendMessageOptions.DontRequireReceiver); // let objects attached to the hand know that a hover has begun
                        }
                    }
                }
            }
        }


        //-------------------------------------------------
        // Active GameObject attached to this Hand
        //-------------------------------------------------
        public GameObject currentAttachedObject
        {
            get
            {
                CleanUpAttachedObjectStack();

                if (attachedObjects.Count > 0)
                {
                    return attachedObjects[attachedObjects.Count - 1].attachedObject;
                }

                return null;
            }
        }

        public AttachedObject? currentAttachedObjectInfo
        {
            get
            {
                CleanUpAttachedObjectStack();

                if (attachedObjects.Count > 0)
                {
                    return attachedObjects[attachedObjects.Count - 1];
                }

                return null;
            }
        }

        //-------------------------------------------------
        // Attach a GameObject to this GameObject
        //
        // objectToAttach - The GameObject to attach
        // flags - The flags to use for attaching the object
        // attachmentPoint - Name of the GameObject in the hierarchy of this Hand which should act as the attachment point for this GameObject
        //-------------------------------------------------
        public void AttachObject(GameObject objectToAttach, GrabTypes grabbedWithType, AttachmentFlags flags = defaultAttachmentFlags, Transform attachmentOffset = null)
        {
            AttachedObject attachedObject = new AttachedObject();
            attachedObject.attachmentFlags = flags;
            attachedObject.attachedOffsetTransform = attachmentOffset;
            attachedObject.attachTime = Time.time;

            if (flags == 0)
            {
                flags = defaultAttachmentFlags;
            }

            //Make sure top object on stack is non-null
            CleanUpAttachedObjectStack();

            //Detach the object if it is already attached so that it can get re-attached at the top of the stack
            if (ObjectIsAttached(objectToAttach))
                DetachObject(objectToAttach);

            //Detach from the other hand if requested
            if (attachedObject.HasAttachFlag(AttachmentFlags.DetachFromOtherHand))
            {
                if (otherHand != null)
                    otherHand.DetachObject(objectToAttach);
            }

            if (attachedObject.HasAttachFlag(AttachmentFlags.DetachOthers))
            {
                //Detach all the objects from the stack
                while (attachedObjects.Count > 0)
                {
                    DetachObject(attachedObjects[0].attachedObject);
                }
            }

            if (currentAttachedObject)
            {
                currentAttachedObject.SendMessage("OnHandFocusLost", this, SendMessageOptions.DontRequireReceiver);
            }

            attachedObject.attachedObject = objectToAttach;
            attachedObject.interactable = objectToAttach.GetComponent<Interactable>();
            attachedObject.handAttachmentPointTransform = this.transform;

            if (attachedObject.interactable != null)
            {
                if (attachedObject.interactable.attachEaseIn)
                {
                    attachedObject.easeSourcePosition = attachedObject.attachedObject.transform.position;
                    attachedObject.easeSourceRotation = attachedObject.attachedObject.transform.rotation;
                    attachedObject.interactable.snapAttachEaseInCompleted = false;
                }

                if (attachedObject.interactable.useHandObjectAttachmentPoint)
                    attachedObject.handAttachmentPointTransform = objectAttachmentPoint;
            }

            attachedObject.originalParent = objectToAttach.transform.parent != null ? objectToAttach.transform.parent.gameObject : null;

            attachedObject.attachedRigidbody = objectToAttach.GetComponent<Rigidbody>();
            if (attachedObject.attachedRigidbody != null)
            {
                if (attachedObject.interactable.attachedToHand != null) //already attached to another hand
                {
                    //if it was attached to another hand, get the flags from that hand

                    for (int attachedIndex = 0; attachedIndex < attachedObject.interactable.attachedToHand.attachedObjects.Count; attachedIndex++)
                    {
                        AttachedObject attachedObjectInList = attachedObject.interactable.attachedToHand.attachedObjects[attachedIndex];
                        if (attachedObjectInList.interactable == attachedObject.interactable)
                        {
                            attachedObject.attachedRigidbodyWasKinematic = attachedObjectInList.attachedRigidbodyWasKinematic;
                            attachedObject.attachedRigidbodyUsedGravity = attachedObjectInList.attachedRigidbodyUsedGravity;
                            attachedObject.originalParent = attachedObjectInList.originalParent;
                        }
                    }
                }
                else
                {
                    attachedObject.attachedRigidbodyWasKinematic = attachedObject.attachedRigidbody.isKinematic;
                    attachedObject.attachedRigidbodyUsedGravity = attachedObject.attachedRigidbody.useGravity;
                }
            }

            attachedObject.grabbedWithType = grabbedWithType;

            if (attachedObject.HasAttachFlag(AttachmentFlags.ParentToHand))
            {
                //Parent the object to the hand
                objectToAttach.transform.parent = this.transform;
                attachedObject.isParentedToHand = true;
            }
            else
            {
                attachedObject.isParentedToHand = false;
            }

            if (attachedObject.HasAttachFlag(AttachmentFlags.SnapOnAttach))
            {
                if (attachmentOffset != null)
                {
                    //offset the object from the hand by the positional and rotational difference between the offset transform and the attached object
                    Quaternion rotDiff = Quaternion.Inverse(attachmentOffset.transform.rotation) * objectToAttach.transform.rotation;
                    objectToAttach.transform.rotation = attachedObject.handAttachmentPointTransform.rotation * rotDiff;

                    Vector3 posDiff = objectToAttach.transform.position - attachmentOffset.transform.position;
                    objectToAttach.transform.position = attachedObject.handAttachmentPointTransform.position + posDiff;
                }
                else
                {
                    //snap the object to the center of the attach point
                    objectToAttach.transform.rotation = attachedObject.handAttachmentPointTransform.rotation;
                    objectToAttach.transform.position = attachedObject.handAttachmentPointTransform.position;
                }

                Transform followPoint = objectToAttach.transform;

                attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(followPoint.position);
                attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * followPoint.rotation;
            }
            else
            {
                if (attachmentOffset != null)
                {
                    //get the initial positional and rotational offsets between the hand and the offset transform
                    Quaternion rotDiff = Quaternion.Inverse(attachmentOffset.transform.rotation) * objectToAttach.transform.rotation;
                    Quaternion targetRotation = attachedObject.handAttachmentPointTransform.rotation * rotDiff;
                    Quaternion rotationPositionBy = targetRotation * Quaternion.Inverse(objectToAttach.transform.rotation);

                    Vector3 posDiff = (rotationPositionBy * objectToAttach.transform.position) - (rotationPositionBy * attachmentOffset.transform.position);

                    attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(attachedObject.handAttachmentPointTransform.position + posDiff);
                    attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * (attachedObject.handAttachmentPointTransform.rotation * rotDiff);
                }
                else
                {
                    attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(objectToAttach.transform.position);
                    attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * objectToAttach.transform.rotation;
                }
            }

            if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOnKinematic))
            {
                if (attachedObject.attachedRigidbody != null)
                {
                    attachedObject.collisionDetectionMode = attachedObject.attachedRigidbody.collisionDetectionMode;
                    if (attachedObject.collisionDetectionMode == CollisionDetectionMode.Continuous)
                        attachedObject.attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

                    attachedObject.attachedRigidbody.isKinematic = true;
                }
            }

            if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOffGravity))
            {
                if (attachedObject.attachedRigidbody != null)
                {
                    attachedObject.attachedRigidbody.useGravity = false;
                }
            }

            if (attachedObject.interactable != null && attachedObject.interactable.attachEaseIn)
            {
                attachedObject.attachedObject.transform.position = attachedObject.easeSourcePosition;
                attachedObject.attachedObject.transform.rotation = attachedObject.easeSourceRotation;
            }

            attachedObjects.Add(attachedObject);

            UpdateHovering();
            
            objectToAttach.SendMessage("OnAttachedToHand", this, SendMessageOptions.DontRequireReceiver);
        }

        public bool ObjectIsAttached(GameObject go)
        {
            for (int attachedIndex = 0; attachedIndex < attachedObjects.Count; attachedIndex++)
            {
                if (attachedObjects[attachedIndex].attachedObject == go)
                    return true;
            }

            return false;
        }

        public void ForceHoverUnlock()
        {
            hoverLocked = false;
        }

        //-------------------------------------------------
        // Detach this GameObject from the attached object stack of this Hand
        //
        // objectToDetach - The GameObject to detach from this Hand
        //-------------------------------------------------
        public void DetachObject(GameObject objectToDetach, bool restoreOriginalParent = true)
        {
            int index = attachedObjects.FindIndex(l => l.attachedObject == objectToDetach);
            if (index != -1)
            {
                GameObject prevTopObject = currentAttachedObject;

                Transform parentTransform = null;
                if (attachedObjects[index].isParentedToHand)
                {
                    if (restoreOriginalParent && (attachedObjects[index].originalParent != null))
                    {
                        parentTransform = attachedObjects[index].originalParent.transform;
                    }

                    if (attachedObjects[index].attachedObject != null)
                    {
                        attachedObjects[index].attachedObject.transform.parent = parentTransform;
                    }
                }

                if (attachedObjects[index].HasAttachFlag(AttachmentFlags.TurnOnKinematic))
                {
                    if (attachedObjects[index].attachedRigidbody != null)
                    {
                        attachedObjects[index].attachedRigidbody.isKinematic = attachedObjects[index].attachedRigidbodyWasKinematic;
                        attachedObjects[index].attachedRigidbody.collisionDetectionMode = attachedObjects[index].collisionDetectionMode;
                    }
                }

                if (attachedObjects[index].HasAttachFlag(AttachmentFlags.TurnOffGravity))
                {
                    if (attachedObjects[index].attachedObject != null)
                    {
                        if (attachedObjects[index].attachedRigidbody != null)
                            attachedObjects[index].attachedRigidbody.useGravity = attachedObjects[index].attachedRigidbodyUsedGravity;
                    }
                }

                if (attachedObjects[index].attachedObject != null)
                {
                    if (attachedObjects[index].interactable == null || (attachedObjects[index].interactable != null && attachedObjects[index].interactable.isDestroying == false))
                        attachedObjects[index].attachedObject.SetActive(true);

                    attachedObjects[index].attachedObject.SendMessage("OnDetachedFromHand", this, SendMessageOptions.DontRequireReceiver);
                }

                attachedObjects.RemoveAt(index);

                CleanUpAttachedObjectStack();

                GameObject newTopObject = currentAttachedObject;

                hoverLocked = false;
                
                //Give focus to the top most object on the stack if it changed
                if (newTopObject != null && newTopObject != prevTopObject)
                {
                    newTopObject.SetActive(true);
                    newTopObject.SendMessage("OnHandFocusAcquired", this, SendMessageOptions.DontRequireReceiver);
                }
            }

            CleanUpAttachedObjectStack();
        }

        //-------------------------------------------------
        private void CleanUpAttachedObjectStack()
        {
            attachedObjects.RemoveAll(l => l.attachedObject == null);
        }


        //-------------------------------------------------
        protected virtual void Awake()
        {
            if (hoverSphereTransform == null)
                hoverSphereTransform = this.transform;

            if (objectAttachmentPoint == null)
                objectAttachmentPoint = this.transform;
        }

        //-------------------------------------------------
        protected virtual void Start()
        {
            if (this.gameObject.layer == 0)
                Debug.LogWarning("<b>[SteamVR Interaction]</b> Hand is on default layer. This puts unnecessary strain on hover checks as it is always true for hand colliders (which are then ignored).", this);
            else
                hoverLayerMask &= ~(1 << this.gameObject.layer); //ignore self for hovering

            // allocate array for colliders
            overlappingColliders = new Collider[ColliderArraySize];
        }
        
        //-------------------------------------------------
        protected virtual void UpdateHovering()
        {
            if (hoverLocked)
                return;

            float closestDistance = float.MaxValue;
            Interactable closestInteractable = null;

            if (useHoverSphere)
            {
                float scaledHoverRadius = hoverSphereRadius * Mathf.Abs(Utils.GetLossyScale(hoverSphereTransform));
                CheckHoveringForTransform(hoverSphereTransform.position, scaledHoverRadius, ref closestDistance, ref closestInteractable, Color.green);
            }

            // Hover on this one
            hoveringInteractable = closestInteractable;
        }

        protected virtual bool CheckHoveringForTransform(Vector3 hoverPosition, float hoverRadius, ref float closestDistance, ref Interactable closestInteractable, Color debugColor)
        {
            bool foundCloser = false;

            // null out old vals
            for (int i = 0; i < overlappingColliders.Length; ++i)
            {
                overlappingColliders[i] = null;
            }

            int numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, overlappingColliders, hoverLayerMask.value);

            if (numColliding >= ColliderArraySize)
                Debug.LogWarning("<b>[SteamVR Interaction]</b> This hand is overlapping the max number of colliders: " + ColliderArraySize + ". Some collisions may be missed. Increase ColliderArraySize on Hand.cs");

            // DebugVar
            int iActualColliderCount = 0;

            // Pick the closest hovering
            for (int colliderIndex = 0; colliderIndex < overlappingColliders.Length; colliderIndex++)
            {
                Collider collider = overlappingColliders[colliderIndex];

                if (collider == null)
                    continue;

                Interactable contacting = collider.GetComponentInParent<Interactable>();

                // Yeah, it's null, skip
                if (contacting == null)
                    continue;

                // Ignore this collider for hovering
                IgnoreHovering ignore = contacting.GetComponent<IgnoreHovering>();
                
                if (ignore != null)
                {
                    if (ignore.onlyIgnoreHand == null || ignore.onlyIgnoreHand == this)
                    {
                        continue;
                    }
                }

                //Only check if Interactable dont allow it
                if (!contacting.hoverAttached)
                {
                    // Can't hover over the object if it's attached
                    bool hoveringOverAttached = false;
                    for (int attachedIndex = 0; attachedIndex < attachedObjects.Count; attachedIndex++)
                    {
                        if (attachedObjects[attachedIndex].attachedObject == contacting.gameObject)
                        {
                            hoveringOverAttached = true;
                            break;
                        }
                    }
                
                    if (hoveringOverAttached)
                        continue;
                }
                
                // Best candidate so far...
                float distance = Vector3.Distance(contacting.transform.position, hoverPosition);
                //float distance = Vector3.Distance(collider.bounds.center, hoverPosition);
                bool lowerPriority = false;
                if (closestInteractable != null)
                { // compare to closest interactable to check priority
                    lowerPriority = contacting.hoverPriority < closestInteractable.hoverPriority;
                }
                bool isCloser = (distance < closestDistance);
                if (isCloser && !lowerPriority)
                {
                    closestDistance = distance;
                    closestInteractable = contacting;
                    foundCloser = true;
                }
                iActualColliderCount++;
            }

            if (iActualColliderCount > 0 && iActualColliderCount != prevOverlappingColliders)
            {
                prevOverlappingColliders = iActualColliderCount;
            }

            return foundCloser;
        }
        
        //-------------------------------------------------
        protected virtual void OnEnable()
        {
            // Stagger updates between hands
            float hoverUpdateBegin = ((otherHand != null) && (otherHand.GetInstanceID() < GetInstanceID())) ? (0.5f * hoverUpdateInterval) : (0.0f);
            InvokeRepeating("UpdateHovering", hoverUpdateBegin, hoverUpdateInterval);
        }


        //-------------------------------------------------
        protected virtual void OnDisable()
        {
            CancelInvoke();
        }


        //-------------------------------------------------
        protected virtual void Update()
        {
            GameObject attachedObject = currentAttachedObject;
            if (attachedObject != null)
            {
                attachedObject.SendMessage("HandAttachedUpdate", this, SendMessageOptions.DontRequireReceiver);
            }

            if (hoveringInteractable)
            {
                hoveringInteractable.SendMessage("HandHoverUpdate", this, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// Returns true when the hand is currently hovering over the interactable passed in
        /// </summary>
        public bool IsStillHovering(Interactable interactable)
        {
            return hoveringInteractable == interactable;
        }

        protected virtual void FixedUpdate()
        {
            if (currentAttachedObject != null)
            {
                AttachedObject attachedInfo = currentAttachedObjectInfo.Value;
                if (attachedInfo.attachedObject != null)
                {
                    if (attachedInfo.HasAttachFlag(AttachmentFlags.VelocityMovement))
                    {
                        if (attachedInfo.interactable.attachEaseIn == false || attachedInfo.interactable.snapAttachEaseInCompleted)
                            UpdateAttachedVelocity(attachedInfo);
                    }
                    else
                    {
                        if (attachedInfo.HasAttachFlag(AttachmentFlags.ParentToHand))
                        {
                            attachedInfo.attachedObject.transform.position = TargetItemPosition(attachedInfo);
                            attachedInfo.attachedObject.transform.rotation = TargetItemRotation(attachedInfo);
                        }
                    }


                    if (attachedInfo.interactable.attachEaseIn)
                    {
                        float t = Utils.RemapNumberClamped(Time.time, attachedInfo.attachTime, attachedInfo.attachTime + attachedInfo.interactable.snapAttachEaseInTime, 0.0f, 1.0f);
                        if (t < 1.0f)
                        {
                            if (attachedInfo.HasAttachFlag(AttachmentFlags.VelocityMovement))
                            {
                                attachedInfo.attachedRigidbody.velocity = Vector3.zero;
                                attachedInfo.attachedRigidbody.angularVelocity = Vector3.zero;
                            }
                            t = attachedInfo.interactable.snapAttachEaseInCurve.Evaluate(t);
                            attachedInfo.attachedObject.transform.position = Vector3.Lerp(attachedInfo.easeSourcePosition, TargetItemPosition(attachedInfo), t);
                            attachedInfo.attachedObject.transform.rotation = Quaternion.Lerp(attachedInfo.easeSourceRotation, TargetItemRotation(attachedInfo), t);
                        }
                        else if (!attachedInfo.interactable.snapAttachEaseInCompleted)
                        {
                            attachedInfo.interactable.gameObject.SendMessage("OnThrowableAttachEaseInCompleted", this, SendMessageOptions.DontRequireReceiver);
                            attachedInfo.interactable.snapAttachEaseInCompleted = true;
                        }
                    }
                }
            }
        }

        protected const float MaxVelocityChange = 10f;
        protected const float VelocityMagic = 6000f;
        protected const float AngularVelocityMagic = 50f;
        protected const float MaxAngularVelocityChange = 20f;

        protected void UpdateAttachedVelocity(AttachedObject attachedObjectInfo)
        {
            Vector3 velocityTarget, angularTarget;
            bool success = GetUpdatedAttachedVelocities(attachedObjectInfo, out velocityTarget, out angularTarget);
            if (success)
            {
                float scale = Utils.GetLossyScale(currentAttachedObjectInfo.Value.handAttachmentPointTransform);
                float maxAngularVelocityChange = MaxAngularVelocityChange * scale;
                float maxVelocityChange = MaxVelocityChange * scale;

                attachedObjectInfo.attachedRigidbody.velocity = Vector3.MoveTowards(attachedObjectInfo.attachedRigidbody.velocity, velocityTarget, maxVelocityChange);
                attachedObjectInfo.attachedRigidbody.angularVelocity = Vector3.MoveTowards(attachedObjectInfo.attachedRigidbody.angularVelocity, angularTarget, maxAngularVelocityChange);
            }
        }

        /// <summary>
        /// Snap an attached object to its target position and rotation. Good for error correction.
        /// </summary>
        public void ResetAttachedTransform(AttachedObject attachedObject)
        {
            attachedObject.attachedObject.transform.position = TargetItemPosition(attachedObject);
            attachedObject.attachedObject.transform.rotation = TargetItemRotation(attachedObject);
        }

        protected Vector3 TargetItemPosition(AttachedObject attachedObject)
        {
            return currentAttachedObjectInfo.Value.handAttachmentPointTransform.TransformPoint(attachedObject.initialPositionalOffset);
        }

        protected Quaternion TargetItemRotation(AttachedObject attachedObject)
        {
            return currentAttachedObjectInfo.Value.handAttachmentPointTransform.rotation * attachedObject.initialRotationalOffset;
        }

        protected bool GetUpdatedAttachedVelocities(AttachedObject attachedObjectInfo, out Vector3 velocityTarget, out Vector3 angularTarget)
        {
            bool realNumbers = false;


            float velocityMagic = VelocityMagic;
            float angularVelocityMagic = AngularVelocityMagic;

            Vector3 targetItemPosition = TargetItemPosition(attachedObjectInfo);
            Vector3 positionDelta = (targetItemPosition - attachedObjectInfo.attachedRigidbody.position);
            velocityTarget = (positionDelta * velocityMagic * Time.deltaTime);

            if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
            {
                realNumbers = true;
            }
            else
                velocityTarget = Vector3.zero;


            Quaternion targetItemRotation = TargetItemRotation(attachedObjectInfo);
            Quaternion rotationDelta = targetItemRotation * Quaternion.Inverse(attachedObjectInfo.attachedObject.transform.rotation);


            float angle;
            Vector3 axis;
            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
                angle -= 360;

            if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
            {
                angularTarget = angle * axis * angularVelocityMagic * Time.deltaTime;

                realNumbers &= true;
            }
            else
                angularTarget = Vector3.zero;

            return realNumbers;
        }

        //-------------------------------------------------
        protected virtual void OnDrawGizmos()
        {
            if (useHoverSphere && hoverSphereTransform != null)
            {
                Gizmos.color = Color.green;
                float scaledHoverRadius = hoverSphereRadius * Mathf.Abs(Utils.GetLossyScale(hoverSphereTransform));
                Gizmos.DrawWireSphere(hoverSphereTransform.position, scaledHoverRadius / 2);
            }
        }

        //-------------------------------------------------
        // Continue to hover over this object indefinitely, whether or not the Hand moves out of its interaction trigger volume.
        //
        // interactable - The Interactable to hover over indefinitely.
        //-------------------------------------------------
        public void HoverLock(Interactable interactable)
        {
            hoverLocked = true;
            hoveringInteractable = interactable;
        }


        //-------------------------------------------------
        // Stop hovering over this object indefinitely.
        //
        // interactable - The hover-locked Interactable to stop hovering over indefinitely.
        //-------------------------------------------------
        public void HoverUnlock(Interactable interactable)
        {
            if (hoveringInteractable == interactable)
            {
                hoverLocked = false;
            }
        }

        public GrabTypes GetGrabStarting(GrabTypes explicitType = GrabTypes.None)
        {
            if (explicitType != GrabTypes.None)
            {
                if (explicitType == GrabTypes.Grip && InputManager.instance.GetInput(handType, GrabTypes.Grip).DownState)
                    return GrabTypes.Grip;
            }
            else
            {
                if (InputManager.instance.GetInput(handType, GrabTypes.Grip).DownState)
                    return GrabTypes.Grip;
            }

            return GrabTypes.None;
        }

        public GrabTypes GetGrabEnding(GrabTypes explicitType = GrabTypes.None)
        {
            if (explicitType != GrabTypes.None)
            {
                if (explicitType == GrabTypes.Grip && InputManager.instance.GetInput(handType, GrabTypes.Grip).UpState)
                    return GrabTypes.Grip;
            }
            else
            {
                if (InputManager.instance.GetInput(handType, GrabTypes.Grip).UpState)
                    return GrabTypes.Grip;
            }

            return GrabTypes.None;
        }

        public bool IsGrabEnding(GameObject attachedObject)
        {
            for (int attachedObjectIndex = 0; attachedObjectIndex < attachedObjects.Count; attachedObjectIndex++)
            {
                if (attachedObjects[attachedObjectIndex].attachedObject == attachedObject)
                {
                    return IsGrabbingWithType(attachedObjects[attachedObjectIndex].grabbedWithType) == false;
                }
            }

            return false;
        }

        public bool IsGrabbingWithType(GrabTypes type)
        {
            switch (type)
            {
                case GrabTypes.Grip:
                    return InputManager.instance.GetInput(handType, GrabTypes.Grip).HoldState;

                default:
                    return false;
            }
        }

        public GrabTypes GetBestGrabbingType(GrabTypes preferred = GrabTypes.None)
        {
            if (preferred == GrabTypes.Grip)
            {
                if (InputManager.instance.GetInput(handType, GrabTypes.Grip).HoldState)
                    return GrabTypes.Grip;
            }
            
            if (InputManager.instance.GetInput(handType, GrabTypes.Grip).HoldState)
                return GrabTypes.Grip;

            return GrabTypes.None;
        }
    }


    [System.Serializable]
    public class HandEvent : UnityEvent<Hand> { }
}