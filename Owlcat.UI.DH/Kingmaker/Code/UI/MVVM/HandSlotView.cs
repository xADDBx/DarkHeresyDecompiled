using System;
using System.Collections.Generic;
using Kingmaker.Items;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class HandSlotView : View<EquipSlotVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplates
{
	[SerializeField]
	private OwlcatMultiButton m_Slot;

	[SerializeField]
	private Image m_ItemImage;

	[SerializeField]
	private GameObject m_EmptyImage;

	[SerializeField]
	private GameObject m_VacantBackground;

	private Action m_ClickAction;

	private bool m_CanConfirmConsoleClick;

	public OwlcatMultiButton Slot => m_Slot;

	public void SetClickAction(Action clickAction)
	{
		m_ClickAction = clickAction;
	}

	public void SetCanConfirmClick(bool canConfirm)
	{
		m_CanConfirmConsoleClick = canConfirm;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return base.ViewModel.Tooltip.CurrentValue;
	}

	public void SetFocus(bool value)
	{
		m_Slot.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Slot.IsValid();
	}

	public bool CanConfirmClick()
	{
		if (m_Slot.IsValid())
		{
			return m_CanConfirmConsoleClick;
		}
		return false;
	}

	public void OnConfirmClick()
	{
		m_ClickAction?.Invoke();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	protected override void OnBind()
	{
		base.ViewModel.Item.Subscribe(delegate(ItemEntity item)
		{
			bool hasItem = item != null;
			SetHasItem(hasItem);
		}).AddTo(this);
		base.ViewModel.Icon.Subscribe(delegate(Sprite sprite)
		{
			m_ItemImage.sprite = sprite;
			m_ItemImage.gameObject.SetActive(sprite != null);
			SetHasItem(base.ViewModel.HasItem);
		}).AddTo(this);
		base.ViewModel.CanBeFakeItem.Subscribe(delegate(bool isFake)
		{
			m_Slot.SetInteractable(!isFake);
			m_ItemImage.GetComponent<_2dxFX_GrayScale>().EffectAmount = (isFake ? 1f : 0f);
		}).AddTo(this);
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Slot.OnLeftClickAsObservable(), delegate
		{
			m_ClickAction?.Invoke();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		SetHasItem(hasItem: false);
		m_ItemImage.gameObject.SetActive(value: false);
		m_ItemImage.sprite = null;
	}

	private void SetHasItem(bool hasItem)
	{
		m_VacantBackground.SetActive(hasItem);
		m_EmptyImage.SetActive(!hasItem);
	}
}
