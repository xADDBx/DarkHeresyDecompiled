using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("e63470ddc5d847eb9c563e5240537eec")]
public class BladeDanceFeature : UnitFactComponentDelegate
{
	public BlueprintAbilityReference BladeDanceAbility;

	public ContextValue RateOfAttack;
}
