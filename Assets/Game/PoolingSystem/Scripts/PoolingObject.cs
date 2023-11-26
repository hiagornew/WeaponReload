using MyBox;
using System;
using UnityEngine;

public class PoolingObject : MonoBehaviour
{
    // [SerializeField] private PlaySound groundHitSound;
    private bool playedSound = false;

    [Foldout("Components to reset", true)]

    #region Rigidbody
    [SerializeField] private Rigidbody thisRigidbody;

    // [Flags]
    // private enum RigidbodyReset
    // {
    //     Velocity = 1 << 0,
    //     AngularVelocity = 1 << 1,
    //     UseGravity = 1 << 2,
    //     IsKinematic = 1 << 3,
    //     Constraints = 1 << 4
    // }
    //
    // [ConditionalField(nameof(thisRigidbody))]
    // [EnumFlags]
    // [SerializeField] private RigidbodyReset rigidbodyReset;

    #endregion

    // public void ResetObject()
    // {
    //     if(thisRigidbody)
    //     {
    //         if((rigidbodyReset & RigidbodyReset.Velocity) == RigidbodyReset.Velocity)               thisRigidbody.velocity = Vector3.zero;
    //         if((rigidbodyReset & RigidbodyReset.AngularVelocity) == RigidbodyReset.AngularVelocity) thisRigidbody.angularVelocity = Vector3.zero;
    //         if((rigidbodyReset & RigidbodyReset.UseGravity) == RigidbodyReset.UseGravity)           thisRigidbody.useGravity = true;
    //         if((rigidbodyReset & RigidbodyReset.IsKinematic) == RigidbodyReset.IsKinematic)         thisRigidbody.isKinematic = false;
    //         if((rigidbodyReset & RigidbodyReset.Constraints) == RigidbodyReset.Constraints)         thisRigidbody.constraints = RigidbodyConstraints.None;
    //     }
    // }

    public void Initialize(Vector3 newPosition, Quaternion newRotation, Transform newParent, bool useLocalPosition)
    {
        transform.parent = newParent;

        if (useLocalPosition)
        {
            transform.localPosition = newPosition;
            transform.localRotation = newRotation;
        }
        else
        {
            transform.position = newPosition;
            transform.rotation = newRotation;
        }
    }

    public void Initialize(Vector3 newPosition, Vector3 newRotation, Transform newParent, bool useLocalPosition)
    {
        transform.parent = newParent;

        if (useLocalPosition)
        {
            transform.localPosition = newPosition;
            transform.localEulerAngles = newRotation;
        }
        else
        {
            transform.position = newPosition;
            transform.eulerAngles = newRotation;
        }
    }

    // private void OnCollisionEnter(Collision collision)
    // {
    //     if(collision.transform.CompareTag("Concrete") && !playedSound && groundHitSound != null)
    //     {
    //         groundHitSound.PlayOneShotSound();
    //     }
    // }
}
