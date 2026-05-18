using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Framework;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[Serializable]
[ComponentName("Target Restriction/AbilityUnrestrictedRangeForTarget")]
[TypeId("ea316121d4594ee6be95f23582aa6f10")]
[SetsContextScope(ContextEntryPointKind.AbilityOnCast)]
public class AbilityUnrestrictedRangeForTarget : BlueprintComponent
{
	public PropertyCalculator TargetCondition;

	public bool IsRangeUnrestrictedForTarget(AbilityData ability, TargetWrapper target)
	{
		if (target.Entity == null)
		{
			return false;
		}
		IEvalContext ctx;
		using (EvalContext.PushAbility(ability, target).Get(out ctx))
		{
			return TargetCondition.GetBoolValue(ability.Caster, ctx, target);
		}
	}
}
