using System.Collections.Generic;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenBackgroundBasePhaseDetailedView : CharGenPhaseDetailedView<CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>>
{
	[Header("Description")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private InfoSectionView m_SecondaryInfoView;

	[Header("Selector")]
	[SerializeField]
	private TMP_Text m_HeaderLabel;

	[SerializeField]
	private OwlcatMultiSelectable m_ListSelectable;

	[SerializeField]
	private CharGenBackgroundBasePhaseSelectorView m_CharGenSelectionsCommonPhaseSelectorView;

	private readonly ReactiveProperty<ActivePhaseNavigation> m_ActivePhaseNavigation = new ReactiveProperty<ActivePhaseNavigation>(ActivePhaseNavigation.Menu);

	private readonly ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanDecline = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanShowInfo = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanSwitchNavigation = new ReactiveProperty<bool>();

	private IConsoleEntity m_ContentEntity;

	private bool m_HasTooltip;

	private TooltipConfig m_TooltipConfig;

	private bool m_VerticalEntitiesAdded;

	protected override bool HasYScrollBindInternal => m_InfoView.IsScrollActive;

	protected override void OnBind()
	{
		base.OnBind();
		m_VerticalEntitiesAdded = false;
		m_TooltipConfig = new TooltipConfig
		{
			TooltipPlace = m_SecondaryInfoView?.GetComponent<RectTransform>(),
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		m_InfoView?.Bind(base.ViewModel.InfoVM);
		m_SecondaryInfoView?.Bind(base.ViewModel.SecondaryInfoVM);
		m_CharGenSelectionsCommonPhaseSelectorView.Bind(base.ViewModel.SelectionGroup);
		m_CanDecline.Subscribe(delegate(bool value)
		{
			m_CanGoBackOnDecline.Value = !value;
		}).AddTo(this);
		base.ViewModel.IsCompleted.Subscribe(OnComplete).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_VerticalEntitiesAdded = false;
		base.OnUnbind();
		TooltipHelper.HideTooltip();
		TooltipHelper.HideInfo();
	}

	private void OnComplete(bool state)
	{
		string text = ((base.ViewModel.BlueprintSelectionWithUI == null) ? base.ViewModel.PhaseName.CurrentValue : ((string)(state ? base.ViewModel.BlueprintSelectionWithUI.Title : base.ViewModel.BlueprintSelectionWithUI.CallToAction)));
		if ((bool)m_HeaderLabel)
		{
			m_HeaderLabel.text = text;
		}
		if ((bool)m_ListSelectable)
		{
			m_ListSelectable.SetActiveLayer((!state) ? 1 : 0);
		}
	}

	public void AddInput()
	{
	}

	private void AddInputToPaperHints()
	{
	}
}
