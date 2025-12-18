using Pathfinding.Util;
using UnityEngine;

namespace Kingmaker.Framework.Pathfinding;

public interface IGridObstacle
{
	void Init();

	Rect GetBounds(GraphTransform graphTransform, float marginRadius = 0f);
}
