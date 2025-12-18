using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects.Traps;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[ComponentName("Buff on spawned unit")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("783eb40c7bc69c047b363f92908f7632")]
public class TrapPerceptionBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>, ISubscriber, IInitiatorRulebookSubscriber
{
	public ModifierDescriptor Descriptor;

	public ContextValue Value;

	public void OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
		if (evt.Reason.SourceEntity is TrapObjectData)
		{
			evt.DifficultyModifiers.Add(ModifierType.ValAdd, Value.Calculate(base.Context), base.Fact, Descriptor);
		}
	}

	public void OnEventDidTrigger(RulePerformSkillCheck evt)
	{
	}
}
