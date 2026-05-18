using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("dc5eb27f0bb8409581dc204d42dc5a5d")]
public class IncreaseAbilityGroupRange : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityRange>, IRulebookHandler<RuleCalculateAbilityRange>, ISubscriber, IInitiatorRulebookSubscriber
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	public ContextValue AdditionalRange;

	public bool OnlyNonMelee;

	[SerializeField]
	private BlueprintAbilityGroupReference m_AbilityGroup;

	public BlueprintAbilityGroup AbilityGroup => m_AbilityGroup.Get();

	public void OnEventAboutToTrigger(RuleCalculateAbilityRange evt)
	{
	}

	public void OnEventDidTrigger(RuleCalculateAbilityRange evt)
	{
	}
}
