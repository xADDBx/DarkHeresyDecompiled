using System;
using System.Threading;

namespace Owlcat.UI;

internal sealed class DisposableAction : IDisposable
{
	private volatile Action m_Dispose;

	public DisposableAction(Action dispose)
	{
		m_Dispose = dispose;
	}

	public void Dispose()
	{
		Interlocked.Exchange(ref m_Dispose, null)?.Invoke();
	}
}
