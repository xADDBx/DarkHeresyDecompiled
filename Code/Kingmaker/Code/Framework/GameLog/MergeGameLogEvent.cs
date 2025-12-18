using System.Collections.Generic;

namespace Kingmaker.Code.Framework.GameLog;

public class MergeGameLogEvent<T> : GameLogEvent<MergeGameLogEvent<T>> where T : GameLogEvent
{
	private readonly List<T> m_Events;

	public MergeGameLogEvent()
	{
		m_Events = new List<T>();
	}

	public void AddEvent(T @event)
	{
		m_Events.Add(@event);
	}

	public IReadOnlyList<T> GetEvents()
	{
		return m_Events;
	}
}
