using System;
using Framework.Utility.Attributes;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Framework;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.Restrictions;

[Serializable]
[InspectorExpandedByDefault]
public class RestrictionCalculator
{
	public PropertyCalculator Property = new PropertyCalculator
	{
		Operation = PropertyCalculator.OperationType.And
	};

	public bool Empty => Property.Empty;

	public bool IsPassed([NotNull] IEvalContext evalContext, in StatContext statContext)
	{
		return IsPassed(evalContext, null, null, statContext.Rule, statContext.Ability);
	}

	public bool IsPassed([NotNull] IEvalContext context, MechanicEntity currentEntity = null, TargetWrapper currentTarget = null, RulebookEvent rule = null, AbilityData ability = null)
	{
		if (Empty)
		{
			return true;
		}
		if (currentEntity == null)
		{
			currentEntity = context.ClickedTarget?.Entity ?? context.Owner;
		}
		if (currentEntity == null)
		{
			throw new InvalidOperationException("RestrictionCalculator.IsPassed: cannot resolve currentEntity (context has no ClickedTarget.Entity and no Owner — likely called outside an active EvalContext frame). " + $"Context type={context.GetType().Name}, Property.Empty={Property?.Empty}");
		}
		return IsPassedInternal(currentEntity, context, currentTarget, rule, ability);
	}

	protected virtual bool IsPassedInternal(MechanicEntity entity, IEvalContext context = null, TargetWrapper target = null, RulebookEvent rule = null, AbilityData ability = null)
	{
		if (Property == null)
		{
			return true;
		}
		if (!Property.Empty)
		{
			return Property.GetBoolValue(entity, context, target, rule, ability);
		}
		return true;
	}
}
