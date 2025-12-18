using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem.Rules.Utility;

public interface IRuleWithChanceRoll : IRulebookEvent
{
	int Chance { get; }

	StatType? Stat { get; }

	MechanicEntity? AttackInitiator { get; }

	ChanceRollType RollType { get; }
}
