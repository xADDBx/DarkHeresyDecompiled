using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[ComponentName("Saving throw bonus against fact")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("f4fc985c97f34c84fbc54c81820930c1")]
public class SavingThrowBonusAgainstFact : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber
{
	[SerializeField]
	[FormerlySerializedAs("CheckedFact")]
	private BlueprintFeatureReference m_CheckedFact;

	public ModifierDescriptor Descriptor;

	public int Value;

	public BlueprintFeature CheckedFact => m_CheckedFact?.Get();

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
		MechanicEntity caster = evt.Reason.Caster;
		if (caster != null && caster.Facts.Contains(CheckedFact))
		{
			evt.ValueModifiers.Add(Value * base.Fact.GetRank(), base.Fact, Descriptor);
		}
	}

	public void OnEventDidTrigger(RulePerformSavingThrow evt)
	{
	}
}
