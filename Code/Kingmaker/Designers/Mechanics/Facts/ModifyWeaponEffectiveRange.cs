using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("7a108a43007fe9a46a8e78f13028c195")]
public class ModifyWeaponEffectiveRange : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityRange>, IRulebookHandler<RuleCalculateAbilityRange>, ISubscriber, IInitiatorRulebookSubscriber
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public float modifier = 1f;

	public bool extendAbilityRange;

	public void OnEventAboutToTrigger(RuleCalculateAbilityRange evt)
	{
		if (Restrictions.IsPassed(base.Context, null, null, evt) && (!(base.OwnerBlueprint is BlueprintAbility) || evt.Ability.Blueprint.SameAbility(base.OwnerBlueprint as BlueprintAbility)) && extendAbilityRange)
		{
			int defaultRange = evt.DefaultRange;
			evt.Bonus += Mathf.RoundToInt((float)defaultRange * modifier) - defaultRange;
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityRange evt)
	{
	}
}
