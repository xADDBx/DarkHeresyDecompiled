using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters.Int;

[Serializable]
[TypeId("a916e9198f34412893ae19f9cea62e34")]
public sealed class ReceivedCriticalEffectStacksGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Received critical effect stacks";
	}

	protected override int GetBaseValue()
	{
		return Rulebook.CurrentContext.LastEventOfType<RulePerformCriticalEffects>()?.ResultAmount ?? 0;
	}
}
