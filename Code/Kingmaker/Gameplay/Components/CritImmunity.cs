using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowMultipleComponents]
[ComponentName("Immunity/CritImmunity")]
[TypeId("ed0ad69280244c3d80f22d8ad0dae7ac")]
public sealed class CritImmunity : MechanicEntityFactComponentDelegate, ITargetRulebookHandler<RulePerformCriticalEffects>, IRulebookHandler<RulePerformCriticalEffects>, ISubscriber, ITargetRulebookSubscriber
{
	void IRulebookHandler<RulePerformCriticalEffects>.OnEventAboutToTrigger(RulePerformCriticalEffects evt)
	{
		evt.Immunity.Add(base.Runtime);
	}

	void IRulebookHandler<RulePerformCriticalEffects>.OnEventDidTrigger(RulePerformCriticalEffects evt)
	{
	}
}
