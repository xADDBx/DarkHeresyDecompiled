using System.Collections.Generic;

namespace Kingmaker.Code.Framework.GameLog;

public interface IPatternCollection
{
	IPatternCollection AddPattern(PatternAddEvent pattern);

	IPatternCollection AddPattern(PatternPostAddEvent pattern);

	void ApplyPatterns(List<GameLogEvent> eventsQueue, GameLogEvent @event);

	void Cleanup();
}
