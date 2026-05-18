using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class RankEntryUltimateFeatureUpgradeItemCommonView : VirtualListElementViewBase<BaseRankEntryFeatureVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplate
{
	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	[SerializeField]
	private GameObject m_FocusedMark;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
		m_Description.text = base.ViewModel.FactDescription;
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_MainButton.OnLeftClickAsObservable(), delegate
		{
			DoClick();
		}));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_MainButton.OnConfirmClickAsObservable(), delegate
		{
			DoClick();
		}));
		AddDisposable(m_MainButton.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig
		{
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		}));
		AddDisposable(base.ViewModel.FeatureState.Subscribe(delegate(RankFeatureState value)
		{
			m_MainButton.SetActiveLayer(value.ToString());
		}));
		AddDisposable(base.ViewModel.FocusedState.Subscribe(delegate(bool value)
		{
			if (base.ViewModel.FeatureState.CurrentValue != RankFeatureState.Committed)
			{
				m_FocusedMark.Or(null)?.SetActive(value);
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.ViewModel.SetFocusOn(null);
	}

	private void DoClick()
	{
		RankFeatureState currentValue = base.ViewModel.FeatureState.CurrentValue;
		if (currentValue != RankFeatureState.NotActive && currentValue != RankFeatureState.NotValid)
		{
			base.ViewModel.Select();
			EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
			{
				h.SetFocusOn(base.ViewModel);
			});
		}
		else
		{
			BaseRankEntryFeatureVM focusOn = (base.ViewModel.FocusedState.CurrentValue ? null : base.ViewModel);
			EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
			{
				h.SetFocusOn(focusOn);
			});
		}
	}

	public void SetFocus(bool value)
	{
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return true;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.CurrentValue;
	}

	public bool CanConfirmClick()
	{
		if (m_MainButton.CanConfirmClick())
		{
			RankFeatureState currentValue = base.ViewModel.FeatureState.CurrentValue;
			return currentValue == RankFeatureState.Selectable || currentValue == RankFeatureState.Selected;
		}
		return false;
	}

	public void OnConfirmClick()
	{
		if (base.ViewModel.FeatureState.CurrentValue != RankFeatureState.Selected)
		{
			m_MainButton.OnConfirmClick();
			EventBus.RaiseEvent(delegate(IRankEntryConfirmClickHandler h)
			{
				h.OnRankEntryConfirmClick();
			});
		}
		else
		{
			base.ViewModel.CareerPathVM.SelectNextItem();
			ButtonsSounds.Instance.DoctrineNextButton.Click.Play();
		}
	}

	public string GetConfirmClickHint()
	{
		return (base.ViewModel.FeatureState.CurrentValue == RankFeatureState.Selected) ? UIStrings.Instance.CharGen.Next : UIStrings.Instance.CommonTexts.Select;
	}
}
