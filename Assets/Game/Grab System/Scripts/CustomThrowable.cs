using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(VelocityEstimator))]
public class CustomThrowable : MonoBehaviour
{
    [Foldout("Settings", true)]
    public HoldType holdType;

    [ConditionalField(nameof(holdType), false, HoldType.TwoHanded)]
    public float attachForce = 800.0f;

    [ConditionalField(nameof(holdType), false, HoldType.TwoHanded)]
    public float attachForceDamper = 25.0f;

    [EnumFlags]
    [Tooltip("The flags used to attach the single hand")]
    public Hand.AttachmentFlags firstHandAttachmentFlags;

    [EnumFlags]
    [Tooltip("The flags used to attach the both hands")]
    public Hand.AttachmentFlags secondHandAttachmentFlags;

    public ReleaseStyle releaseVelocityStyle = ReleaseStyle.GetFromHand;

    [Tooltip("The time offset used when releasing the object with the RawFromHand option")]
    [ConditionalField(nameof(releaseVelocityStyle), false, ReleaseStyle.GetFromHand)]
    public float releaseVelocityTimeOffset = -0.011f;

    [ConditionalField(nameof(releaseVelocityStyle), false, ReleaseStyle.NoChange)]
    public float scaleReleaseVelocity = 1.1f;

    [Tooltip(
        "The release velocity magnitude representing the end of the scale release velocity curve. (-1 to disable)")]
    [ConditionalField(nameof(releaseVelocityStyle), false, ReleaseStyle.NoChange)]
    public float scaleReleaseVelocityThreshold = -1.0f;

    [Tooltip(
        "Use this curve to ease into the scaled release velocity based on the magnitude of the measured release velocity. This allows greater differentiation between a drop, toss, and throw.")]
    [ConditionalField(nameof(releaseVelocityStyle), false, ReleaseStyle.NoChange)]
    public AnimationCurve scaleReleaseVelocityCurve = AnimationCurve.EaseInOut(0.0f, 0.1f, 1.0f, 1.0f);

    [Tooltip("When detaching the object, should it return to its original parent?")]
    public bool restoreOriginalParent = false;

    [Foldout("References", true)]
    [SerializeField, ReadOnly]
    private GrabbableObject grabbableObject;

    [ReadOnly]
    public Interactable interactable;

    [ConditionalField(nameof(holdType), false, HoldType.TwoHanded), ReadOnly]
    public Interactable secondHandInteractable;

    [ReadOnly]
    public VelocityEstimator velocityEstimator;

    [ReadOnly]
    public new Rigidbody rigidbody;

    [Foldout("Events", true)]
    public UnityEvent onPickUp;

    public UnityEvent onDetachFromHand;
    public HandEvent onHeldUpdate;

    [Foldout("Information", true)]
    [SerializeField, ReadOnly]
    private bool perfomingTwoHanded;
    [SerializeField, ReadOnly]
    private List<Vector3> holdingPoints = new List<Vector3>();

    protected bool attached = false;
    protected float attachTime;
    protected Vector3 attachPosition;
    protected Quaternion attachRotation;
    protected Transform attachEaseInTransform;
    protected RigidbodyInterpolation hadInterpolation = RigidbodyInterpolation.None;

    protected InputManager inputManager;

#if UNITY_EDITOR
    [ButtonMethod]
    protected virtual string RefreshComponents()
    {
        rigidbody = null;
        interactable = null;
        velocityEstimator = null;
        grabbableObject = null;
        inputManager = null;

        EditorUtility.SetDirty(this);

        //OnValidate();

        return "References updated";
    }
#endif

    private void OnValidate()
    {
        if (!interactable)
        {
            var interactables = GetComponentsInChildren<Interactable>();

            for (var i = 0; i < interactables.Length; i++)
            {
                if (interactables[i].gameObject == gameObject)
                {
                    interactable = interactables[i];
                    continue;
                }

                if (holdType != HoldType.TwoHanded)
                {
                    continue;
                }

                secondHandInteractable = interactables[i];
            }

            firstHandAttachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand |
                                       Hand.AttachmentFlags.DetachOthers |
                                       Hand.AttachmentFlags.VelocityMovement |
                                       Hand.AttachmentFlags.TurnOffGravity;

            secondHandAttachmentFlags = 0;
        }

        if (!rigidbody)
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        if (!velocityEstimator)
        {
            velocityEstimator = GetComponent<VelocityEstimator>();

            if (velocityEstimator == null)
            {
                velocityEstimator = gameObject.AddComponent<VelocityEstimator>();
            }
        }

        if (!grabbableObject)
        {
            grabbableObject = GetComponent<GrabbableObject>();
        }

        if (!inputManager)
        {
            inputManager = FindObjectOfType<InputManager>();
        }
    }

    protected virtual void Awake()
    {
        rigidbody.maxAngularVelocity = 50.0f;
    }

    public void Attach(Hand hand, GameObject gameObjectToAttach = null)
    {
        grabbableObject.AddHand(hand);
        
        if (holdType == HoldType.TwoHanded)
        {
            if (grabbableObject.holdingHands.Count > 1)
            {
                SwapTwoHanded(gameObjectToAttach);
                return;
            }
        }
        
        hand.AttachObject(gameObject, inputManager.grabGrabType, firstHandAttachmentFlags);
    }

    public void Detach(Hand hand, GameObject gameObjectToDetach = null)
    {
        hand.DetachObject(gameObjectToDetach ? gameObjectToDetach : gameObject);
    }

    private void SwapTwoHanded(GameObject gameObjectToAttach)
    {
        Detach(grabbableObject.firstGrabBehaviour.hand);

        PhysicsAttach(grabbableObject.firstGrabBehaviour, grabbableObject.firstHandHoldPoint);
        PhysicsAttach(grabbableObject.secondGrabBehaviour,grabbableObject.secondHandHoldPoint, gameObjectToAttach);

        perfomingTwoHanded = true;
    }

    protected virtual void OnAttachedToHand(Hand hand)
    {
        hadInterpolation = rigidbody.interpolation;

        attached = true;

        onPickUp.Invoke();

        hand.HoverLock(null);

        rigidbody.interpolation = RigidbodyInterpolation.None;

        if (velocityEstimator != null)
        {
            velocityEstimator.BeginEstimatingVelocity();
        }

        attachTime = Time.time;
        attachPosition = transform.position;
        attachRotation = transform.rotation;
    }

    protected virtual void OnDetachedFromHand(Hand hand)
    {
        attached = false;

        onDetachFromHand.Invoke();

        hand.HoverUnlock(null);
    }

    public void ApplyDetachVelocities(Hand hand)
    {
        rigidbody.interpolation = hadInterpolation;

        Vector3 velocity;
        Vector3 angularVelocity;

        GetReleaseVelocities(hand, out velocity, out angularVelocity);

        rigidbody.velocity = velocity;
        rigidbody.angularVelocity = angularVelocity;
    }

    public virtual void GetReleaseVelocities(Hand hand, out Vector3 velocity, out Vector3 angularVelocity)
    {
        if (hand.noSteamVRFallbackCamera && releaseVelocityStyle != ReleaseStyle.NoChange)
        {
            releaseVelocityStyle =
                ReleaseStyle.ShortEstimation; // only type that works with fallback hand is short estimation.
        }

        switch (releaseVelocityStyle)
        {
            case ReleaseStyle.ShortEstimation:
                if (velocityEstimator != null)
                {
                    velocityEstimator.FinishEstimatingVelocity();
                    velocity = velocityEstimator.GetVelocityEstimate();
                    angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
                }
                else
                {
                    velocity = rigidbody.velocity;
                    angularVelocity = rigidbody.angularVelocity;
                }

                break;
            case ReleaseStyle.AdvancedEstimation:
                hand.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
                break;
            case ReleaseStyle.GetFromHand:
                velocity = hand.GetTrackedObjectVelocity(releaseVelocityTimeOffset);
                angularVelocity = hand.GetTrackedObjectAngularVelocity(releaseVelocityTimeOffset);
                break;
            default:
            case ReleaseStyle.NoChange:
                velocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;
                break;
        }

        if (releaseVelocityStyle != ReleaseStyle.NoChange)
        {
            float scaleFactor = 1.0f;
            if (scaleReleaseVelocityThreshold > 0)
            {
                scaleFactor =
                    Mathf.Clamp01(
                        scaleReleaseVelocityCurve.Evaluate(velocity.magnitude / scaleReleaseVelocityThreshold));
            }

            velocity *= (scaleFactor * scaleReleaseVelocity);
        }
    }

    protected virtual void HandAttachedUpdate(Hand hand)
    {
        if (onHeldUpdate != null)
        {
            onHeldUpdate.Invoke(hand);
        }
    }

    protected virtual void OnHandFocusAcquired(Hand hand)
    {
        gameObject.SetActive(true);

        if (velocityEstimator != null)
        {
            velocityEstimator.BeginEstimatingVelocity();
        }
    }

    protected virtual void OnHandFocusLost(Hand hand)
    {
        gameObject.SetActive(false);

        if (velocityEstimator != null)
        {
            velocityEstimator.FinishEstimatingVelocity();
        }
    }

    private void PhysicsAttach(GrabBehaviour grabBehaviour,GrabbableHoldPoint grabbableHoldPoint, GameObject gameObjectToAttach = null)
    {
        grabBehaviour.hand.HoverLock(null);
        
        var offset = grabbableHoldPoint.transform.position - rigidbody.worldCenterOfMass;
        offset = Mathf.Min(offset.magnitude, 1.0f) * offset.normalized;
        var holdingPoint = rigidbody.transform.InverseTransformPoint(rigidbody.worldCenterOfMass + offset);

        grabBehaviour.hand.AttachObject(gameObjectToAttach ? gameObjectToAttach : gameObject, inputManager.grabGrabType,
            secondHandAttachmentFlags);
        
        holdingPoints.Add(holdingPoint);
    }
    
    public void PhysicsDetach()
    {
        for (var i = 0; i < grabbableObject.holdingHands.Count; i++)
        {
            var hand = grabbableObject.holdingHands[i];
            Detach(hand, hand.currentAttachedObject);
        }
        
        holdingPoints.Clear();

        perfomingTwoHanded = false;
    }

    private void FixedUpdate()
    {
        if (!perfomingTwoHanded)
        {
            return;
        }

        for (var i = 0; i < grabbableObject.holdingHands.Count; i++)
        {
            var holdingPoint = holdingPoints[i];
            var hand = grabbableObject.holdingHands[i];
            
            var targetPoint = rigidbody.transform.TransformPoint(holdingPoint);
            var vdisplacement = hand.transform.position - targetPoint;

            rigidbody.AddForceAtPosition(attachForce * vdisplacement, targetPoint, ForceMode.Acceleration);
            rigidbody.AddForceAtPosition(-attachForceDamper * rigidbody.GetPointVelocity(targetPoint), targetPoint,
                ForceMode.Acceleration);

            rigidbody.angularVelocity = Vector3.zero;
        }
        
        SetTwoHandsRotation();
    }

    public void SetTwoHandsRotation()
    {
        Transform gripHand = null;
        Transform guardHand = null;
        
        for (var i = 0; i < grabbableObject.grabbableHoldPoints.Length; i++)
        {
            var holdPoint = grabbableObject.grabbableHoldPoints[i];

            if (holdPoint.handPoserName == "HoldingGrip")
            {
                gripHand = holdPoint.grabBehaviour.hand.transform;
                continue;
            }
            
            guardHand = holdPoint.grabBehaviour.hand.transform;
        }

        if (gripHand == null || guardHand == null)
        {
            return;
        }

        var target = guardHand.position - gripHand.position;
        var lookRotation = Quaternion.LookRotation(target);
        
        var gripRotation = Vector3.zero;
        gripRotation.z = gripHand.eulerAngles.z;
        
        lookRotation *= Quaternion.Euler(gripRotation);
        transform.rotation = lookRotation;
    }
}