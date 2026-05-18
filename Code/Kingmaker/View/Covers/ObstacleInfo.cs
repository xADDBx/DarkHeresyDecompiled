using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.Pathfinding;
using Pathfinding;

namespace Kingmaker.View.Covers;

public readonly struct ObstacleInfo
{
	public readonly GridNodeBase Node;

	public readonly int ObstacleDirection;

	private readonly EntityRef<MechanicEntity> _entityRef;

	[CanBeNull]
	public GridObstacle GridObstacle
	{
		get
		{
			if (!IsObstacle)
			{
				return null;
			}
			return AdditionalGraphDataManager.Instance.GetGridData().Obstacles.GetObstacle(Node, (GridNodeDirection)ObstacleDirection).Source;
		}
	}

	[CanBeNull]
	public MechanicEntity Entity => _entityRef.Entity ?? (GridObstacle?.EntityView?.Data as MechanicEntity);

	public bool IsObstacle
	{
		get
		{
			if (Node != null)
			{
				int obstacleDirection = ObstacleDirection;
				if (obstacleDirection >= 0)
				{
					return obstacleDirection < 8;
				}
				return false;
			}
			return false;
		}
	}

	public ObstacleInfo(GridNodeBase node, GridNodeBase coveredNode)
	{
		Node = node;
		ObstacleDirection = ((coveredNode != node && coveredNode.GetObstacleWithNode(node).HasValue) ? GraphHelper.GuessDirection(coveredNode.Vector3Position() - node.Vector3Position()) : (-1));
		_entityRef = null;
	}

	public ObstacleInfo(GridNodeBase node, int obstacleDirection = -1, MechanicEntity obstacleEntity = null)
	{
		Node = node;
		ObstacleDirection = obstacleDirection;
		_entityRef = obstacleEntity;
	}
}
