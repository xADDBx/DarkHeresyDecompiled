using System.Collections.Generic;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenSummaryPhaseDetailedConsoleView : CharGenSummaryPhaseDetailedView
{
	[Header("Common")]
	[SerializeField]
	private CharInfoAbilityScoresBlockBaseView m_AbilityScores;

	[Header("Secondary Info")]
	[SerializeField]
	private GameObject m_SecondaryInfoViewContainer;

	[SerializeField]
	private InfoSectionView m_SecondaryInfoView;

	private readonly ReactiveProperty<ActivePhaseNavigation> m_ActivePhaseNavigation = new ReactiveProperty<ActivePhaseNavigation>(ActivePhaseNavigation.Menu);

	private readonly ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanDecline = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanShowInfo = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanSwitchNavigation = new ReactiveProperty<bool>();

	private IConsoleEntity m_ContentEntity;

	private bool m_HasTooltip;

	private bool m_IsOnRightNavigation;

	private TooltipConfig m_TooltipConfig;

	public override void Initialize()
	{
		base.Initialize();
		m_SecondaryInfoViewContainer.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_TooltipConfig = new TooltipConfig
		{
			TooltipPlace = m_SecondaryInfoView.GetComponent<RectTransform>(),
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		m_SecondaryInfoView.Bind(base.ViewModel.SecondaryInfoVM);
		m_CanGoNextOnConfirm.Value = true;
		m_CanDecline.Subscribe(delegate(bool value)
		{
			m_CanGoBackOnDecline.Value = !value;
		}).AddTo(this);
	}

	public void AddInput()
	{
	}
}
