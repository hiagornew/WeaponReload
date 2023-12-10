using Foxtrot.GrabSystem.Scripts;
using UnityEngine;

namespace Foxtrot.Shared.Scripts
{
	public class IgnoreHovering : MonoBehaviour
	{
		[Tooltip( "If Hand is not null, only ignore the specified hand" )]
		public Hand onlyIgnoreHand = null;
	}
}
