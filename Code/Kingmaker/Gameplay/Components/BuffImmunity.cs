using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowMultipleComponents]
[ComponentName("Immunity/BuffImmunity")]
[TypeId("612a897cf3044484bbdd1e4ca1fcb4a5")]
public sealed class BuffImmunity : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateCanApplyBuff>, IRulebookHandler<RuleCalculateCanApplyBuff>, ISubscriber, IInitiatorRulebookSubscriber
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public bool DisableGameLog;

	void IRulebookHandler<RuleCalculateCanApplyBuff>.OnEventAboutToTrigger(RuleCalculateCanApplyBuff evt)
	{
		if (Restrictions.IsPassed(evt.Context, null, null, evt))
		{
			evt.Immunity.Add(base.Runtime);
			evt.DisableGameLog = DisableGameLog;
		}
	}

	void IRulebookHandler<RuleCalculateCanApplyBuff>.OnEventDidTrigger(RuleCalculateCanApplyBuff evt)
	{
	}
}
