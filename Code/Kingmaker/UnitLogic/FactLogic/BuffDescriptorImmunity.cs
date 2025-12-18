using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("3577eb7fd5b1caa4ca13855b16201704")]
public class BuffDescriptorImmunity : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateCanApplyBuff>, IRulebookHandler<RuleCalculateCanApplyBuff>, ISubscriber, IInitiatorRulebookSubscriber
{
	public SpellDescriptorWrapper Descriptor;

	[SerializeField]
	[FormerlySerializedAs("IgnoreFeature")]
	private BlueprintUnitFactReference m_IgnoreFeature;

	public bool CheckFact;

	[ShowIf("CheckFact")]
	[SerializeField]
	[FormerlySerializedAs("FactToCheck")]
	private BlueprintUnitFactReference m_FactToCheck;

	public BlueprintUnitFact IgnoreFeature => m_IgnoreFeature?.Get();

	public BlueprintUnitFact FactToCheck => m_FactToCheck?.Get();

	private bool IsImmune(MechanicsContext context)
	{
		return false;
	}

	public void OnEventAboutToTrigger(RuleCalculateCanApplyBuff evt)
	{
		if (IsImmune(evt.Context))
		{
			evt.Immunity.Add(base.Runtime);
		}
	}

	public void OnEventDidTrigger(RuleCalculateCanApplyBuff evt)
	{
	}
}
