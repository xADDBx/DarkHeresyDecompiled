using System;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.Attack;

[Serializable]
[TypeId("b56e43debfbc431fa725c5580cd4999a")]
public sealed class AttackTriggerTarget : AttackTrigger, ITargetRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, ITargetRulebookSubscriber
{
	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
		TryTrigger(evt, before: true);
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		TryTrigger(evt, before: false);
	}
}
