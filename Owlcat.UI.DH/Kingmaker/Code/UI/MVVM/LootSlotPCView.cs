using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LootSlotPCView : LootSlotView
{
	[SerializeField]
	private ItemSlotPCView m_ItemSlotPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotPCView.Bind(base.ViewModel);
		AddDisposable(m_ItemSlotPCView.OnSingleLeftClickAsObservable.Subscribe(base.OnClick));
		AddDisposable(m_ItemSlotPCView.OnDoubleClickAsObservable.Subscribe(base.OnDoubleClick));
		AddDisposable(m_ItemSlotPCView.OnBeginDragCommand.Subscribe(OnBeginDrag));
		AddDisposable(m_ItemSlotPCView.OnEndDragCommand.Subscribe(OnEndDrag));
	}

	protected override void SetupContextMenu()
	{
		UIContextMenu contextMenu = UIStrings.Instance.ContextMenu;
		List<ContextMenuCollectionEntity> contextMenu2 = new List<ContextMenuCollectionEntity>
		{
			new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem),
			new ContextMenuCollectionEntity(contextMenu.Trash, delegate
			{
				base.ViewModel.MarkAsTrashLoot(isMarkedAsTrash: true);
			}, !base.ViewModel.IsNotable.CurrentValue && !base.ViewModel.MarkedAsTrash.CurrentValue && !base.ViewModel.IsTrash.CurrentValue && base.ViewModel.HasItem),
			new ContextMenuCollectionEntity(contextMenu.NotTrash, delegate
			{
				base.ViewModel.MarkAsTrashLoot(isMarkedAsTrash: false);
			}, !base.ViewModel.IsNotable.CurrentValue && base.ViewModel.MarkedAsTrash.CurrentValue && !base.ViewModel.IsTrash.CurrentValue && base.ViewModel.HasItem)
		};
		base.ViewModel.SetContextMenu(contextMenu2);
	}

	private void OnBeginDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStart(base.Item);
		});
	}

	private void OnEndDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStop();
		});
	}
}
