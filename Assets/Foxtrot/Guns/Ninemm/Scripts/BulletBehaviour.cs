using Foxtrot.Audio.Scripts;
using Foxtrot.Guns.Shared.Scripts;
using Foxtrot.Shared.Scripts;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Foxtrot.Guns.Ninemm.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class BulletBehaviour : MonoBehaviour
    {
        [Foldout("Settings")]
        [SerializeField]
        private bool impactOnFirstCollision = true;

        [SerializeField]
        private bool debug = false;

        [Foldout("References", true)]
        public new Rigidbody rigidbody;
        [SerializeField] 
        private PlaySound impactSound;

        [HideInInspector]
        public BulletImpactController bulletImpactController;

        private bool alreadyImpacted;

        private void OnValidate()
        {
            if (!rigidbody)
            {
                rigidbody = GetComponent<Rigidbody>();

                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }

        private void OnEnable()
        {
            alreadyImpacted = false;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (impactOnFirstCollision)
            {
                if (alreadyImpacted)
                {
                    return;
                }

                alreadyImpacted = true;
            }

            var otherGameObject = other.gameObject;
            var collisionPoint = other.contacts[0].point;
            var collisionNormal = other.contacts[0].normal;

            BulletTrail.instance.SetTrailFinalPosition(collisionPoint);

            switch (otherGameObject.tag)
            {
                case "Concrete":
                    var impact = Instantiate(bulletImpactController.concreteImpactPrefab, collisionPoint, Quaternion.LookRotation(collisionNormal), null);
                    var decal = PoolingSystem.Scripts.PoolingSystem.Instance.InstantiateObject(PoolingSystem.Scripts.PoolingSystem.PoolObject.Decal, collisionPoint, Quaternion.LookRotation(-collisionNormal), otherGameObject.transform);

                    decal.transform.position += -decal.transform.forward * 0.0001f;

                    var randomScale = Random.Range(0.02f, 0.15f);
                    decal.transform.localScale = 
                        new Vector3(randomScale,randomScale,randomScale);
                    var localEulerAngles = decal.transform.localEulerAngles;
                    localEulerAngles = 
                        new Vector3(localEulerAngles.x,localEulerAngles.y,Random.Range(0f,360f));
                    decal.transform.localEulerAngles = localEulerAngles;

                    if (debug)
                    {
                        decal.hideFlags = HideFlags.HideInHierarchy;
                        impact.hideFlags = HideFlags.HideInHierarchy;
                    }

                    impactSound.PlayOneShotSound();
                    break;
            }
        }
    }
}