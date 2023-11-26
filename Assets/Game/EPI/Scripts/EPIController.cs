using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class EPIController : MonoBehaviour
{
    [Foldout("References", true)] 
    [SerializeField]
    private Transform wardrobeTransform;
    [SerializeField]
    private Transform standTransform;
    [SerializeField] 
    private CollectBehaviour[] collectBehaviours;

    [Foldout("Information", true)] 
    [SerializeField, ReadOnly]
    private Player player;
    [SerializeField,ReadOnly]
    private GrabBehaviour[] grabBehaviours;
    [SerializeField,ReadOnly]
    private List<CollectBehaviour> listOfCollectBehaviour = new List<CollectBehaviour>();

    private WaitForSeconds waitForSeconds;
    
    private void OnValidate()
    {
        if (grabBehaviours == null || grabBehaviours.Length == 0 )
        {
            grabBehaviours = FindObjectsOfType<GrabBehaviour>();
        }
    }

    private IEnumerator Start()
    {
        player = Player.instance;
        
        SteamVR_Fade.Start(Color.black,0f,true);

        player.transform.position = wardrobeTransform.position;

        yield return waitForSeconds;
        
        SteamVR_Fade.Start(Color.clear,1f,true);
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

        StartCoroutine(StartStandPoint());
    }

    private IEnumerator StartStandPoint()
    {
        SteamVR_Fade.Start(Color.black,1f,true);
        
        yield return waitForSeconds;

        player.transform.position = standTransform.position;
        
        for (var i = 0; i < grabBehaviours.Length; i++)
        {
            grabBehaviours[i].grabingStep = GrabBehaviour.GrabingStep.Waiting;
        }
        
        yield return waitForSeconds;
        
        SteamVR_Fade.Start(Color.clear,1f,true);
    }
}
