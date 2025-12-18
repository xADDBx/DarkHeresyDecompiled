using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateMoralePhaseDuration : RulebookTargetEvent
{
	public readonly MoralePhaseType MoralePhase;

	public readonly MoraleEventType EventType;

	public readonly CompositeModifiersManager Modifiers = new CompositeModifiersManager();

	private static MoraleRoot Settings => MoraleRoot.Instance;

	public int Rounds => Modifiers.Value;

	public RuleCalculateMoralePhaseDuration([NotNull] IMechanicEntity initiator, IMechanicEntity target, MoralePhaseType type, MoraleEventType eventType)
		: base(initiator, target)
	{
		MoralePhase = type;
		EventType = eventType;
		Modifiers.Add(ModifierType.ValAdd, Settings.MoralLockRoundsLength, this, ModifierDescriptor.BaseValue);
	}

	public override void OnTrigger(RulebookEventContext context)
	{
	}
}
