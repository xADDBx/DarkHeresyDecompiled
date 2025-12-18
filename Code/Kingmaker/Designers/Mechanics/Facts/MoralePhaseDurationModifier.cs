using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("a42f3ef69b3944e69219430723ff3a2b")]
public abstract class MoralePhaseDurationModifier : UnitFactComponentDelegate
{
	public MoralePhaseType TriggeringPhase;

	public RestrictionCalculator Restrictions;

	public ContextValue RoundsModifier;

	public ModifierDescriptor Descriptor = ModifierDescriptor.UntypedStackable;

	protected void TryApply(RuleCalculateMoralePhaseDuration evt)
	{
		if (evt.MoralePhase == TriggeringPhase && Restrictions.IsPassed(base.Context, base.Owner))
		{
			evt.Modifiers.Add(ModifierType.ValAdd, RoundsModifier.Value, base.Fact, Descriptor);
		}
	}
}
