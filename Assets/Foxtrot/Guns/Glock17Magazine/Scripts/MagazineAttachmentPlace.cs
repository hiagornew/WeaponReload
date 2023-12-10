using Foxtrot.Audio.Scripts;
using Foxtrot.Guns.Glock17Magazine.Scripts;
using Foxtrot.Guns.Shared.Scripts;
using MyBox;
using UnityEngine;

namespace Foxtrot.Guns.Glock17_Magazine.Scripts
{
    public class MagazineAttachmentPlace : MonoBehaviour
    {
        [Foldout("References", true)]
        [SerializeField] private GunInformation gunInformation;
        public Transform startPosition;
        public Transform endPosition;
        [SerializeField] private PlaySound attachMag;
        [SerializeField] private PlaySound releaseMag;

        [HideInInspector]
        public MagazineBehaviour currentMagazine;

        private void OnValidate()
        {
            if (!gunInformation)
            {
                gunInformation = GetComponentInParent<GunInformation>();
            }
        }

        public void AttachMagazine(MagazineBehaviour magazineBehaviour)
        {
            if (attachMag != null)
            {
                attachMag.PlayOneShotSound();
            }
            gunInformation.LoadNewMagazine(magazineBehaviour);
            currentMagazine = magazineBehaviour;
        }

        public void EjectMagazine()
        {
            if (releaseMag != null)
            {
                releaseMag.PlayOneShotSound(); 
            }
            gunInformation.RemoveMagazine();
            currentMagazine.EjectFromGun(true);
            currentMagazine = null;
        }
    }
}
