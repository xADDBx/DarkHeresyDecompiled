using Code.Enums;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.RuleSystem.Rules.RuleDOT;

public class RuleCalculateDOT : RulebookTargetEvent
{
	public readonly DOT Type;

	public readonly int BaseRank;

	public readonly CompositeModifiersManager RankModifier = new CompositeModifiersManager(0);

	public int ResultRank { get; private set; }

	public RuleCalculateDOT([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, DOT type, int baseRank)
		: base(initiator, target)
	{
		Type = type;
		BaseRank = baseRank;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		ResultRank = RankModifier.Apply(BaseRank);
	}
}
