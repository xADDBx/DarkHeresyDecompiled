using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters.Checkers;

[Serializable]
[TypeId("126120fa9573426f84e995ef42933bc6")]
public sealed class CheckCurrentEntityCoveredTargetFromAttack : BoolPropertyGetter, PropertyContextAccessor.IOptionalTargetByType, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Current entity is cover of {Target} in context of attack";
	}

	protected override bool GetBaseValue()
	{
		MechanicEntity entityByType = EvalContext.Current.GetEntityByType(Target);
		if (entityByType == null)
		{
			return false;
		}
		RulePerformAttack rulePerformAttack = Rulebook.Instance.Context.LastEventOfType<RulePerformAttack>();
		if (rulePerformAttack == null)
		{
			return false;
		}
		if (rulePerformAttack.Target == base.CurrentEntity && rulePerformAttack.RollPerformAttackRule.Target == entityByType)
		{
			return rulePerformAttack.RollPerformAttackRule.ResultIsCoverHit;
		}
		return false;
	}
}
