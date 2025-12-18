using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.SriptZones;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.AreaEffects.Shapes;

public sealed class AreaEffectShapePattern : IScriptZoneShape
{
	private NodeList m_GridData = NodeList.Empty;

	private GridNodeBase m_PatternApplicationNode;

	private Bounds m_Bounds;

	public bool ApplicationNodeExists => m_PatternApplicationNode != null;

	public NodeList CoveredNodes => m_GridData;

	public Vector3 Center()
	{
		return m_PatternApplicationNode.Vector3Position();
	}

	public void SetPattern([NotNull] GridNodeBase appliedNode, float appliedPositionY, in OrientedPatternData pattern)
	{
		m_PatternApplicationNode = appliedNode;
		GridGraph gridGraph = (GridGraph)appliedNode.Graph;
		PatternGridData pattern2 = PatternGridData.Create(pattern.Nodes.Select((GridNodeBase i) => i.CoordinatesInGrid).ToTempHashSet(), disposable: false);
		try
		{
			m_GridData = new NodeList(gridGraph, in pattern2);
			Vector3 min = new Vector3((float)pattern2.Bounds.xmin - 0.5f, -1f, (float)pattern2.Bounds.ymin - 0.5f);
			Vector3 max = new Vector3((float)pattern2.Bounds.xmax + 0.5f, 1f, (float)pattern2.Bounds.ymax + 0.5f);
			Bounds bounds = default(Bounds);
			bounds.SetMinMax(min, max);
			m_Bounds = gridGraph.transform.Transform(bounds);
		}
		finally
		{
			((IDisposable)pattern2).Dispose();
		}
	}

	public bool Contains(Vector3 point, IntRect size)
	{
		using (ProfileScope.New("AreaEffectShapePattern.Contains"))
		{
			GridNodeBase nearestNodeXZUnwalkable = point.GetNearestNodeXZUnwalkable();
			return Contains(nearestNodeXZUnwalkable, size);
		}
	}

	public bool Contains(GridNodeBase node, IntRect size)
	{
		if (m_PatternApplicationNode == null)
		{
			return false;
		}
		using (ProfileScope.New("AreaEffectShapePattern.Contains"))
		{
			if (size.Width <= 1 && size.Height <= 1)
			{
				return m_GridData.Contains(node);
			}
			foreach (GridNodeBase occupiedNode in GridAreaHelper.GetOccupiedNodes(node, size))
			{
				if (m_GridData.Contains(occupiedNode))
				{
					return true;
				}
			}
			return false;
		}
	}

	public Bounds GetBounds()
	{
		return m_Bounds;
	}
}
