using Foxtrot.GrabSystem.Scripts;
using Foxtrot.Guns.Shared.Scripts;
using Foxtrot.Shared.Scripts;

namespace Foxtrot.Guns.HK417.Scripts
{
    public class HK417Behaviour : GunBehaviour
    {
        protected override void HandAttachedUpdate(Hand hand)
        {
            base.HandAttachedUpdate(hand);
            TryCycleSelector(hand);
        }

        private void TryCycleSelector(Hand hand)
        {
            if (!InputManager.GetInput(hand.handType, Hand.GrabTypes.Primary).DownState)
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
}
