using System.Linq;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("66e032e5cf38801428940a1a0d14b946")]
public class AbilityEffectRunAction : AbilityApplyEffect, IAbilityPrediction
{
	public ActionList Actions;

	public ActionList ActionsOnAlly;

	public ActionList ActionsOnEnemy;

	public void CollectPrediction(AbilityPredictionContext context)
	{
		MechanicEntity maybeCaster = context.ExecutionContext.MaybeCaster;
		MechanicEntity currentTarget = context.CurrentTarget;
		ActionList actions = GetActions(maybeCaster, currentTarget);
		if (actions != Actions)
		{
			context.ProcessActionList(Actions);
		}
		context.ProcessActionList(actions);
	}

	public override void Apply(AbilityExecutionContext context, TargetWrapper target)
	{
		PFLog.Ability.Log($"Apply {context.Ability.Blueprint.name} ability effect to {target}");
		ActionList actions = GetActions(context.Caster, target);
		if (actions != Actions)
		{
			Actions?.Run();
		}
		actions?.Run();
	}

	[CanBeNull]
	private ActionList GetActions(MechanicEntity caster, TargetWrapper target)
	{
		MechanicEntity entity = target.Entity;
		if (entity == null)
		{
			return Actions;
		}
		if (caster.IsAlly(entity))
		{
			return ActionsOnAlly;
		}
		if (caster.IsEnemy(entity))
		{
			return ActionsOnEnemy;
		}
		return Actions;
	}

	public bool IsValidToCast(TargetWrapper target, MechanicEntity caster, Vector3 casterPosition)
	{
		ActionList actions = GetActions(caster, target);
		if (IsValidToCast(actions, target, caster, casterPosition))
		{
			if (actions != Actions)
			{
				return IsValidToCast(Actions, target, caster, casterPosition);
			}
			return true;
		}
		return false;
	}

	public bool IsValidToCast([CanBeNull] ActionList actions, TargetWrapper target, MechanicEntity caster, Vector3 casterPosition)
	{
		if (actions?.Actions == null || actions.Actions.Length == 0)
		{
			return true;
		}
		return actions.Actions.All((GameAction a) => !(a is ContextAction contextAction) || contextAction.IsValidToCast(target, caster, casterPosition));
	}
}
