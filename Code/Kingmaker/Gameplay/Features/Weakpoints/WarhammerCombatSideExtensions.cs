using System;

namespace Kingmaker.Gameplay.Features.Weakpoints;

public static class WarhammerCombatSideExtensions
{
	public static WeakpointSide Opposite(this WeakpointSide side)
	{
		return side switch
		{
			WeakpointSide.Front => WeakpointSide.Back, 
			WeakpointSide.Left => WeakpointSide.Right, 
			WeakpointSide.Right => WeakpointSide.Left, 
			WeakpointSide.Back => WeakpointSide.Front, 
			_ => throw new ArgumentOutOfRangeException("side", side, null), 
		};
	}
}
