using MyBox;
using UnityEngine;

namespace Foxtrot.Guns.Shared.Scripts
{
    public class BulletImpactController : MonoBehaviour
    {
        [Foldout("Prefab References", true)]
        public GameObject concreteImpactPrefab;

        public GameObject concreteDecalPrefab;
    }
}
