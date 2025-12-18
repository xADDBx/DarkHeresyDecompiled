using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("228f1225a79243aebd68d0e9df209f5e")]
public abstract class MoraleChangeModifier : MechanicEntityFactComponentDelegate
{
	[Obsolete]
	public enum FilterType
	{
		None,
		GainMorale,
		LoseMorale
	}

	public enum InverseType
	{
		None,
		Any,
		GainMorale,
		LoseMorale
	}

	public MoraleEventType EventFilter = (MoraleEventType)(-1);

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public InverseType Inverse;

	public ContextValueModifierWithType ValueModifier = new ContextValueModifierWithType();

	public ContextValueModifierWithType PositiveValueModifier = new ContextValueModifierWithType();

	public ContextValueModifierWithType NegativeValueModifier = new ContextValueModifierWithType();

	public ContextValueModifierWithType BottomLimitModifier = new ContextValueModifierWithType();

	public ContextValueModifierWithType TopLimitModifier = new ContextValueModifierWithType();

	public ContextValueModifierWithType AutoRegenModifier = new ContextValueModifierWithType();

	public ModifierDescriptor Descriptor;

	protected void TryApply(RuleCalculateMoraleChange evt)
	{
		if (IsEventSuitable(evt))
		{
			ValueModifier.TryApply(evt.ValueModifier, base.Fact, Descriptor);
			PositiveValueModifier.TryApply(evt.PositiveValueModifier, base.Fact, Descriptor);
			NegativeValueModifier.TryApply(evt.NegativeValueModifier, base.Fact, Descriptor);
			BottomLimitModifier.TryApply(evt.BottomLimitModifier, base.Fact, Descriptor);
			TopLimitModifier.TryApply(evt.TopLimitModifier, base.Fact, Descriptor);
			if (evt.EventType.HasAnyFlag(MoraleEventType.TurnStart))
			{
				AutoRegenModifier.TryApply(evt.ValueModifier, base.Fact, Descriptor);
			}
			switch (Inverse)
			{
			case InverseType.Any:
				evt.InversePositiveFlag.Add(base.Fact, Descriptor);
				evt.InverseNegativeFlag.Add(base.Fact, Descriptor);
				break;
			case InverseType.GainMorale:
				evt.InversePositiveFlag.Add(base.Fact, Descriptor);
				break;
			case InverseType.LoseMorale:
				evt.InverseNegativeFlag.Add(base.Fact, Descriptor);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case InverseType.None:
				break;
			}
		}
	}

	private bool IsEventSuitable(RuleCalculateMoraleChange evt)
	{
		if (evt.EventType.HasAnyFlag(EventFilter))
		{
			return Restrictions.IsPassed(base.Context, evt.Target, null, evt);
		}
		return false;
	}
}
