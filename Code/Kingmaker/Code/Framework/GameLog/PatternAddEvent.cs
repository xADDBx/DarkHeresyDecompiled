using System.Collections.Generic;

namespace Kingmaker.Code.Framework.GameLog;

public abstract class PatternAddEvent
{
	public bool TryApply(List<GameLogEvent> queue, GameLogEvent @in, out GameLogEvent @out)
	{
		if (@in != null && TryApplyImpl(queue, @in, out @out))
		{
			if (@out == null)
			{
				@out = @in;
			}
			return true;
		}
		@out = null;
		return false;
	}

	protected abstract bool TryApplyImpl(List<GameLogEvent> queue, GameLogEvent @in, out GameLogEvent @out);
}
