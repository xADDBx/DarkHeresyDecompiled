using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("86cfc07f29bec904ab50406e1afe7640")]
public class WarhammerRefundAbilityCost : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IInitiatorRulebookSubscriber
{
	public bool ForOneAbility;

	[ShowIf("ForOneAbility")]
	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	public bool ForMultipleAbilities;

	[ShowIf("ForMultipleAbilities")]
	[SerializeField]
	public List<BlueprintAbilityReference> Abilities;

	public bool ForAbilityGroup;

	[ShowIf("ForAbilityGroup")]
	public BlueprintAbilityGroupReference AbilityGroup;

	public bool refundAP;

	public BlueprintAbility Ability => m_Ability?.Get();

	private void RunAction(AbilityData ability, TargetWrapper _)
	{
		if ((!ForOneAbility || ability.Blueprint.SameAbility(Ability)) && (!ForMultipleAbilities || Abilities.ContainsAbility(ability.Blueprint)) && (!ForAbilityGroup || ability.Blueprint.AbilityGroups.Contains(AbilityGroup)) && refundAP && base.Context.MaybeCaster != null)
		{
			base.Context.MaybeCaster.GetCombatStateOptional()?.GainActionPoints(ability.CalculateActionPointCost(), base.Context);
		}
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		RunAction(evt.Ability, evt.AbilityTarget);
	}
}
