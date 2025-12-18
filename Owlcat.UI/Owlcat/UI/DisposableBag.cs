using System;

namespace Owlcat.UI;

internal struct DisposableBag : IDisposable
{
	private Type m_Type;

	private IDisposable[] m_Disposables;

	private int m_Count;

	private bool m_IsDisposed;

	private bool m_IsClearing;

	public DisposableBag(Type type, int capacity)
	{
		m_Type = type;
		m_Disposables = ((capacity == 0) ? null : new IDisposable[capacity]);
		m_Count = 0;
		m_IsDisposed = false;
		m_IsClearing = false;
	}

	public void Add(IDisposable disposable)
	{
		if (m_IsDisposed)
		{
			disposable.Dispose();
			return;
		}
		if (m_Disposables == null)
		{
			m_Disposables = new IDisposable[4];
		}
		else if (m_Count == m_Disposables.Length)
		{
			Array.Resize(ref m_Disposables, m_Count * 2);
		}
		m_Disposables[m_Count++] = disposable;
	}

	public readonly bool Remove(IDisposable disposable)
	{
		if (m_Disposables != null)
		{
			for (int i = 0; i < m_Count; i++)
			{
				if (m_Disposables[i] == disposable)
				{
					m_Disposables[i] = null;
					return true;
				}
			}
		}
		return false;
	}

	public void Clear()
	{
		if (m_Disposables != null)
		{
			m_IsClearing = true;
			for (int num = m_Count - 1; num >= 0; num--)
			{
				m_Disposables[num]?.Dispose();
			}
			m_IsClearing = false;
			m_Disposables = null;
			m_Count = 0;
		}
	}

	public void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			Clear();
		}
	}
}
