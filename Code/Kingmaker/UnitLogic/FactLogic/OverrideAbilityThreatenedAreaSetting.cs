using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete("Duplicating functionality of BlueprintAbilityModifier")]
[TypeId("b1c174e0e5e64195a67810a635bee1de")]
public class OverrideAbilityThreatenedAreaSetting : UnitFactComponentDelegate
{
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public BlueprintAbility.UsingInThreateningAreaType ThreatenedAreaRule;

	protected override void OnActivateOrPostLoad()
	{
	}

	protected override void OnDeactivate()
	{
	}

	public BlueprintAbility.UsingInThreateningAreaType? GetThreatenedAreaRule(AbilityData ability)
	{
		using AbilityExecutionContext context = ability.ClaimExecutionContext(base.Owner);
		return Restriction.IsPassed(context) ? new BlueprintAbility.UsingInThreateningAreaType?(ThreatenedAreaRule) : null;
	}
}
