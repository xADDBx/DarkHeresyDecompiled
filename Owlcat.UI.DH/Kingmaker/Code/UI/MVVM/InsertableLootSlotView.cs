using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Kingmaker.UI.Sound;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InsertableLootSlotView : ItemSlotView<InsertableLootSlotVM>
{
	[SerializeField]
	private GameObject m_CanNotInsert;

	public bool CanInsert => base.ViewModel.CanInsert.CurrentValue;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.CanInsert.Subscribe(delegate(bool value)
		{
			if (m_CanNotInsert != null)
			{
				m_CanNotInsert.gameObject.SetActive(!value);
			}
		}));
	}

	protected override void ClearView()
	{
		base.ClearView();
		if (m_CanNotInsert != null)
		{
			m_CanNotInsert.gameObject.SetActive(value: false);
		}
	}

	protected void OnClick()
	{
		if (base.ViewModel.CanInsert.CurrentValue)
		{
			UISounds.Instance.PlayItemSound(SlotAction.Put, base.ViewModel.ItemEntity, equipSound: false);
			EventBus.RaiseEvent(delegate(INewSlotsHandler h)
			{
				h.HandleTryInsertSlot(base.ViewModel);
			});
		}
	}
}
