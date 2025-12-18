using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("dee683dec5044013980e0bb396bd3c46")]
public class WarhammerDisableAttacksWithChosenWeapon : UnitFactComponentDelegate
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<WarhammerUnitPartDisableAttack>()?.RetainDisabled(base.Fact);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<WarhammerUnitPartDisableAttack>()?.ReleaseDisabled(base.Fact);
	}
}
