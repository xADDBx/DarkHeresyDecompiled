using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters.Checkers;

[Serializable]
[TypeId("6f026025f718423890b360b882bee362")]
public sealed class CheckPreciseAttackGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check is precise attack";
	}

	protected override bool GetBaseValue()
	{
		return (this.GetAbility() ?? Rulebook.Instance.Context.LastEventOfType<RulePerformAttack>()?.Ability)?.IsPrecise ?? false;
	}
}
