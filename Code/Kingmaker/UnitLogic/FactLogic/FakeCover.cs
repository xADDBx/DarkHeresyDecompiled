using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("LoS and Covers/FakeCover")]
[TypeId("dcf580788bcc4b01be0853ad7f2b44cd")]
public class FakeCover : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, ITargetRulebookSubscriber
{
	[SerializeField]
	private LosCalculations.CoverType m_CoverType;

	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private bool m_IgnoreRealCover;

	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		if (m_Restrictions.IsPassed(base.Context, null, null, evt))
		{
			evt.SetFakeCover(m_CoverType, m_IgnoreRealCover);
		}
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
	{
	}
}
