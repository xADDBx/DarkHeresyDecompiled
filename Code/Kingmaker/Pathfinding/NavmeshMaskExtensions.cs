namespace Kingmaker.Pathfinding;

public static class NavmeshMaskExtensions
{
	public static bool IsAnySet(this NavmeshMask self, NavmeshMask mask)
	{
		return (self & mask) != 0;
	}

	public static bool IsAllSet(this NavmeshMask self, NavmeshMask mask)
	{
		return (self & mask) == mask;
	}
}
