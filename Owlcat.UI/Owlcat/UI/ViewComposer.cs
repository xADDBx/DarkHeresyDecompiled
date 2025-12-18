using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Owlcat.UI;

public class ViewComposer : IViewComposer
{
	private class Item
	{
		public readonly object Data;

		private readonly Action m_Show;

		private readonly Action m_Hide;

		private bool m_IsShown;

		private bool m_IsShownTarget;

		public Item(object data, Action show, Action hide)
		{
			Data = data;
			m_Show = show;
			m_Hide = hide;
		}

		public void Show()
		{
			m_IsShownTarget = true;
		}

		public void Hide()
		{
			m_IsShownTarget = false;
		}

		public void Sync()
		{
			if (m_IsShown != m_IsShownTarget)
			{
				m_IsShown = m_IsShownTarget;
				if (m_IsShown)
				{
					m_Show();
				}
				else
				{
					m_Hide();
				}
			}
		}
	}

	private readonly List<IViewComposerRule> m_Rules;

	private readonly List<Item> m_Stack = new List<Item>();

	private readonly List<Item> m_Deque = new List<Item>();

	public ViewComposer(params IViewComposerRule[] rules)
	{
		m_Rules = new List<IViewComposerRule>(rules);
	}

	public void AddRule(IViewComposerRule rule)
	{
		m_Rules.Add(rule);
		ValidateAndSync();
	}

	public void RemoveRule(IViewComposerRule rule)
	{
		m_Rules.Remove(rule);
		ValidateAndSync();
	}

	public void Add(object data, Action show, Action hide)
	{
		m_Deque.Add(new Item(data, show, hide));
		ValidateAndSync();
	}

	public void Remove(object data)
	{
		int num = m_Deque.FindIndex((Item x) => x.Data.Equals(data));
		if (num != -1)
		{
			m_Deque.RemoveAt(num);
			return;
		}
		Item item = m_Stack.Find((Item x) => x.Data.Equals(data));
		if (item != null)
		{
			m_Stack.Remove(item);
			item.Hide();
			item.Sync();
			ValidateAndSync();
		}
	}

	private void ValidateAndSync()
	{
		int num = 0;
		while (Validate() && num < 100)
		{
			num++;
		}
		for (int i = 0; i < m_Deque.Count; i++)
		{
			m_Deque[i].Sync();
		}
		for (int j = 0; j < m_Stack.Count; j++)
		{
			m_Stack[j].Sync();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool Validate()
	{
		for (int i = 0; i < m_Stack.Count; i++)
		{
			Item item = m_Stack[i];
			if (IsForbridden(item))
			{
				m_Deque.Insert(0, item);
				m_Stack.RemoveAt(i);
				item.Hide();
				return true;
			}
		}
		for (int j = 0; j < m_Deque.Count; j++)
		{
			Item item2 = m_Deque[j];
			if (!IsForbridden(item2))
			{
				m_Stack.Add(item2);
				m_Deque.RemoveAt(j);
				item2.Show();
				return true;
			}
		}
		return false;
	}

	private bool IsForbridden(Item candidate)
	{
		bool flag = false;
		for (int i = 0; i < m_Stack.Count; i++)
		{
			if (flag)
			{
				break;
			}
			flag |= IsForbridden(candidate, m_Stack[i]);
		}
		return flag;
	}

	private bool IsForbridden(Item candidate, Item alreadyOnScreen)
	{
		bool flag = false;
		for (int i = 0; i < m_Rules.Count; i++)
		{
			if (flag)
			{
				break;
			}
			flag |= m_Rules[i].IsForbidden(candidate.Data, alreadyOnScreen.Data);
		}
		if (alreadyOnScreen.Data is IViewComposerRule viewComposerRule)
		{
			flag |= viewComposerRule.IsForbidden(candidate.Data, alreadyOnScreen.Data);
		}
		if (candidate.Data is IViewComposerRule viewComposerRule2)
		{
			flag |= viewComposerRule2.IsForbidden(candidate.Data, alreadyOnScreen.Data);
		}
		return flag;
	}
}
