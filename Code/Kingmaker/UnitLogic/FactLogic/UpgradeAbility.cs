using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("7b4e1ad2c1084f73aa7f7f03e92b73e2")]
public class UpgradeAbility : MechanicEntityFactComponentDelegate, IHiddenFacts
{
	public BpRef<BlueprintAbility> BaseAbility;

	public IEnumerable<BlueprintFact> Facts => new BlueprintAbility[1] { BaseAbility.Blueprint };

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartHiddenFacts>().Add(base.Fact, this);
		base.Owner.GetOptional<PartAbilityModifiers>()?.RemoveAllManuallyAddedModifiers(BaseAbility);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartHiddenFacts>()?.Remove(base.Fact, this);
	}
}
