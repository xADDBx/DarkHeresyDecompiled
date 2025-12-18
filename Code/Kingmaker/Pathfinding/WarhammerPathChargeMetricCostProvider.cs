using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Pathfinding;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerPathChargeMetricCostProvider : ITraversalCostProvider<WarhammerPathChargeMetric>
{
	public const float InvalidLength = 1000f;

	[NotNull]
	private readonly BaseUnitEntity m_Unit;

	[NotNull]
	private readonly GridNodeBase m_OriginNode;

	[NotNull]
	private readonly GridNodeBase m_TargetNode;

	private readonly WarhammerPathCostModifier m_CostModifier;

	public WarhammerPathChargeMetricCostProvider([NotNull] BaseUnitEntity unit, GridNodeBase origin, [NotNull] GridNodeBase targetNode)
	{
		m_Unit = unit;
		m_OriginNode = origin;
		m_TargetNode = targetNode;
		m_CostModifier = WarhammerPathCostModifier.Get(unit);
	}

	public WarhammerPathChargeMetric Calc(in WarhammerPathChargeMetric distanceFrom, in GraphNode from, in GraphNode to)
	{
		bool flag = PathExtras.IsDiagonal((GridNodeBase)to, (GridNodeBase)from);
		int diagonalsCount = distanceFrom.DiagonalsCount + (flag ? 1 : 0);
		float length = distanceFrom.Length + Calc(from, to);
		if (m_CostModifier.IsForbidden(to))
		{
			length = ConfigRoot.Instance.CombatRoot.ForbiddenNodesTraverseCost;
		}
		return new WarhammerPathChargeMetric(length, diagonalsCount);
	}

	private float Calc(GraphNode from, GraphNode to)
	{
		Vector2 vector = (m_TargetNode.Vector3Position() - from.Vector3Position()).To2D();
		Vector2 vector2 = (m_TargetNode.Vector3Position() - to.Vector3Position()).To2D();
		if (Math.Abs(vector.x) < Math.Abs(vector2.x) || Math.Abs(vector.y) < Math.Abs(vector2.y))
		{
			return 1000f;
		}
		Vector3 vector3 = m_OriginNode.Vector3Position();
		Vector3 vector4 = m_TargetNode.Vector3Position();
		Vector3 vector5 = to.Vector3Position();
		if ((double)Math.Abs((vector4.x - vector3.x) * (vector3.z - vector5.z) - (vector3.x - vector5.x) * (vector4.z - vector3.z)) / Math.Sqrt((vector4.x - vector3.x) * (vector4.x - vector3.x) + (vector4.z - vector3.z) * (vector4.z - vector3.z)) > (double)(Mathf.Sqrt(2f) * 1.Cells().Meters * 1.1f))
		{
			return 1000f;
		}
		return 1f;
	}

	WarhammerPathChargeMetric ITraversalCostProvider<WarhammerPathChargeMetric>.Calc(in WarhammerPathChargeMetric distanceFrom, in GraphNode from, in GraphNode to)
	{
		return Calc(in distanceFrom, in from, in to);
	}
}
