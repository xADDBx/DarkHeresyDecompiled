using System.Collections.Generic;

namespace Kingmaker.RuleSystem.Rules.Interfaces;

public interface IRuleRollDice
{
	List<int> RollHistory { get; }

	int Result { get; }

	List<RerollData> Rerolls { get; }
}
