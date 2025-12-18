using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[Serializable]
[ComponentName("LoS and Covers/AbilityIgnoreLosForTarget")]
[TypeId("11be9e4a458424f42bc07a9d9d3f4d14")]
public class AbilityIgnoreLosForTarget : BlueprintComponent
{
	public PropertyCalculator TargetCondition;

	public bool IsIgnoredLoSForTarget(AbilityData ability, TargetWrapper target)
	{
		if (target.Entity == null)
		{
			return false;
		}
		using AbilityExecutionContext context = ability.ClaimExecutionContext(target);
		return TargetCondition.GetBoolValue(ability.Caster, context, target);
	}
}
