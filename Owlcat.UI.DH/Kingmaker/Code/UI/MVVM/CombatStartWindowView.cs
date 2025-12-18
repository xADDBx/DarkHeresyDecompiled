using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CombatStartWindowView : View<CombatStartWindowVM>
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_CanDeployLabel;

	[SerializeField]
	private TextMeshProUGUI m_CannotDeployLabel;

	[SerializeField]
	protected OwlcatMultiButton m_StartBattleButton;

	[SerializeField]
	private TextMeshProUGUI m_ButtonLabel;

	[SerializeField]
	private CombatStartCoopProgressBaseView m_ProgressBaseView;

	private IDisposable m_CantStartBattleHint;

	private void Awake()
	{
		base.gameObject.SetActive(value: false);
		UITurnBasedTexts turnBasedTexts = UIStrings.Instance.TurnBasedTexts;
		m_ButtonLabel.text = turnBasedTexts.StartBattle.Text;
		m_CanDeployLabel.text = turnBasedTexts.DeploymentPhaseBattle.Text;
		m_CannotDeployLabel.text = turnBasedTexts.CannotDeploy.Text;
		if (m_ProgressBaseView != null)
		{
			m_ProgressBaseView.Initialize();
		}
	}

	protected override void OnBind()
	{
		Appear();
		m_CanDeployLabel.gameObject.SetActive(base.ViewModel.CanDeploy);
		m_CannotDeployLabel.gameObject.SetActive(!base.ViewModel.CanDeploy);
		base.ViewModel.CanStartCombat.CombineLatest(base.ViewModel.CannotStartCombatReason, (bool can, string reason) => new { can, reason }).Subscribe(value =>
		{
			m_StartBattleButton.Interactable = value.can;
			if (value.can)
			{
				m_CantStartBattleHint?.Dispose();
			}
			else
			{
				m_CantStartBattleHint = m_StartBattleButton.SetHint(value.reason);
			}
		}).AddTo(this);
		if (m_ProgressBaseView != null)
		{
			m_ProgressBaseView.Bind(base.ViewModel.CoopProgressVM);
		}
	}

	protected override void OnUnbind()
	{
		Disappear();
		m_CantStartBattleHint?.Dispose();
	}

	private void Appear()
	{
		m_FadeAnimator.AppearAnimation();
	}

	private void Disappear()
	{
		m_FadeAnimator.DisappearAnimation();
	}
}
