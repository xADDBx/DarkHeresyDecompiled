using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.Pathfinding;
using UnityEngine;

namespace Kingmaker.Framework.EntitySystem.Interfaces.View;

public interface IAbstractDestructibleEntityView : IMapObjectView, IMechanicEntityView, IEntityView
{
	GridObstacle[] CurrentStageGridObstacles { get; }

	GridObstacle[] WholeStageGridObstacles { get; }

	GridObstacle[] AllGridObstacles { get; }

	Bounds RenderersBounds { get; }

	Vector3 OvertipPosition { get; }

	bool VisibleInExploration { get; }
}
