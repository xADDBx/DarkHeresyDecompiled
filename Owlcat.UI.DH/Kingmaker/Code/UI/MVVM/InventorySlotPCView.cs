using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using R3;
using R3.Triggers;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InventorySlotPCView : InventorySlotView
{
	[Header("PC")]
	[SerializeField]
	protected ItemSlotPCView m_ItemSlotPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotPCView.Bind(base.ViewModel);
		m_ItemSlotPCView.SetMainButtonHoverSound(ButtonSoundsEnum.NoSound);
		AddDisposable(m_ItemSlotPCView.OnSingleLeftClickAsObservable.Subscribe(OnClick));
		AddDisposable(m_ItemSlotPCView.OnDoubleClickAsObservable.Subscribe(OnDoubleClick));
		AddDisposable(m_ItemSlotPCView.OnBeginDragCommand.Subscribe(base.OnBeginDrag));
		AddDisposable(m_ItemSlotPCView.OnEndDragCommand.Subscribe(base.OnEndDrag));
		AddDisposable(m_ItemSlotPCView.OnPointerEnterAsObservable().Subscribe(delegate
		{
			OnHoverStart();
		}));
		AddDisposable(m_ItemSlotPCView.OnPointerExitAsObservable().Subscribe(delegate
		{
			OnHoverEnd();
		}));
	}

	protected override void SetupContextMenu()
	{
		UIContextMenu contextMenu = UIStrings.Instance.ContextMenu;
		List<ContextMenuCollectionEntity> contextMenu2 = new List<ContextMenuCollectionEntity>
		{
			new ContextMenuCollectionEntity(contextMenu.Interact, base.ViewModel.Interact, base.ViewModel.HasInteractions.CurrentValue),
			new ContextMenuCollectionEntity(contextMenu.Equip, base.EquipItem, base.ViewModel.IsEquipPossible),
			new ContextMenuCollectionEntity(UIStrings.Instance.LootWindow.SendToInventory, delegate
			{
				MoveToInventory(immediately: true);
			}, !base.ViewModel.IsInStash && base.ViewModel.HasItem),
			new ContextMenuCollectionEntity(contextMenu.Split, Split, base.ViewModel.IsSplitPossible),
			new ContextMenuCollectionEntity(contextMenu.Drop, base.DropItem, base.ViewModel.CanDropItem),
			new ContextMenuCollectionEntity(contextMenu.Trash, delegate
			{
				base.ViewModel.MarkAsTrashLoot(isMarkedAsTrash: true);
			}, !base.ViewModel.IsNotable.CurrentValue && !base.ViewModel.MarkedAsTrash.CurrentValue && !base.ViewModel.IsTrash.CurrentValue && base.ViewModel.HasItem),
			new ContextMenuCollectionEntity(contextMenu.NotTrash, delegate
			{
				base.ViewModel.MarkAsTrashLoot(isMarkedAsTrash: false);
			}, !base.ViewModel.IsNotable.CurrentValue && base.ViewModel.MarkedAsTrash.CurrentValue && !base.ViewModel.IsTrash.CurrentValue && base.ViewModel.HasItem),
			new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem)
		};
		base.ViewModel.SetContextMenu(contextMenu2);
	}
}
