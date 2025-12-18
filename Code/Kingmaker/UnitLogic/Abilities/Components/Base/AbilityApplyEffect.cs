using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintAbilityModifier))]
[TypeId("47e8b94537afb3641af59ff9a5cd48e8")]
public abstract class AbilityApplyEffect : BlueprintComponent
{
	public abstract void Apply(AbilityExecutionContext context, TargetWrapper target);
}
