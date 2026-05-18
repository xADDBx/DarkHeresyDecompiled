using System.Collections.Generic;

namespace Kingmaker.Visual.Sound;

public class UnitAsksQueue
{
	private List<AskSchedulingEntry> m_Queue = new List<AskSchedulingEntry>();

	public bool IsEmpty => m_Queue.Count == 0;

	public void Schedule(AskSchedulingEntry entry)
	{
		int entryPriority = entry.Priority;
		int num = m_Queue.FindLastIndex((AskSchedulingEntry e) => e.Priority >= entryPriority);
		if (num == -1)
		{
			m_Queue.Add(entry);
		}
		else
		{
			m_Queue.Insert(num + 1, entry);
		}
	}

	public void ClearQueue()
	{
		m_Queue.Clear();
	}

	public bool TryPopFromQueue(out AskSchedulingEntry nextEntry)
	{
		nextEntry = null;
		if (m_Queue.Count == 0)
		{
			return false;
		}
		nextEntry = m_Queue[0];
		m_Queue.RemoveAt(0);
		return true;
	}

	public bool IsInQueue(AskWrapper wrapper)
	{
		return m_Queue.Exists((AskSchedulingEntry a) => a.Wrapper == wrapper);
	}
}
