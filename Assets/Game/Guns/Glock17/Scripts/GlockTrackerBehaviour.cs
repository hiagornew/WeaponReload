using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class GlockTrackerBehaviour : GunBehaviour
{
    private Quaternion previousRotation;
    private Coroutine recoilRoutine;
    
    protected override void OnValidate()
    {
        if (!inputManager)
        {
            inputManager = FindObjectOfType<InputManager>();
        }
        
        if (!fireBehaviour)
        {
            fireBehaviour = GetComponentInChildren<FireBehaviour>();
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

    private void LateUpdate()
    {
        if (!gunInformation.fixedMagazine)
        {
            return;
        }
        
        TryShoot(null);
    }

    protected override void TryShoot(Hand hand)
    {
        var pressState = inputManager.GetTrackerTriggerState();
        
        if (pressState != InputManager.PressStateType.Down)
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
                break;
            case FireMode.Automatic:
                break;
            case FireMode.Burst:
                break;
        }
        
        Trigger();
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
    
    protected override void AddRecoil()
    {
        if (recoilRoutine != null)
        {
            transform.localRotation = previousRotation;
            StopCoroutine(recoilRoutine);
            recoilRoutine = null;
        }
        
        recoilRoutine = StartCoroutine(RecoilSimulation());
    }

    private IEnumerator RecoilSimulation()
    {
        previousRotation = transform.localRotation;
        transform.Rotate(recoilVector, recoilForce, Space.Self);

        yield return transform.DOLocalRotateQuaternion(previousRotation, 0.1f).SetEase(Ease.InFlash).WaitForCompletion();
        recoilRoutine = null;
    }
}
