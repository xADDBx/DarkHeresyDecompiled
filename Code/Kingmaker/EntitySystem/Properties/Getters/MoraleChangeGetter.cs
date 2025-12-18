using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("49818a0c0afa0554f8cd9d916ff38b84")]
public class MoraleChangeGetter : IntPropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		RulebookEvent rule = this.GetRule();
		if (rule is RulePerformMoraleChange rulePerformMoraleChange)
		{
			return rulePerformMoraleChange.ResultDelta;
		}
		if (rule is RuleCalculateMoraleChange ruleCalculateMoraleChange)
		{
			return ruleCalculateMoraleChange.ValueModifier.Value;
		}
		throw new ElementLogicException(this);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Morale Change";
	}
}
