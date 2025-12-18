using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("3308af13ece2c324e8017c9b973e5f61")]
public class IsUnconscious : Condition
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override string GetConditionCaption()
	{
		return $"{Unit} is unconscious";
	}

	protected override bool CheckCondition()
	{
		AbstractUnitEntity value = Unit.GetValue();
		if (value != null)
		{
			return !value.IsConscious;
		}
		return false;
	}
}
