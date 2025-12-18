using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Pathfinding;

namespace Kingmaker.View.Covers;

public struct LosDescription
{
	private EntityRef<MechanicEntity>? m_ObstacleEntity;

	public readonly LosCalculations.CoverType CoverType;

	public readonly LosCalculations.CoverType OriginalCoverType;

	public readonly ObstacleInfo Obstacle;

	public GridNodeBase ObstacleNode => Obstacle.Node;

	[CanBeNull]
	public MechanicEntity ObstacleEntity
	{
		get
		{
			EntityRef<MechanicEntity> valueOrDefault = m_ObstacleEntity.GetValueOrDefault();
			EntityRef<MechanicEntity> entityRef;
			if (!m_ObstacleEntity.HasValue)
			{
				valueOrDefault = Obstacle.Entity;
				m_ObstacleEntity = valueOrDefault;
				entityRef = valueOrDefault;
			}
			else
			{
				entityRef = valueOrDefault;
			}
			return entityRef;
		}
	}

	public LosDescription(LosCalculations.CoverType coverType, ObstacleInfo obstacle, LosCalculations.CoverType? originalCoverType = null)
	{
		CoverType = coverType;
		Obstacle = obstacle;
		m_ObstacleEntity = null;
		OriginalCoverType = originalCoverType ?? coverType;
	}

	public LosDescription(LosCalculations.CoverType coverType, [CanBeNull] GridNodeBase obstacleNode = null, int fenceDirection = -1)
		: this(coverType, new ObstacleInfo(obstacleNode, fenceDirection))
	{
	}

	public LosDescription WithNewCoverType(LosCalculations.CoverType coverType)
	{
		LosDescription result = new LosDescription(coverType, Obstacle, CoverType);
		result.m_ObstacleEntity = m_ObstacleEntity;
		return result;
	}

	public static implicit operator LosDescription((LosCalculations.CoverType Type, GridNodeBase Node) t)
	{
		return new LosDescription(t.Type, t.Node);
	}

	public static implicit operator LosDescription((LosCalculations.CoverType Type, ObstacleInfo Obstacle) t)
	{
		return new LosDescription(t.Type, t.Obstacle);
	}

	public static implicit operator LosCalculations.CoverType(LosDescription description)
	{
		return description.CoverType;
	}
}
