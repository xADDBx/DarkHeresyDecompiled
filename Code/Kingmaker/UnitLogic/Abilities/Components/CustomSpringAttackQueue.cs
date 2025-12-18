using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("a03ec6f0e58b4a2e8339d3e2471854bb")]
public class CustomSpringAttackQueue : AbilityCustomLogic
{
	public BlueprintAbilityReference DeathWaltz;

	public BlueprintAbilityReference MoveAbility;

	public BlueprintBuffReference TemporaryBuff;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		MechanicEntity caster = context.Caster;
		UnitPartSpringAttack sprintAttackPart = caster.GetOptional<UnitPartSpringAttack>();
		PartUnitCommands commands = caster.GetCommandsOptional();
		if (sprintAttackPart == null || commands == null)
		{
			yield break;
		}
		Vector3 turnStartPosition = sprintAttackPart.TurnStartPosition;
		Buff temporaryBuff = caster.Buffs.Add(TemporaryBuff, context);
		List<SpringAttackEntry> springAttackEntries = sprintAttackPart.Entries;
		int index;
		for (index = springAttackEntries.Count; index > 0; index--)
		{
			SpringAttackEntry entry = Enumerable.FirstOrDefault(springAttackEntries, (SpringAttackEntry p) => p.Index == index);
			if (entry == null || !(entry.NewPosition != entry.OldPosition))
			{
				continue;
			}
			if (caster.Position != entry.NewPosition)
			{
				BaseUnitEntity firstUnit = entry.NewPosition.GetNearestNodeXZUnwalkable().GetFirstUnit();
				if (!(firstUnit is UnitEntity) || !firstUnit.IsDeadOrUnconscious)
				{
					UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(CreateAbility(MoveAbility, context), entry.NewPosition)
					{
						FreeAction = true
					};
					UnitCommandHandle moveHandle = commands.AddToQueue(cmdParams);
					while (!moveHandle.IsFinished)
					{
						yield return null;
					}
				}
			}
			UnitUseAbilityParams cmdParams2 = new UnitUseAbilityParams(CreateAbility(DeathWaltz, context), entry.OldPosition)
			{
				FreeAction = true
			};
			UnitCommandHandle jumpHandle = commands.AddToQueue(cmdParams2);
			while (!jumpHandle.IsFinished)
			{
				yield return null;
			}
			if (entry.AreaMark.Entity != null)
			{
				entry.AreaMark.Entity.ForceEnded = true;
			}
			springAttackEntries.Remove(entry);
		}
		sprintAttackPart.RemoveEntries();
		AbilityData ability = CreateAbility(MoveAbility, context);
		if (turnStartPosition.GetNearestNodeXZUnwalkable().GetFirstUnit() is UnitEntity { IsDeadOrUnconscious: false })
		{
			List<GridNodeBase> list = new List<GridNodeBase>();
			for (int i = 0; i < 8; i++)
			{
				GridNodeBase gridNodeBase = turnStartPosition.GetNearestNodeXZUnwalkable()?.GetNeighbourAlongDirection(i);
				if (gridNodeBase != null && (gridNodeBase.GetFirstUnit()?.IsDeadOrUnconscious ?? true))
				{
					list.Add(gridNodeBase);
				}
			}
			turnStartPosition = ((!list.Any()) ? caster.Position : list.Random(PFStatefulRandom.Mechanics).Vector3Position());
		}
		UnitUseAbilityParams cmdParams3 = new UnitUseAbilityParams(ability, turnStartPosition)
		{
			FreeAction = true
		};
		UnitCommandHandle lastMoveHandle = commands.AddToQueue(cmdParams3);
		while (!lastMoveHandle.IsFinished)
		{
			yield return null;
		}
		temporaryBuff?.Remove();
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	private AbilityData CreateAbility(BlueprintAbilityReference ability, AbilityExecutionContext context)
	{
		return new AbilityData(ability, context.Caster);
	}
}
