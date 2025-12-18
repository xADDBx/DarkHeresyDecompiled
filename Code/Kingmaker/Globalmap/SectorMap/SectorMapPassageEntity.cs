using System;

namespace Kingmaker.Globalmap.SectorMap;

[Obsolete]
public class SectorMapPassageEntity
{
	[Obsolete]
	public enum PassageDifficulty
	{
		Safe,
		Unsafe,
		Dangerous,
		Deadly
	}

	[Obsolete]
	public enum ExploreStatus
	{
		UnExplored,
		ExploredFromOneSide,
		Explored
	}
}
