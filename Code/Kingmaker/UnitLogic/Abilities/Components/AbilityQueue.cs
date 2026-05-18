using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("a2cb91a2b5d142648acab0e10a1bc6f1")]
public class AbilityQueue : AbilityDeliverEffect
{
	[Serializable]
	public class AbilityQueueItem
	{
		public ConditionsChecker Conditions;

		public BlueprintAbilityReference Ability;
	}

	[SerializeField]
	private List<AbilityQueueItem> m_AbilityQueue = new List<AbilityQueueItem>();

	public override bool IsEngageUnit => true;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		foreach (AbilityQueueItem item in m_AbilityQueue)
		{
			if (item.Conditions.Check())
			{
				MechanicEntity caster = context.Caster;
				PartUnitCommands commandsOptional = caster.GetCommandsOptional();
				if (commandsOptional == null)
				{
					PFLog.Default.Error(context.AbilityBlueprint, "Actual Caster has no commands");
					continue;
				}
				UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(new AbilityData(item.Ability, caster), context.ClickedTarget)
				{
					FreeAction = true
				};
				commandsOptional.AddToQueue(cmdParams);
			}
		}
		yield return null;
	}
}
