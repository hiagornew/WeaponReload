using Foxtrot.EPI.Scripts;
using Foxtrot.GrabSystem.Scripts;
using Foxtrot.Shared.Scripts;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Foxtrot.Scenario.EPI.Scripts
{
    public class CollectBehaviour : MonoBehaviour
    {
        [Foldout("Settings", true)] 
        [SerializeField]
        private float duration = 1;

        [Foldout("References", true)] 
        [SerializeField]
        private Renderer[] renderers;
        [SerializeField]
        private Image filledImage;
    
        [Foldout("Information", true)] 
        [SerializeField, ReadOnly]
        private InputManager inputManager;
        [SerializeField, ReadOnly] 
        private EPIController epiController;

        private Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand;
    
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (!inputManager)
            {
                inputManager = FindObjectOfType<InputManager>();
            }
        
            if (!epiController)
            {
                epiController = FindObjectOfType<EPIController>();
            }
        
            if (!filledImage)
            {
                filledImage = GetComponentInChildren<Image>();
            }

            if (renderers == null || renderers.Length == 0 )
            {
                renderers = GetComponentsInChildren<Renderer>();
            }
        }
        #endif

        private void Start()
        {
            filledImage.fillAmount = 0;
        }

        private void HandAttachedUpdate(Hand hand)
        {
            var startingGrabType = hand.GetBestGrabbingType(Hand.GrabTypes.Grip);

            if (startingGrabType == Hand.GrabTypes.None)
            {
                DetachHand(hand);
                return;
            }
        
            if (filledImage.fillAmount < 1f)
            {
                filledImage.fillAmount += Time.deltaTime / duration;
                return;
            }

            if (filledImage.fillAmount >= 1f)
            {
                gameObject.AddComponent<IgnoreHovering>();

                for (var i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = false;
                }
            
                epiController.EPICollected(this);
            
                DetachHand(hand);
            }
        }

        private void DetachHand(Hand hand)
        {
            filledImage.fillAmount = 0;
            
            hand.DetachObject(gameObject);
            hand.HoverUnlock(null);
        }

        private void HandHoverUpdate(Hand hand)
        {
            var startingGrabType = hand.GetBestGrabbingType(Hand.GrabTypes.Grip);
        
            if (startingGrabType == Hand.GrabTypes.None)
            {
                return;
            }
        
            hand.AttachObject(gameObject,startingGrabType, attachmentFlags);
            hand.HoverLock(null);
        }
    }
}
