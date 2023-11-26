using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;

public abstract class GunBehaviour : MonoBehaviour
{
    [Foldout("Settings", true)]
    [SerializeField]
    protected FireMode fireMode;
    [SerializeField] protected int roundsPerMinute = 9999;
    protected float fireInterval;
    protected float timeSinceLastBullet;
    [SerializeField] protected int recoilForce;
    [SerializeField] protected Axis_t recoilAxis = Axis_t.XAxis;
    protected Vector3 recoilVector;

    [Foldout("Events", true)]
    [SerializeField] protected UnityEvent OnFiredLastBullet;
    [SerializeField] protected UnityEvent OnFireBullet;
    [SerializeField] protected UnityEvent OnReleaseLock;

    [Foldout("References", true)]
    [SerializeField] protected GunInformation gunInformation;
    public GrabbableObject grabbableObject;
    public MagazineAttachmentPlace magazineAttachmentPlace;
    [SerializeField]
    protected InputManager inputManager;
    [SerializeField] protected Animator gunAnimator;
    [SerializeField] protected PlaySound shootingSound;

    [SerializeField]
    protected FireBehaviour fireBehaviour;

    protected Hand attachedHand;

    protected virtual void OnValidate()
    {
        if (!inputManager)
        {
            inputManager = FindObjectOfType<InputManager>();
        }
        
        if (!fireBehaviour)
        {
            fireBehaviour = GetComponentInChildren<FireBehaviour>();
        }

        if (!grabbableObject)
        {
            grabbableObject = GetComponent<GrabbableObject>();
        }

        if (!magazineAttachmentPlace)
        {
            magazineAttachmentPlace = GetComponentInChildren<MagazineAttachmentPlace>();
        }

        if(!gunInformation)
        {
            gunInformation = GetComponentInParent<GunInformation>();
        }

        if (!gunAnimator)
        {
            gunAnimator = GetComponentInParent<Animator>();
        }
    }

    public enum FireMode
    {
        Locked,
        Single,
        SemiAutomatic,
        Automatic,
        Burst,
    }
    public enum Axis_t
    {
        XAxis,
        YAxis,
        ZAxis
    };

    protected virtual void Start()
    {
        recoilVector = new Vector3(0.0f, 0.0f, 0.0f);
        recoilVector[(int)recoilAxis] = 1.0f;

        fireInterval = 60f / (float)roundsPerMinute;
        timeSinceLastBullet = fireInterval;
    }

    protected virtual void HandAttachedUpdate(Hand hand)
    {
        if (grabbableObject.throwable.holdType == HoldType.TwoHanded)
        {
            hand = null;
        
            for (var i = 0; i < grabbableObject.holdingHands.Count; i++)
            {
                hand = grabbableObject.GetGripHand();

                if (hand)
                {
                    break;
                }
            }

            if (!hand)
            {
                return;
            }
        }
        
        TryEjectMagazine(hand);
        TryShoot(hand);
        TryReleaseLock(hand);
    }

    protected virtual void TryShoot(Hand hand)
    {
        var startingGrabType = hand.GetBestGrabbingType(inputManager.fireGrabType, true);

        if (startingGrabType == GrabTypes.None)
        {
            return;
        }
        
        switch (fireMode)
        {
            case FireMode.Locked:
                return;
            case FireMode.Single:
                break;
            case FireMode.SemiAutomatic:
                if (!inputManager.HandCanInteract(hand))
                {
                    return;
                }

                inputManager.PauseHand(hand, inputManager.fireGrabType);
                break;
            case FireMode.Automatic:
                break;
            case FireMode.Burst:
                break;
        }
        
        Trigger();
    }

    protected virtual void TryEjectMagazine(Hand hand)
    {
        if (!magazineAttachmentPlace)
        {
            return;
        }
        
        if (!magazineAttachmentPlace.currentMagazine)
        {
            return;
        }
        
        var dPadType = inputManager.GetBestDPadPressingType(hand, InputManager.PressStateType.Down);

        if (dPadType != InputManager.DPadType.West)
        {
            return;
        }

        magazineAttachmentPlace.EjectMagazine();
    }

    protected virtual void TryReleaseLock(Hand hand)
    {
        var dPadType = inputManager.GetBestDPadPressingType(hand, InputManager.PressStateType.Down);

        if (dPadType != InputManager.DPadType.North)
        {
            return;
        }

        OnReleaseLock.Invoke();
    }
    
    protected virtual void OnAttachedToHand(Hand hand)
    {
        attachedHand = hand;
    }

    protected virtual void OnDetachedFromHand(Hand hand)
    {
        attachedHand = null;
    }

    protected virtual void Trigger()
    {
        if(!gunInformation.CanFire())
        {
            return;
        }

        if(!CheckRPM())
        {
            return;
        }

        fireBehaviour.Shoot();

        OnFireBullet.Invoke();
        StartCoroutine(CountTimeSinceLastShot());

        AddRecoil();
        if (shootingSound != null)
        {
            shootingSound.PlayOneShotSound();
        }

        if (gunInformation.HasBulletsInMagazine())
        {
            gunAnimator.SetTrigger("Fire");
            gunInformation.LoadBulletInChamber();
        }
        else
        {
            OnFiredLastBullet.Invoke();
        }
    }

    protected virtual void AddRecoil()
    {
        transform.Rotate(recoilVector, recoilForce, Space.Self);

        var grabArray = grabbableObject.GetActiveGrabBehaviours();

        for (var i = 0; i < grabArray.Length; i++)
        {
            grabArray[i].handPulseController.BeginPulseLoop(0.2f);
        }
    }

    protected virtual bool CheckRPM()
    {
        return timeSinceLastBullet >= fireInterval;
    }

    protected virtual IEnumerator CountTimeSinceLastShot()
    {
        timeSinceLastBullet = 0;
        while (timeSinceLastBullet < fireInterval)
        {
            yield return null;

            timeSinceLastBullet += Time.deltaTime;
        }
    }
}
