using System;
using System.Collections.Generic;

namespace Kingmaker.Code.UI.MVVM;

public sealed class MainMenuWindowsQueue
{
	private readonly Queue<Action<Action>> m_Steps = new Queue<Action<Action>>();

	public MainMenuWindowsQueue Then(Action<Action> step)
	{
		m_Steps.Enqueue(step);
		return this;
	}

	public void Run()
	{
		RunNext();
		void RunNext()
		{
			if (m_Steps.Count != 0)
			{
				m_Steps.Dequeue()(RunNext);
			}
		}
	}
}
