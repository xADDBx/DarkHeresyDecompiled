using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components;

[ComponentName("Veil/RecalculateOnVeilChange")]
[TypeId("127c62a8175744e0b8b81987fbe7b1d6")]
public class RecalculateOnVeilChange : UnitFactComponentDelegate, IVeilDamageHandler, ISubscriber
{
	public void OnEventAboutToTrigger(RuleCalculateVeilDamage evt)
	{
	}

	public void HandleVeilDamageChanged(int delta, int value)
	{
		base.Fact.Reapply();
	}
}
