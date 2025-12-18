using Kingmaker.Framework.Pathfinding;
using Pathfinding;

namespace Kingmaker.Pathfinding;

public static class AdditionalGraphDataExtensions
{
	public static GridObstacleDescription? GetObstacleWithNode(this GridNodeBase thisNode, GridNodeBase otherNode)
	{
		AdditionalGraphData graphData = AdditionalGraphDataManager.Instance.GetGraphData(thisNode.GraphIndex);
		GridNodeDirection direction = thisNode.GetDirection(otherNode);
		GridObstacleCache.Entry obstacle = graphData.Obstacles.GetObstacle(thisNode, direction);
		if (obstacle == null)
		{
			return null;
		}
		return new GridObstacleDescription(obstacle.Type, obstacle.Top, obstacle.Bottom);
	}

	public static bool IsFloor(this GridNodeBase node)
	{
		return AdditionalGraphDataManager.Instance.GetGraphData(node.GraphIndex).NodeData[node.NodeInGridIndex].IsFloor;
	}
}
