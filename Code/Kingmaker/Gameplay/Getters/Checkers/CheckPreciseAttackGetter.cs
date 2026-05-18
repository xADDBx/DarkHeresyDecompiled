using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters.Checkers;

[Serializable]
[TypeId("6f026025f718423890b360b882bee362")]
public sealed class CheckPreciseAttackGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public BlueprintReference<BlueprintBodyPart> bodyPart;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check is precise attack" + BodyPartDescription();
	}

	protected override bool GetBaseValue()
	{
		AbilityData abilityData = EvalContext.Current.Ability ?? Rulebook.Instance.Context.LastEventOfType<RulePerformAttack>()?.Ability;
		bool flag = bodyPart.IsEmpty() || abilityData?.PreciseBodyPart == bodyPart.Blueprint;
		return (abilityData?.IsPrecise ?? false) && flag;
	}

	private string BodyPartDescription()
	{
		if (bodyPart.IsEmpty())
		{
			return "";
		}
		return " targeting " + bodyPart.NameSafe();
	}
}
