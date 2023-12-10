using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Foxtrot.Target.Scripts
{
    public class TargetsController : MonoBehaviour
    {
        [Foldout("References", true)]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private List<TargetBehaviour> targets = new List<TargetBehaviour>();

        private int targetIndex = 0;

        private void OnValidate()
        {
            if(!targetCamera)
            {
                targetCamera = GetComponentInChildren<Camera>();
            }

            if (targets == null || targets.Count == 0)
            {
                targets = FindObjectsOfType<TargetBehaviour>().ToList();
            }
        }

        public void MoveTargetCamera(TargetBehaviour target)
        {
            targetIndex = targets.IndexOf(target);

            ChangeTargetCameraLocationAndParent();
        }

        public void CycleNextCamera()
        {
            targetIndex++;
            if(targetIndex >= targets.Count)
            {
                targetIndex = 0;
            }

            ChangeTargetCameraLocationAndParent();
        }

        public void CyclePreviousCamera()
        {
            targetIndex--;
            if (targetIndex < 0)
            {
                targetIndex = targets.Count - 1;
            }

            ChangeTargetCameraLocationAndParent();
        }

        private void ChangeTargetCameraLocationAndParent()
        {
            targetCamera.transform.parent = targets[targetIndex].transform;
            targetCamera.transform.position = targets[targetIndex].cameraTransform.position;
            targetCamera.transform.rotation = targets[targetIndex].cameraTransform.rotation;
        }

        public void PullTargetsIn()
        {
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].transform.DOLocalMoveZ(0f, 1 + i).SetEase(Ease.InOutSine);
            }
        }

        public void SendTargetsAway()
        {
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].transform.DOLocalMoveZ(((i + 1) * -5), 1 + i).SetEase(Ease.InOutSine);
            }
        }
    }
}
