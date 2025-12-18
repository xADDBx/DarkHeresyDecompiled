using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("16478eab25dfeb64b8375927b3e8f08b")]
public class AbilityStarshipPushPhaseRestriction : BlueprintComponent
{
	public bool allowMiddlePhase;
}
