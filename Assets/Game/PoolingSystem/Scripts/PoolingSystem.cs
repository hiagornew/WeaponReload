using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingSystem : MonoBehaviour
{
    public static PoolingSystem instance;

    public enum PoolObject
    {
        Decal,
        Cartridge9mm,
        Projectile9mm,
        bullet9mm,
        Cartridge762x51,
        Projectile762x51,
        bullet762x51
    }

    [Foldout("Parents", true)]
    [SerializeField] private Transform decalParent;

    [SerializeField] private Transform cartridge9mmParent;
    [SerializeField] private Transform projectile9mmParent;
    [SerializeField] private Transform bullet9mmParent;

    [SerializeField] private Transform cartridge762x51Parent;
    [SerializeField] private Transform projectile762x51Parent;
    [SerializeField] private Transform bullet762x51Parent;

    private Queue<PoolingObject> decals;

    private Queue<PoolingObject> cartridges9mm;
    private Queue<PoolingObject> projectiles9mm;
    private Queue<PoolingObject> bullets9mm;

    private Queue<PoolingObject> cartridges762x51;
    private Queue<PoolingObject> projectiles762x51;
    private Queue<PoolingObject> bullets762x51;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        InitializeQueues();
    }

    private void InitializeQueues()
    {
        decals = new Queue<PoolingObject>(decalParent.GetComponentsInChildren<PoolingObject>(true));

        cartridges9mm = new Queue<PoolingObject>(cartridge9mmParent.GetComponentsInChildren<PoolingObject>(true));
        projectiles9mm = new Queue<PoolingObject>(projectile9mmParent.GetComponentsInChildren<PoolingObject>(true));
        bullets9mm = new Queue<PoolingObject>(bullet9mmParent.GetComponentsInChildren<PoolingObject>(true));

        cartridges762x51 = new Queue<PoolingObject>(cartridge762x51Parent.GetComponentsInChildren<PoolingObject>(true));
        projectiles762x51 = new Queue<PoolingObject>(projectile762x51Parent.GetComponentsInChildren<PoolingObject>(true));
        bullets762x51 = new Queue<PoolingObject>(bullet762x51Parent.GetComponentsInChildren<PoolingObject>(true));
    }

    public GameObject InstantiateObject(PoolObject objectToInstantiate, Vector3 newPosition, Quaternion newRotation, Transform newParent = null, bool useLocalPosition = false)
    {
        PoolingObject newObject = new PoolingObject();

        switch (objectToInstantiate)
        {
            case PoolObject.Decal:
                if(newParent == null)
                {
                    newParent = decalParent;
                }

                newObject = decals.Dequeue();
                decals.Enqueue(newObject);
                break;

            case PoolObject.Cartridge9mm:
                if (newParent == null)
                {
                    newParent = cartridge9mmParent;
                }

                newObject = cartridges9mm.Dequeue();
                cartridges9mm.Enqueue(newObject);
                break;

            case PoolObject.Projectile9mm:
                if (newParent == null)
                {
                    newParent = projectile9mmParent;
                }

                newObject = projectiles9mm.Dequeue();
                projectiles9mm.Enqueue(newObject);
                break;

            case PoolObject.bullet9mm:
                if (newParent == null)
                {
                    newParent = bullet9mmParent;
                }

                newObject = bullets9mm.Dequeue();
                bullets9mm.Enqueue(newObject);
                break;

            case PoolObject.Cartridge762x51:
                if (newParent == null)
                {
                    newParent = cartridge762x51Parent;
                }

                newObject = cartridges762x51.Dequeue();
                cartridges762x51.Enqueue(newObject);
                break;

            case PoolObject.Projectile762x51:
                if (newParent == null)
                {
                    newParent = projectile762x51Parent;
                }

                newObject = projectiles762x51.Dequeue();
                projectiles762x51.Enqueue(newObject);
                break;

            case PoolObject.bullet762x51:
                if (newParent == null)
                {
                    newParent = bullet762x51Parent;
                }

                newObject = bullets762x51.Dequeue();
                bullets762x51.Enqueue(newObject);
                break;
        }

        // newObject.ResetObject();
        newObject.Initialize(newPosition, newRotation, newParent, useLocalPosition);
        newObject.gameObject.SetActive(false);
        newObject.gameObject.SetActive(true);

        return newObject.gameObject;
    }

    public GameObject InstantiateObject(PoolObject objectToInstantiate, Vector3 newPosition, Vector3 newRotation, Transform newParent = null, bool useLocalPosition = false)
    {
        PoolingObject newObject = new PoolingObject();

        switch (objectToInstantiate)
        {
            case PoolObject.Decal:
                if (newParent == null)
                {
                    newParent = decalParent;
                }

                newObject = decals.Dequeue();
                decals.Enqueue(newObject);
                break;

            case PoolObject.Cartridge9mm:
                if (newParent == null)
                {
                    newParent = cartridge9mmParent;
                }

                newObject = cartridges9mm.Dequeue();
                cartridges9mm.Enqueue(newObject);
                break;

            case PoolObject.Projectile9mm:
                if (newParent == null)
                {
                    newParent = projectile9mmParent;
                }

                newObject = projectiles9mm.Dequeue();
                projectiles9mm.Enqueue(newObject);
                break;

            case PoolObject.bullet9mm:
                if (newParent == null)
                {
                    newParent = bullet9mmParent;
                }

                newObject = bullets9mm.Dequeue();
                bullets9mm.Enqueue(newObject);
                break;

            case PoolObject.Cartridge762x51:
                if (newParent == null)
                {
                    newParent = cartridge762x51Parent;
                }

                newObject = cartridges762x51.Dequeue();
                cartridges762x51.Enqueue(newObject);
                break;

            case PoolObject.Projectile762x51:
                if (newParent == null)
                {
                    newParent = projectile762x51Parent;
                }

                newObject = projectiles762x51.Dequeue();
                projectiles762x51.Enqueue(newObject);
                break;

            case PoolObject.bullet762x51:
                if (newParent == null)
                {
                    newParent = bullet762x51Parent;
                }

                newObject = bullets762x51.Dequeue();
                bullets762x51.Enqueue(newObject);
                break;
        }

        // newObject.ResetObject();
        newObject.Initialize(newPosition, newRotation, newParent, useLocalPosition);
        newObject.gameObject.SetActive(false);
        newObject.gameObject.SetActive(true);

        return newObject.gameObject;
    }
}
