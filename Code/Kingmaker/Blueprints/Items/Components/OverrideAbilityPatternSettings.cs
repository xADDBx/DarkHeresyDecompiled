using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.PatternAttack;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("2968fb1da46b4f719f835b33145974f1")]
public class OverrideAbilityPatternSettings : UnitFactComponentDelegate
{
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public AbilityAoEPatternSettings PatternSettings = new AbilityAoEPatternSettings();

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAbilityPatternSettings>().Add(this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAbilityPatternSettings>()?.Remove(this);
	}

	[CanBeNull]
	public AbilityAoEPatternSettings GetPatternSettings(AbilityData ability)
	{
		using AbilityExecutionContext context = ability.ClaimExecutionContext(base.Owner);
		return Restriction.IsPassed(context, base.Owner) ? PatternSettings : null;
	}
}
