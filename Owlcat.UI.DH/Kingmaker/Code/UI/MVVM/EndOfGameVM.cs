using System;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class EndOfGameVM : ViewModel
{
	private readonly Action m_OnDispose;

	public EndOfGameVM(Action onDispose)
	{
		m_OnDispose = onDispose;
	}

	public void Close()
	{
		m_OnDispose?.Invoke();
	}
}
