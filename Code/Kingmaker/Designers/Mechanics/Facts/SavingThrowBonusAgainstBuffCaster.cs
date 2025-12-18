using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("36e0e76d6e174ef28bd66ade47a316ac")]
public class SavingThrowBonusAgainstBuffCaster : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber
{
	public ModifierDescriptor ModifierDescriptor;

	public int Multiplier = 1;

	public ContextValue Bonus;

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public SavingThrowType Type;

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
		MechanicEntity mechanicEntity = evt.Reason.Ability?.Caster;
		if (mechanicEntity != null && mechanicEntity == base.Context.MaybeCaster && (Type == SavingThrowType.Unknown || evt.Type == Type) && Restrictions.IsPassed(base.Context, null, null, evt))
		{
			int value = Bonus.Calculate(base.Context) * Multiplier;
			evt.ValueModifiers.Add(value, base.Fact, ModifierDescriptor);
		}
	}

	public void OnEventDidTrigger(RulePerformSavingThrow evt)
	{
	}
}
