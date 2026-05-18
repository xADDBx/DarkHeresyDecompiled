using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace Owlcat.UI;

[SelectionBase]
public abstract class View<T> : MonoBehaviour, IBindable<T>, IBindable
{
	private DisposableBag m_Disposables;

	private DisposableAction m_Unbind;

	public T ViewModel { get; private set; }

	[DebuggerStepThrough]
	public void Bind(T source)
	{
		Unbind();
		if (source == null)
		{
			return;
		}
		if (source is ViewModel viewModel)
		{
			if (viewModel.IsDisposed)
			{
				UnityEngine.Debug.LogError($"Trying to Bind() to a disposed view model, type={typeof(T)}");
				return;
			}
			viewModel.Add(m_Unbind = new DisposableAction(Unbind));
		}
		ViewModel = source;
		m_Disposables = new DisposableBag(GetType(), 0);
		OnBind();
	}

	[DebuggerStepThrough]
	public void Unbind()
	{
		if (ViewModel != null)
		{
			if (ViewModel is ViewModel viewModel)
			{
				viewModel.Remove(Interlocked.Exchange(ref m_Unbind, null));
			}
			try
			{
				m_Disposables.Clear();
				OnUnbind();
			}
			finally
			{
				ViewModel = default(T);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		try
		{
			Unbind();
		}
		catch (Exception exception)
		{
			UIKitLogger.Exception("Exception in " + GetType().Name + ".OnDestroy", exception);
		}
	}

	protected virtual void OnBind()
	{
	}

	protected virtual void OnUnbind()
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
