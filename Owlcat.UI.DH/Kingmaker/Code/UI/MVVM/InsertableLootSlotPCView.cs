using Kingmaker.PubSubSystem.Core;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InsertableLootSlotPCView : InsertableLootSlotView
{
	[Header("PC")]
	[SerializeField]
	private ItemSlotPCView m_ItemSlotPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotPCView.Bind(base.ViewModel);
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_ItemSlotPCView.OnSingleLeftClickAsObservable, delegate
		{
			if (m_ItemSlotPCView.ViewModel.HasItem)
			{
				OnClick();
			}
		}));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_ItemSlotPCView.OnDoubleClickAsObservable, delegate
		{
			if (m_ItemSlotPCView.ViewModel.HasItem)
			{
				OnClick();
			}
		}));
		AddDisposable(m_ItemSlotPCView.OnBeginDragCommand.Subscribe(OnBeginDrag));
		AddDisposable(m_ItemSlotPCView.OnEndDragCommand.Subscribe(OnEndDrag));
	}

	protected void OnBeginDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStart(base.Item);
		});
	}

	protected void OnEndDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStop();
		});
		if (m_ItemSlotPCView.ViewModel.HasItem)
		{
			OnClick();
		}
	}
}
