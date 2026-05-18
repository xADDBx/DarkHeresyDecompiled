using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("850e481c33c718f419591b97dfed5f54")]
public class SavingThrowBonusAgainstAbilityType : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber
{
	public AbilityType AbilityType;

	public ModifierDescriptor ModifierDescriptor;

	public int Value;

	public ContextValue Bonus;

	public bool OnlyPositiveValue;

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSavingThrow evt)
	{
	}
}
