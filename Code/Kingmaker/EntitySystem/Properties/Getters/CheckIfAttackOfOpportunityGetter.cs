using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("5ffd55f3680349e0b0dda12f892c7a12")]
public class CheckIfAttackOfOpportunityGetter : BoolPropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		AbilityData abilityData = base.PropertyContext.Ability ?? this.GetRule().Reason.Ability;
		if ((object)abilityData != null && abilityData.IsAttackOfOpportunity)
		{
			return true;
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if Attack of Opportunity";
	}
}
