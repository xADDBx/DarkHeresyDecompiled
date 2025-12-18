using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components.Defence;

[Serializable]
[ComponentName("Defence/DefenceModifierDefender")]
[TypeId("3aea4d4ca1a44155bb360fd875d554c8")]
public class DefenceModifierDefender : DefenceModifier, ITargetRulebookHandler<RuleCalculateDefence>, IRulebookHandler<RuleCalculateDefence>, ISubscriber, ITargetRulebookSubscriber
{
	public void OnEventAboutToTrigger(RuleCalculateDefence evt)
	{
		TryApply(evt);
	}

	public void OnEventDidTrigger(RuleCalculateDefence evt)
	{
	}
}
