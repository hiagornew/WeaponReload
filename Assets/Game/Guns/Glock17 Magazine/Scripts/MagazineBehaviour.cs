using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class MagazineBehaviour : CustomLinearDrive
{
    [Foldout("Settings", true)]
    [SerializeField]
    private int maxBullets = 17;
    [SerializeField]
    private float distanceToExit = 1f;

    [Foldout("References", true)]
    [SerializeField]
    private GrabbableObject grabbableObject;
    [SerializeField] private GameObject bulletObject;


    [Foldout("Debug", true)]
    [SerializeField] private bool refillOnEject;

    private GunBehaviour gunBehaviour;

    private bool path;

    [ReadOnly]
    public int bulletsQuantity;

    private Coroutine ejecting;

    private void Start()
    {
        LoadBullets();
    }

    private void LoadBullets()
    {
        bulletsQuantity = maxBullets;
        bulletObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Magazine"))
        {
            return;
        }

        var collidedGunBehaviour = other.GetComponentInParent<GunBehaviour>();

        StartAttachingMagazine(collidedGunBehaviour);
    }

    private void OnDetachedFromHand(Hand hand)
    {
        if (!path)
        {
            return;
        }
        
        if (LinearMapping.value < 0.5f)
        {
            EjectFromGun();
        }
    }

    private void StartAttachingMagazine(GunBehaviour gunBehaviour)
    {
        if (path)
        {
            return;
        }
        
        if (grabbableObject.holdingHands.Count == 0)
        {
            return;
        }

        if (this.gunBehaviour)
        {
            return;
        }

        this.gunBehaviour = gunBehaviour;

        if (this.gunBehaviour.grabbableObject.holdingHands.Count == 0)
        {
            return;
        }
        
        SetProperties(this.gunBehaviour.magazineAttachmentPlace.startPosition, 
            this.gunBehaviour.magazineAttachmentPlace.endPosition, 
            grabbableObject.transform);

        grabbableObject.rigidbody.isKinematic = true;
        grabbableObject.SetColliders(false);

        var grabbableTransform = hostTransform;
        grabbableTransform.parent = this.gunBehaviour.magazineAttachmentPlace.transform;
        grabbableTransform.position = startTransform.position;
        grabbableTransform.rotation = startTransform.rotation;

        initialMappingOffset = LinearMapping.value;
        UpdateLinearMapping(grabbableTransform);
        UpdateHandPoser();

        initialMappingOffset = LinearMapping.value - CalculateLinearMapping(grabbableObject.holdingHands[0].transform);

        path = true;
    }

    private void AttachToGun()
    {
        path = false;

        LinearMapping.value = 1;
        
        var grabbableTransform = hostTransform;
        grabbableTransform.position = endTransform.position;
        grabbableTransform.rotation = endTransform.rotation;

        var lastHand = grabbableObject.holdingHands[0];
        grabbableObject.Detach(lastHand);
        
        gunBehaviour.magazineAttachmentPlace.AttachMagazine(this);
        
        SnapToSecondHand(lastHand);
        
        grabbableObject.AddIgnoreHovering();
    }
    
    private void SnapToSecondHand(Hand lastHand)
    {
        var secondHandBehaviour = gunBehaviour.GetComponentInChildren<SecondHandBehaviour>();

        if (!secondHandBehaviour)
        {
            return;
        }
        
        secondHandBehaviour.Grab(lastHand);
    }
    
    private void ExitAttachingMagazine(bool maintain = true)
    {
        path = false;

        hostTransform.parent = null;

        grabbableObject.rigidbody.isKinematic = false;
        grabbableObject.SetColliders(true);
        
        if (maintain)
        {
            StartCoroutine(WaitHandReposition());
        }
        
        gunBehaviour = null;
        
        LinearMapping.value = 0;
        UpdateHandPoser();
    }

    public void EjectFromGun(bool isAttached = false)
    {
        if (ejecting != null)
        {
            return;
        }
        
        ejecting = StartCoroutine(EjectAnimation());

        if(refillOnEject)
        {
            LoadBullets();
        }
    }

    public void RemoveBullet()
    {
        bulletsQuantity--;

        if(bulletsQuantity <= 0)
        {
            bulletObject.SetActive(false);
        }
    }

    protected virtual void HandAttachedUpdate(Hand hand)
    {
        if (!path)
        {
            return;
        }

        var distance = Vector3.Distance(hand.transform.position, endTransform.position);

        if (distance > distanceToExit)
        {
            ExitAttachingMagazine();
            return;
        }

        if (grabbableObject.holdingHands.Count > 0)
        {
            UpdateLinearMapping(grabbableObject.holdingHands[0].transform);
            UpdateHandPoser();
        }
        
        if (LinearMapping.value == 1)
        {
            AttachToGun();
        }
    }

    private void UpdateHandPoser()
    {
        grabbableObject.throwable.interactable.skeletonPoser.SetBlendingBehaviourValue("Pushing", LinearMapping.value);
    }

    private IEnumerator EjectAnimation()
    {
        grabbableObject.AddIgnoreHovering();
        
        yield return hostTransform.DOLocalMove(startTransform.localPosition, 0.2f).SetEase(Ease.Linear).WaitForCompletion();
        
        ExitAttachingMagazine(false);
        
        grabbableObject.RemoveIgnoreHovering();

        ejecting = null;
    }

    private IEnumerator WaitHandReposition()
    {
        grabbableObject.SetColliders(false, true);
        
        while (Vector3.Distance(grabbableObject.transform.position, grabbableObject.holdingHands[0].transform.position) > 0.15f)
        {
            yield return null;
        }
        
        grabbableObject.SetColliders(true, true);
    }

    private void OnDrawGizmosSelected()
    {
        if (grabbableObject.holdingHands.Count == 0)
        {
            return;
        }
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(grabbableObject.holdingHands[0].transform.position, distanceToExit);
    }
}