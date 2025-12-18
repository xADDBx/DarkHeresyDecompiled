using System;

namespace Kingmaker.Pathfinding;

[Flags]
public enum NavmeshMask : byte
{
	TerrainObstacle = 1,
	ForbidArea = 2,
	AllowArea = 4,
	LosBlockingFloor = 8,
	Removed = 3
}
