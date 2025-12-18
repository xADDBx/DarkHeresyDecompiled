using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items.Slots;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.Controllers.Combat;

public class AttackOfOpportunityController : IController, IUnitRunCommandHandler, ISubscriber, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, IUnitCommandEndHandler, IUnitMoveHandler, ISubscriber<IAbstractUnitEntity>, IDirectMovementHandler
{
	public void HandleDirectMovementStarted(ForcedPath path, bool disableAttacksOfOpportunity)
	{
		if (!disableAttacksOfOpportunity)
		{
			BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
			if (baseUnitEntity != null)
			{
				IEnumerable<AttackOfOpportunityData> attacks = baseUnitEntity.CalculateAttackOfOpportunity(path);
				baseUnitEntity.GetOrCreate<PartIncomingAttacksOfOpportunity>().SetAttacks(attacks);
			}
		}
	}

	public void HandleDirectMovementEnded()
	{
		EventInvokerExtensions.BaseUnitEntity?.Remove<PartIncomingAttacksOfOpportunity>();
	}

	public void HandleUnitRunCommand(AbstractUnitCommand command)
	{
		if (command is UnitMoveToProper { DisableAttackOfOpportunity: false } unitMoveToProper)
		{
			IEnumerable<AttackOfOpportunityData> attacks = unitMoveToProper.CalculateAttackOfOpportunity();
			unitMoveToProper.Executor.GetOrCreate<PartIncomingAttacksOfOpportunity>().SetAttacks(attacks);
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (!(command is UnitUseAbility unitUseAbility))
		{
			return;
		}
		foreach (AttackOfOpportunityData item in unitUseAbility.CalculateAttackOfOpportunity())
		{
			MakeAttackOfOpportunity(item.Attacker, unitUseAbility.Executor, item.Reason);
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command is UnitMoveToProper)
		{
			command.Executor.Remove<PartIncomingAttacksOfOpportunity>();
		}
	}

	public void HandleUnitMovement(AbstractUnitEntity unit)
	{
		if (!TurnController.IsInTurnBasedCombat() || !(unit is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		PartIncomingAttacksOfOpportunity optional = unit.GetOptional<PartIncomingAttacksOfOpportunity>();
		if (optional == null)
		{
			return;
		}
		while (optional.NextAttack.HasValue)
		{
			AttackOfOpportunityData value = optional.NextAttack.Value;
			float magnitude = (baseUnitEntity.Position - optional.StartPosition).magnitude;
			float magnitude2 = (value.Position - optional.StartPosition).magnitude;
			if (!(magnitude < magnitude2))
			{
				optional.AcceptNextAttack();
				bool made = MakeAttackOfOpportunity(value.Attacker, baseUnitEntity, null);
				UpdateAttackOfOpportunityMadeThisTurnCount(value.Attacker, made);
				continue;
			}
			break;
		}
	}

	private static bool MakeAttackOfOpportunity(BaseUnitEntity attacker, BaseUnitEntity target, BlueprintFact reason)
	{
		bool result = false;
		int count = ((!attacker.Features.DualWielderAttackOfOpportunity) ? 1 : 2);
		GridNodeBase gridNodeBase = null;
		foreach (WeaponSlot item in attacker.GetThreatHands().Take(count))
		{
			UnitAttackOfOpportunityParams cmdParams = new UnitAttackOfOpportunityParams(target, reason, item.Weapon);
			GridNodeBase gridNodeBase2 = FindSuitablePositionForAttackOfOpportunity(attacker, target);
			if (gridNodeBase2 == null)
			{
				continue;
			}
			if (gridNodeBase2 == attacker.CurrentUnwalkableNode || gridNodeBase2 == gridNodeBase)
			{
				gridNodeBase = gridNodeBase2;
				attacker.Commands.AddToQueue(cmdParams);
				result = true;
				continue;
			}
			WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(attacker.MovementAgent, gridNodeBase2.Vector3Position());
			using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, attacker))
			{
				UnitMoveToProperParams cmdParams2 = new UnitMoveToProperParams(ForcedPath.Construct(warhammerPathPlayer), 0f, null)
				{
					SlowMotionRequired = true
				};
				gridNodeBase = gridNodeBase2;
				attacker.Commands.AddToQueue(cmdParams2);
				attacker.Commands.AddToQueue(cmdParams);
				result = true;
			}
		}
		return result;
	}

	public bool Provoke(BaseUnitEntity target, BaseUnitEntity attacker, BlueprintFact reason)
	{
		if (!attacker.CanMakeAttackOfOpportunity(target))
		{
			return false;
		}
		bool flag = MakeAttackOfOpportunity(attacker, target, reason);
		UpdateAttackOfOpportunityMadeThisTurnCount(attacker, flag);
		return flag;
	}

	private static void UpdateAttackOfOpportunityMadeThisTurnCount(BaseUnitEntity attacker, bool made)
	{
		if (made)
		{
			PartUnitCombatState combatStateOptional = attacker.GetCombatStateOptional();
			if (combatStateOptional != null)
			{
				combatStateOptional.AttacksOfOpportunityMadeThisTurnCount++;
			}
		}
	}

	public bool Provoke(BaseUnitEntity target, BaseUnitEntity attacker, EntityFact reason)
	{
		return Provoke(target, attacker, reason.Blueprint);
	}

	public void Provoke(BaseUnitEntity target, BlueprintFact reason)
	{
		foreach (BaseUnitEntity engagedByUnit in target.GetEngagedByUnits())
		{
			Provoke(target, engagedByUnit, reason);
		}
	}

	public void Provoke(BaseUnitEntity target, EntityFact reason)
	{
		Provoke(target, reason.Blueprint);
	}

	[CanBeNull]
	private static GridNodeBase FindSuitablePositionForAttackOfOpportunity(BaseUnitEntity attacker, BaseUnitEntity target)
	{
		GridNode gridNode = attacker?.CurrentUnwalkableNode;
		if (gridNode == null)
		{
			return null;
		}
		int threatRange = attacker.GetThreatHand().Weapon.ThreatRange;
		int num = attacker.DistanceToInCells(target);
		if (threatRange >= num)
		{
			return gridNode;
		}
		int width = target.SizeRect.Width;
		int height = target.SizeRect.Height;
		int num2 = num + Math.Max(width, height) + Math.Min(width, height) / 2 + 1;
		List<(GraphNode Node, int Distance)> list = PathfindingService.Instance.FindAllReachableTiles_Blocking(attacker.MovementAgent, gridNode.Vector3Position(), num2).Keys.Select((GraphNode i) => (Node: i, Distance: target.DistanceToInCells(i.Vector3Position()))).ToTempList();
		list.Sort(((GraphNode Node, int Distance) n1, (GraphNode Node, int Distance) n2) => n1.Distance.CompareTo(n2.Distance));
		foreach (var item in list)
		{
			if (attacker.IsThreat(target.CurrentUnwalkableNode, item.Node.Vector3Position(), target.SizeRect) && attacker.CanStandHere(item.Node))
			{
				return (GridNodeBase)item.Node;
			}
		}
		return null;
	}
}
