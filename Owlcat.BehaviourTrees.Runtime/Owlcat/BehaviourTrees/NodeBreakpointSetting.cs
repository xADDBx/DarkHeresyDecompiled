using System;

namespace Owlcat.BehaviourTrees;

[Flags]
public enum NodeBreakpointSetting : byte
{
	None = 0,
	ForwardVisit = 1,
	BackwardVisit = 2,
	EveryRunningTick = 4
}
