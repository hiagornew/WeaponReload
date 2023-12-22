using System;
using System.Collections;
using DG.Tweening;
using Foxtrot.Shared.Scripts;
using MyBox;
using UnityEngine;

namespace Foxtrot.GrabSystem.Scripts
{
    public class GrabBehaviour : MonoBehaviour
    {
        [Foldout("Settings", true)]
        [SerializeField]
        private bool showLaser;

        [SerializeField] private float distance;
        [SerializeField] private float scanSize;
        [SerializeField] private LayerMask grabbableLayers;

        public Hand hand;

        [Foldout("Prefabs", true)] [SerializeField]
        private GameObject scanPrefab;

        [SerializeField] private GameObject iconPrefab;

        [Foldout("Information", true)] [ReadOnly]
        public GrabbableObject currentGrabbedObject;

        [SerializeField, ReadOnly] private GameObject instantiatedScan;
        [SerializeField, ReadOnly] private GameObject instantiatedIcon;
        [ReadOnly] public GrabingStep grabingStep;
        
        private Transform lastIconTransform;
        private Coroutine grabbingAnimation;

        private InputManager _inputManager;

        public enum GrabingStep
        {
            Paused,
            Waiting,
            Scaning,
            Grabbed,
        }

        private void Start()
        {
            _inputManager = InputManager.instance;
        }

        private void Update()
        {
            switch (grabingStep)
            {
                case GrabingStep.Waiting:
                    Scaning();
                    break;
                case GrabingStep.Scaning:
                    Scaning();
                    EndingScan();
                    break;
            }
        }

        private void Scaning()
        {
            if (!_inputManager.HandCanInteract(hand))
            {
                return;
            }

            if (currentGrabbedObject)
            {
                return;
            }

            if (hand.currentAttachedObject)
            {
                return;
            }

            if (hand.hoveringInteractable)
            {
                ClearInstantiateScan();
                return;
            }

            var startingGrabType = hand.GetBestGrabbingType(Hand.GrabTypes.Grip);

            if (startingGrabType == Hand.GrabTypes.None)
            {
                if (!_inputManager.GetInput(hand.handType, Hand.GrabTypes.Grip).UpState)
                {
                    return;
                }

                grabingStep = GrabingStep.Waiting;
                ClearInstantiateScan();
                return;
            }

            grabingStep = GrabingStep.Scaning;

            if (!showLaser)
            {
                return;
            }

            RaycastHit hit;
            Vector3 point;

            if (!Physics.Raycast(transform.position, transform.forward, out hit, distance, 1 << 0 | 1 << 9))
            {
                return;
            }

            point = hit.point;

            if (Physics.SphereCast(transform.position, scanSize / 2, transform.forward, out hit, distance, grabbableLayers))
            {
                if (lastIconTransform != hit.collider.transform)
                {
                    ClearInstantiateIcon();
                }

                var colliderTransform = hit.collider.transform;
                lastIconTransform = colliderTransform;

                point = colliderTransform.position;

                if (!instantiatedIcon)
                {
                    instantiatedIcon = Instantiate(iconPrefab, point, Quaternion.identity, null);
                }
            }
            else
                if (instantiatedIcon)
                {
                    ClearInstantiateIcon();
                }

            if (!instantiatedScan)
            {
                instantiatedScan = Instantiate(scanPrefab, transform.position, Quaternion.identity, null);
            }

            var offset = point - transform.position;
            var position = transform.position + (offset / 2);

            instantiatedScan.transform.localScale = new Vector3(scanSize / 2, offset.magnitude / 2, scanSize / 2);
            instantiatedScan.transform.up = offset;
            instantiatedScan.transform.position = position;

            if (instantiatedIcon)
            {
                instantiatedIcon.transform.LookAt(transform.position, Vector3.up);
            }
        }

        private void EndingScan()
        {
            if (!_inputManager.GetInput(hand.handType, Hand.GrabTypes.Grip).UpState)
            {
                return;
            }

            ClearInstantiateScan();

            if (!TryGrab())
            {
                grabingStep = GrabingStep.Waiting;
            }
        }

        private bool TryGrab()
        {
            RaycastHit hit;

            if (!Physics.SphereCast(transform.position, scanSize / 2, transform.forward, out hit, distance,
                    grabbableLayers))
            {
                return false;
            }

            var grabbable = hit.collider.GetComponentInParent<GrabbableObject>();

            if (!grabbable)
            {
                return false;
            }

            if (grabbable.ignoreHovering)
            {
                return false;
            }

            if (grabbable.firstGrabBehaviour)
            {
                return false;
            }

            if (grabbingAnimation != null)
            {
                return false;
            }

            if (grabbable.throwable.holdType == HoldType.TwoHanded)
            {
                var holdPoint = hit.collider.GetComponent<GrabbableHoldPoint>();

                if (!holdPoint)
                {
                    return false;
                }

                if (holdPoint.grabBehaviour)
                {
                    return false;
                }

                GrabObject(holdPoint, true);
            }
            else
            {
                GrabObject(grabbable, true);
            }

            return true;
        }

        public void GrabObject(GrabbableObject grabbable, bool byDistance = false)
        {
            currentGrabbedObject = grabbable;

            grabingStep = GrabingStep.Grabbed;

            if (byDistance)
            {
                grabbingAnimation = StartCoroutine(GrabbingAnimation());
                return;
            }

            currentGrabbedObject.Grab(hand, this);
        }

        public void GrabObject(GrabbableHoldPoint grabbableHoldPoint, bool byDistance = false)
        {
            grabbableHoldPoint.grabBehaviour = this;

            currentGrabbedObject = grabbableHoldPoint.grabbableObject;

            grabingStep = GrabingStep.Grabbed;

            if (byDistance)
            {
                grabbingAnimation = StartCoroutine(GrabbingAnimation(grabbableHoldPoint));
                return;
            }

            currentGrabbedObject.Grab(hand, grabbableHoldPoint, currentGrabbedObject.firstHandHoldPoint);
        }

        private IEnumerator GrabbingAnimation()
        {
            currentGrabbedObject.rigidbody.isKinematic = true;
            currentGrabbedObject.SetColliders(false);

            var tweener = currentGrabbedObject.transform.DOMove(transform.position, 0.1f);
            yield return tweener.WaitForCompletion();

            currentGrabbedObject.Grab(hand, this);

            currentGrabbedObject.rigidbody.isKinematic = false;
            currentGrabbedObject.SetColliders(true);

            grabbingAnimation = null;
        }

        private IEnumerator GrabbingAnimation(GrabbableHoldPoint grabbableHoldPoint)
        {
            currentGrabbedObject.rigidbody.isKinematic = true;
            currentGrabbedObject.SetColliders(false);

            var tweener = currentGrabbedObject.transform.DOMove(transform.position, 0.1f);
            yield return tweener.WaitForCompletion();

            currentGrabbedObject.Grab(hand, grabbableHoldPoint, currentGrabbedObject.firstHandHoldPoint);

            currentGrabbedObject.rigidbody.isKinematic = false;
            currentGrabbedObject.SetColliders(true);

            grabbingAnimation = null;
        }

        public void GrabEnding()
        {
            if (!_inputManager.HandCanInteract(hand))
            {
                return;
            }

            if (!_inputManager.GetInput(hand.handType, Hand.GrabTypes.Grip).DownState)
            {
                return;
            }

            _inputManager.PauseHand(hand, Hand.GrabTypes.Grip);

            currentGrabbedObject.Detach(hand);
        }

        public void GrabEnding(GrabbableHoldPoint grabbableHoldPoint)
        {
            var distance = Vector3.Distance(grabbableHoldPoint.holdPosition.position, hand.transform.position);

            if (currentGrabbedObject.holdingHands.Count < 2 || 
                !grabbableHoldPoint.detachByDistance || 
                distance < grabbableHoldPoint.distanceToDetach)
            {
                if (!_inputManager.HandCanInteract(hand))
                {
                    return;
                }

                if (!_inputManager.GetInput(hand.handType, Hand.GrabTypes.Grip).DownState)
                {
                    return;
                }

                _inputManager.PauseHand(hand, Hand.GrabTypes.Grip);
            }

            currentGrabbedObject.Detach(hand, grabbableHoldPoint);
        }

        private void ClearInstantiateScan()
        {
            if (!showLaser)
            {
                return;
            }

            if (!instantiatedScan)
            {
                return;
            }

            Destroy(instantiatedScan);
            instantiatedScan = null;

            ClearInstantiateIcon();
        }

        private void ClearInstantiateIcon()
        {
            if (!instantiatedIcon)
            {
                return;
            }

            lastIconTransform = null;

            Destroy(instantiatedIcon);
            instantiatedIcon = null;
        }
    }
}