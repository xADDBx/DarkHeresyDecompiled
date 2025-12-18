using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d9675b68ab7715d42ba3bb44256ddd6b")]
public class AddAbilityUseTargetTrigger : UnitFactComponentDelegate, ITargetRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, ITargetRulebookSubscriber
{
	public ActionList Action;

	public bool AfterCast;

	public AbilityType Type;

	public bool ToCaster;

	public bool SpellList;

	[ShowIf("SpellList")]
	[SerializeField]
	[FormerlySerializedAs("Spells")]
	private BlueprintAbilityReference[] m_Spells = new BlueprintAbilityReference[0];

	public ReferenceArrayProxy<BlueprintAbility> Spells
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] spells = m_Spells;
			return spells;
		}
	}

	private void RunAction(RulePerformAbility evt)
	{
		_ = evt.Target;
		if ((evt.Reason.Ability.Blueprint.Type == Type && !SpellList) || Spells.ContainsAbility(evt.Reason.Ability.Blueprint))
		{
			if (ToCaster)
			{
				base.Fact.RunActionInContext(Action, evt.ConcreteInitiator.ToITargetWrapper());
			}
			else
			{
				base.Fact.RunActionInContext(Action, base.OwnerTargetWrapper);
			}
		}
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
		if (!AfterCast)
		{
			RunAction(evt);
		}
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (AfterCast)
		{
			RunAction(evt);
		}
	}
}
