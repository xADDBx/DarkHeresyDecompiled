using System;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.Attack;

[Serializable]
[TypeId("003c5cc2077040279d9b618b3f49c297")]
public sealed class AttackTriggerGlobal : AttackTrigger, IGlobalRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, IGlobalRulebookSubscriber
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
