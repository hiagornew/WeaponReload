using System.Collections;
using Foxtrot.Audio.Scripts;
using Foxtrot.GrabSystem.Scripts;
using Foxtrot.Guns.Shared.Scripts;
using MyBox;
using UnityEngine;

namespace Foxtrot.Guns.Glock17.Scripts
{
    [SelectionBase]
    public class SlideBehaviour : MonoBehaviour
    {
        [Foldout("Settings", true)]
        [SerializeField] private float timeToCloseSlide = 0.05f;
        [Range(0, 1)]
        [SerializeField] private float fullyOpenSlideThreshold = 0.85f;
        [SerializeField] private float baseEjectionForce = 2.5f;
        [SerializeField] private bool openSlideAfterLastBullet;

        [Foldout("References", true)]
        [ReadOnly]
        [SerializeField] private GunInformation gunInformation;
        [ReadOnly]
        [SerializeField] private Animator gunAnimator;
        [ReadOnly]
        [SerializeField] private LinearMapping linearMapping;
        [SerializeField] private Transform ejectPoint;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject casingPrefab;
        [SerializeField] private PlaySound slidePullSound;
        [SerializeField] private PlaySound slideReturnSound;

        [Foldout("Information", true)]
        [ReadOnly]
        [SerializeField] private bool isFullyOpen = false;
        [ReadOnly]
        [SerializeField] private bool isLocked = false;

        private const string IsInteractingWithSlide = "IsInteractingWithSlide";
        private const string NormalizedSlideTime = "NormalizedSlideTime";
        private const string NormalizedReturnTime = "NormalizedReturnTime";
        private const string IsSlideClosed = "IsSlideClosed";
        private const string PullBackSlide = "PullBackSlide";

        private void OnValidate()
        {
            if(!gunAnimator)
            {
                gunAnimator = GetComponentInParent<Animator>();
            }

            if (!linearMapping)
            {
                linearMapping = GetComponent<LinearMapping>();
            }

            if (!gunInformation)
            {
                gunInformation = GetComponentInParent<GunInformation>();
            }
        }

        private void OnAttachedToHand(Hand hand)
        {
            isLocked = false;
            gunAnimator.speed = 1;
            gunAnimator.SetBool(IsInteractingWithSlide, true);
            SetSlideClosed(false);
            StopAllCoroutines();
        }

        private void HandAttachedUpdate(Hand hand)
        {
            UpdateNormalizedTimes();

            if(linearMapping.value >= fullyOpenSlideThreshold && !isFullyOpen)
            {
                isFullyOpen = true;

                if(slidePullSound != null)
                {
                    slidePullSound.PlayOneShotSound();
                }
                if (gunInformation.hasBulletInChamber)
                {
                    EjectBullet();
                }
            }
            else if(linearMapping.value < fullyOpenSlideThreshold && isFullyOpen)
            {
                if (slideReturnSound != null)
                {
                    slideReturnSound.PlayOneShotSound();
                }

                if (gunInformation.HasBulletsInMagazine())
                {
                    gunInformation.LoadBulletInChamber();
                }
                isFullyOpen = false;
            }
        }

        private void OnDetachedFromHand(Hand hand)
        {
            gunAnimator.SetBool(IsInteractingWithSlide, false);
            StartCoroutine(ReturnLinearMappingToZero());
        }

        private IEnumerator ReturnLinearMappingToZero()
        {
            while(linearMapping.value > 0)
            {
                linearMapping.value -= Time.deltaTime / timeToCloseSlide;
                yield return null;
                UpdateNormalizedTimes();

                if (linearMapping.value < fullyOpenSlideThreshold && isFullyOpen)
                {
                    if (slideReturnSound != null)
                    {
                        slideReturnSound.PlayOneShotSound();
                    }

                    if (gunInformation.HasBulletsInMagazine())
                    {
                        gunInformation.LoadBulletInChamber();
                    }
                    isFullyOpen = false;
                }

            }

            linearMapping.value = 0;
            UpdateNormalizedTimes();
            SetSlideClosed(true);
        }

        private void SetSlideClosed(bool value)
        {
            gunAnimator.SetBool(IsSlideClosed, value);
            gunInformation.isSlideClosed = value;
        }

        private void UpdateNormalizedTimes()
        {
            gunAnimator.SetFloat(NormalizedSlideTime, linearMapping.value);
            gunAnimator.SetFloat(NormalizedReturnTime, 1 - linearMapping.value);
        }

        public void OpenSlideAndLock()
        {
            SetSlideClosed(false);
            gunInformation.hasBulletInChamber = false;

            isLocked = true;
            isFullyOpen = true;
            if (openSlideAfterLastBullet)
            {
                linearMapping.value = fullyOpenSlideThreshold;
                UpdateNormalizedTimes();
                gunAnimator.speed = 0;
                gunAnimator.Play(PullBackSlide, 0, linearMapping.value); 
            }
        }

        public void ReleaseLock()
        {
            if(!isLocked)
            {
                return;
            }

            gunAnimator.speed = 1;
            isLocked = false;

            if (!openSlideAfterLastBullet)
            {
                if (gunInformation.HasBulletsInMagazine())
                {
                    gunInformation.LoadBulletInChamber();
                }
            }

            StartCoroutine(ReturnLinearMappingToZero());
        }

        public void EjectCasing()
        {
            GameObject cartridge = global::Foxtrot.PoolingSystem.Scripts.PoolingSystem.Instance.InstantiateObject(global::Foxtrot.PoolingSystem.Scripts.PoolingSystem.PoolObject.Cartridge9mm, ejectPoint.position, ejectPoint.rotation);
            cartridge.GetComponent<Rigidbody>().velocity = cartridge.transform.right * (baseEjectionForce * Random.Range(.5f, 1.5f));
        }

        public void EjectBullet()
        {
            gunInformation.hasBulletInChamber = false;
            GameObject bullet = global::Foxtrot.PoolingSystem.Scripts.PoolingSystem.Instance.InstantiateObject(global::Foxtrot.PoolingSystem.Scripts.PoolingSystem.PoolObject.bullet9mm, ejectPoint.position, ejectPoint.rotation);
            bullet.GetComponent<Rigidbody>().velocity = bullet.transform.right * (baseEjectionForce * Random.Range(.5f, 1.5f));
        }
    }
}
