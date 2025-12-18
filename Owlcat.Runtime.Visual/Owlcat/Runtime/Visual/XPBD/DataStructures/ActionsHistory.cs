using System;

namespace Owlcat.Runtime.Visual.XPBD.DataStructures;

public struct ActionsHistory
{
	public enum ActionType : byte
	{
		None,
		Register,
		Unregister
	}

	private const uint kCountMask = 15u;

	private uint m_History;

	public ActionType this[int index]
	{
		get
		{
			int count = Count;
			if (index < 0 || index >= count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			int num = (index + 1) * 2 + 4;
			return (ActionType)((m_History >> num) & 3u);
		}
	}

	public int Count => (int)(m_History & 0xF);

	public ActionType Last
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				throw new InvalidOperationException("History is empty.");
			}
			return this[count - 1];
		}
	}

	public void Push(in ActionType action)
	{
		int num = Count + 1;
		if (num > 13)
		{
			throw new ArgumentOutOfRangeException();
		}
		int num2 = num * 2 + 4;
		uint num3 = (uint)action << num2;
		m_History |= num3;
		m_History &= 4294967280u;
		m_History |= (uint)num;
	}

	public void Clear()
	{
		m_History = 0u;
	}
}
