using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.RuleBurst;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("021e41e47a524979b673afecd1f9f37e")]
public sealed class BurstWeightModifierInitiator : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateTargetInBurst>, IRulebookHandler<RuleCalculateTargetInBurst>, ISubscriber, IInitiatorRulebookSubscriber
{
	[SerializeField]
	private BurstWeightSettings m_BurstWeightSettings;

	public void OnEventAboutToTrigger(RuleCalculateTargetInBurst evt)
	{
		evt.OverridenBurstSettings = m_BurstWeightSettings;
	}

	public void OnEventDidTrigger(RuleCalculateTargetInBurst evt)
	{
	}
}
