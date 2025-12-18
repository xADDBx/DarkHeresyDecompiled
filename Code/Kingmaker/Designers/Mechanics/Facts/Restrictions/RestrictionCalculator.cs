using System;
using Framework.Utility.Attributes;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
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

	public bool IsPassed([NotNull] MechanicsContext context, MechanicEntity currentEntity = null, TargetWrapper currentTarget = null, RulebookEvent rule = null, AbilityData ability = null)
	{
		if (currentEntity == null)
		{
			currentEntity = context.ClickedTarget.Entity ?? context.MaybeOwner ?? throw new NullReferenceException();
		}
		return IsPassedInternal(new PropertyContext(currentEntity, context, currentTarget, rule, ability));
	}

	public bool IsPassed(PropertyContext context)
	{
		return IsPassedInternal(context);
	}

	protected virtual bool IsPassedInternal(PropertyContext context)
	{
		if (Property == null)
		{
			return true;
		}
		if (!Property.Empty)
		{
			return Property.GetBoolValue(context);
		}
		return true;
	}
}
