using MyBox;
using UnityEngine;

namespace Foxtrot.Target.Scripts
{
    public class TargetBehaviour : MonoBehaviour
    {
        [Foldout("Settings", true)]
        public bool isPaper;
    
        [Foldout("References", true)]
        public Transform cameraTransform;
        [SerializeField]
        private GameObject holeDecal;
        [SerializeField]
        private TargetsController targetsController;

        private void OnValidate()
        {
            if(!targetsController)
            {
                targetsController = FindObjectOfType<TargetsController>();
            }
        }

        public void CreateHole(Vector3 point)
        {
            point.z = transform.position.z;
            var go = Instantiate(holeDecal, point, Quaternion.Euler(new Vector3(90,0,0)), transform);
        
            Debug.Log("foi?", go);
        
            targetsController.MoveTargetCamera(this);
        }
    }
}
