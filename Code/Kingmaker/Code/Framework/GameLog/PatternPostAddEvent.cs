using System.Collections.Generic;

namespace Kingmaker.Code.Framework.GameLog;

public abstract class PatternPostAddEvent
{
	public void Apply(List<GameLogEvent> queue, GameLogEvent @event)
	{
		ApplyImpl(queue, @event);
	}

	protected abstract void ApplyImpl(List<GameLogEvent> queue, GameLogEvent @event);
}
