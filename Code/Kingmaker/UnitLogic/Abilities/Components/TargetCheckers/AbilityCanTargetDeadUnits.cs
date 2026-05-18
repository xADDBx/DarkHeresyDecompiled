using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Framework;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("e2beab7835754e42b16177b95d1e3b12")]
[SetsContextScope(ContextEntryPointKind.AbilityOnCast)]
public sealed class AbilityCanTargetDeadUnits : BlueprintComponent, ICanTargetDeadUnits, IAbilityTargetRestriction
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public LocalizedString UnavailableReason;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return Restrictions.IsPassed(EvalContext.Current, ability.Caster, target, null, ability);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		string text = UnavailableReason?.Text;
		if (text == null || string.IsNullOrEmpty(text))
		{
			return ConfigRoot.Instance.LocalizedTexts.Reasons.UnavailableGeneric.Text;
		}
		return text;
	}
}
