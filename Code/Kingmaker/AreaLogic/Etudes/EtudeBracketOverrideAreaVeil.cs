using System;
using Kingmaker.Code.AreaLogic;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[Serializable]
[Obsolete]
[TypeId("5e6356b32b3149b68d77a844fb2cbab5")]
public class EtudeBracketOverrideAreaVeil : EtudeBracketTrigger, IGlobalRulebookHandler<RuleCalculateVeilDamage>, IRulebookHandler<RuleCalculateVeilDamage>, ISubscriber, IGlobalRulebookSubscriber
{
	public int Value;

	protected override void OnEnter()
	{
		PartVeil veil = Game.Instance.LoadedArea.Veil;
		if (Value > veil.Damage)
		{
			veil.Damage = Value;
		}
	}

	protected override void OnExit()
	{
	}

	void IRulebookHandler<RuleCalculateVeilDamage>.OnEventAboutToTrigger(RuleCalculateVeilDamage evt)
	{
		evt.MinVeilDamageModifiers.Add(Value, base.Fact, ModifierDescriptor.UntypedUnstackable);
	}

	void IRulebookHandler<RuleCalculateVeilDamage>.OnEventDidTrigger(RuleCalculateVeilDamage evt)
	{
	}
}
