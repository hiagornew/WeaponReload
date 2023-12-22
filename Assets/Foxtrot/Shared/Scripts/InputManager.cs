using System.Collections.Generic;
using System.Linq;
using Foxtrot.GrabSystem.Scripts;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Foxtrot.Shared.Scripts
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager instance { get; private set; }
        
        [Foldout("References", true)]
        [SerializeField]
        private InputActionAsset inputActions;
        [SerializeField]
        private GrabBehaviour[] grabBehaviours;
        
        private Input _leftGrabInput;
        private Input _rightGrabInput;

        private Input _leftTriggerInput;
        private Input _rightTriggerInput;

        private Input _leftPrimaryInput;
        private Input _rightPrimaryInput;
        
        private Input _leftSecondaryInput;
        private Input _rightSecondaryInput;
        
        private List<Hand> toBeRemoved = new();
    
        private Dictionary<Hand,Hand.GrabTypes> pausedHands = new();

        public Input GetInput(Hand.HandType handType, Hand.GrabTypes grabTypes)
        {
            switch (handType)
            {
                case Hand.HandType.LeftHand:
                    switch (grabTypes)
                    {
                        case Hand.GrabTypes.Grip:
                            return _leftGrabInput;
                        case Hand.GrabTypes.Trigger:
                            return _leftTriggerInput;
                        case Hand.GrabTypes.Primary:
                            return _leftPrimaryInput;
                        case Hand.GrabTypes.Secondary:
                            return _leftSecondaryInput;
                    }
                    break;
                case Hand.HandType.RightHand:
                    switch (grabTypes)
                    {
                        case Hand.GrabTypes.Grip:
                            return _rightGrabInput;
                        case Hand.GrabTypes.Trigger:
                            return _rightTriggerInput;
                        case Hand.GrabTypes.Primary:
                            return _rightPrimaryInput;
                        case Hand.GrabTypes.Secondary:
                            return _rightSecondaryInput;
                    }
                    break;
            }

            return null;
        }

        public class Input
        {
            private bool _downState;
            private bool _holdState;
            private bool _upState;

            public bool DownState => _downState;
            public bool HoldState => _holdState;
            public bool UpState => _upState;
            
            public void HandleInputAction(InputAction inputAction)
            {
                _downState = inputAction.WasPressedThisFrame();
                _holdState = inputAction.WasReleasedThisFrame();
                _upState = inputAction.IsPressed();
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            _leftGrabInput = StartInputAction(_leftGrabInput, Hand.GrabTypes.Grip, Hand.HandType.LeftHand);
            _rightGrabInput = StartInputAction(_rightGrabInput, Hand.GrabTypes.Grip, Hand.HandType.RightHand);
            
            _leftTriggerInput = StartInputAction(_leftTriggerInput, Hand.GrabTypes.Trigger, Hand.HandType.LeftHand);
            _rightTriggerInput = StartInputAction(_rightTriggerInput, Hand.GrabTypes.Trigger, Hand.HandType.RightHand);
            
            _leftPrimaryInput = StartInputAction(_leftPrimaryInput, Hand.GrabTypes.Primary, Hand.HandType.LeftHand);
            _rightPrimaryInput = StartInputAction(_rightPrimaryInput, Hand.GrabTypes.Primary, Hand.HandType.RightHand);
        }
        
        private void Update()
        {
            for (var i = 0; i < toBeRemoved.Count; i++)
            {
                var hand = toBeRemoved[i];

                if (!pausedHands.ContainsKey(hand))
                {
                    continue;
                }

                pausedHands.Remove(hand);
            }
        
            toBeRemoved.Clear();
        
            for (var i = 0; i < pausedHands.Count; i++)
            {
                var hand = pausedHands.Keys.ElementAt(i);
                var grabType = pausedHands[hand];

                if (hand)
                {
                    if (toBeRemoved.Contains(hand))
                    {
                        continue;
                    }
                    
                    if (!GetInput(hand.handType, grabType).UpState)
                    {
                        continue;
                    }
                }
            
                toBeRemoved.Add(hand);
            }
        }

        private Input StartInputAction(Input input, Hand.GrabTypes grabTypes, Hand.HandType handType)
        {
            input = new Input();
            
            var actionMapName = string.Empty;
            var inputActionName = string.Empty;

            actionMapName = handType switch
            {
                Hand.HandType.LeftHand => "LeftHand",
                Hand.HandType.RightHand => "RightHand",
                _ => actionMapName
            };

            inputActionName = grabTypes switch
            {
                Hand.GrabTypes.Grip => "Grab",
                Hand.GrabTypes.Trigger => "Trigger",
                Hand.GrabTypes.Primary => "Primary",
                Hand.GrabTypes.Secondary => "Secondary",
                _ => inputActionName
            };

            var inputAction = inputActions.FindActionMap(actionMapName).FindAction(inputActionName);
            
            Debug.Log($"[Test] StartInputAction ActionMapName: {actionMapName} InputActionName: {inputActionName} InputAction: {inputAction.name}");

            inputAction.performed += ctx => input.HandleInputAction(ctx.action);
            inputAction.canceled += ctx => input.HandleInputAction(ctx.action);
            
            inputAction.Enable();

            return input;
        }
        
        public void PauseHand(Hand hand, Hand.GrabTypes grabType)
        {
            if (pausedHands.ContainsKey(hand))
            {
                return;
            }
        
            pausedHands.Add(hand, grabType);
        }
    
        public bool HandCanInteract(Hand hand)
        {
            return !pausedHands.ContainsKey(hand);
        }
        
        public GrabBehaviour FindGrabBehaviourByHand(Hand hand)
        {
            for (var i = 0; i < grabBehaviours.Length; i++)
            {
                var grabBehaviour = grabBehaviours[i];

                if (!grabBehaviour.hand.Equals(hand))
                {
                    continue;
                }
            
                return grabBehaviour;
            }

            return null;
        }
    }
}