using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[Serializable]
[ComponentName("Target Restriction/AbilityUnrestrictedRangeForTarget")]
[TypeId("ea316121d4594ee6be95f23582aa6f10")]
public class AbilityUnrestrictedRangeForTarget : BlueprintComponent
{
	public PropertyCalculator TargetCondition;

	public bool IsRangeUnrestrictedForTarget(AbilityData ability, TargetWrapper target)
	{
		if (target.Entity == null)
		{
			return false;
		}
		using AbilityExecutionContext context = ability.ClaimExecutionContext(target);
		return TargetCondition.GetBoolValue(ability.Caster, context, target);
	}
}
