using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("8a78d682014c463fbeea513add996d23")]
public class AbilitySpecialMoraleAction : BlueprintComponent
{
	public MoraleAbilityType MoralePhaseType;
}
