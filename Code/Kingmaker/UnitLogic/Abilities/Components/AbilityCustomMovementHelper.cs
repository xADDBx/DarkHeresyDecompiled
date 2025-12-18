using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

public static class AbilityCustomMovementHelper
{
	public static (OrientedPatternData Pattern, List<GridNodeBase> Path) GetOrientedPatternAndPath(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, int minRangeCells, bool coveredTargetsOnly, bool stepThroughTarget, bool stopOnFirstEncounter, bool ignoreEnemies, bool ignoreAllies, bool isCharge, bool stopBeforeTargetNode)
	{
		MechanicEntity caster = ability.Caster;
		int rangeCells = ability.RangeCells;
		bool ignoreBlockers = !stopOnFirstEncounter || ignoreEnemies || ignoreAllies;
		List<GridNodeBase> path = GetPath(ability, casterNode, targetNode, stepThroughTarget, ignoreBlockers);
		if (path.Empty() || (!stepThroughTarget && !IsPathToTargetReachDestination(caster, targetNode, path)))
		{
			List<GridNodeBase> list = TempList.Get<GridNodeBase>();
			list.Add(targetNode);
			return (Pattern: new OrientedPatternData(list, casterNode), Path: TempList.Get<GridNodeBase>());
		}
		GridNodeBase gridNodeBase = path[0];
		Vector2Int vector2Int = casterNode.CoordinatesInGrid - gridNodeBase.CoordinatesInGrid;
		List<Vector2Int> list2 = (from i in caster.GetSortedNodesByDistanceToTarget(casterNode, targetNode.Vector3Position())
			select i.CoordinatesInGrid - casterNode.CoordinatesInGrid).ToTempList();
		GridGraph g2 = (GridGraph)gridNodeBase.Graph;
		List<GridNodeBase> list3 = TempList.Get<GridNodeBase>();
		HashSet<GridNodeBase> hashSet = TempHashSet.Get<GridNodeBase>();
		int j = 0;
		GridNodeBase gridNodeBase2 = null;
		for (; j < path.Count; j++)
		{
			GridNodeBase gridNodeBase3 = path[j];
			GridNodeBase actualNode = GetNode(g2, gridNodeBase3.CoordinatesInGrid + vector2Int);
			if (actualNode == null)
			{
				break;
			}
			int warhammerLength = GraphHelper.GetWarhammerLength(actualNode.CoordinatesInGrid - casterNode.CoordinatesInGrid);
			if (!stepThroughTarget && warhammerLength > rangeCells)
			{
				break;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			hashSet.Clear();
			foreach (Vector2Int item in list2)
			{
				hashSet.Add(GetNode(g2, actualNode.CoordinatesInGrid + item));
			}
			foreach (GridNodeBase item2 in hashSet)
			{
				BaseUnitEntity firstUnit = item2.GetFirstUnit();
				flag3 |= firstUnit != null && firstUnit != caster && firstUnit.IsConscious;
				if (firstUnit != null)
				{
					flag |= flag3 && firstUnit.IsEnemy(caster);
					flag2 |= flag3 && firstUnit.IsAlly(caster);
				}
			}
			if (!coveredTargetsOnly || flag3)
			{
				list3.AddRange(hashSet);
			}
			if (!stepThroughTarget && stopOnFirstEncounter && ((!ignoreAllies && flag2) || (!ignoreEnemies && flag)))
			{
				gridNodeBase2 = actualNode;
				break;
			}
			if (stopBeforeTargetNode && j > 0 && Enumerable.Any(list2, (Vector2Int offset) => actualNode.CoordinatesInGrid + offset == targetNode.CoordinatesInGrid))
			{
				gridNodeBase2 = GetNode(g2, path[j - 1].CoordinatesInGrid + vector2Int);
				break;
			}
		}
		path.RemoveRange(j, path.Count - j);
		if (isCharge && path.Count >= minRangeCells && !list3.Contains(gridNodeBase2 ?? targetNode))
		{
			list3.Add(targetNode);
		}
		return (Pattern: new OrientedPatternData(list3, list3.FirstItem()), Path: path);
		static GridNodeBase GetNode(NavGraph g, Vector2Int i)
		{
			return ((GridGraph)g).GetNode(i.x, i.y);
		}
	}

	public static List<GridNodeBase> GetPath(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, bool stepThroughTarget, bool ignoreBlockers)
	{
		if (!stepThroughTarget)
		{
			return GetPathToTarget(ability, casterNode, targetNode, ignoreBlockers);
		}
		return GetStepThroughTargetPath(ability, casterNode, targetNode);
	}

	private static List<GridNodeBase> GetPathToTarget(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, bool ignoreBlockers)
	{
		BaseUnitEntity baseUnitEntity = targetNode.GetFirstUnit();
		if (baseUnitEntity != null && baseUnitEntity.IsDeadOrUnconscious && ability.Data.CanTargetPoint)
		{
			baseUnitEntity = null;
		}
		return PathfindingService.Instance.FindPathChargeTB_Blocking(ability.Caster.MaybeMovementAgent, casterNode.Vector3Position(), targetNode.Vector3Position(), ability.RangeCells, ignoreBlockers, baseUnitEntity).path.Cast<GridNodeBase>().ToTempList();
	}

	public static bool IsPathToTargetReachDestination(MechanicEntity caster, GridNodeBase targetNode, List<GridNodeBase> path)
	{
		if (path.Empty())
		{
			return false;
		}
		return caster.GetOccupiedNodes(path[path.Count - 1].Vector3Position()).Any((GridNodeBase i) => DistanceToInCells(i, caster.SizeRect, targetNode) < 2 && i.HasMeleeLos(targetNode));
		static int DistanceToInCells(GridNodeBase origin, IntRect size, GridNodeBase target)
		{
			return WarhammerGeometryUtils.DistanceToInCells(origin.Vector3Position(), size, target.Vector3Position(), default(IntRect));
		}
	}

	private static List<GridNodeBase> GetStepThroughTargetPath(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode)
	{
		List<GridNodeBase> list = TempList.Get<GridNodeBase>();
		BaseUnitEntity firstUnit = targetNode.GetFirstUnit();
		if (firstUnit == null)
		{
			return list;
		}
		MechanicEntity caster = ability.Caster;
		GridNodeBase innerNodeNearestToTarget = caster.GetInnerNodeNearestToTarget(casterNode, targetNode.Vector3Position());
		Vector2 normalized = (firstUnit.GetInnerNodeNearestToTarget(innerNodeNearestToTarget.Vector3Position()).Vector3Position() - innerNodeNearestToTarget.Vector3Position()).To2D().normalized;
		if (normalized.sqrMagnitude < 1E-06f)
		{
			return list;
		}
		Linecast.Ray2NodeOffsets offsets = new Linecast.Ray2NodeOffsets(casterNode.CoordinatesInGrid, normalized);
		foreach (GridNodeBase item2 in new Linecast.Ray2Nodes((GridGraph)casterNode.Graph, in offsets))
		{
			if (list.Any())
			{
				if (!GraphHelper.HasConnectionBetweenNodes(list[list.Count - 1], item2))
				{
					GridNodeBase item;
					if (list.Count < 2)
					{
						item = casterNode;
					}
					else
					{
						item = list[list.Count - 2];
					}
					list.Add(item);
					return list;
				}
			}
			list.Add(item2);
			bool flag = false;
			bool flag2 = false;
			foreach (GridNodeBase occupiedNode in caster.GetOccupiedNodes(item2.Vector3Position()))
			{
				BaseUnitEntity firstUnit2 = occupiedNode.GetFirstUnit();
				flag2 = flag2 || firstUnit2 != null;
				if (firstUnit2 != null && firstUnit2 != caster && firstUnit2 != firstUnit)
				{
					list.Clear();
					flag = true;
					break;
				}
			}
			if (flag || !flag2)
			{
				break;
			}
		}
		return list;
	}
}
