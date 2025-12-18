using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.Framework;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintToggleAbility))]
[TypeId("d88d19701efb4611af3271325cb15734")]
public class UpgradeToggleAbility : MechanicEntityFactComponentDelegate, IHiddenFacts
{
	public BpRef<BlueprintToggleAbility> BaseAbility;

	public IEnumerable<BlueprintFact> Facts => new BlueprintToggleAbility[1] { BaseAbility.Blueprint };

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartHiddenFacts>().Add(base.Fact, this);
		base.Owner.Facts.Get<ToggleAbility>((BlueprintToggleAbility?)BaseAbility)?.SetEnabled(value: false);
		base.Owner.GetOptional<PartAbilityModifiers>()?.RemoveAllBoundModifiers(BaseAbility);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartHiddenFacts>()?.Remove(base.Fact, this);
	}
}
