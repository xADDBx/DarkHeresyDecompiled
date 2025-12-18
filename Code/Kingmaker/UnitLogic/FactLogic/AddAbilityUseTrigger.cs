using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
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
[TypeId("1c2eac83b528cef478121194c8c7f502")]
public class AddAbilityUseTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IInitiatorRulebookSubscriber, ITargetRulebookHandler<RulePerformAbility>, ITargetRulebookSubscriber
{
	public ActionList Action;

	public bool triggerWhenOwnerIsCaster = true;

	[ShowIf("triggerWhenOwnerIsCaster")]
	public bool assignOwnerAsTarget;

	public bool triggerWhenOwnerIsTarget;

	[ShowIf("triggerWhenOwnerIsTarget")]
	public bool assignCasterAsTarget;

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
	[SerializeField]
	private BlueprintAbilityGroupReference m_AbilityGroup;

	public BlueprintAbility Ability => m_Ability?.Get();

	public BlueprintAbilityGroup AbilityGroup => m_AbilityGroup.Get();

	private void RunAction(BlueprintAbilityWrapper ability, MechanicEntity initiator, TargetWrapper target)
	{
		bool flag = triggerWhenOwnerIsCaster && initiator == base.Context.MaybeOwner;
		bool flag2 = triggerWhenOwnerIsTarget && target.Entity == base.Context.MaybeOwner;
		if ((flag || flag2) && (!ForOneAbility || ability.SameAbility(Ability)) && (!ForMultipleAbilities || Abilities.ContainsAbility(ability)) && (!ForAbilityGroup || ability.AbilityGroups.Contains(AbilityGroup)))
		{
			base.Fact.RunActionInContext(Action, (flag2 && assignCasterAsTarget) ? ((TargetWrapper)initiator) : ((flag && assignOwnerAsTarget) ? ((TargetWrapper)base.Context.MaybeOwner) : target));
		}
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		RunAction(evt.Ability.Blueprint, evt.ConcreteInitiator, evt.AbilityTarget);
	}
}
