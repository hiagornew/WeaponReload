using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class FireBehaviour : MonoBehaviour
{
    [Foldout("Settings", true)]
    [SerializeField]
    private float bulletSpeed = 375;
    [SerializeField]
    private bool debug = false;
    [SerializeField]
    private LayerMask targetLayers;

    [Foldout("References", true)]
    [SerializeField]
    private BulletImpactController bulletImpactController;
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private GameObject muzzleFlash;
    
    private List<BulletBehaviour> bullets = new List<BulletBehaviour>();

    private void OnValidate()
    {
        if (!bulletImpactController)
        {
            bulletImpactController = FindObjectOfType<BulletImpactController>();
        }
    }

    public void Shoot()
    {
        BulletTrail.instance.SetTrailInitialPosition(transform.position);

        var bulletGameObject = PoolingSystem.instance.InstantiateObject(PoolingSystem.PoolObject.Projectile9mm, transform.position, Quaternion.identity);
        var bullet = bulletGameObject.GetComponent<BulletBehaviour>();

        bullet.bulletImpactController = bulletImpactController;

        bullet.transform.rotation = Quaternion.LookRotation(transform.forward);
        bullet.rigidbody.velocity = transform.forward * bulletSpeed;

        var muzzle = Instantiate(muzzleFlash, transform.position, transform.rotation, null);
        
        if (!debug)
        {
            muzzle.hideFlags = HideFlags.HideInHierarchy;
        }

        if (Physics.Raycast(transform.position,transform.forward,out var hit, float.PositiveInfinity, targetLayers, QueryTriggerInteraction.Collide))
        {
            var targetBehaviour = hit.collider.GetComponentInParent<TargetBehaviour>();

            if (targetBehaviour)
            {
                if (targetBehaviour.isPaper)
                {
                    targetBehaviour.CreateHole(hit.point);
                }
            }
        }
    }
}
