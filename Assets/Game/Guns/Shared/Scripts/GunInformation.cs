using MyBox;
using UnityEngine;

public class GunInformation : MonoBehaviour
{
    [ReadOnly]
    [SerializeField] private int totalBulletsQuantity;
    public bool fixedMagazine;

    public bool hasBulletInChamber;
    public bool isSlideClosed;

    public MagazineBehaviour attachedMagazine;

    private void Start()
    {
        Reset();
    }

    public void Reset()
    {
        hasBulletInChamber = fixedMagazine;
        isSlideClosed = true;
    }

    public bool CanFire()
    {
        return isSlideClosed && hasBulletInChamber;
    }

    public bool HasBulletsInMagazine()
    {
        if (fixedMagazine)
        {
            return true;
        }
        
        totalBulletsQuantity = GetTotalBulletsQuantity();

        if(!attachedMagazine)
        {
            return false;
        }

        return attachedMagazine.bulletsQuantity > 0;
    }

    public void LoadNewMagazine(MagazineBehaviour newMagazine)
    {
        attachedMagazine = newMagazine;
        totalBulletsQuantity = GetTotalBulletsQuantity();
    }

    public void RemoveMagazine()
    {
        attachedMagazine = null;

        totalBulletsQuantity = GetTotalBulletsQuantity();
    }

    public virtual void LoadBulletInChamber()
    {
        if (fixedMagazine)
        {
            return;
        }
        
        hasBulletInChamber = true;
        attachedMagazine.RemoveBullet();

        totalBulletsQuantity = GetTotalBulletsQuantity();
    }

    public int GetTotalBulletsQuantity()
    {
        return (attachedMagazine ? attachedMagazine.bulletsQuantity : 0) + (hasBulletInChamber ? 1 : 0);
    }

}
