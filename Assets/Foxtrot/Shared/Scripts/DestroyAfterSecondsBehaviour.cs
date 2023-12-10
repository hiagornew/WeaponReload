using System.Collections;
using MyBox;
using UnityEngine;

namespace Foxtrot.Shared.Scripts
{
    public class DestroyAfterSecondsBehaviour : MonoBehaviour
    {
        [Foldout("Settings", true)]
        [SerializeField]
        private float delayToDie = 0.5f;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(delayToDie);
            Destroy(gameObject);
        }
    }
}
