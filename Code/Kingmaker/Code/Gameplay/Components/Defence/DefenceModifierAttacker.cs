using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components.Defence;

[Serializable]
[ComponentName("Defence/DefenceModifierAttacker")]
[TypeId("5ddec4bbd57c4351abe700f32a93d202")]
public class DefenceModifierAttacker : DefenceModifier, IInitiatorRulebookHandler<RuleCalculateDefence>, IRulebookHandler<RuleCalculateDefence>, ISubscriber, IInitiatorRulebookSubscriber
{
	public void OnEventAboutToTrigger(RuleCalculateDefence evt)
	{
		TryApply(evt);
	}

	public void OnEventDidTrigger(RuleCalculateDefence evt)
	{
	}
}
