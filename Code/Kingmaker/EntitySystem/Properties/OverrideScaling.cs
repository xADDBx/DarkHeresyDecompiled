using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Gameplay.Features.Scaling.Parts;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties;

[Serializable]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Scaling/OverrideScaling")]
[TypeId("2aaa1989ea674ad29f2abf821802d69e")]
public sealed class OverrideScaling : MechanicEntityFactComponentDelegate
{
	public BpRef<BlueprintMechanicEntityFact>[] SuitableFacts;

	public BpRef<BlueprintAbilityGroup>[] SuitableAbilityGroups;

	public LocalizedString Description;

	public PropertyCalculator Calculator;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartOverrideScaling>().Add(base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartOverrideScaling>()?.Remove(base.Fact, this);
	}
}
