using Pathfinding;

namespace Kingmaker.Pathfinding;

public static class GridNodeBaseExtensions
{
	public static string AsString(this GridNodeBase node)
	{
		return $"{node.GetType().Name}({node.XCoordinateInGrid}, {node.ZCoordinateInGrid})";
	}

	public static string AsString(this GraphNode node)
	{
		if (node is GridNodeBase node2)
		{
			return node2.AsString();
		}
		return node.ToString();
	}
}
