using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class HK417Behaviour : GunBehaviour
{
    protected override void HandAttachedUpdate(Hand hand)
    {
        base.HandAttachedUpdate(hand);
        TryCycleSelector(hand);
    }

    private void TryCycleSelector(Hand hand)
    {
        var touchPadPressDirection = inputManager.GetBestDPadPressingType(hand, InputManager.PressStateType.Down);

        if (touchPadPressDirection != InputManager.DPadType.East)
        {
            return;
        }

        CycleSelector();
    }

    private void CycleSelector()
    {
        switch (fireMode)
        {
            default:
            case FireMode.Locked:
                fireMode = FireMode.SemiAutomatic;
                break;

            case FireMode.SemiAutomatic:
                fireMode = FireMode.Automatic;
                break;

            case FireMode.Automatic:
                fireMode = FireMode.Locked;
                break;
        }
    }
}
