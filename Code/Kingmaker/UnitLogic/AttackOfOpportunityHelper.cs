using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public static class AttackOfOpportunityHelper
{
	private static class IsWaitingForIncomingAooChecker
	{
		public static MechanicEntity Unit;

		public static bool Check(UnitAttackOfOpportunity i)
		{
			if (!i.IsActed)
			{
				return i.TargetUnit == Unit;
			}
			return false;
		}
	}

	private static Func<UnitAttackOfOpportunity, bool> CheckIsWaitingForIncomingAoo = (UnitAttackOfOpportunity i) => IsWaitingForIncomingAooChecker.Check(i);

	public static IEnumerable<AttackOfOpportunityData> CalculateAttackOfOpportunity(this UnitMoveToProper move)
	{
		return move.Executor.CalculateAttackOfOpportunity(move.ForcedPath);
	}

	public static IEnumerable<AttackOfOpportunityData> CalculateAttackOfOpportunity(this UnitUseAbility useAbility)
	{
		return useAbility.Executor.CalculateAttackOfOpportunity(useAbility.Ability);
	}

	public static IEnumerable<AttackOfOpportunityData> CalculateAttackOfOpportunity(this BaseUnitEntity target, IEnumerable<Vector3> path)
	{
		return target.CalculateAttackOfOpportunity(path.Select((Vector3 i) => i.GetNearestNodeXZUnwalkable()).NotNull());
	}

	public static IEnumerable<AttackOfOpportunityData> CalculateAttackOfOpportunity(this BaseUnitEntity target, Path path)
	{
		List<GraphNode> path2 = path.path;
		if (path2 == null || path2.Count <= 0)
		{
			return target.CalculateAttackOfOpportunity(path.vectorPath);
		}
		return target.CalculateAttackOfOpportunity(path.path);
	}

	public static IEnumerable<AttackOfOpportunityData> CalculateAttackOfOpportunity(this BaseUnitEntity target, IEnumerable<GraphNode> path)
	{
		GridNodeBase prevNode = target.CurrentUnwalkableNode;
		if (prevNode == null)
		{
			yield break;
		}
		foreach (GraphNode item in path)
		{
			if (!(item is GridNodeBase node) || node == prevNode)
			{
				continue;
			}
			foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.EntityPools.AllBaseAwakeUnits)
			{
				if (allBaseAwakeUnit is UnitEntity unitEntity && allBaseAwakeUnit.IsInCombat && unitEntity != target && unitEntity.CanMakeAttackOfOpportunity(target) && unitEntity.IsThreat(target, prevNode) && !unitEntity.IsThreat(target, node))
				{
					yield return new AttackOfOpportunityData(unitEntity, prevNode.Vector3Position());
				}
			}
			prevNode = node;
		}
	}

	public static IEnumerable<AttackOfOpportunityData> CalculateAttackOfOpportunity(this BaseUnitEntity target, AbilityData ability)
	{
		if (!ability.ProvokesAttackOfOpportunity || (target.HasMechanicFeature(MechanicsFeatureType.PsychicPowersDoNotProvokeAoO) && ability.Blueprint.AbilityParamsSource == WarhammerAbilityParamsSource.PsychicPower))
		{
			yield break;
		}
		foreach (BaseUnitEntity engagedByUnit in target.GetEngagedByUnits())
		{
			if (engagedByUnit.CanMakeAttackOfOpportunity(target))
			{
				yield return new AttackOfOpportunityData(engagedByUnit, target.Position, ability.Blueprint.OriginalBlueprint);
			}
		}
	}

	public static bool IsThreat(this BaseUnitEntity attacker, BaseUnitEntity target, GridNodeBase targetNode = null)
	{
		if (targetNode == null)
		{
			targetNode = target.CurrentUnwalkableNode;
		}
		if (!attacker.CanAct || !attacker.IsEnemy(target))
		{
			return false;
		}
		WeaponSlot threatHand = attacker.GetThreatHand();
		if (threatHand?.Weapon.AttackOfOpportunityAbility == null)
		{
			return false;
		}
		return attacker.IsAttackOfOpportunityReach(target, targetNode, threatHand);
	}

	public static bool IsThreat(this BaseUnitEntity attacker, GraphNode targetNode, IntRect sizeRect = default(IntRect))
	{
		return attacker.IsThreat(targetNode, attacker.Position, sizeRect);
	}

	public static bool IsThreat(this BaseUnitEntity attacker, GraphNode targetNode, Vector3 attackerPosition, IntRect targetSize = default(IntRect))
	{
		if (!attacker.CanAct)
		{
			return false;
		}
		WeaponSlot threatHand = attacker.GetThreatHand();
		if (threatHand?.Weapon.AttackOfOpportunityAbility == null)
		{
			return false;
		}
		return attacker.IsAttackOfOpportunityReach(targetNode, attackerPosition, targetSize, threatHand);
	}

	public static bool IsAttackOfOpportunityReach(this BaseUnitEntity attacker, BaseUnitEntity target, GridNodeBase targetNode, WeaponSlot hand)
	{
		return attacker.IsAttackOfOpportunityReach(attacker.CurrentUnwalkableNode, target, targetNode, hand);
	}

	public static bool IsAttackOfOpportunityReach(this BaseUnitEntity attacker, GridNodeBase attackerNode, BaseUnitEntity target, GridNodeBase targetNode, WeaponSlot hand)
	{
		using (ProfileScope.New("IsAttackOfOpportunityReach"))
		{
			int threatRange = hand.Weapon.ThreatRange;
			return attacker.InRangeInCells(targetNode.Vector3Position(), target.SizeRect, threatRange) && attacker.Vision.HasLOS(targetNode, attackerNode.Vector3Position() + LosCalculations.EyeShift) && LosCalculations.HasMeleeLos(attacker.Position, attacker.SizeRect, targetNode.Vector3Position(), target.SizeRect);
		}
	}

	public static bool CanMakeAttackOfOpportunity(this BaseUnitEntity attacker, BaseUnitEntity target)
	{
		if ((Game.Instance.Controllers.TurnController.CurrentUnit == attacker && !attacker.HasMechanicFeature(MechanicsFeatureType.CanAoODuringOwnTurn)) || (bool)attacker.Features.DisableAttacksOfOpportunity)
		{
			return false;
		}
		if (target.GetMechanicFeature(MechanicsFeatureType.DoNotProvokeAttacksOfOpportunity).Value)
		{
			return false;
		}
		if (!attacker.IsEnemy(target))
		{
			return false;
		}
		if (target.LifeState.IsDead)
		{
			return false;
		}
		if (!attacker.CombatState.CanActInCombat || !attacker.CombatState.CanAttackOfOpportunity || !attacker.CanAct || attacker.IsInvisible)
		{
			return false;
		}
		return !AbstractUnitCommand.CommandTargetUntargetable(attacker, target);
	}

	public static HashSet<GraphNode> GetThreateningArea(this BaseUnitEntity unit)
	{
		HashSet<GraphNode> result = TempHashSet.Get<GraphNode>();
		unit.CollectThreateningAreaNodes(result);
		return result;
	}

	public static void CollectThreateningAreaNodes(this BaseUnitEntity unit, HashSet<GraphNode> result)
	{
		WeaponSlot threatHand = unit.GetThreatHand();
		if (threatHand?.Weapon.AttackOfOpportunityAbility == null)
		{
			return;
		}
		int threatRange = threatHand.Weapon.ThreatRange;
		HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
		GridPatterns.AddCircleNodes(hashSet, threatRange);
		foreach (GridNodeBase occupiedNode in unit.GetOccupiedNodes())
		{
			int xCoordinateInGrid = occupiedNode.XCoordinateInGrid;
			int zCoordinateInGrid = occupiedNode.ZCoordinateInGrid;
			GridGraph gridGraph = (GridGraph)occupiedNode.Graph;
			foreach (Vector2Int item in hashSet)
			{
				GridNodeBase node = gridGraph.GetNode(xCoordinateInGrid + item.x, zCoordinateInGrid + item.y);
				if (node != null && !result.Contains(node) && (float)occupiedNode.CellDistanceTo(node) <= unit.Corpulence + (float)threatRange && occupiedNode.HasMeleeLos(node))
				{
					result.Add(node);
				}
			}
		}
	}

	public static bool IsWaitingForIncomingAttackOfOpportunity([NotNull] this MechanicEntity unit)
	{
		IsWaitingForIncomingAooChecker.Unit = unit;
		if (TurnController.IsInTurnBasedCombat())
		{
			return UnitAttackOfOpportunity.AllActive.HasItem(CheckIsWaitingForIncomingAoo);
		}
		return false;
	}
}
