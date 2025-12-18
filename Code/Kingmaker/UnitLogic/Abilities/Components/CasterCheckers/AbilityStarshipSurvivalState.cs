using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("06c3e1c03ba57754b85c5f3434e2ca54")]
public class AbilityStarshipSurvivalState : BlueprintComponent
{
	public int healthPctEqualOrLess = 100;

	public int oneOfShieldsPctEqualOrLess = 100;
}
