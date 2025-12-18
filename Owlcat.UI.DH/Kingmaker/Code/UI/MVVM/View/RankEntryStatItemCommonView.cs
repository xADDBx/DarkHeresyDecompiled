using System.Collections.Generic;
using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class RankEntryStatItemCommonView : VirtualListElementViewBase<RankEntrySelectionStatVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[SerializeField]
	private TextMeshProUGUI m_StatDisplayName;

	[SerializeField]
	private TextMeshProUGUI m_StatIncreaseText;

	[SerializeField]
	private Image m_RecommendMark;

	[SerializeField]
	private GameObject m_FocusedMark;

	[SerializeField]
	private GameObject m_ShortNameBlock;

	[SerializeField]
	private TextMeshProUGUI m_ShortName;

	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_StatDisplayName, m_StatIncreaseText);
		}
		m_StatDisplayName.text = base.ViewModel.StatDisplayName;
		m_ShortName.text = base.ViewModel.ShortName;
		m_ShortNameBlock.SetActive(!string.IsNullOrEmpty(base.ViewModel.ShortName));
		if (m_RecommendMark != null)
		{
			m_RecommendMark.gameObject.SetActive(base.ViewModel.IsRecommended);
			AddDisposable(m_RecommendMark.SetHint(UIStrings.Instance.CharacterSheet.RecommendedByCareerPath));
		}
		AddDisposable(m_MainButton.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig
		{
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		}));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_MainButton.OnLeftClickAsObservable(), delegate
		{
			DoClick();
		}));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_MainButton.OnConfirmClickAsObservable(), delegate
		{
			DoClick();
		}));
		AddDisposable(base.ViewModel.StatIncreaseLabel.Subscribe(delegate(string value)
		{
			m_StatIncreaseText.text = value;
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
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.ViewModel.SetFocusOn(null);
		m_TextHelper.Dispose();
	}

	private void DoClick()
	{
		RankFeatureState currentValue = base.ViewModel.FeatureState.CurrentValue;
		if (currentValue != RankFeatureState.NotActive && currentValue != RankFeatureState.NotValid && currentValue != RankFeatureState.Committed)
		{
			base.ViewModel.Select();
			EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
			{
				h.SetFocusOn(base.ViewModel);
			});
		}
		else
		{
			RankEntrySelectionStatVM focusOn = (base.ViewModel.FocusedState.CurrentValue ? null : base.ViewModel);
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
			return;
		}
		base.ViewModel.CareerPathVM.SelectNextItem();
		UISounds.Instance.Sounds.Buttons.DoctrineNextButtonClick.Play();
	}

	public string GetConfirmClickHint()
	{
		return (base.ViewModel.FeatureState.CurrentValue == RankFeatureState.Selected) ? UIStrings.Instance.CharGen.Next : UIStrings.Instance.CommonTexts.Select;
	}
}
