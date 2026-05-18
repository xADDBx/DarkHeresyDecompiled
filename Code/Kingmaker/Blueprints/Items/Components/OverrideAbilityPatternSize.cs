using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("d547ea8990fe7bc46b3b54f5952c8df6")]
public class OverrideAbilityPatternSize : UnitFactComponentDelegate
{
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public ContextValue AddToSize;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAbilityPatternSettings>().Add(this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAbilityPatternSettings>()?.Remove(this);
	}

	public void OverrideSize(AbilityData ability, IAbilityAoEPatternProvider originPattern)
	{
		if (Restriction.IsPassed(base.Context, base.Owner, null, null, ability))
		{
			int value = AddToSize.Calculate(base.Context);
			originPattern.OverrideHaloSize(value);
		}
	}
}
