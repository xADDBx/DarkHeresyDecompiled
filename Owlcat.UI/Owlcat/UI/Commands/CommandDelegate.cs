using System;
using UnityEngine;

namespace Owlcat.UI.Commands;

internal readonly struct CommandDelegate
{
	private readonly Action m_Action0;

	private readonly Action<float> m_Action1;

	private readonly Action<Vector2> m_Action2;

	public CommandDelegate(Action a0, Action<float> a1, Action<Vector2> a2)
	{
		m_Action0 = a0;
		m_Action1 = a1;
		m_Action2 = a2;
	}

	public void Invoke(InputEvent e)
	{
		m_Action0?.Invoke();
		m_Action1?.Invoke(e.GetAxis());
		m_Action2?.Invoke(e.GetAxis2D());
	}
}
