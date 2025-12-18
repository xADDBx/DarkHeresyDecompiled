using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Covers;
using Kingmaker.View.Scene.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public class DestructibleCoverInfoProvider : IEntityInfoProvider<GameObject>
{
	public bool TryGetEntityInfo(GameObject gameObject, out IEntityInfo entityInfo)
	{
		if (gameObject.TryGetComponent<AbstractDestructibleEntityView>(out var component))
		{
			entityInfo = GetCoverInfo(component);
			return entityInfo != null;
		}
		if (gameObject.TryGetComponent<GridObstacle>(out var component2))
		{
			entityInfo = GetCoverInfo(component2);
			return entityInfo != null;
		}
		entityInfo = null;
		return false;
	}

	private DestructibleCoverInfo GetCoverInfo(GridObstacle gridObstacle)
	{
		if ((Object)(object)gridObstacle == null || !(gridObstacle.EntityView is AbstractDestructibleEntityView entityView))
		{
			return null;
		}
		return GetCoverInfo(entityView);
	}

	private DestructibleCoverInfo GetCoverInfo(AbstractDestructibleEntityView entityView)
	{
		DestructibleEntity data = entityView.Data;
		if (LosCalculations.GetCoverType(data.Position) == LosCalculations.CoverType.LosBlocker)
		{
			return null;
		}
		PartHealth health = data.Health;
		return new DestructibleCoverInfo
		{
			MaxDurability = health.MaxHitPoints,
			CurrentDurability = health.HitPointsLeft,
			WorldPosition = entityView.ViewTransform.position
		};
	}
}
