using System;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformMoraleChange : RulebookTargetEvent
{
	public readonly MoraleEventType EventType;

	public readonly RuleCalculateMoraleChange RuleCalculateMoraleChange;

	[CanBeNull]
	public MechanicsContext SourceContext { get; set; }

	public int MoraleAfterEvent { get; private set; }

	public int MoraleBeforeEvent { get; private set; }

	public MoralePhaseType? ResultMoralePhase { get; private set; }

	public CompositeModifiersManager ValueModifier => RuleCalculateMoraleChange.ValueModifier;

	public CompositeModifiersManager BottomLimitModifier => RuleCalculateMoraleChange.BottomLimitModifier;

	public CompositeModifiersManager TopLimitModifier => RuleCalculateMoraleChange.TopLimitModifier;

	public int ResultDelta => RuleCalculateMoraleChange.ResultDelta;

	public int ResultDeltaRaw => RuleCalculateMoraleChange.ResultDeltaRaw;

	public int BaseValue => RuleCalculateMoraleChange.BaseValue;

	public int TopLimit => TopLimitModifier.Value;

	public int BottomLimit => BottomLimitModifier.Value;

	[NotNull]
	public new BaseUnitEntity TargetUnit => (base.Target as BaseUnitEntity) ?? throw new InvalidOperationException("Target is not a BaseUnitEntity");

	public bool FromCriticalEffect
	{
		get
		{
			if (SourceContext?.Blueprint is BlueprintBuff blueprintBuff)
			{
				return blueprintBuff.CriticalEffect;
			}
			return false;
		}
	}

	public RulePerformMoraleChange([NotNull] IMechanicEntity initiator, IMechanicEntity target, MoraleEventType eventType, int baseValue = 0)
		: base(initiator, target)
	{
		EventType = eventType;
		RuleCalculateMoraleChange = new RuleCalculateMoraleChange(initiator, target, eventType, baseValue);
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(RuleCalculateMoraleChange);
		MoraleBeforeEvent = TargetUnit.Morale.Value;
		TargetUnit.Morale.SetValue(RuleCalculateMoraleChange.Result, base.Initiator, FromCriticalEffect);
		MoraleAfterEvent = TargetUnit.Morale.Value;
		ResultMoralePhase = TargetUnit.Morale.Phase;
	}
}
