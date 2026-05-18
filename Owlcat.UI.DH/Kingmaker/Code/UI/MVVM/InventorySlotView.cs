using DG.Tweening;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InventorySlotView : ItemSlotView<ItemSlotVM>
{
	[SerializeField]
	protected GameObject m_PossibleTargetHighlight;

	[SerializeField]
	protected OwlcatMultiSelectable m_InteractionStateSelectable;

	[Header("UsableStacks")]
	[SerializeField]
	private GameObject m_UsableStacksContainer;

	[SerializeField]
	private TextMeshProUGUI m_UsableStacksCount;

	private Tween m_BlinkTween;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_PossibleTargetHighlight != null)
		{
			AddDisposable(base.ViewModel.PossibleTarget.Subscribe(m_PossibleTargetHighlight.SetActive));
		}
		AddDisposable(base.ViewModel.NeedBlink.Subscribe(Blink));
		AddDisposable(base.ViewModel.UsableCount.Subscribe(delegate(int value)
		{
			m_UsableStacksContainer.Or(null)?.SetActive(value > 0);
			if ((bool)m_UsableStacksCount)
			{
				m_UsableStacksCount.text = value.ToString();
			}
		}));
		base.ViewModel.HasInteractions.Subscribe(delegate(bool value)
		{
			m_InteractionStateSelectable.Or(null)?.SetActiveLayer(value ? 1 : 0);
		}).AddTo(this);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_BlinkTween?.Kill();
		m_BlinkTween = null;
	}

	protected virtual void OnClick()
	{
		if (RootUIContext.Instance.CurrentServiceWindow != ServiceWindowsType.Inventory && !base.ViewModel.IsInStash)
		{
			MoveToInventory(immediately: false);
		}
	}

	protected void Blink()
	{
		if ((bool)m_BlinkMark)
		{
			SystemSounds.Instance.Systems.BlinkAttentionMark.Play();
			m_BlinkTween?.Kill();
			m_BlinkMark.alpha = 1f;
			m_BlinkTween = m_BlinkMark.DOFade(0f, 0.65f).SetLoops(2).SetEase(Ease.OutSine)
				.SetUpdate(isIndependentUpdate: true);
		}
	}

	protected virtual void OnDoubleClick()
	{
		if (base.ViewModel.IsReadableItem)
		{
			base.ViewModel.ShowInfo();
		}
		else
		{
			EquipItem();
		}
	}

	protected void OnHoverStart()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotHoverHandler h)
		{
			h.HandleHoverStart(base.Item);
		});
	}

	protected void OnHoverEnd()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotHoverHandler h)
		{
			h.HandleHoverStop();
		});
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
	}

	protected void EquipItem()
	{
		EventBus.RaiseEvent(delegate(IInventoryHandler h)
		{
			h.TryEquip(base.ViewModel);
		});
	}

	protected void MoveToInventory(bool immediately)
	{
		if (base.ViewModel.ItemEntity != null)
		{
			UISounds.Instance.PlayItemSound(SlotAction.Put, base.ViewModel.ItemEntity, equipSound: false);
		}
		EventBus.RaiseEvent(delegate(IInventoryHandler h)
		{
			h.TryMoveToInventory(base.ViewModel, immediately);
		});
	}

	protected void DropItem()
	{
		EventBus.RaiseEvent(delegate(IInventoryHandler h)
		{
			h.TryDrop(base.ViewModel);
		});
	}

	protected virtual void Split()
	{
		EventBus.RaiseEvent(delegate(INewSlotsHandler h)
		{
			h.HandleTrySplitSlot(base.ViewModel);
		});
	}
}
