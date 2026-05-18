using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Pathfinding;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Controllers.Units;

public static class AbilityApproachHelper
{
	private static AbilityData m_CachedAbility;

	private static MechanicEntity m_CachedTargetEntity;

	private static List<GraphNode> m_CachedMovableArea;

	private static Vector3 m_CachedTargetPosition;

	private static Vector3 m_CachedCasterPosition;

	private static GridNodeBase m_CachedResult;

	private static bool m_CachedHasResult;

	private static readonly string m_FindNodeProfileScopeId;

	static AbilityApproachHelper()
	{
		m_FindNodeProfileScopeId = "AbilityApproachHelper.TryFindApproachNode";
	}

	public static bool IsApproachCandidate([CanBeNull] AbilityData ability, [CanBeNull] TargetWrapper target)
	{
		if (ability == null || target?.Entity == null)
		{
			return false;
		}
		if (ability.TargetAnchor != AbilityTargetAnchor.Unit)
		{
			return false;
		}
		if (!ability.IsMelee)
		{
			return false;
		}
		if (!(ability.Caster is BaseUnitEntity))
		{
			return false;
		}
		return true;
	}

	public static bool TryFindApproachNode([NotNull] AbilityData ability, [NotNull] TargetWrapper target, out GridNodeBase approachNode)
	{
		using (ProfileScope.New(m_FindNodeProfileScopeId))
		{
			approachNode = null;
			if (!IsApproachCandidate(ability, target))
			{
				return false;
			}
			if (!(ability.Caster is BaseUnitEntity baseUnitEntity))
			{
				return false;
			}
			List<GraphNode> currentUnitMovableArea = Game.Instance.Controllers.UnitMovableAreaController.CurrentUnitMovableArea;
			if (currentUnitMovableArea == null || currentUnitMovableArea.Count == 0)
			{
				return false;
			}
			GridNodeBase nearestNode = target.NearestNode;
			if (nearestNode == null)
			{
				return false;
			}
			Vector3 position = baseUnitEntity.Position;
			Vector3 vector = target.Entity?.Position ?? Vector3.zero;
			if ((object)m_CachedAbility == ability && m_CachedTargetEntity == target.Entity && m_CachedMovableArea == currentUnitMovableArea && m_CachedCasterPosition == position && m_CachedTargetPosition == vector)
			{
				approachNode = m_CachedResult;
				return m_CachedHasResult;
			}
			GridNodeBase gridNodeBase = null;
			float num = float.MaxValue;
			HashSet<GraphNode> value;
			using (CollectionPool<HashSet<GraphNode>, GraphNode>.Get(out value))
			{
				for (int i = 0; i < currentUnitMovableArea.Count; i++)
				{
					value.Add(currentUnitMovableArea[i]);
				}
				int radius = Mathf.Max(1, ability.RangeCells);
				foreach (GridNodeBase item in GridAreaHelper.GetNodesSpiralAround(nearestNode, target.SizeRect, radius))
				{
					if (item != null && value.Contains(item) && ability.CanTargetFromNode(item, nearestNode, target, out var _, out var _, out var _))
					{
						float sqrMagnitude = (item.Vector3Position() - position).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							gridNodeBase = item;
						}
					}
				}
			}
			m_CachedAbility = ability;
			m_CachedTargetEntity = target.Entity;
			m_CachedMovableArea = currentUnitMovableArea;
			m_CachedCasterPosition = position;
			m_CachedTargetPosition = m_CachedTargetEntity?.Position ?? Vector3.zero;
			m_CachedResult = gridNodeBase;
			m_CachedHasResult = gridNodeBase != null;
			approachNode = gridNodeBase;
			return gridNodeBase != null;
		}
	}
}
