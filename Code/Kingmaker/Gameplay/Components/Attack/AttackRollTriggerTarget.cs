using System;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.Attack;

[Serializable]
[TypeId("cbfc5b7d5be196f4cb0f6616415c6a44")]
public sealed class AttackRollTriggerTarget : AttackRollTrigger, ITargetRulebookHandler<RulePerformAttackRoll>, IRulebookHandler<RulePerformAttackRoll>, ISubscriber, ITargetRulebookSubscriber
{
	public void OnEventAboutToTrigger(RulePerformAttackRoll evt)
	{
		TryTrigger(evt, before: true);
	}

	public void OnEventDidTrigger(RulePerformAttackRoll evt)
	{
		TryTrigger(evt, before: false);
	}
}
