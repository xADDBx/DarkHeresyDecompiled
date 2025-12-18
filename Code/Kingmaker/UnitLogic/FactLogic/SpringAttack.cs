using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("002a7f83be294e62bce99246de4f5fb4")]
public class SpringAttack : UnitFactComponentDelegate
{
	public BlueprintAbilityReference DeathWaltz;

	public BlueprintAbilityReference DeathWaltzUltimate;

	public BlueprintAreaEffectReference AreaMark;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartSpringAttack>().DeathWaltzBlueprint = DeathWaltz;
		base.Owner.GetOrCreate<UnitPartSpringAttack>().DeathWaltzUltimateBlueprint = DeathWaltzUltimate;
		base.Owner.GetOrCreate<UnitPartSpringAttack>().AreaMark = AreaMark;
		base.Owner.GetOrCreate<UnitPartSpringAttack>().SpringAttackFeature = base.Fact;
	}
}
