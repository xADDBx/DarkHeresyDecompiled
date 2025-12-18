using Kingmaker.Pathfinding;
using Kingmaker.View.MapObjects.SriptZones;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.AreaEffects.Shapes;

public sealed class AreaEffectShapeAllArea : IScriptZoneShape
{
	public NodeList CoveredNodes => default(NodeList);

	public Vector3 Center()
	{
		return Vector3.zero;
	}

	public bool Contains(Vector3 point, IntRect size)
	{
		return true;
	}

	public bool Contains(GridNodeBase node, IntRect size)
	{
		return true;
	}

	public Bounds GetBounds()
	{
		return default(Bounds);
	}
}
