using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.Code.View.UI.UIUtils;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryContext : ViewModel, ICounterWindowUIHandler, ISubscriber, IContextMenuHandler
{
	private readonly ReactiveProperty<CounterWindowVM> m_CounterWindowVM;

	private readonly ReactiveProperty<ContextMenuVM> m_ContextMenuVM;

	public InventoryContext(ReactiveProperty<CounterWindowVM> counterWindowVM, ReactiveProperty<ContextMenuVM> contextMenuVM)
	{
		m_CounterWindowVM = counterWindowVM;
		m_ContextMenuVM = contextMenuVM;
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		m_CounterWindowVM.ClearDisposableValue();
		m_ContextMenuVM.ClearDisposableValue();
	}

	void ICounterWindowUIHandler.HandleOpen(CounterWindowType type, ItemEntity item, Action<int> command)
	{
		m_CounterWindowVM.ClearDisposableValue();
		m_CounterWindowVM.Value = new CounterWindowVM(type, item, command, delegate
		{
			m_CounterWindowVM.ClearDisposableValue();
		});
	}

	void ICounterWindowUIHandler.HandleCloseCounterWindow()
	{
		m_CounterWindowVM.ClearDisposableValue();
	}

	void IContextMenuHandler.HandleContextMenuRequest(IContextMenuCollection collection)
	{
		m_ContextMenuVM.ClearDisposableValue();
		if (collection != null)
		{
			m_ContextMenuVM.Value = new ContextMenuVM(collection);
		}
	}
}
