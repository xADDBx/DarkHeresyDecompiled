using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Owlcat.AI;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerPathAiMetricCostProvider : ITraversalCostProvider<WarhammerPathAiMetric>
{
	private enum CellType
	{
		Normal,
		AooThreateningArea
	}

	private readonly AbstractUnitEntity m_Unit;

	private readonly IReadOnlyDictionary<GraphNode, AiBrainHelper.IThreatsInfo> m_ThreateningAreaCells;

	private readonly AiThreatsHandlingStrategy m_ThreatsHandlingStrategy;

	public WarhammerPathAiMetricCostProvider(AbstractUnitEntity unit, IReadOnlyDictionary<GraphNode, AiBrainHelper.IThreatsInfo> threateningAreaCells, GraphNode targetNode, AiThreatsHandlingStrategy threatsHandlingStrategy = AiThreatsHandlingStrategy.AvoidIfPossible)
	{
		m_Unit = unit;
		m_ThreateningAreaCells = threateningAreaCells;
		m_ThreatsHandlingStrategy = threatsHandlingStrategy;
	}

	public WarhammerPathAiMetric Calc(in WarhammerPathAiMetric distanceFrom, in GraphNode from, in GraphNode to)
	{
		if (from is LinkNode && to is LinkNode)
		{
			return distanceFrom;
		}
		if (from is LinkNode && to is GridNodeBase)
		{
			return distanceFrom;
		}
		if (from is GridNodeBase && to is LinkNode linkNode)
		{
			return new WarhammerPathAiMetric(distanceFrom.DiagonalsCount, distanceFrom.Length + linkNode.linkSource.costFactor, distanceFrom.Delay, distanceFrom.EnteredAoE, distanceFrom.LeavedAoE, distanceFrom.StepsInsideDamagingAoE, distanceFrom.ProvokedAttacks);
		}
		if (!m_ThreateningAreaCells.TryGetValue(from, out var prevTd))
		{
			prevTd = AiBrainHelper.EmptyThreatsInfo;
		}
		if (!m_ThreateningAreaCells.TryGetValue(to, out var nextTd))
		{
			nextTd = AiBrainHelper.EmptyThreatsInfo;
		}
		float num = 100f;
		int num2 = Mathf.Clamp(prevTd.DamagingOnMoveAreaEffects.Count, 0, 3);
		num += (float)num2 * 100f;
		int num3 = nextTd.AreaEffects.Count((AreaEffectEntity ae) => !prevTd.AreaEffects.Contains(ae));
		int num4 = prevTd.AreaEffects.Count((AreaEffectEntity ae) => !nextTd.AreaEffects.Contains(ae));
		num += 300f * (float)num3 + 33f * (float)num4;
		int num5 = prevTd.AooUnits.Count((BaseUnitEntity un) => !nextTd.AooUnits.Contains(un)) + nextTd.OverwatchUnits.Count();
		num += (float)num5 * 1000f;
		if (m_ThreatsHandlingStrategy == AiThreatsHandlingStrategy.AvoidAlways)
		{
			bool num6 = nextTd.AreaEffects.Count > 0 || num5 > 0;
			bool flag = num3 > 0 || num2 > 0;
			if (num6 || flag)
			{
				num += 1E+09f;
			}
		}
		bool flag2 = PathExtras.IsDiagonal((GridNodeBase)to, (GridNodeBase)from);
		float num7 = ((!(distanceFrom.DiagonalsCount % 2 == 1 && flag2)) ? 1 : 2);
		float num8 = Calc(from, to);
		num7 *= num8;
		float num9 = distanceFrom.Length + num7;
		float num10 = num9 - distanceFrom.Length;
		int num11 = num2;
		if (m_ThreatsHandlingStrategy == AiThreatsHandlingStrategy.AvoidAlways)
		{
			num11 = Math.Max(num11, nextTd.AreaEffects.Count);
		}
		int enteredAoE = distanceFrom.EnteredAoE + Math.Max(0, num3);
		int leavedAoE = distanceFrom.LeavedAoE + Math.Max(0, num4);
		int stepsInsideDamagingAoE = distanceFrom.StepsInsideDamagingAoE + num11;
		int provokedAttacks = distanceFrom.ProvokedAttacks + num5;
		float length = num9;
		float delay = distanceFrom.Delay + num10 + (flag2 ? (0.1f * num8) : 0f) + num;
		return new WarhammerPathAiMetric(distanceFrom.DiagonalsCount + (flag2 ? 1 : 0), length, delay, enteredAoE, leavedAoE, stepsInsideDamagingAoE, provokedAttacks);
	}

	private float Calc(GraphNode from, GraphNode to)
	{
		float cellCost = GetCellCost(GetCellType(from));
		if (NodeLinksExtensions.AreConnected(from, to, out var currentLink))
		{
			return cellCost * currentLink.CostFactor;
		}
		return cellCost;
	}

	private CellType GetCellType(GraphNode cell)
	{
		if ((bool)m_Unit.Features.IgnoreThreateningAreaForMovementCostCalculation)
		{
			return CellType.Normal;
		}
		if (!m_ThreateningAreaCells.TryGetValue(cell, out var value) || value.AooUnits.Count <= 0)
		{
			return CellType.Normal;
		}
		return CellType.AooThreateningArea;
	}

	private float GetCellCost(CellType cellType)
	{
		return cellType switch
		{
			CellType.Normal => m_Unit.Blueprint.WarhammerMovementApPerCell, 
			CellType.AooThreateningArea => m_Unit.GetWarhammerMovementApPerCellThreateningArea(), 
			_ => throw new ArgumentOutOfRangeException("cellType", cellType, null), 
		};
	}

	WarhammerPathAiMetric ITraversalCostProvider<WarhammerPathAiMetric>.Calc(in WarhammerPathAiMetric distanceFrom, in GraphNode from, in GraphNode to)
	{
		return Calc(in distanceFrom, in from, in to);
	}
}
