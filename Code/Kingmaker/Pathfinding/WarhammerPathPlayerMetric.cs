namespace Kingmaker.Pathfinding;

public readonly struct WarhammerPathPlayerMetric
{
	public readonly int DiagonalsCount;

	public readonly int DiagonalsCountTotal;

	public readonly int NavLinksCount;

	public readonly float Length;

	public readonly bool IsOneWayPath;

	public readonly int PseudoCostOfFreeMovement;

	public WarhammerPathPlayerMetric(int diagonalsCount, int diagonalsCountTotal, int navLinksCount, float length, int cellCount, bool isOneWayPath)
	{
		DiagonalsCount = diagonalsCount;
		DiagonalsCountTotal = diagonalsCountTotal;
		NavLinksCount = navLinksCount;
		Length = length;
		IsOneWayPath = isOneWayPath;
		PseudoCostOfFreeMovement = cellCount;
	}

	public override string ToString()
	{
		return $"L:{Length}";
	}
}
