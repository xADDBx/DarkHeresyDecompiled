using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Gameplay.Features.AreaEffects.Parts;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.AreaEffects.Components;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Immunity/AreaEffectImmunity")]
[TypeId("60d19e8804e64dae98d6cf1f572dadce")]
public sealed class AreaEffectImmunity : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAreaEffectImmunity>().Register(base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAreaEffectImmunity>()?.Unregister(base.Fact, this);
	}
}
