using System;

namespace Owlcat.UI;

public abstract class ViewModel : IDisposable
{
	private DisposableBag m_Disposables;

	private bool m_Disposed;

	public ViewModel()
	{
		m_Disposables = new DisposableBag(GetType(), 0);
		m_Disposables.Add(new DisposableAction(OnDispose));
	}

	~ViewModel()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (!m_Disposed)
		{
			m_Disposed = true;
			m_Disposables.Dispose();
			GC.SuppressFinalize(this);
		}
	}

	protected virtual void OnDispose()
	{
	}

	internal void Add(IDisposable disposable)
	{
		m_Disposables.Add(disposable);
	}

	internal bool Remove(IDisposable disposable)
	{
		return m_Disposables.Remove(disposable);
	}
}
