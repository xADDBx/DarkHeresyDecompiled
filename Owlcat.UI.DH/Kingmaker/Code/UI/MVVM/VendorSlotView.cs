using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class VendorSlotView : VendorGenericSlotView<ItemSlotBaseView>, IHasTooltipTemplates
{
	[SerializeField]
	private TextMeshProUGUI m_DisplayNameText;

	[SerializeField]
	private TextMeshProUGUI m_ItemTypeText;

	[SerializeField]
	private GameObject m_DiscountBlock;

	[SerializeField]
	private TextMeshProUGUI m_OldCostText;

	[SerializeField]
	private TextMeshProUGUI m_CurrentCostText;

	[SerializeField]
	private Image m_Frame;

	[SerializeField]
	private TextMeshProUGUI m_ReputationValue;

	[SerializeField]
	private OwlcatMultiButton m_LockButton;

	[SerializeField]
	private CanvasGroup m_LockButtonImage;

	[SerializeField]
	private Color m_LockHintTextColor = Color.red;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DiscountBlock.gameObject.SetActive(base.ViewModel.HasDiscount);
		if (base.ViewModel.HasDiscount)
		{
			m_OldCostText.text = ((base.ViewModel.ItemPriceNoDiscount.CurrentValue > 0.0) ? UIUtilityText.GetCostFormatted((float)base.ViewModel.ItemPriceNoDiscount.CurrentValue) : string.Empty);
			if (base.ViewModel.ItemPriceNoDiscount.CurrentValue <= 0.0)
			{
				m_DiscountBlock.SetActive(value: false);
			}
		}
		base.ViewModel.ItemPrice.Subscribe(delegate(double value)
		{
			string text = ((value > 0.0) ? UIUtilityText.GetCostFormatted((float)value) : string.Empty);
			m_CurrentCostText.text = text;
		}).AddTo(this);
		base.ViewModel.DisplayName.Subscribe(delegate(string value)
		{
			m_DisplayNameText.text = value;
		}).AddTo(this);
		base.ViewModel.TypeName.Subscribe(delegate(string value)
		{
			m_ItemTypeText.text = value;
		}).AddTo(this);
		VendorHelper.TradeLogic.HideUnavailable.Subscribe(delegate
		{
			RefreshItem();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_LockButton.OnLeftClickAsObservable(), delegate
		{
			LockButtonClicked();
		}).AddTo(this);
		if (!string.IsNullOrEmpty(base.ViewModel.ReputationLockedHintText))
		{
			m_LockButton.SetHint(base.ViewModel.ReputationLockedHintText, null, m_LockHintTextColor);
		}
	}

	public override void RefreshItem()
	{
		base.RefreshItem();
		m_VendorSlotButton.SetActiveLayer(base.ViewModel.GetVendorItemButtonLayerNumber);
		m_ReputationValue.text = base.ViewModel.ReputationValueToUnlock.ToString();
		m_Frame.raycastTarget = base.ViewModel.IsLockedByRep;
		if (base.ViewModel.IsItemSoldToVendor)
		{
			GameObject obj = base.gameObject;
			int active;
			if (VendorHelper.TradeLogic.ShowSoldItemsToVendorFilter.Value && base.ViewModel.HasItem)
			{
				ItemSlotVM viewModel = base.ViewModel;
				active = ((viewModel != null && viewModel.Item.CurrentValue.Count > 0) ? 1 : 0);
			}
			else
			{
				active = 0;
			}
			obj.SetActive((byte)active != 0);
		}
		else if (VendorHelper.TradeLogic.HideUnavailable.Value)
		{
			base.gameObject.SetActive(!base.ViewModel.IsItemUnavailable);
		}
		else
		{
			GameObject obj2 = base.gameObject;
			int active2;
			if (base.ViewModel.HasItem)
			{
				ItemSlotVM viewModel2 = base.ViewModel;
				active2 = ((viewModel2 != null && viewModel2.Item.CurrentValue.Count > 0) ? 1 : 0);
			}
			else
			{
				active2 = 0;
			}
			obj2.SetActive((byte)active2 != 0);
		}
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return base.ViewModel.Tooltip.CurrentValue;
	}

	private void LockButtonClicked()
	{
		if ((bool)m_LockButtonImage)
		{
			UISounds.Instance.Sounds.Systems.BlinkAttentionMark.Play();
			m_LockButtonImage.alpha = 1f;
			m_LockButtonImage.DOFade(0f, 0.65f).SetLoops(2).SetEase(Ease.OutSine)
				.OnComplete(delegate
				{
					m_LockButtonImage.alpha = 1f;
				})
				.SetUpdate(isIndependentUpdate: true);
		}
	}
}
