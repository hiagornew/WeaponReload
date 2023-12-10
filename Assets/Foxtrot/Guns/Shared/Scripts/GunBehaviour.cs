using System.Collections;
using Foxtrot.Audio.Scripts;
using Foxtrot.GrabSystem.Scripts;
using Foxtrot.Guns.Glock17_Magazine.Scripts;
using Foxtrot.Shared.Scripts;
using MyBox;
using UnityEngine;
using UnityEngine.Events;

namespace Foxtrot.Guns.Shared.Scripts
{
    public abstract class GunBehaviour : MonoBehaviour
    {
        [Foldout("Settings", true)]
        [SerializeField]
        protected FireMode fireMode;
        [SerializeField]
        protected int roundsPerMinute = 9999;
        protected float fireInterval;
        protected float timeSinceLastBullet;
        [SerializeField]
        protected int recoilForce;
        [SerializeField]
        protected Axis_t recoilAxis = Axis_t.XAxis;
        protected Vector3 recoilVector;
        [Foldout("Events", true)]
        [SerializeField]
        protected UnityEvent OnFiredLastBullet;
        [SerializeField]
        protected UnityEvent OnFireBullet;
        [SerializeField]
        protected UnityEvent OnReleaseLock;
        [Foldout("References", true)]
        [SerializeField]
        protected GunInformation gunInformation;
        public GrabbableObject grabbableObject;
        public MagazineAttachmentPlace magazineAttachmentPlace;
        [SerializeField]
        protected Animator gunAnimator;
        [SerializeField]
        protected PlaySound shootingSound;
        [SerializeField]
        protected FireBehaviour fireBehaviour;
        protected Hand attachedHand;
        
        protected InputManager InputManager;
        private static readonly int Fire = Animator.StringToHash("Fire");

        protected virtual void OnValidate()
        {
            if (!InputManager)
            {
                InputManager = FindObjectOfType<InputManager>();
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

            if (!gunInformation)
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
            InputManager = InputManager.instance;

            recoilVector = new Vector3(0.0f, 0.0f, 0.0f);
            recoilVector[(int) recoilAxis] = 1.0f;

            fireInterval = 60f / (float) roundsPerMinute;
            timeSinceLastBullet = fireInterval;
        }

        protected virtual void HandAttachedUpdate(Hand hand)
        {
            if (grabbableObject.throwable.holdType == HoldType.TwoHanded)
            {
                //TODO PREVENT RELEASE MAGAZINE IF PRESSING WITH PIPE HAND
            }

            TryEjectMagazine(hand);
            TryShoot(hand);
            TryReleaseLock(hand);
        }

        protected virtual void TryShoot(Hand hand)
        {
            var startingGrabType = hand.GetBestGrabbingType(Hand.GrabTypes.Trigger);

            if (startingGrabType == Hand.GrabTypes.None)
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
                    if (!InputManager.HandCanInteract(hand))
                    {
                        return;
                    }

                    InputManager.PauseHand(hand, Hand.GrabTypes.Trigger);
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
            
            if (!InputManager.GetInput(hand.handType, Hand.GrabTypes.Primary).DownState)
            {
                return;
            }

            magazineAttachmentPlace.EjectMagazine();
        }

        protected virtual void TryReleaseLock(Hand hand)
        {
            if (!InputManager.GetInput(hand.handType, Hand.GrabTypes.Primary).DownState)
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
            if (!gunInformation.CanFire())
            {
                return;
            }

            if (!CheckRPM())
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
                gunAnimator.SetTrigger(Fire);
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
}