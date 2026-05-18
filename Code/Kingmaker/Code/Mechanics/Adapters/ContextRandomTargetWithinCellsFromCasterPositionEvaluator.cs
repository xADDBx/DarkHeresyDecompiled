using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[Obsolete]
[TypeId("b29cfc3138924d069d58375def75a80e")]
public class ContextRandomTargetWithinCellsFromCasterPositionEvaluator : PositionEvaluator
{
	public ConditionsChecker ConditionsOnTarget;

	public ContextValue Range;

	public override string GetCaption()
	{
		return "Evaluate position of random enemy around caster";
	}

	protected override Vector3 GetValueInternal()
	{
		return Vector3.zero;
	}

	private bool IsConditionPassed(AbilityExecutionContext context, BaseUnitEntity unit)
	{
		using (EvalContext.PushContext(context, unit))
		{
			return ConditionsOnTarget.Check();
		}
	}
}
