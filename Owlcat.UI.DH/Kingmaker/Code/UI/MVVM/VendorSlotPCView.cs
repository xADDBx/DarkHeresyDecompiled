using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorSlotPCView : VendorSlotView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotView.Bind(base.ViewModel);
		if (m_ItemSlotView is ItemSlotPCView itemSlotPCView)
		{
			ObservableSubscribeExtensions.Subscribe(m_VendorSlotButton.OnSingleLeftClickAsObservable(), delegate
			{
				DoActionIfAvailable(base.OnClick);
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(m_VendorSlotButton.OnLeftDoubleClickAsObservable(), delegate
			{
				DoActionIfAvailable(base.OnDoubleClick);
			}).AddTo(this);
			itemSlotPCView.OnEndDragCommand.Subscribe(DelUpd).AddTo(this);
			RefreshItem();
		}
	}

	private void DoActionIfAvailable(Action action)
	{
		if (!base.ViewModel.IsItemUnavailable)
		{
			action?.Invoke();
		}
	}

	private void DelUpd()
	{
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			RefreshItem();
		}).AddTo(this);
	}

	protected override void SetupContextMenu()
	{
		UIContextMenu contextMenu = UIStrings.Instance.ContextMenu;
		List<ContextMenuCollectionEntity> contextMenu2 = new List<ContextMenuCollectionEntity>
		{
			new ContextMenuCollectionEntity(contextMenu.Buy, base.ViewModel.VendorTryBuyAll, base.ViewModel.HasItem && !base.ViewModel.IsLockedByRep),
			new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem)
		};
		base.ViewModel.SetContextMenu(contextMenu2);
	}
}
