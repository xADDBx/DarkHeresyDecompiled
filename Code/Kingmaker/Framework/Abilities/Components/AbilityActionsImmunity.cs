using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.Abilities.Components;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Immunity/AbilityActionsImmunity")]
[TypeId("ac869d657b7b4721bd38bb5756d76929")]
public sealed class AbilityActionsImmunity : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAbilityActionsImmunity>().Register(base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAbilityActionsImmunity>()?.Unregister(base.Fact, this);
	}
}
