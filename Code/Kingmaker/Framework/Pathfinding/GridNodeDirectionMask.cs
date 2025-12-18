using System;

namespace Kingmaker.Framework.Pathfinding;

[Flags]
public enum GridNodeDirectionMask
{
	None = 0,
	S = 1,
	E = 2,
	N = 4,
	W = 8,
	SE = 0x10,
	NE = 0x20,
	NW = 0x40,
	SW = 0x80
}
