using System.Collections;
using System.Collections.Generic;
using Foxtrot.GrabSystem.Scripts;
using Foxtrot.Scenario.EPI.Scripts;
using MyBox;
using UnityEngine;

namespace Foxtrot.EPI.Scripts
{
    public class EPIController : MonoBehaviour
    {
        [Foldout("References", true)]
        [SerializeField] 
        private CollectBehaviour[] collectBehaviours;

        [Foldout("Information", true)]
        [SerializeField,ReadOnly]
        private GrabBehaviour[] grabBehaviours;
        [SerializeField,ReadOnly]
        private List<CollectBehaviour> listOfCollectBehaviour = new();

        private WaitForSeconds waitForSeconds;
    
        private void OnValidate()
        {
            if (grabBehaviours == null || grabBehaviours.Length == 0 )
            {
                grabBehaviours = FindObjectsOfType<GrabBehaviour>();
            }
        }

        public void EPICollected(CollectBehaviour collectBehaviour)
        {
            if (listOfCollectBehaviour.Contains(collectBehaviour))
            {
                return;
            }

            listOfCollectBehaviour.Add(collectBehaviour);

            if (listOfCollectBehaviour.Count != collectBehaviours.Length)
            {
                return;
            }
        }
    }
}
