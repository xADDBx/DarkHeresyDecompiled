using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("599e0ccccecb495469f5898591a28e1c")]
public class WarhammerAbilityCostChangeContextValue : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityActionPointCost>, IRulebookHandler<RuleCalculateAbilityActionPointCost>, ISubscriber, IInitiatorRulebookSubscriber
{
	public ContextValue Value;

	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	public BlueprintAbility Ability => m_Ability?.Get();

	public void OnEventAboutToTrigger(RuleCalculateAbilityActionPointCost evt)
	{
	}

	public void OnEventDidTrigger(RuleCalculateAbilityActionPointCost evt)
	{
	}
}
