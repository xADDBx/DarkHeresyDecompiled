using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using R3;
using UnityEngine.Pool;

namespace Owlcat.UI;

public sealed class ViewService : IDisposable
{
	private abstract class Item
	{
		public abstract Task Dismiss();

		public abstract Task Restore();

		public abstract void Dispose(bool disposeSourceAfterTransition);
	}

	private class Item<T> : Item
	{
		private readonly ViewService m_Service;

		private readonly T m_Source;

		private IBindable m_View;

		private CancellationTokenSource m_ViewCancel;

		private Transition m_Transition;

		private bool m_IsDisposed;

		private bool m_IsShown;

		private bool m_IsHidden;

		public Item(ViewService service, T source)
		{
			m_Source = source;
			m_Transition = Transition.None;
			m_Service = service;
			m_Service.m_Composer.Add(source, OnShow, OnHide);
		}

		private void OnShow()
		{
			m_IsShown = true;
			Refresh();
		}

		private void OnHide()
		{
			m_IsShown = false;
			Refresh();
		}

		public override Task Dismiss()
		{
			ViewFactoryPolicy viewFactoryPolicy = ((m_View == null) ? ViewFactoryPolicy.Default : ViewFactoryPolicy.GetCustomAttribute(m_View.GetType()));
			m_IsHidden = !viewFactoryPolicy.Flags.HasFlag(ViewFactoryPolicyFlag.DontDismiss);
			return Refresh();
		}

		public override Task Restore()
		{
			m_IsHidden = false;
			return Refresh();
		}

		private Task Refresh()
		{
			return OnVisibilityChanged(m_IsShown && !m_IsHidden);
		}

		private async Task OnVisibilityChanged(bool value)
		{
			_ = 2;
			try
			{
				await m_Transition;
				if (value && m_View == null && m_ViewCancel == null)
				{
					m_ViewCancel = new CancellationTokenSource();
					m_View = await m_Service.m_Factory.Retain(m_Source, m_ViewCancel.Token);
					m_ViewCancel?.Dispose();
					m_ViewCancel = null;
					m_Transition = m_Service.m_Transitor.Show(m_View);
				}
				else if (!value && m_ViewCancel != null)
				{
					m_ViewCancel.Cancel();
					m_ViewCancel.Dispose();
					m_ViewCancel = null;
				}
				else if (!value && m_View != null)
				{
					IBindable view = m_View;
					m_View = null;
					m_Transition = m_Service.m_Transitor.Hide(view);
					await m_Transition;
					m_Service.m_Factory.Release(view);
					if (m_IsDisposed && m_Source is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception message)
			{
				UIKitLogger.Exception(message);
			}
			finally
			{
				m_Transition = Transition.None;
				m_ViewCancel = null;
			}
		}

		public override void Dispose(bool disposeSource)
		{
			if (disposeSource)
			{
				m_IsDisposed = true;
			}
			if (m_View == null && m_ViewCancel == null && m_Source is IDisposable disposable)
			{
				disposable.Dispose();
			}
			m_ViewCancel?.Cancel();
			m_Service.m_Composer.Remove(m_Source);
		}
	}

	private readonly IViewComposer m_Composer;

	private readonly IViewTransitor m_Transitor;

	private IViewFactory m_Factory;

	private readonly Dictionary<object, Item> m_Items = new Dictionary<object, Item>();

	public ViewService(IViewComposer composer, IViewFactory factory, IViewTransitor transitor)
	{
		m_Composer = composer;
		m_Factory = factory;
		m_Transitor = transitor;
	}

	public async Task Reload(IViewFactory factory)
	{
		await Task.WhenAll(m_Items.Values.Select((Item x) => x.Dismiss()));
		m_Factory = factory;
		await Task.WhenAll(m_Items.Values.Select((Item x) => x.Restore()));
	}

	public void Add<T>(T source)
	{
		m_Items.Add(source, new Item<T>(this, source));
		if (source is ViewModel viewModel)
		{
			Disposable.Create(viewModel, OnViewModelDisposed).AddTo(viewModel);
		}
	}

	private void OnViewModelDisposed<T>(T source)
	{
		Remove(source, dispose: false);
	}

	public void Remove<T>(T source, bool dispose)
	{
		if (m_Items.TryGetValue(source, out var value) && m_Items.Remove(source))
		{
			value.Dispose(dispose);
		}
	}

	void IDisposable.Dispose()
	{
		List<Item> value;
		using (CollectionPool<List<Item>, Item>.Get(out value))
		{
			value.AddRange(m_Items.Values);
			m_Items.Clear();
			foreach (Item item in value)
			{
				item.Dispose(disposeSourceAfterTransition: false);
			}
		}
	}
}
